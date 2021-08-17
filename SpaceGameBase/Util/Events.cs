using PetKar.Events;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameBase.Util
{
    /// <summary>
    /// Priority for an event.
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// This event priority should be used if you only want to observe what happened.
        /// For instance, with an ICancelable event, other event handlers won't trigger after it is canceled,
        /// except the Monitor level, which will (but you can't un-cancel it).
        /// </summary>
        Monitor = 10,

        /// <summary>
        /// Lowest event priority.
        /// </summary>
        Lowest = 7,

        /// <summary>
        /// Lower event priority.
        /// </summary>
        Lower = 6,

        /// <summary>
        /// Low event priority.
        /// </summary>
        Low = 5,

        /// <summary>
        /// Normal (default) event priority.
        /// </summary>
        Normal = 4,

        /// <summary>
        /// High event priority.
        /// </summary>
        High = 3,

        /// <summary>
        /// Higher event priority.
        /// </summary>
        Higher = 2,

        /// <summary>
        /// Highest event priority.
        /// </summary>
        Highest = 1,
    }

    /// <summary>
    /// Attribute used for specifying event priority of a method.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class EventPriorityAttribute : Attribute
    {
        /// <summary>
        /// The event priority.
        /// </summary>
        public EventPriority Priority { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="priority">The event priority.</param>
        public EventPriorityAttribute( EventPriority priority )
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// An event handler for events with no specific parameters with priority support.
    /// </summary>
    public class PriorityEventHandler
    {
        /// <summary>
        /// The name of the event handler.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The event handler delegate type.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void EventHandler( object sender, EventArgs args );

        private readonly SimplePriorityQueue<EventHandler, int> handlers = new SimplePriorityQueue<EventHandler, int>();

        /// <summary>
        /// Where you register/deregister your event handler.
        /// </summary>
        public event EventHandler Event
        {
            add
            {
                int priority = (int) EventPriority.Normal;
                var attr = value.Method.CustomAttributes.FirstOrDefault( a => a.AttributeType == typeof( EventPriorityAttribute ) );
                if ( attr != null )
                    priority = (int) (EventPriority) attr.ConstructorArguments[0].Value;
                handlers.Enqueue( value, priority );
            }
            remove
            {
                handlers.Remove( value );
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the event handler.</param>
        public PriorityEventHandler( string name )
        {
            Name = name;
        }

        /// <summary>
        /// Invoke the event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        public void Invoke( object sender )
        {
            //Log.Verbose( $"Invoking event {Name}..." );

            var args = new EventArgs();
            foreach ( var handler in handlers )
            {
                try
                {
                    handler.Invoke( sender, args );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while handling event {Name}:\n{e}" );
                }
            }
        }
    }

    /// <summary>
    /// An event handler for events with a specific EventArgs parameter with priority support.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    public class PriorityEventHandler<T>
    {
        /// <summary>
        /// The name of the event handler.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The event handler delegate type.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void EventHandler( object sender, T args );

        private readonly SimplePriorityQueue<EventHandler, int> handlers = new SimplePriorityQueue<EventHandler, int>();

        /// <summary>
        /// Where you register/deregister your event handler.
        /// </summary>
        public event EventHandler Event
        {
            add
            {
                int priority = (int) EventPriority.Normal;
                var attr = value.Method.CustomAttributes.FirstOrDefault( a => a.AttributeType == typeof( EventPriorityAttribute ) );
                if ( attr != null )
                    priority = (int) (EventPriority) attr.ConstructorArguments[0].Value;
                handlers.Enqueue( value, priority );
            }
            remove
            {
                handlers.Remove( value );
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The event handler name.</param>
        public PriorityEventHandler( string name )
        {
            Name = name;
        }

        /// <summary>
        /// Invoke the event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public void Invoke( object sender, T args )
        {
            //Log.Verbose( $"Invoking event {Name}..." );

            foreach ( var handler in handlers )
            {
                try
                {
                    handler.Invoke( sender, args );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while handling event {Name}:\n{e}" );
                }
            }
        }
    }

    /// <summary>
    /// An event handler for cancelable events with a specific EventArgs parameter with priority support.
    /// </summary>
    /// <typeparam name="T">The event type.</typeparam>
    public class CancelablePriorityEventHandler<T> where T : ICancelable
    {
        /// <summary>
        /// The name of the event handler.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The event delegate type.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        public delegate void EventHandler( object sender, T args );

        private readonly SimplePriorityQueue<EventHandler, int> handlers = new SimplePriorityQueue<EventHandler, int>();
        private readonly List<EventHandler> monitors = new List<EventHandler>();

        /// <summary>
        /// Where you register/deregister your event.
        /// </summary>
        public event EventHandler Event
        {
            add
            {
                int priority = (int) EventPriority.Normal;
                var attr = value.Method.CustomAttributes.FirstOrDefault( a => a.AttributeType == typeof( EventPriorityAttribute ) );
                if ( attr != null )
                    priority = (int) (EventPriority) attr.ConstructorArguments[0].Value;
                if ( priority == (int) EventPriority.Monitor )
                    monitors.Add( value );
                else
                    handlers.Enqueue( value, priority );
            }
            remove
            {
                monitors.Remove( value );
                handlers.Remove( value );
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The event name.</param>
        public CancelablePriorityEventHandler( string name )
        {
            Name = name;
        }

        /// <summary>
        /// Invoke the event handler.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        /// <returns>True if you should continue, false otherwise.</returns>
        public bool Invoke( object sender, T args )
        {
            //Log.Verbose( $"Invoking event {Name}..." );

            foreach ( var handler in handlers )
            {
                try
                {
                    handler.Invoke( sender, args );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while handling event {Name}:\n{e}" );
                }
                if ( args.Cancel )
                    break;
            }

            bool canceled = args.Cancel;
            foreach ( var monitor in monitors )
            {
                try
                {
                    monitor.Invoke( sender, args );
                }
                catch ( Exception e )
                {
                    Log.Error( $"Exception while handling event {Name}:\n{e}" );
                }
            }

            return !canceled;
        }
    }
}
