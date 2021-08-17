using Godot;
using Medallion.Collections;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase
{
	/// <summary>
	/// The module manager interface.
	/// </summary>
	public interface IModuleManager
	{
		/// <summary>
		/// Check if a module is loaded.
		/// </summary>
		/// <param name="id">The ID of the module to check.</param>
		/// <returns>True if loaded, false otherwise.</returns>
		bool IsLoaded( string id );
	}

	/// <summary>
	/// The module manager itself.
	/// </summary>
	internal class ModuleManager : IModuleManager
	{
#if !GODOT_WEB
		private class ModuleZip
		{
			public string path;
			public IManifest manifest;
			public ZipArchive zip;
		}
		private ModuleZip currProcessing;
#else
		private object currProcessing;
#endif

		internal readonly List<Module> modules = new List<Module>();

		internal ModuleManager()
		{
		}

		internal void Initialize()
		{
			Log.Info( "Initializing module manager..." );
			LoadModules();

			// Load translations
			Log.Info( "Loading translations..." );
			var localeCodeRegex = new System.Text.RegularExpressions.Regex( "..(_..)?" );
			foreach ( var module in modules )
			{
				Log.Debug( $"Loading translations for module {module.Manifest.Id}" );
				using ( var dir = new Godot.Directory() )
				{
					dir.Open( $"res://module/{module.Manifest.Id}/i18n/" );
					dir.ListDirBegin();
					for ( string file = dir.GetNext(); !string.IsNullOrEmpty( file ); file = dir.GetNext() )
					{
						// Tried using TranslationServer.GetLoadedLocales() but it was empty???
						// I guess it only has entries for ones the project has locales for at build time.
						// Since I'm loading everything at run-time...
						if ( file.EndsWith( ".json" ) && localeCodeRegex.IsMatch( file.Substring( 0, file.Length - 5 ) ) )
							LocalizationManager.RegisterTranslations( file.Substring( 0, file.Length - 5 ), $"res://module/{module.Manifest.Id}/i18n/{file}" );
					}
					dir.ListDirEnd();
				}
			}

			// Initialize modules
			Log.Info( "Notifying modules loading completed..." );
			foreach ( var module in modules )
			{
				try
				{
					Log.Debug( $"Notifying module {module.Manifest.Id}" );
					module.AfterModulesLoaded();
				}
				catch ( Exception e )
				{
					Log.Error( $"Exception while notifying module {module.Manifest.Id}:\n{e}" );
				}
			}
		}

		/// <summary>
		/// Check if a module is loaded.
		/// </summary>
		/// <param name="id">The module to check if is loaded.</param>
		/// <returns>True if it is loaded, false otherwise.</returns>
		public bool IsLoaded( string id )
		{
			return modules.Find( m => m.Manifest.Id == id ) != null;
		}

		private void LoadModules()
		{
#if GODOT_WEB
			Log.Info( "Loading modules not supported on web!" );
#else
			Log.Info( "Loading modules..." );

			// Get path to modules
			// OS.HasFeature( "standalone" ) will be false for run from editor, true otherwise
			// Important because OS.GetExecutablePath() will return the Godot binary path when run from editor.
			var exePath = OS.HasFeature( "standalone" ) ? OS.GetExecutablePath().GetBaseDir() : ProjectSettings.GlobalizePath( "res://" );
			var modulesPath = System.IO.Path.Combine( exePath, "modules" );
			Log.Debug( "Path to modules: " + modulesPath );

			if ( !System.IO.Directory.Exists( modulesPath ) )
			{
				Log.Debug( "Modules directory does not exist, creating..." );
				System.IO.Directory.CreateDirectory( modulesPath );
			}

			// Find all packs
			List< ModuleZip > packs = new List< ModuleZip >();
			var files = System.IO.Directory.GetFiles( modulesPath );
			foreach ( var file in files )
			{
				if ( file.StartsWith( "." ) || !file.EndsWith( ".zip" ) )
					continue;

				// Open zip and get manifest
				ZipArchive zip = ZipFile.OpenRead( file );
				var manifestEntry = zip.GetEntry( "manifest.json" );
				if ( manifestEntry == null )
				{
					Log.Error( $"Zip {file} has no manifest.json!" );
					continue;
				}

				// Deserialize manifest, add to packs
				using ( var stream = manifestEntry.Open() )
				using ( var reader = new System.IO.StreamReader( stream ) )
				{
					var manifestStr = reader.ReadToEnd();
					var manifest = JsonConvert.DeserializeObject<Manifest>( manifestStr );
					packs.Add( new ModuleZip() { path = file, manifest = manifest, zip = zip } );
				}
			}

			// Detect missing dependencies and remove modules having them
			List< ModuleZip > packsToRemove = new List< ModuleZip >();
			foreach ( var pack in packs )
			{
				if ( ( pack.manifest.Dependencies?.Length ?? 0 ) <= 0 )
					continue;

				bool[] packsFound = new bool[ pack.manifest.Dependencies.Length ];
				foreach ( var otherPack in packs )
				{
					for ( int i = 0; i < pack.manifest.Dependencies.Length; ++i )
					{
						if ( pack.manifest.Dependencies[ i ] == otherPack.manifest.Id )
							packsFound[ i ] = true;
					}
				}

				if ( !packsFound.Contains( false ) )
				{
					Log.Error( $"\"{pack.manifest.Name}\" is missing dependencies!" );
					packsToRemove.Add( pack );
				}
			}
			packs.RemoveAll( mz => packsToRemove.Contains( mz ) );
			foreach ( var pack in packsToRemove )
			{
				pack.zip.Dispose();
			}

			// Sort alphabetically so load order is consistent between runs when sorted later.
			// (This is why a stable sort is used.)
			packs.Sort( ( mz1, mz2 ) => mz1.manifest.Id.CompareTo( mz2.manifest.Id ) );

			// Sort by dependencies
			Func< ModuleZip, IEnumerable< ModuleZip > > getDeps = ( curr ) =>
			{
				List< ModuleZip > deps = new List< ModuleZip >();
				foreach ( var pack in packs )
				{
					if ( curr.manifest.Dependencies.Contains( pack.manifest.Id ) )
						deps.Add( pack );
				}
				return deps;
			};
			packs.StableOrderTopologicallyBy( getDeps );

			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			// Load the module packs, and module objects into modules.
			foreach ( var pack in packs )
			{
				try
				{
					currProcessing = pack;

					// Get bytes of assembly and pdb (if exists)
					byte[] asmBytes, pdbBytes = null;
					var asmEntry = pack.zip.GetEntry( $"{pack.manifest.Id}.dll" );
					var pdbEntry = pack.zip.GetEntry( $"{pack.manifest.Id}.pdb" );
					using ( var zstream = asmEntry.Open() )
					using ( var mstream = new MemoryStream( ( int ) asmEntry.Length ) )
					{
						zstream.CopyTo( mstream );
						asmBytes = mstream.ToArray();
					}
					if ( pdbEntry != null )
					{
						using ( var zstream = asmEntry.Open() )
						using ( var mstream = new MemoryStream( ( int ) pdbEntry.Length ) )
						{
							zstream.CopyTo( mstream );
							pdbBytes = mstream.ToArray();
						}
					}

					// Load
					Assembly asm;
					if ( pdbBytes == null )
						asm = Assembly.Load( asmBytes );
					else
						asm = Assembly.Load( asmBytes, pdbBytes );

					// Create instance and add to modules
					var moduleType = asm.ExportedTypes.First( t => t.IsSubclassOf( typeof( Module ) ) );
					var module = ( Module ) Activator.CreateInstance( moduleType );
					module.Manifest = pack.manifest;

					modules.Add( module );

					// Dispose of the pack zip so we can load it with Godot
					pack.zip?.Dispose();
					pack.zip = null;

					// Load the resource pack
					// Maybe later I should allow replacing files? Not sure
					if ( !ProjectSettings.LoadResourcePack( pack.path, false ) )
						Log.Error( $"Failed to load resource pack for module {pack.manifest.Id}!" );

					// Not sure why I had this here...
					/*
					Action<string> list = null;
					list = (dir_) =>
					{
						using ( var dir = new Godot.Directory() )
						{
							dir.Open( dir_ );
							dir.ListDirBegin();
							for ( var thing = dir.GetNext(); thing != ""; thing = dir.GetNext() )
							{
								if ( thing == "." || thing == ".." )
									continue;
								if ( dir.CurrentIsDir() )
								{
									Log.Info( $"{thing}/" );
									list( $"{dir_}/{thing}" );
								}
								else
									Log.Info( thing );

							}
							dir.ListDirEnd();
						}
					};
					list( "res://" );
					*/

				}
				catch ( Exception e )
				{
					Log.Error( "Exception loading module: " + e );
				}
				finally
				{
					pack.zip?.Dispose();
				}
			}
#endif
		}

		private Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
		{
			Log.Info( "TODO: Assembly resolving for modules" );
			if ( currProcessing != null )
			{
				// give this priority
			}

			// load from all modules
			return null;
		}
	}
}
