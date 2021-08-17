using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// The extension methods to make LambdaWrapper work.
    /// </summary>
    public static class LambdaExtensions
    {
        /// <summary>
        /// Connect the wrapper to a given object and signal.
        /// </summary>
        /// <param name="obj">The Godot object to connect to.</param>
        /// <param name="signal">The signal to connect to.</param>
        /// <param name="action">The action for the lambda.</param>
        /// <param name="binds">Any parameter bindings for the lambda.</param>
        /// <param name="flags">The connection flags for Godot.</param>
        public static void ConnectLambda( this Godot.Object obj, string signal, Action action, Godot.Collections.Array binds = null, uint flags = 0 )
        {
            obj.Connect( signal, new LambdaWrapper( action ), nameof( LambdaWrapper.Invoke ), binds, flags );
        }

        /// <summary>
        /// Connect the wrapper to a given object and signal.
        /// </summary>
        /// <param name="obj">The Godot object to connect to.</param>
        /// <param name="signal">The signal to connect to.</param>
        /// <param name="action">The action for the lambda.</param>
        /// <param name="binds">Any parameter bindings for the lambda.</param>
        /// <param name="flags">The connection flags for Godot.</param>
        public static void ConnectLambda<T>( this Godot.Object obj, string signal, Action<T> action, Godot.Collections.Array binds = null, uint flags = 0 )
        {
            obj.Connect( signal, new LambdaWrapper<T>( action ), nameof( LambdaWrapper<T>.Invoke ), binds, flags );
        }
    }

    /// <summary>
    /// Used for connecting lambdas to Godot signals.
    /// </summary>
    class LambdaWrapper : Godot.Object
    {
        private readonly Action action;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="theAction">The action to call when the signal triggers.</param>
        public LambdaWrapper( Action theAction )
        {
            action = theAction;
        }

        /// <summary>
        /// Invoke the action given in the constructor.
        /// </summary>
        public void Invoke()
        {
            action.Invoke();
        }
    }

    /// <summary>
    /// Used for connecting lambdas to Godot signals.
    /// </summary>
    class LambdaWrapper<T> : Godot.Object
    {
        private readonly Action<T> action;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="theAction">The action to call when the signal triggers.</param>
        public LambdaWrapper( Action<T> theAction )
        {
            action = theAction;
        }

        /// <summary>
        /// Invoke the action given in the constructor with the given arguments.
        /// </summary>
        public void Invoke( T t )
        {
            action.Invoke( t );
        }
    }
}
