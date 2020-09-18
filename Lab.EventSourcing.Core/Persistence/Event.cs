using System;

namespace Lab.EventSourcing.Core
{
    public class Event
    {
        public Guid ModelId { get; private set; }
        public int ModelVersion { get; private set; }
        public DateTime When { get; private set; }
        public string Type { get; private set; }
        public string Data { get; private set; }

        public static Event Create(Guid id, int version, string type, DateTime when, string data) =>
            new Event
            {
                ModelId = id,
                ModelVersion = version,
                Type = type,
                When = when,
                Data = data
            };
    }
}
