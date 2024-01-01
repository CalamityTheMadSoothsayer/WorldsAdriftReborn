using Bossa.Travellers.Inventory;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Game.Items;
using Imp = Improbable.Collections;

namespace WorldsAdriftRebornGameServer.Game.Components.Data
{
    // TODO: Use ProtoContract and serialize to persistent file system
    public class Inventory
    {
        public InventoryState.Data Data;
        public ScalaSlottedInventoryItem?[,] ItemGrid;
        private Dictionary<int, (int, int)> ItemIdToIndices;

        public InventoryState.Update Update => (InventoryState.Update)Data.ToUpdate();

        public Inventory(InventoryState.Data data)
        {
            Data = data;

            // TODO: Allow different sizes
            ItemGrid = new ScalaSlottedInventoryItem?[10, 19];
            ItemIdToIndices = new Dictionary<int, (int, int)>();

            // Populate the array and dictionary
            foreach (var item in Data.Value.inventoryList)
            {
                if (item.xPosition < 0 || item.yPosition < 0) continue;
                ItemGrid[item.xPosition, item.yPosition] = item;
                ItemIdToIndices[item.itemId] = (item.xPosition, item.yPosition);
            }
        }

        public bool GetItem( Func<ScalaSlottedInventoryItem, bool> predicate, out ScalaSlottedInventoryItem item )
        {
            try
            {
                item = Data.Value.inventoryList.First(predicate);
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }

            return true;
        }

        public bool GetItem(int itemId, out ScalaSlottedInventoryItem item, bool notInGrid = false)
        {
            if (notInGrid)
            {
                return GetItem(i => i.itemId == itemId, out item);
            }

            if (ItemIdToIndices.TryGetValue(itemId, out var indices))
            {
                item = ItemGrid[indices.Item1, indices.Item2]!.Value;
                return true;
            }
            item = default;
            return false;
        }

        public bool UpdateItem(ScalaSlottedInventoryItem item)
        {
            if (!ItemIdToIndices.TryGetValue(item.itemId, out var indices))
            {
                return false; 
            }

            if (item is { xPosition: >= 0, yPosition: >= 0 })
            {
                var (width, height) = ItemHelper.GetDimensions(item.itemTypeId);

                for (int x = indices.Item1; x < indices.Item1 + (item.rotated ? height : width); x++)
                {
                    for (int y = indices.Item2; y < indices.Item2 + (item.rotated ? width : height); y++)
                    {
                        if (ItemGrid[x, y].HasValue && ItemGrid[x, y]!.Value.itemId != item.itemId)
                        {
                            return false;
                        }
                    }
                }

                // No overlapping items, insert the item
                for (int x = indices.Item1; x < indices.Item1 + (item.rotated ? height : width); x++)
                {
                    for (int y = indices.Item2; y < indices.Item2 + (item.rotated ? width : height); y++)
                    {
                        ItemGrid[x, y] = item;
                    }
                }
            }
            
            Data.Value.inventoryList = new Imp.List<ScalaSlottedInventoryItem>(ItemGrid.Cast<ScalaSlottedInventoryItem?>().Where(i => i.HasValue).Select(i => i!.Value).ToList()); // TODO: FIX PERF
            return true;
        }

        public bool RemoveItem(int itemId)
        {
            if (!ItemIdToIndices.TryGetValue(itemId, out var indices))
            {
                return false; 
            }

            var itemToRemove = ItemGrid[indices.Item1, indices.Item2]!.Value;
            var (width, height) = ItemHelper.GetDimensions(itemToRemove.itemTypeId);

            for (int x = indices.Item1; x < indices.Item1 + width; x++)
            {
                for (int y = indices.Item2; y < indices.Item2 + height; y++)
                {
                    ItemGrid[x, y] = null;
                }
            }

            ItemIdToIndices.Remove(itemId);
            Data.Value.inventoryList = new Imp.List<ScalaSlottedInventoryItem>(ItemGrid.Cast<ScalaSlottedInventoryItem?>().Where(i => i.HasValue).Select(i => i!.Value).ToList());
            return true;
        }
        
        public Tuple<int, int>? FindUnassignedPosition(ScalaSlottedInventoryItem item)
        {
            var (width, height) = ItemHelper.GetDimensions(item.itemTypeId);

            for (int x = 0; x <= ItemGrid.GetLength(0) - width; x++)
            {
                for (int y = 0; y <= ItemGrid.GetLength(1) - height; y++)
                {
                    bool unassignedPosition = true;

                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            if (ItemGrid[x + i, y + j] == null) continue;

                            unassignedPosition = false;
                            break;
                        }

                        if (!unassignedPosition)
                        {
                            break;
                        }
                    }

                    if (unassignedPosition)
                    {
                        return new Tuple<int, int>(x, y);
                    }
                }
            }

            return null;
        }


    }

    internal static class InventoryManager
    {
        internal static Inventory GetPlayerInventory(ENetPeerHandle player, long entityId)
        {
            return new Inventory(EntityManager.GlobalEntityRealm[entityId].Get<InventoryState>().Value.Get());
        }
    }

}
