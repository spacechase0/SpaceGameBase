using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// Index of all registries and their contents.
    /// </summary>
    public static class RegistryIndex
    {
        private static readonly Dictionary<string, RegistryBase> registries = new Dictionary<string, RegistryBase>();
        private static readonly Dictionary<ContextualId, object> data = new Dictionary<ContextualId, object>();

        internal static void Add( RegistryBase registry )
        {
            registries.Add( registry.Id, registry );
        }

        internal static void AddData( ContextualId id, object data )
        {
            RegistryIndex.data.Add( id, data );
        }

        /// <summary>
        /// Does the specified ID exist.
        /// </summary>
        /// <param name="id">The ID</param>
        /// <returns></returns>
        public static bool HasData( ContextualId id )
        {
            return data.ContainsKey( id );
        }
    }

    /// <summary>
    /// Base class for registries.
    /// </summary>
    public class RegistryBase
    {
        /// <summary>
        /// The ID of this registry.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The constructor.
        /// Also registers with RegistryIndex.
        /// </summary>
        /// <param name="id">The ID of this registry.</param>
        protected RegistryBase( string id )
        {
            Id = id;
            RegistryIndex.Add( this );
        }
    }

    /// <summary>
    /// Registry class, for associating unique data with a ContextualId.
    /// Registries are global, and so don't use ContextualId for their own IDs.
    /// Objects registered with a registry are also registered with RegistryIndex, 
    /// and so much be unique across all registries.
    /// </summary>
    /// <typeparam name="T">The type of the objects stored in this registry.</typeparam>
    public class Registry<T> : RegistryBase
    {
        internal Dictionary<ContextualId, T> objects = new Dictionary<ContextualId, T>();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="id">The ID of the registry.</param>
        public Registry( string id )
        : base( id )
        {
        }

        /// <summary>
        /// Add an object to this registry.
        /// </summary>
        /// <param name="id">The ID of the object.</param>
        /// <param name="obj">The object itself.</param>
        public void Add( ContextualId id, T obj )
        {
            if ( objects.ContainsKey( id ) || RegistryIndex.HasData( id ) )
                throw new ArgumentException( $"An object already exists somewhere for the ID '{id}'.", nameof( id ) );

            Log.Verbose( $"Adding {id}={obj} to registry {Id}..." );
            objects.Add( id, obj );
            RegistryIndex.AddData( id, obj );
        }

        /// <summary>
        /// Get an object with the specified ID.
        /// </summary>
        /// <param name="id">The Id of the object.</param>
        /// <returns>The object with the given ID.</returns>
        public T Get( ContextualId id )
        {
            return objects[ id ];
        }

        /// <summary>
        /// Get the keys for every entry in this registry.
        /// </summary>
        /// <returns></returns>
        public ContextualId[] GetAll()
        {
            return objects.Keys.ToArray();
        }

        // Should I add the ability to remove? Hmm
    }
}
