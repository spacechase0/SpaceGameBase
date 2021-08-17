using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// An ID with a "context", ie. source or domain.
    /// This is akin to Minecraft's ResourceLocation, which is also used for IDs (kinda).
    /// </summary>
    public struct ContextualId
    {
        /// <summary>
        /// The context of this ID.
        /// This would be "spacechase0.PetKAR.CoreModule" for the base game, or something like "spacechase0.PetKAR.DLC_01" for DLC.
        /// For a mod, it'd be the mod ID.
        /// </summary>
        public string Context { get; }

        /// <summary>
        /// The ID itself. This must be unique within its context.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="id">The id.</param>
        public ContextualId( string context, string id )
        {
            Context = context;
            Id = id;
        }

        /// <summary>
        /// Constructor from a single string.
        /// </summary>
        /// <param name="str">The string, must be in context:id format.</param>
        public ContextualId( string str )
        {
            if ( !str.Contains( ':' ) )
                throw new ArgumentException( "Bad format for str" );

            int colon = str.IndexOf( ':' );
            Context = str.Substring( 0, colon );
            Id = str.Substring( colon + 1 );
        }

        /// <summary>
        /// Get a version of the ToString() safe for node paths.
        /// </summary>
        /// <returns>A node-path-safe string for this contextual ID.</returns>
        public string GetNodePathSafeString()
        {
            return ToString().Replace( ':', '$' ).Replace( '.', '_' );
        }

        /// <inheritdoc/>
        public override bool Equals( object obj )
        {
            if ( obj is ContextualId id )
            {
                return Context == id.Context && Id == id.Id;
            }
            return false;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Context + ":" + Id;
        }

        /// <summary>
        /// Compare two contextual IDs to see if they are equal.
        /// </summary>
        /// <param name="a">The first ID to check.</param>
        /// <param name="b">The second ID to check.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public static bool operator ==( ContextualId a, ContextualId b )
        {
            return a.Equals( b );
        }

        /// <summary>
        /// Compare two contextual IDs to see if they aren't equal.
        /// </summary>
        /// <param name="a">The first ID to check.</param>
        /// <param name="b">The second ID to check.</param>
        /// <returns>True if not equal, false otherwise.</returns>
        public static bool operator !=( ContextualId a, ContextualId b )
        {
            return !a.Equals( b );
        }
    }
}
