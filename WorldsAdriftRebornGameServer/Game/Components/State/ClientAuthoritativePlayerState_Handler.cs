using Bossa.Travellers.Player;
using Improbable.Corelibrary.Transforms;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components.Data;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Handlers
{
    [ComponentStateHandler]
    internal class ClientAuthoritativePlayerStateHandler : IComponentStateHandler<ClientAuthoritativePlayerState,
        ClientAuthoritativePlayerState.Update, ClientAuthoritativePlayerState.Data>
    {

        public override uint ComponentId => 1073;
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, ClientAuthoritativePlayerState.Update clientComponentUpdate, ClientAuthoritativePlayerState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            ClientAuthoritativePlayerState.Update serverComponentUpdate = (ClientAuthoritativePlayerState.Update)serverComponentData.ToUpdate();
            
            // TODO: Update TransformState, LocationState
            // var location = Utils.GetStateUpdate<LocationState, LocationState.Update, LocationState.Data>(player, entityId, 1242);
            // location.SetTimestamp(serverComponentData.Value.timestamp);
            //
            var transform = Utils.GetStateUpdate<TransformState, TransformState.Update, TransformState.Data>(player, entityId, 190602);
            transform.SetTimestamp(serverComponentData.Value.timestamp);
            
            //
            // if (serverComponentData.Value.relativeTo.IsValid())
            // {
            //     location.SetRelative(new RelativeLocation(serverComponentData.Value.relativeTo, serverComponentData.Value.positionRelative, serverComponentData.Value.rotationRelative));
            //     transform.SetLocalPosition(new FixedPointVector3(new Improbable.Collections.List<long> {(long) serverComponentData.Value.positionRelative.X, (long) serverComponentData.Value.positionRelative.Y, (long) serverComponentData.Value.positionRelative.Z}));
            //     transform.SetParent(new Option<Parent>(new Parent(serverComponentData.Value.relativeTo, "~")));
            // }
            // else
            // {
            //     transform.SetLocalPosition(new FixedPointVector3(new Improbable.Collections.List<long> {0, 0, 0}));
            //     transform.SetLocalRotation(new Quaternion32(0));
            //     transform.SetParent(null);
            // }

            // Console.WriteLine("Received relative position: " + serverComponentData.Value.positionRelative);

            // TODO: THESE TWO ARE THE WORKING-ISH LINES
            // var response = Positions.RelativePositionUpdate(entityId, serverComponentData.Value.relativeTo.Id, serverComponentData.Value.positionRelative);
            // Console.WriteLine("Calculated absolute position: " + response.Absolute);

            // transform.SetParent(new Parent(response.HasParent ? response.ParentId : new EntityId(-1), "~"));
            // transform.SetLocalPosition(response.Relative);

            // SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId, 1242, 190602 }, new System.Collections.Generic.List<object> { serverComponentUpdate, location, transform });
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId, 190602 }, new System.Collections.Generic.List<object> { serverComponentUpdate, transform });
            // SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
