using System.Runtime.InteropServices;
using Bossa.Travellers.Interact;
using Improbable;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Singleton;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Handlers
{
    [ComponentStateHandler]
    internal class InteractAgentStateHandler : IComponentStateHandler<InteractAgentState, InteractAgentState.Update, InteractAgentState.Data>
    {
        public override uint ComponentId => 1211;

        // public override InteractAgentState.Data Fetch( ENetPeerHandle player, long entityId )
        // {
        //     InteractAgentState.Data iaData = new InteractAgentState.Data(new InteractAgentStateData(true,
        //         new EntityId(0),
        //         new EntityId(0),
        //         new EntityId(0),
        //         new Improbable.Math.Vector3f(0f, 0f, 0f),
        //         new Improbable.Math.Coordinates(),
        //         ItemHelper.SALVAGE_REPAIR_TOOL, 0));
        //
        // }

        public override void HandleUpdate( ENetPeerHandle player, long entityId,
            InteractAgentState.Update clientComponentUpdate, InteractAgentState.Data serverComponentData )
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            InteractAgentState.Update serverComponentUpdate = (InteractAgentState.Update)serverComponentData.ToUpdate();

            // TODO: Update PlayerMultitoolVisualizer states depending on the itemSlot


            for (int j = 0; j < serverComponentUpdate.changeMode.Count; j++)           
            {
                Console.WriteLine("[info] mode changed");
                Console.WriteLine("[info] new mode: " + serverComponentUpdate.changeMode[j].itemSlot);
            }

            for (int j = 0; j < serverComponentUpdate.useItemKeyPressed.Count; j++)
            {
                Console.WriteLine("[info] game use item");
                Console.WriteLine("[info] slot: " + serverComponentUpdate.useItemKeyPressed[j].itemSlot);
                Console.WriteLine("[info] slot: " + serverComponentUpdate.useItemKeyPressed[j].sourcePosition.X + " " + clientComponentUpdate.useItemKeyPressed[j].sourcePosition.Z);
            }

            for (int j = 0; j < serverComponentUpdate.releaseInteraction.Count; j++)
            {
                Console.WriteLine("[info] game release interact");
                Console.WriteLine("[info] entity: " + serverComponentUpdate.releaseInteraction[j].interactEntityId.Id);
            }

            for (int j = 0; j < serverComponentUpdate.interactWithObject.Count; j++)
            {
                Console.WriteLine("[info] game interact with object");
                Console.WriteLine("[info] entity: " + serverComponentUpdate.interactWithObject[j].target.Id);
                Console.WriteLine("[info] verb: " + serverComponentUpdate.interactWithObject[j].verb);
            }

            for (int j = 0; j < serverComponentUpdate.useItemKeyReleased.Count; j++)
            {
                Console.WriteLine("[info] key released");
                Console.WriteLine("[info] time: " + serverComponentUpdate.useItemKeyReleased[j].timeButtonHeld);
            }

            GameState.Instance.ComponentMap[player][entityId][1211] = StoreInteract(serverComponentData);
            SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { ComponentId }, new List<object> { serverComponentUpdate });
        }

        // TODO: Make everything that gets sent through `SendComponentUpdateOp` automagically update the GameState triple dict
        private static unsafe ulong StoreInteract(InteractAgentState.Data data)
        {
            var refId = ClientObjects.Instance.CreateReference(data);
            ComponentProtocol.ClientObject wrapper = new ComponentProtocol.ClientObject();
            wrapper.Reference = refId;

            uint length = 0;
            byte* buffer;
            ComponentProtocol.ClientSerialize serialize = Marshal.GetDelegateForFunctionPointer<ComponentProtocol.ClientSerialize>(ComponentsManager.Instance.ClientComponentVtables[1211].Serialize);
            serialize(1211, 2, &wrapper, &buffer, &length);

            return refId;
        }
    }
}
