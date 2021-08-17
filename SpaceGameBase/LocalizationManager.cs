using Godot;
using Newtonsoft.Json;
using PetKar.Util;
using SpaceGameBase.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase
{
    /// <summary>
    /// Translation data for deserializing from JSON.
    /// </summary>
    public class TranslationData
    {
        /// <summary>
        /// The translated asset index.
        /// </summary>
        public Dictionary<string, string> Assets { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The translated strings.
        /// </summary>
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Handles localization on top of Godot.
    /// </summary>
    public static class LocalizationManager
    {
        /// <summary>
        /// Maintains a list of officially supported language codes.
        /// </summary>
        public static class Languages
        {
            /// <summary>
            /// English.
            /// </summary>
            public static readonly string English = "en";
        }

        private readonly static Dictionary<string, Dictionary<string, string>> translations = GetDefaultTranslations();
        private static Dictionary<string, Translation> godotTranslations;

        /// <summary>
        /// The current language code.
        /// </summary>
        public static string CurrentLanguage = Languages.English;

        /// <summary>
        /// Get the list of available languages.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetLanguages()
        {
            return translations.Keys;
        }

        /// <summary>
        /// Register translations from a file for the given language.
        /// For modules, these are automatically loaded from the i18n/ folder.
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="path"></param>
        public static void RegisterTranslations( string lang, string path )
        {
            var data = JsonConvert.DeserializeObject<TranslationData>( FileUtil.ReadTextFile( path ) );

            foreach ( var str in data.Strings )
            {
                RegisterStringTranslation( lang, str.Key, str.Value );
            }

            foreach ( var asset in data.Assets )
            {
                RegisterAssetTranslation( lang, asset.Key, asset.Value );
            }
        }

        /// <summary>
        /// Register a translation for English.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <param name="value">The translation string.</param>
        public static void RegisterStringTranslation( string key, string value )
        {
            RegisterStringTranslation( Languages.English, key, value );
        }

        /// <summary>
        /// Register a translation for the specified language.
        /// </summary>
        /// <param name="lang">The language code for the language.</param>
        /// <param name="key">The translation key.</param>
        /// <param name="value">The translation string.</param>
        public static void RegisterStringTranslation( string lang, string key, string value )
        {
            translations[ lang ].Add( key, value );
            godotTranslations[ lang ].AddMessage( key, value );
        }

        /// <summary>
        /// Register an asset translation for the specified language.
        /// TODO: Test
        /// </summary>
        /// <param name="lang">The language code for the language.</param>
        /// <param name="key">The asset path for the asset before translation remapping.</param>
        /// <param name="value">The asset path for the translated asset.</param>
        public static void RegisterAssetTranslation( string lang, string key, string value )
        {
            // Let's hope this doesn't break in later Godot releases!

            var remaps = ( Godot.Collections.Dictionary ) ProjectSettings.GetSetting( "locale/translation_remaps" );
            if ( !remaps.Contains( key ) )
            {
                remaps.Add( key, new string[] { $"{value}:{lang}" } );
                return;
            }

            List<string> langMaps = new List<string>( ( string[] ) remaps[ key ] );
            langMaps.Add( $"{value}:{lang}" );
            remaps[ key ] = langMaps.ToArray();
        }


        /// <summary>
        /// Get a translation for the current language for the specified key.
        /// Falls back to english, and then the message key.
        /// </summary>
        /// <param name="key">The translation key.</param>
        /// <returns>The translation string.</returns>
        public static string GetStringTranslation( string key )
        {
            if ( !translations[ CurrentLanguage ].ContainsKey( key ) )
            {
                if ( !translations[ Languages.English ].ContainsKey( key ) )
                    return key;
                return translations[ Languages.English ][ key ];
            }
            return translations[ CurrentLanguage ][ key ];
        }

        /// <summary>
        /// Get a translation for the specified language for the specified key, or null if there is none.
        /// </summary>
        /// <param name="lang">The language code.</param>
        /// <param name="key">The translation key.</param>
        /// <returns>The translation string.</returns>
        public static string GetStringTranslation( string lang, string key )
        {
            if ( !translations[ lang ].ContainsKey( key ) )
            {
                return null;
            }
            return translations[ lang ][ key ];
        }

        private static Dictionary<string, Dictionary<string, string>> GetDefaultTranslations()
        {
            godotTranslations = new Dictionary<string, Translation>();

            var ret = new Dictionary<string, Dictionary<string, string>>();

            // Register basic stuff
            ret.Add( Languages.English, new Dictionary<string, string>() );

            // Register with Godot translation stuff
            foreach ( var lang in ret )
            {
                var tr = new Translation();
                foreach ( var msg in lang.Value )
                {
                    tr.AddMessage( msg.Key, msg.Value );
                }
                godotTranslations.Add( lang.Key, tr );
                TranslationServer.AddTranslation( tr );
            }

            return ret;
        }
    }
}
