using Lab.EventSourcing.Core;
using System;
using Xunit;

namespace Lab.EventSourcing.Inventory.Test
{
    public class EventStoreCase
    {
        [Fact]
        public void Should_Persist_Events()
        {
            var eventStore = EventStore.Create();
            Inventory inventory = Inventory.Create();
            
            eventStore.Commit(inventory);
            var storedInventory = Inventory.Load(eventStore.GetById(inventory.Id));

            Assert.Empty(inventory.PendingEvents);
            Assert.Equal(inventory.Id, storedInventory.Id);
        }

        [Fact]
        public void Should_Load_All_Events()
        {
            var eventStore = EventStore.Create();
            Inventory inventory = Inventory.Create();
            var productId = Guid.NewGuid();
            var productQuantity = 10;
            inventory.AddProduct(productId, productQuantity);

            eventStore.Commit(inventory);
            var storedInventory = Inventory.Load(eventStore.GetById(inventory.Id));

            Assert.Equal(inventory.Id, storedInventory.Id);
            Assert.Equal(productQuantity, storedInventory.GetProductCount(productId));
        }

        [Fact]
        public void Should_Load_Created_Only()
        {
            var eventStore = EventStore.Create();
            Inventory inventory = Inventory.Create();
            var productId = Guid.NewGuid();
            inventory.AddProduct(productId, 10);

            eventStore.Commit(inventory);
            var storedInventory = Inventory.Load(eventStore.GetByVersion(inventory.Id, 1));

            Assert.Equal(inventory.Id, storedInventory.Id);
            Assert.Equal(0, storedInventory.GetProductCount(productId));
        }
    }
}
