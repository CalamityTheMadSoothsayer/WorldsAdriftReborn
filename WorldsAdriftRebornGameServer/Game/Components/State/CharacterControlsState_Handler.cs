using Bossa.Travellers.Controls;
using Bossa.Travellers.Player;
using Improbable.Collections;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class CharacterControlsDataHandler : IComponentStateHandler<CharacterControlsData,
        CharacterControlsData.Update, CharacterControlsData.Data>
    {

        public override uint ComponentId => 1072;
        
        public override void HandleUpdate(ENetPeerHandle player, long entityId, CharacterControlsData.Update clientComponentUpdate, CharacterControlsData.Data serverComponentData)
        {
            if (clientComponentUpdate.chatKey.HasValue)
                Console.WriteLine("ChatKey: " + clientComponentUpdate.chatKey.Value);
            if (clientComponentUpdate.lookRotation.HasValue)
                Console.WriteLine("LookRotation: " + clientComponentUpdate.lookRotation.Value);
            if (clientComponentUpdate.escapeKey.HasValue)
                Console.WriteLine("EscapeKey: " + clientComponentUpdate.escapeKey.Value);
            if (clientComponentUpdate.sitKey.HasValue)
                Console.WriteLine("SitKey: " + clientComponentUpdate.sitKey.Value);
            if (clientComponentUpdate.uiFocusKey.HasValue)
                Console.WriteLine("UIFocusKey: " + clientComponentUpdate.uiFocusKey.Value);

            foreach (var r in clientComponentUpdate.respawn)
            {
                Console.WriteLine($"Requested Respawn");
                var entity = EntityManager.GlobalEntityRealm[entityId];
                var respawnState = entity.Get<RespawnState>().Value;
                var data = respawnState.Get().Value;
                var update = respawnState.ToUpdate().Get();
                update.SetDeathCount(data.deathCount + 1);
                update.AddShowDeathScreen(new ShowDeathScreen());
                update.SetWaitingRespawn(true);
                update.SetWaitingForRespawnDecision(true);
                update.SetWaitingRespawnParent(Island.IslandSpawners[0].FirstRespawner);
                
                entity.Update(update);
                SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { 1092 }, new System.Collections.Generic.List<object> { update });
            }
            foreach (var s in clientComponentUpdate.switchControl)
            {
                Console.WriteLine($"Requested SwitchControl");
            }
            foreach (var s in clientComponentUpdate.sitKeyPressed)
            {
                Console.WriteLine($"Requested Sit");
            }
            
            clientComponentUpdate.ApplyTo(serverComponentData);
            CharacterControlsData.Update serverComponentUpdate = (CharacterControlsData.Update)serverComponentData.ToUpdate();

            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
