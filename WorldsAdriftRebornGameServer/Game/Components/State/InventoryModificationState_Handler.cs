using Bossa.Travellers.Inventory;
using Bossa.Travellers.Player;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components.Data;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Handlers
{
    [ComponentStateHandler]
    internal class InventoryModificationStateHandler : IComponentStateHandler<InventoryModificationState,
        InventoryModificationState.Update, InventoryModificationState.Data>
    {
        public override uint ComponentId => 1082;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, InventoryModificationState.Update clientComponentUpdate, InventoryModificationState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            var inventory = InventoryManager.GetPlayerInventory(player, entityId);

            InventoryModificationState.Update serverComponentUpdate = (InventoryModificationState.Update)serverComponentData.ToUpdate();

            for (int j = 0; j < clientComponentUpdate.equipWearable.Count; j++)
            {
                Console.WriteLine("[info] game wants to equip a wearable");
                Console.WriteLine("[info] id: " + clientComponentUpdate.equipWearable[j].itemId);
                Console.WriteLine("[info] slot: " + clientComponentUpdate.equipWearable[j].slotId);
                Console.WriteLine("[info] lockbox: " + clientComponentUpdate.equipWearable[j].isLockboxItem);

                // send updates to equip the wearables
                var storedWearableUtilsState = Utils.GetStateUpdate<WearableUtilsState, WearableUtilsState.Update, WearableUtilsState.Data>(player, entityId, 1280);
                // TODO: Support "customisations" from the player properties data object, which also means supporting lockbox
                var storedPlayerPropertiesState = Utils.GetStateUpdate<PlayerPropertiesState, PlayerPropertiesState.Update, PlayerPropertiesState.Data>(player, entityId, 1088);

                storedWearableUtilsState.SetItemIds(new Improbable.Collections.List<int> { clientComponentUpdate.equipWearable[j].itemId }).SetHealths(new Improbable.Collections.List<float> { 100f }).SetActive(new Improbable.Collections.List<bool> { true });
                if (!inventory.GetItem(clientComponentUpdate.equipWearable[j].itemId, out var targetItem)) continue;  // TODO: MAYBE DONT SILENT ERROR

                targetItem.slotType = ItemHelper.GetItem(targetItem.itemTypeId).characterSlot;
                
                inventory.UpdateItem(targetItem);

                // NOTE: its absolutely crucial to send 1081 before 1088, this is because 1081 sets the item slotType to something meaningfull while 1088 expects some meaningful value if it should be equipped
                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1280, 1081, 1088 }, new List<object> { storedWearableUtilsState, inventory.Update, storedPlayerPropertiesState });
            }
            for (int j = 0; j < clientComponentUpdate.equipTool.Count; j++)
            {
                Console.WriteLine("[info] game wants to equip a tool");
                Console.WriteLine("[info] id: " + clientComponentUpdate.equipTool[j].itemId);
                
                var storedPlayerPropertiesState = Utils.GetStateUpdate<PlayerPropertiesState, PlayerPropertiesState.Update, PlayerPropertiesState.Data>(player, entityId, 1088);
                if (!inventory.GetItem(clientComponentUpdate.equipTool[j].itemId, out var targetItem)) continue; 

                // TODO: IMPORTANT INFO ! THE GAME DOES NOT PROVIDE ANY FUNCTIONALITY FOR UNEQUIPPING A TOOL. THIS IS BECAUSE THIS EVENT IS EXCLUSIVELY USED FOR THE SCANNER. THE SCANNER IS AN ITEM YOU HAVE TO CRAFT AND THEN EQUIP IN VANILLA.
                
                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1081, 1088 }, new List<object> { inventory.Update, storedPlayerPropertiesState });
            }
            for (int j = 0; j < clientComponentUpdate.craftItem.Count; j++)
            {
                Console.WriteLine("[info] game wants to craft an item");
                Console.WriteLine("[info] inventoryEntityId: " + clientComponentUpdate.craftItem[j].inventoryEntityId);
                Console.WriteLine("[info] itemTypeId: " + clientComponentUpdate.craftItem[j].itemTypeId);
                Console.WriteLine("[info] amount: " + clientComponentUpdate.craftItem[j].amount);
            }
            for (int j = 0; j < clientComponentUpdate.crossInventoryMoveItem.Count; j++)
            {
                Console.WriteLine("[info] game wants to cross inventory move item");
                Console.WriteLine("[info] srcItemId: " + clientComponentUpdate.crossInventoryMoveItem[j].srcItemId);
                Console.WriteLine("[info] xPos: " + clientComponentUpdate.crossInventoryMoveItem[j].xPos);
                Console.WriteLine("[info] yPos: " + clientComponentUpdate.crossInventoryMoveItem[j].yPos);
                Console.WriteLine("[info] rotate: " + clientComponentUpdate.crossInventoryMoveItem[j].rotate);
                Console.WriteLine("[info] srcInventoryEntityId: " + clientComponentUpdate.crossInventoryMoveItem[j].srcInventoryEntityId);
                Console.WriteLine("[info] destInventoryItemId: " + clientComponentUpdate.crossInventoryMoveItem[j].destInventoryEntityId);
                Console.WriteLine("[info] isLockBoxItem: " + clientComponentUpdate.crossInventoryMoveItem[j].isLockboxItem);
            }
            for (int j = 0; j < clientComponentUpdate.moveItem.Count; j++)
            {
                var moveItem = clientComponentUpdate.moveItem[j];
                Console.WriteLine("[info] game wants to move an inventory item");
                Console.WriteLine("[info] inventoryEntityId: " + moveItem.inventoryEntityId);
                Console.WriteLine("[info] itemId: " + moveItem.itemId);
                Console.WriteLine("[info] xPos: " + moveItem.xPos);
                Console.WriteLine("[info] yPos: " + moveItem.yPos);
                Console.WriteLine("[info] rotate: " + moveItem.rotate);
                Console.WriteLine("[info] isLockboxItem: " + clientComponentUpdate.moveItem[j].isLockboxItem);
                
                if (!inventory.GetItem(moveItem.itemId, out var targetItem)) continue;  // TODO: MAYBE DONT SILENT ERROR

                if (moveItem.yPos < 0 || moveItem.xPos < 0)  // 
                {
                    inventory.RemoveItem(moveItem.itemId);
                }
                else
                {
                    targetItem.rotated = moveItem.rotate;  
                    targetItem.xPosition = moveItem.xPos;
                    targetItem.yPosition = moveItem.yPos;
                    if (!inventory.UpdateItem(targetItem))
                    {
                        Console.WriteLine("UpdateItem failed");
                    }
                }
                
                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1081 }, new List<object> { inventory.Update });
            }
            for (int j = 0; j < clientComponentUpdate.removeFromHotBar.Count; j++)
            {
                Console.WriteLine("[info] game wants to remove from hotbar");
                Console.WriteLine("[info] slotIndex: " + clientComponentUpdate.removeFromHotBar[j].slotIndex);
                Console.WriteLine("[info] isLockboxItem: " + clientComponentUpdate.removeFromHotBar[j].isLockboxItem);

                if (!inventory.GetItem(item => item.hotBarSlotNum == clientComponentUpdate.removeFromHotBar[j].slotIndex, out var targetItem)) continue;

                var newPos = inventory.FindUnassignedPosition(targetItem);
                if (newPos != null)
                {
                    targetItem.xPosition = newPos.Item1;
                    targetItem.yPosition = newPos.Item2;
                }
                else
                {
                    targetItem.xPosition = 0;
                    targetItem.yPosition = 0;
                    Console.WriteLine("Issue finding unassigned position");
                }
                targetItem.hotBarSlotNum = -1;
                targetItem.rotated = false;
                
                if (!inventory.UpdateItem(targetItem))
                {
                    Console.WriteLine("UpdateItem failed");
                }
                
                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1081 }, new List<object> { inventory.Update });
            }
            for (int j = 0; j < clientComponentUpdate.assignToHotBar.Count; j++)
            {
                Console.WriteLine("[info] game wants to assign to hotbar");
                Console.WriteLine("[info] itemId: " + clientComponentUpdate.assignToHotBar[j].itemId);
                Console.WriteLine("[info] slotIndex: " + clientComponentUpdate.assignToHotBar[j].slotIndex);
                Console.WriteLine("[info] isLockboxItem: " + clientComponentUpdate.assignToHotBar[j].isLockboxItem);
                
                if (!inventory.GetItem(clientComponentUpdate.assignToHotBar[j].itemId, out var targetItem, true)) continue;

                targetItem.hotBarSlotNum = clientComponentUpdate.assignToHotBar[j].slotIndex;
                
                if (!inventory.UpdateItem(targetItem))
                {
                    Console.WriteLine("UpdateItem failed");
                }
                
                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1081 }, new List<object> { inventory.Update });
            }

            SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { ComponentId }, new List<object> { serverComponentUpdate });
        }
    }
}
