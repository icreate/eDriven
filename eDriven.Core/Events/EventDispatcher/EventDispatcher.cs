#region License

/*
 
Copyright (c) 2012 Danko Kozar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 
*/

#endregion License

using System;
using System.Collections.Generic;
using UnityEngine; // debugging only

namespace eDriven.Core.Events
{
    /// <summary>
    /// The EventDispatcher
    /// Base class for all classes that dispatch events
    /// </summary>
    /// <remarks>Coded by Danko Kozar</remarks>
    public class EventDispatcher : IEventDispatcher, IEventQueue, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EventDispatcher()
        {
            _eventDispatcherTarget = this;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public EventDispatcher(object target)
        {
            if (null == target)
                throw new Exception("Target cannot be null");

            _eventDispatcherTarget = target;
        }

        #region Properties

#if DEBUG
        /// <summary>
        /// Debug mode
        /// </summary>
        public static bool DebugMode;
#endif

        #endregion

        #region Members

        private readonly object _eventDispatcherTarget;

        /// <summary>
        /// A dictionary holding the references to event listeners
        /// For each event type, more that one event listener could be subscribed (one-to-many relationship)
        /// </summary>
        private readonly Dictionary<EventTypePhase, List<EventHandler>> _eventHandlerDict = new Dictionary<EventTypePhase, List<EventHandler>>();
        
        /// <summary>
        /// The queue used for delayed processing
        /// </summary>
        private readonly List<Event> _queue = new List<Event>();

        #endregion

        #region Subscribing / unsubscribing

        /// <summary>
        /// Adds the event listener
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Event handler</param>
        /// <param name="phases">Event bubbling phases that we listen to</param>
        public virtual void AddEventListener(string eventType, EventHandler handler, EventPhase phases)
        {
            var arr = EventPhaseHelper.BreakUpPhases(phases);

            foreach (EventPhase phase in arr)
            {
                EventTypePhase key = new EventTypePhase(eventType, phase);

                if (!_eventHandlerDict.ContainsKey(key)) // avoid key duplication
                    _eventHandlerDict.Add(key, new List<EventHandler>());

                if (!_eventHandlerDict[key].Contains(handler)) // avoid key + value duplication
                    _eventHandlerDict[key].Add(handler);
            }
        }

        /// <summary>
        /// AddEventListener Overload
        /// Assumes that useCapturePhase is false
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Event handler (function)</param>
        public virtual void AddEventListener(string eventType, EventHandler handler)
        {
            AddEventListener(eventType, handler, EventPhase.Target/* | EventPhase.Bubbling*/);
        }

        /// <summary>
        /// Removes the event listener
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Event handler (function)</param>
        /// <param name="phases">Event bubbling phases that we listen to</param>
        public virtual void RemoveEventListener(string eventType, EventHandler handler, EventPhase phases)
        {
            var arr = EventPhaseHelper.BreakUpPhases(phases);

            foreach (EventPhase phase in arr)
            {
                EventTypePhase key = new EventTypePhase(eventType, phase);

                if (_eventHandlerDict.ContainsKey(key))
                {
                    if (_eventHandlerDict[key].Contains(handler))
                        _eventHandlerDict[key].Remove(handler);

                    if (_eventHandlerDict[key].Count == 0) // cleanup
                        _eventHandlerDict.Remove(key);
                }
            }
        }
        
        /// <summary>
        /// Removes the event listener
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Event handler (function)</param>
        public virtual void RemoveEventListener(string eventType, EventHandler handler)
        {
            RemoveEventListener(eventType, handler, EventPhase.Target/* | EventPhase.Bubbling*/); // this is the default
        }

