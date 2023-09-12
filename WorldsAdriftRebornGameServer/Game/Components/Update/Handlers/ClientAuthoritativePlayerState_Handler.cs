using Bossa.Travellers.Motion;
using Bossa.Travellers.Player;
using Improbable;
using Improbable.Collections;
using Improbable.Corelibrary.Math;
using Improbable.Corelibrary.Transforms;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Components.Data;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Update.Handlers
{
    [RegisterComponentUpdateHandler]
    internal class ClientAuthoritativePlayerState_Handler : IComponentUpdateHandler<ClientAuthoritativePlayerState,
        ClientAuthoritativePlayerState.Update, ClientAuthoritativePlayerState.Data>
    {

        public ClientAuthoritativePlayerState_Handler() { Init(1073); }
        protected override void Init( uint ComponentId )
        {
            this.ComponentId = ComponentId;
        }
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, ClientAuthoritativePlayerState.Update clientComponentUpdate, ClientAuthoritativePlayerState.Data serverComponentData)
        {
            clientComponentUpdate.ApplyTo(serverComponentData);
            ClientAuthoritativePlayerState.Update serverComponentUpdate = (ClientAuthoritativePlayerState.Update)serverComponentData.ToUpdate();
            
            // TODO: Update TransformState, LocationState
            // var location = Utils.GetStateUpdate<LocationState, LocationState.Update, LocationState.Data>(player, entityId, 1242);
            // location.SetTimestamp(serverComponentData.Value.timestamp);
            //
            // var transform = Utils.GetStateUpdate<TransformState, TransformState.Update, TransformState.Data>(player, entityId, 190602);
            // transform.SetTimestamp(serverComponentData.Value.timestamp);
            //
            // if (serverComponentData.Value.relativeTo.IsValid())
            // {
            //     // TODO: Utilities for converting between compressed data variants Vector3f <-> FixedPointVector3 and Quaternion <-> Quaternion32
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

            Console.WriteLine("Received relative position: " + serverComponentData.Value.positionRelative);
            // Console.WriteLine("rotr: " + serverComponentData.Value.rotationRelative);
            // Console.WriteLine("rto: " + serverComponentData.Value.relativeTo);
            // Console.WriteLine("rbias: " + serverComponentData.Value.relativeBias);
            // Console.WriteLine("grounded: " + serverComponentData.Value.grounded);
            // Console.WriteLine("timestamp: " + serverComponentData.Value.timestamp);
            // Console.WriteLine("knocked out " + serverComponentData.Value.knockedOut);
            // Console.WriteLine("last req " + serverComponentData.Value.lastExecutedRequest);
            // Console.WriteLine("id " + entityId);

            // SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId, 1242, 190602 }, new System.Collections.Generic.List<object> { serverComponentUpdate, location, transform });
            SendOPHelper.SendComponentUpdateOp(player, entityId, new System.Collections.Generic.List<uint> { ComponentId }, new System.Collections.Generic.List<object> { serverComponentUpdate });
        }
    }
}
