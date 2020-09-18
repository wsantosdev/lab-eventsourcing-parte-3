using Lab.EventSourcing.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lab.EventSourcing.Inventory
{
    public class Inventory : EventSourcingModel<Inventory>
    {
        private ConcurrentDictionary<Guid, int> _stock = new ConcurrentDictionary<Guid, int>();

        protected Inventory(Inventory snapshot, IEnumerable<ModelEventBase> persistedEvents)
            : base(snapshot, persistedEvents) { }

        protected Inventory(IEnumerable<ModelEventBase> persistedEvents) : base(persistedEvents) { }

        protected Inventory() : base(null, null) { }

        public static Inventory Create()
        {
            var inventory = new Inventory();
            inventory.RaiseEvent(new InventoryCreated(Guid.NewGuid()));

            return inventory;
        }

        public void AddProduct(Guid id, int quantity)
        {
            if (quantity == 0)
                throw new InvalidOperationException("The quantity must be greater than zero.");
            
            RaiseEvent(new ProductAdded(Id, NextVersion, id, quantity));
        }

        public void RemoveProduct(Guid id, int quantity)
        {
            if (!_stock.ContainsKey(id))
                throw new InvalidOperationException("Product not found.");

            if (_stock[id] < quantity)
                throw new InvalidOperationException($"The requested quantity is unavailable. Current quantity: {_stock[id]}.");
                
            RaiseEvent(new ProductRemoved(Id, NextVersion, id, quantity));
        }

        public int GetProductCount(Guid productId)
        {
            return _stock.TryGetValue(productId, out int quantity) 
                ? quantity
                : 0;
        }

        protected override void ApplySnapshot(Inventory snapshot) =>
            (Id, Version, _stock) = (snapshot.Id, snapshot.Version, snapshot._stock);

        protected override void Apply(IEvent pendingEvent)
        {
            switch(pendingEvent)
            {
                case InventoryCreated created:
                    Apply(created);
                    break;
                case ProductAdded added:
                    Apply(added);
                    break;
                case ProductRemoved removed:
                    Apply(removed);
                    break;
                default:
                    throw new ArgumentException($"Invalid event type: {pendingEvent.GetType()}.");
            }
        }

        protected void Apply(InventoryCreated pending) =>
            Id = pending.ModelId;

        protected void Apply(ProductAdded pending) =>
            _stock.AddOrUpdate(pending.ProductId, pending.Quantity,
                               (productId, currentQuantity) => currentQuantity += pending.Quantity);

        protected void Apply(ProductRemoved pending) =>
            _stock[pending.ProductId] -= pending.Quantity;
    }
}
