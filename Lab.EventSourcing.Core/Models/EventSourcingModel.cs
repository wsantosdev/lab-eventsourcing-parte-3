using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Lab.EventSourcing.Core
{
    public abstract class EventSourcingModel<T> where T : EventSourcingModel<T>
    {
        private Queue<IEvent> _pendingEvents = new Queue<IEvent>();
        public IEnumerable<IEvent> PendingEvents { get => _pendingEvents.AsEnumerable(); }
        public Guid Id { get; protected set; }
        public int Version { get; protected set; } = 0;
        protected int NextVersion { get => Version + 1; }

        protected EventSourcingModel(IEnumerable<ModelEventBase> persistedEvents)
        {
            if (persistedEvents != null)
                ApplyPersistedEvents(persistedEvents);
        }

        public static T Load(IEnumerable<ModelEventBase> persistendEvents) =>
            (T)Activator.CreateInstance(typeof(T),
                                         BindingFlags.NonPublic | BindingFlags.Instance,
                                         null,
                                         new object[] { persistendEvents },
                                         CultureInfo.InvariantCulture);

        protected EventSourcingModel(T snapshot, IEnumerable<ModelEventBase> persistedEvents) 
        {
            if(snapshot != null)
                ApplySnapshot(snapshot);
            
            if(persistedEvents != null)
                ApplyPersistedEvents(persistedEvents);
        }

        public static T Load(T snapshot, IEnumerable<ModelEventBase> persistendEvents) =>
            (T) Activator.CreateInstance(typeof(T),
                                         BindingFlags.NonPublic | BindingFlags.Instance,
                                         null,
                                         new object [] { snapshot, persistendEvents }, 
                                         CultureInfo.InvariantCulture);

        protected abstract void ApplySnapshot(T snapshot);
        
        protected void ApplyPersistedEvents(IEnumerable<ModelEventBase> events)
        {
            foreach (var e in events)
            {
                Apply(e);
                Version = e.ModelVersion;
            }
        }

        protected void RaiseEvent<TEvent>(TEvent pendingEvent) where TEvent: ModelEventBase
        {
            _pendingEvents.Enqueue(pendingEvent);
            Apply(pendingEvent);
            Version = pendingEvent.ModelVersion;
        }

        protected abstract void Apply(IEvent pendingEvent);

        public void Commit() =>
            _pendingEvents.Clear();
    }
}