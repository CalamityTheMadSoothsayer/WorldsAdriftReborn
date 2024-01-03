using Bossa.Travellers.Interact;
using Bossa.Travellers.Items;
using Improbable;
using Improbable.Math;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class InteractAgentStateHandler : IComponentStateHandler<InteractAgentState, InteractAgentState.Update, InteractAgentState.Data>
    {
        public override uint ComponentId => 1211;

        // public override InteractAgentState.Data Init( ENetPeerHandle player, long entityId )
        // {
        //     return new InteractAgentState.Data(new InteractAgentStateData(true,
        //         new EntityId(-1),
        //         new EntityId(0),
        //         new EntityId(0),
        //         new Improbable.Math.Vector3f(0f, 0f, 0f),
        //         new Improbable.Math.Coordinates(),
        //         ItemHelper.SALVAGE_REPAIR_TOOL,
        //         0));
        // }

        public void OnItemSlotChanged( ENetPeerHandle client, CoreEntity entity, int oldSlot, int newSlot)
        {
            var serverState = entity.Get<InteractAgentServerState>();
            var multitoolState = entity.Get<MultiToolPlayerState>();
            if (!serverState.HasValue || !multitoolState.HasValue)
            {
                return;
            }

            var serverStateUpdate = serverState.Value.ToUpdate().Get();
            var playerMultitoolUpdate =multitoolState.Value.ToUpdate().Get();

            switch (newSlot)
            {
                case ItemHelper.SALVAGE_REPAIR_TOOL:
                    Console.WriteLine("Player switched to salvage tool");
                    serverStateUpdate.SetMultitoolMode(MultitoolMode.Salvage);
                    serverStateUpdate.SetSelectedHotbarItem(ItemHelper.GetDefaultItems()[0]);
                    playerMultitoolUpdate.SetMode(MultitoolMode.Salvage);
                    playerMultitoolUpdate.SetIsVisible(true);
                    break;
                case ItemHelper.REPAIR_TOOL:
                    Console.WriteLine("Player switched to repair tool");
                    serverStateUpdate.SetMultitoolMode(MultitoolMode.Repair);
                    serverStateUpdate.SetSelectedHotbarItem(ItemHelper.GetDefaultItems()[1]);
                    playerMultitoolUpdate.SetMode(MultitoolMode.Repair);
                    playerMultitoolUpdate.SetIsVisible(true);
                    break;
                case ItemHelper.SHIP_PART_SCANNER_TOOL:
                    Console.WriteLine("Player switched buildtool");
                    serverStateUpdate.SetMultitoolMode(MultitoolMode.Default);
                    serverStateUpdate.SetSelectedHotbarItem(ItemHelper.GetDefaultItems()[2]);
                    playerMultitoolUpdate.SetMode(MultitoolMode.Default);
                    playerMultitoolUpdate.SetIsVisible(true);
                    break;
                case ItemHelper.SCANNER_TOOL:
                    Console.WriteLine("Player switched to scanner");
                    serverStateUpdate.SetMultitoolMode(MultitoolMode.Default);
                    serverStateUpdate.SetSelectedHotbarItem(ItemHelper.GetDefaultItems()[3]);
                    playerMultitoolUpdate.SetMode(MultitoolMode.Default);
                    playerMultitoolUpdate.SetIsVisible(true);
                    break;
                default:
                    Console.WriteLine("Player switched item");
                    // TODO: Make this work with non-gauntlet items
                    serverStateUpdate.SetMultitoolMode(MultitoolMode.Default);
                    serverStateUpdate.SetSelectedHotbarItem(null);
                    playerMultitoolUpdate.SetMode(MultitoolMode.Default);
                    playerMultitoolUpdate.SetIsVisible(false);
                    break;
            }
            
            entity.Update(serverStateUpdate);
            entity.Update(playerMultitoolUpdate);

            SendOPHelper.SendComponentUpdateOp(client, entity.Id, new System.Collections.Generic.List<uint> { 1212, 2105 }, new System.Collections.Generic.List<object> { serverStateUpdate, playerMultitoolUpdate });
        }

        public void OnUseItemKeyHeldUpdate(bool lastUseItemKeyHeld, bool nextUseItemKeyHeld)
        {
            Console.WriteLine("UseItemKeyHeld: " + lastUseItemKeyHeld + " -> " + nextUseItemKeyHeld);
        }

        public void OnLookingAtUpdate(EntityId lastLookingAt, EntityId nextLookingAt)
        {
            Console.WriteLine("LookingAt: " + lastLookingAt + " -> " + nextLookingAt);
        }

        public void OnLookingAtInteractiveUpdate(EntityId lastLookingAtInteractive, EntityId nextLookingAtInteractive)
        {
            Console.WriteLine("LookingAtInteractive: " + lastLookingAtInteractive + " -> " + nextLookingAtInteractive);
        }

        public void OnDebugLookingAtUpdate(EntityId lastDebugLookingAt, EntityId nextDebugLookingAt)
        {
            Console.WriteLine("DebugLookingAt: " + lastDebugLookingAt + " -> " + nextDebugLookingAt);
        }

        public void OnLookDirectionEulerUpdate(Improbable.Math.Vector3f lastLookDirectionEuler, Improbable.Math.Vector3f nextLookDirectionEuler)
        {
            Console.WriteLine("LookDirectionEuler: " + lastLookDirectionEuler + " -> " + nextLookDirectionEuler);
        }

        public void OnLookHitPointUpdate(Coordinates lastLookHitPoint, Coordinates nextLookHitPoint)
        {
            Console.WriteLine("LookHitPoint: " + lastLookHitPoint + " -> " + nextLookHitPoint);
        }

        public void OnSelectedHotbarUpdate(int lastSelectedHotbar, int nextSelectedHotbar)
        {
            Console.WriteLine("SelectedHotbar: " + lastSelectedHotbar + " -> " + nextSelectedHotbar);
        }

        public override void HandleUpdate( ENetPeerHandle player, long entityId,
            InteractAgentState.Update clientComponentUpdate, InteractAgentState.Data serverComponentData )
        {
            var entity = EntityManager.GlobalEntityRealm[entityId];
            
            if (clientComponentUpdate.itemSlot.HasValue &&
                clientComponentUpdate.itemSlot.Value != serverComponentData.Value.itemSlot)
            {
                OnItemSlotChanged(player, entity, serverComponentData.Value.itemSlot, clientComponentUpdate.itemSlot.Value);
            }

            if (clientComponentUpdate.useItemKeyHeld.HasValue && clientComponentUpdate.useItemKeyHeld.Value != serverComponentData.Value.useItemKeyHeld)
                OnUseItemKeyHeldUpdate(serverComponentData.Value.useItemKeyHeld, clientComponentUpdate.useItemKeyHeld.Value);

            if (clientComponentUpdate.lookingAt.HasValue && clientComponentUpdate.lookingAt.Value != serverComponentData.Value.lookingAt)
                OnLookingAtUpdate(serverComponentData.Value.lookingAt, clientComponentUpdate.lookingAt.Value);

            if (clientComponentUpdate.lookingAtInteractive.HasValue && clientComponentUpdate.lookingAtInteractive.Value != serverComponentData.Value.lookingAtInteractive)
                OnLookingAtInteractiveUpdate(serverComponentData.Value.lookingAtInteractive, clientComponentUpdate.lookingAtInteractive.Value);

            if (clientComponentUpdate.debugLookingAt.HasValue && clientComponentUpdate.debugLookingAt.Value != serverComponentData.Value.debugLookingAt)
                OnDebugLookingAtUpdate(serverComponentData.Value.debugLookingAt, clientComponentUpdate.debugLookingAt.Value);

            if (clientComponentUpdate.lookDirectionEuler.HasValue && clientComponentUpdate.lookDirectionEuler.Value != serverComponentData.Value.lookDirectionEuler)
                OnLookDirectionEulerUpdate(serverComponentData.Value.lookDirectionEuler, clientComponentUpdate.lookDirectionEuler.Value);

            if (clientComponentUpdate.lookHitPoint.HasValue && clientComponentUpdate.lookHitPoint.Value != serverComponentData.Value.lookHitPoint)
                OnLookHitPointUpdate(serverComponentData.Value.lookHitPoint, clientComponentUpdate.lookHitPoint.Value);

            if (clientComponentUpdate.selectedHotbar.HasValue && clientComponentUpdate.selectedHotbar.Value != serverComponentData.Value.selectedHotbar)
                OnSelectedHotbarUpdate(serverComponentData.Value.selectedHotbar, clientComponentUpdate.selectedHotbar.Value);

            // clientComponentUpdate.AddChangeMode(new ChangeMode(clientComponentUpdate.itemSlot.Value));
            for (int j = 0; j < clientComponentUpdate.changeMode.Count; j++)           
            {
                Console.WriteLine($"[info] mode changed; new mode: {clientComponentUpdate.changeMode[j].itemSlot}");
            }
            
            for (int j = 0; j < clientComponentUpdate.useItemKeyPressed.Count; j++)
            {
                Console.WriteLine($"[info] game use item; slot: {clientComponentUpdate.useItemKeyPressed[j].itemSlot}; position: {clientComponentUpdate.useItemKeyPressed[j].sourcePosition.X}, {clientComponentUpdate.useItemKeyPressed[j].sourcePosition.Z}");
            }

            for (int j = 0; j < clientComponentUpdate.releaseInteraction.Count; j++)
            {
                Console.WriteLine($"[info] game release interact; entity: {clientComponentUpdate.releaseInteraction[j].interactEntityId.Id}");
            }

            for (int j = 0; j < clientComponentUpdate.interactWithObject.Count; j++)
            {
                Console.WriteLine($"[info] game interact with object; entity: {clientComponentUpdate.interactWithObject[j].target.Id}; verb: {clientComponentUpdate.interactWithObject[j].verb}");
            }

            for (int j = 0; j < clientComponentUpdate.useItemKeyReleased.Count; j++)
            {
                Console.WriteLine($"[info] key released; time: {clientComponentUpdate.useItemKeyReleased[j].timeButtonHeld}");
            }
            
            entity.Update(clientComponentUpdate);

            // SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { entity.Get<InteractAgentState>().Value.ToUpdate().Get() });
        }

        // TODO: Make everything that gets sent through `SendComponentUpdateOp` automagically update the GameState triple dict
        
    }
}