        /// <summary>
        /// Returns true if handler is mapped to any of the specified phases
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="handler"></param>
        /// <param name="phases"></param>
        /// <returns></returns>
        public virtual bool MappedToAnyPhase(string eventType, EventHandler handler, EventPhase phases)
        {
            var arr = EventPhaseHelper.BreakUpPhases(phases);

            foreach (EventPhase phase in arr)
            {
                EventTypePhase key = new EventTypePhase(eventType, phase);
                if (_eventHandlerDict.ContainsKey(key) && _eventHandlerDict[key].Contains(handler))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Removes all listeners for the spacified event type (both capture and bubbling phase)
        /// </summary>
        /// <param name="eventType">Event type</param>
        public virtual void RemoveAllListeners(string eventType)
        {
            RemoveAllListeners(eventType, EventPhase.Bubbling);
            RemoveAllListeners(eventType, EventPhase.Target);
            RemoveAllListeners(eventType, EventPhase.Capture);
        }

        /// <summary>
        /// Removes all listeners for the spacified event type and phases
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// /// <param name="phases"></param>
        public virtual void RemoveAllListeners(string eventType, EventPhase phases)
        {
            var arr = EventPhaseHelper.BreakUpPhases(phases);

            foreach (EventPhase phase in arr)
            {
                EventTypePhase key = new EventTypePhase(eventType, phase);
                if (_eventHandlerDict.ContainsKey(key))
                {
                    _eventHandlerDict[key].Clear();
                    _eventHandlerDict.Remove(key);
                }
            }
        }

        /// <summary>
        /// Returns true if EventDispatcher has any registered listeners for a specific type and phase
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool HasEventListener(string eventType)
        {
            return _eventHandlerDict.ContainsKey(new EventTypePhase(eventType, EventPhase.Target)); // TODO: Optimize
        }

        /// <summary>
        /// Returns true if there are any subscribers in bubbling hierarchy
        /// Override in superclass
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public virtual bool HasBubblingEventListener(string eventType)
        {
            return HasEventListener(eventType);
        }

        #endregion

        #region Dispatching

        /// <summary>
        /// Dispatches an event with the option of late processing (immediate = TRUE/FALSE)
        /// </summary>
        /// <param name="e">Event</param>
        /// <param name="immediate">Process immediatelly or delayed?</param>
        public virtual void DispatchEvent(Event e, bool immediate)
        {
#if DEBUG
            if (DebugMode)
                Debug.Log(string.Format("Dispatching event [{0}]", e));
#endif
            // do nothing if event has already been canceled
            if (e.Canceled)
                return;

            // set target if not already set
            if (null == e.Target)
                e.Target = _eventDispatcherTarget;

            // set current target if not already set
            if (null == e.CurrentTarget)
                e.CurrentTarget = e.Target;

            if (immediate)
            {
                /**
                 * 1) Immediate dispatching
                 * The code from the event listener is run NOW
                 * */
                ProcessEvent(e);
            }
            else
            {
                /**
                 * 2) Delayed dispatching
                 * Processed when ProcessQueue is called by the external code
                 * */
                EnqueueEvent(e);
            }
        }

        /// <summary>
        /// Dispatches an event immediatelly
        /// </summary>
        /// <param name="e">Event</param>
        public virtual void DispatchEvent(Event e)
        {
            DispatchEvent(e, true);
        }

        #endregion

        #region Event processing

        /// <summary>
        /// Could be overriden in a subclass (for instance to implement event bubbling)
        /// </summary>
        /// <param name="e">Event to dispatch</param>
        protected virtual void ProcessEvent(Event e)
        {
            ExecuteListeners(e);
        }

        /// <summary>
        /// Executes event handlers listening for a particular event type
        /// NOTE: Public by purpose
        /// </summary>
        /// <param name="e">Event to dispatch</param>
        /// <remarks>Conceived by Danko Kozar</remarks>
        public virtual void ExecuteListeners(Event e)
        {
            // return if event canceled
            if (e.Canceled)
                return;

            EventTypePhase key = new EventTypePhase(e.Type, e.Phase);

            // find event handlers subscribed for this event
            if (_eventHandlerDict.ContainsKey(key) && null != _eventHandlerDict[key])
            {
                // execute each handler
                _eventHandlerDict[key].ForEach(
                    delegate(EventHandler handler)
                    {
                        if (e.Canceled) // the event might have been canceled by the previous listener in the collection
                            return;

                        handler(e); // execute the handler with an event as argument
                    }
                );
            }
        }

        #endregion

        #region Queued processing

        /// <summary>
        /// Adds an event to the queue
        /// The queue will be processed when ProcessQueue() manually executed
        /// </summary>
        /// <param name="e"></param>
        public virtual void EnqueueEvent(Event e)
        {
#if DEBUG
            if (DebugMode)
                Debug.Log(string.Format("Enqueueing event [{0}]. Queue count: {1}", e, _queue.Count));
#endif
            _queue.Add(e);
        }

        /// <summary>
        /// If events are added to queue, they are waiting to be fired<br/>
        /// in the same order they are added
        /// </summary>
        public virtual void ProcessQueue()
        {
            // return if nothing to process
            if (0 == _queue.Count)
            {
#if DEBUG
                if (DebugMode)
                    Debug.Log(string.Format("No enqueued events to process"));
#endif
                return;
            }

            // dispatch each event in the queue now
            _queue.ForEach(delegate(Event e)
                               {
                                   ExecuteListeners(e);
                               });

#if DEBUG
            if (DebugMode)
                Debug.Log(string.Format("Processed {0} enqueued events", _queue.Count));
#endif

            // empty the queue
            _queue.Clear();
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            List<EventTypePhase> keysToRemove = new List<EventTypePhase>();
            foreach (EventTypePhase key in _eventHandlerDict.Keys)
            {
                keysToRemove.Add(key);
            }

            keysToRemove.ForEach(key => RemoveAllListeners(key.EventType, key.Phase));

            _eventHandlerDict.Clear();
        }

        #endregion

        #region Preventing default

        /// <summary>
        /// Exposes the cancelable event to the outside if there are listeners for that event type
        /// If default prevented, returns false
        /// If not, returns true
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="bubbles"></param>
        /// <returns></returns>
        /// <remarks>Conceived by Danko Kozar</remarks>
        public bool IsDefaultPrevented(string eventType, bool bubbles)
        {
            // prevent removing
            if (HasEventListener(eventType))
            {
                var e = new Event(eventType, bubbles, true);

                DispatchEvent(e);

                return e.DefaultPrevented;
            }
            
            return false; // return false because there are no listeners to prevent default
        }

        /// <summary>
        /// No-bubbling version
        /// </summary>
        /// <param name="eventType"></param>
        /// <returns></returns>
        public bool IsDefaultPrevented(string eventType)
        {
            return IsDefaultPrevented(eventType, false);
        }

        #endregion

    }
}