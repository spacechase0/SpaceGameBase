using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// Various extensions for Godot.Node.
    /// </summary>
    public static class GodotNodeExtensions
    {
        /// <summary>
        /// Dump info on the immediate children of a node into the console/log.
        /// </summary>
        /// <param name="node">The node.</param>
        public static void DumpChildren( this Node node )
        {
            Log.Debug( $"{node} {node.Name} {node.GetType()} has {node.GetChildCount()} children:" );
            for ( int i = 0; i < node.GetChildCount(); ++i )
            {
                var child = node.GetChild( i );
                Log.Debug( $"\t{child} {child.Name} {child.GetType()}" );
            }
        }

        /// <summary>
        /// Based on Node.FindNode.
        /// Finds all descendants of this node whose name matches mask as in String.match (i.e.
        /// case-sensitive, but "*" matches zero or more characters and "?" matches any single
        /// character except ".").
        /// Note: It does not match against the full path, just against individual node names.
        /// If owned is true, this method only finds nodes whose owner is this node. This
        /// is especially important for scenes instantiated through a script, because those
        /// scenes don't have an owner.
        /// </summary>
        /// <param name="node">The node to search the children of.</param>
        /// <param name="pattern">The pattern to check names against.</param>
        /// <param name="recursive">If the search is recursive.</param>
        /// <param name="owned">If the method should only find nodes whose owner is this node.</param>
        /// <returns>A list with the matching children.</returns>
        public static List<Node> FindNodes( this Node node, string pattern, bool recursive = true, bool owned = true )
        {
            return FindNodesImpl( node, node, pattern, recursive, owned );
        }

        private static List<Node> FindNodesImpl( Node owner, Node node, string pattern, bool recursive, bool owned )
        {
            var ret = new List<Node>();

            // For some reason Godot's match causes a stackoverflow. Using regex instead
            var regex = new Regex(pattern.Replace('?', '.').Replace( "*", ".*" ) );

            foreach ( var child_ in node.GetChildren() )
            {
                var child = child_ as Node;

                if ( regex.IsMatch( child.Name ) && ( !owned || owned && child.Owner == owner ) )
                {
                    ret.Add( child );
                }

                if ( recursive )
                    ret.AddRange( FindNodesImpl( owner, child, pattern, recursive, owned ) );
            }

            return ret;
        }
    }
}
