using System;

namespace Lab.EventSourcing.Core
{
    public class Snapshot
    {
        public Guid ModelId { get; private set; }
        public int ModelVersion { get; private set; }
        public string Type { get; private set; }
        public string Data { get; private set; }

        public static Snapshot Create(Guid id, int version, string type, string data) =>
            new Snapshot
            {
                ModelId = id,
                ModelVersion = version,
                Type = type,
                Data = data
            };
    }
}
