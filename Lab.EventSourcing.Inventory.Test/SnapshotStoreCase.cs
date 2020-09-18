using Lab.EventSourcing.Core;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using Xunit;

namespace Lab.EventSourcing.Inventory.Test
{
    public class SnapshotStoreCase
    {
        [Fact]
        public void Should_Take_Snapshot()
        {
            //Arrange
            var eventStore = EventStore.Create();
            var snapshotStore = SnapshotStore.Create();

            Inventory inventory = Inventory.Create();
            var productId = Guid.NewGuid();
            inventory.AddProduct(productId, 10);
            
            //Act
            if (snapshotStore.ShouldTakeSnapshot(inventory.Version))
                snapshotStore.Save(inventory);

            productId = Guid.NewGuid();
            inventory.AddProduct(productId, 20);
            productId = Guid.NewGuid();
            inventory.AddProduct(productId, 30);
            
            eventStore.Commit(inventory);

            var snapshot = snapshotStore.GetById<Inventory>(inventory.Id);
            var persistedEvents = eventStore.GetFromVersion(snapshot.Id, snapshot.Version);
            var newInventory = Inventory.Load(snapshot, persistedEvents);

            //Assert
            Assert.Equal(inventory.Id, snapshot.Id);
            Assert.Equal(2, snapshot.Version);
            Assert.Equal(inventory.Version, newInventory.Version);
            Assert.Equal(30, newInventory.GetProductCount(productId));
        }
    }
}
