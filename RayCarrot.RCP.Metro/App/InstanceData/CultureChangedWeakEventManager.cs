using System;
using System.Globalization;
using System.Windows;

namespace RayCarrot.RCP.Metro
{
    public class CultureChangedWeakEventManager : WeakEventManager
    {
        private CultureChangedWeakEventManager()
        {

        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(IAppInstanceData source, EventHandler<PropertyChangedEventArgs<CultureInfo>> handler)
        {
            if (source == null) 
                throw new ArgumentNullException(nameof(source));
            if (handler == null) 
                throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(IAppInstanceData source, EventHandler<PropertyChangedEventArgs<CultureInfo>> handler)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static CultureChangedWeakEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(CultureChangedWeakEventManager);
                CultureChangedWeakEventManager manager = (CultureChangedWeakEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new CultureChangedWeakEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        /// <summary>
        /// Return a new list to hold listeners to the event.
        /// </summary>
        protected override ListenerList NewListenerList()
        {
            return new ListenerList<PropertyChangedEventArgs<CultureInfo>>();
        }

        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected override void StartListening(object source)
        {
            IAppInstanceData typedSource = (IAppInstanceData)source;
            typedSource.CultureChanged += OnSomeEvent;
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected override void StopListening(object source)
        {
            IAppInstanceData typedSource = (IAppInstanceData)source;
            typedSource.CultureChanged -= OnSomeEvent;
        }

        /// <summary>
        /// Event handler for the SomeEvent event.
        /// </summary>
        private void OnSomeEvent(object sender, PropertyChangedEventArgs<CultureInfo> e)
        {
            DeliverEvent(sender, e);
        }
    }
}