using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase
{
    /// <summary>
    /// Static log abstraction, in case we change logging later to log4net or something.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// Debug mode.
        /// </summary>
        public static bool DoDebug =
#if DEBUG
            true
#else
            false
#endif
            ;

        /// <summary>
        /// Verbose logging. Only active in Debug mode.
        /// </summary>
        public static bool DoVerbose = false;


        /// <summary>
        /// Output verbose debug information, only in debug mode with verbose logging on.
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static void Verbose( object str )
        {
            if ( DoVerbose && DoDebug )
#if GODOT_WEB
                GD.Print( $"<{Assembly.GetCallingAssembly().GetName().Name}> {str}" );
#else
                GD.Print( $"<{new StackFrame( 1 ).GetMethod().DeclaringType.FullName}> {str}" );
#endif
        }

        /// <summary>
        /// Output debug information, only in debug mode.
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static void Debug( object str )
        {
            if ( DoDebug )
#if GODOT_WEB
                GD.Print( $"<{Assembly.GetCallingAssembly().GetName().Name}> {str}" );
#else
                GD.Print( $"<{new StackFrame( 1 ).GetMethod().DeclaringType.FullName}> {str}" );
#endif
        }

        /// <summary>
        /// Output information.
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static void Info( object str )
        {
            GD.Print( $"[{Assembly.GetCallingAssembly().GetName().Name}] {str}" );
        }

        /// <summary>
        /// Output an error.
        /// </summary>
        /// <param name="str"></param>
        [MethodImpl( MethodImplOptions.NoInlining )]
        public static void Error( object str )
        {
            GD.PrintErr( $"[{Assembly.GetCallingAssembly().GetName().Name}] {str}" );
        }
    }
}
