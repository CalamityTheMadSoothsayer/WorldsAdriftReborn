using Bossa.Travellers.Items;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.State
{
    [ComponentStateHandler]
    internal class MultiToolPlayerStateHandler : IComponentStateHandler<MultiToolPlayerState,
        MultiToolPlayerState.Update, MultiToolPlayerState.Data>
    {

        public override uint ComponentId => 2105;
        
        public void OnEngagedUpdate(bool wasOn, bool isOn)
        {
            Console.WriteLine("IsOn: " + wasOn + " -> " + isOn);
        }

        public void OnModeUpdate(MultitoolMode oldMode, MultitoolMode newMode)
        {
            Console.WriteLine("Mode: " + oldMode + " -> " + newMode);
        }

        public void OnVisibilityUpdate(bool wasVisible, bool isVisible)
        {
            Console.WriteLine("IsVisible: " + wasVisible + " -> " + isVisible);
        }

        public void OnSalvagerBlastDamageUpdate(int oldDamage, int newDamage)
        {
            Console.WriteLine("SalvagerBlastDamage: " + oldDamage + " -> " + newDamage);
        }

        public override void HandleUpdate(ENetPeerHandle player, long entityId, MultiToolPlayerState.Update clientComponentUpdate, MultiToolPlayerState.Data serverComponentData)
        {
            if (clientComponentUpdate.mode.HasValue && !clientComponentUpdate.mode.Value.Equals(serverComponentData.Value.mode))
                OnModeUpdate(serverComponentData.Value.mode, clientComponentUpdate.mode.Value);

            if (clientComponentUpdate.isVisible.HasValue && !clientComponentUpdate.isVisible.Value.Equals(serverComponentData.Value.isVisible))
                OnVisibilityUpdate(serverComponentData.Value.isVisible, clientComponentUpdate.isVisible.Value);

            if (clientComponentUpdate.salvagerBlastDamage.HasValue && !clientComponentUpdate.salvagerBlastDamage.Value.Equals(serverComponentData.Value.salvagerBlastDamage))
                OnSalvagerBlastDamageUpdate(serverComponentData.Value.salvagerBlastDamage, clientComponentUpdate.salvagerBlastDamage.Value);

            for (var i = 0; i < clientComponentUpdate.shotEntityEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests shotEntity");
                Console.WriteLine(clientComponentUpdate.shotEntityEvent[i].entityId);
                Console.WriteLine(clientComponentUpdate.shotEntityEvent[i].offset);
            }
            for (var i = 0; i < clientComponentUpdate.shotWorldEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests shotWorld");
                Console.WriteLine(clientComponentUpdate.shotWorldEvent[i].shotCoordinates);
            }
            for (var i = 0; i < clientComponentUpdate.toggleModeEvent.Count; i++)
            {
                Console.WriteLine("INFO - Game requests toggleMode");
            }
            
            clientComponentUpdate.ApplyTo(serverComponentData);
            MultiToolPlayerState.Update serverComponentUpdate = (MultiToolPlayerState.Update)serverComponentData.ToUpdate();

            
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
