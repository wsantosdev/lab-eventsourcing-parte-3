using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Lab.EventSourcing.Core
{
    public class SnapshotStore
    {
        private const int SnapshotThreshold = 2;
        private readonly JsonSerializerSettings _jsonSerializerSettings = 
            new JsonSerializerSettings { ContractResolver = new CustomContractResolver() };

        private readonly SnapshotDbContext _dbContext;

        public static SnapshotStore Create() =>
            new SnapshotStore();

        private SnapshotStore() =>
            _dbContext = new SnapshotDbContext(new DbContextOptionsBuilder<SnapshotDbContext>()
                                                                .UseInMemoryDatabase(databaseName: "SnapshotStore")
                                                                .EnableSensitiveDataLogging()
                                                                .Options);

        public bool ShouldTakeSnapshot(int modelVersion) =>
            modelVersion % SnapshotThreshold == 0;

        public void Save<TModel>(TModel model) where TModel : EventSourcingModel<TModel>
        {
            _dbContext.Snapshots.Add(Snapshot.Create(model.Id,
                                                     model.Version,
                                                     model.GetType().AssemblyQualifiedName,
                                                     JsonConvert.SerializeObject(model, _jsonSerializerSettings)));

            _dbContext.SaveChanges();
        }

        public TModel GetById<TModel>(Guid id) where TModel : EventSourcingModel<TModel>
        {
            return  _dbContext.Snapshots.Where(s => s.ModelId == id)
                                        .OrderByDescending(s => s.ModelVersion)
                                        .Take(1)
                                        .Select(s => JsonConvert.DeserializeObject(s.Data, Type.GetType(s.Type), _jsonSerializerSettings))
                                        .Cast<TModel>()
                                        .FirstOrDefault();
        }

        private class SnapshotDbContext : DbContext
        {
            public SnapshotDbContext(DbContextOptions<SnapshotDbContext> options) : base(options) { }
            public DbSet<Snapshot> Snapshots { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder) =>
                modelBuilder.Entity<Snapshot>().HasKey(k => new { k.ModelId, k.ModelVersion });
        }
    }
}
