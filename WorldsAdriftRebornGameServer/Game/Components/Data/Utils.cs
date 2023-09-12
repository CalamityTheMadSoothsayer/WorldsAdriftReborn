using Improbable.Worker;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;

namespace WorldsAdriftRebornGameServer.Game.Components.Data
{
    internal static class Utils
    {
        internal static U GetStateData<S, U>(ENetPeerHandle player, long entityId, uint componentId) where S : IComponentMetaclass where U : IComponentData<S>
        {
            return (U)ClientObjects.Instance.Dereference(GameState.Instance.ComponentMap[player][entityId][componentId]);
        }
        internal static Data GetStateUpdate<Origin, Data, Update>(ENetPeerHandle player, long entityId, uint componentId) where Origin : IComponentMetaclass where Update : IComponentData<Origin> where Data : IComponentUpdate<Origin> 
        {
            return (Data)((Update)ClientObjects.Instance.Dereference(GameState.Instance.ComponentMap[player][entityId][componentId])).ToUpdate();
        }
    }
}
