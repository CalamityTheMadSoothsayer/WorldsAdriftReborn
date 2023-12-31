using Bossa.Travellers.Player;
using Improbable;
using Improbable.Collections;
using Improbable.Corelib.Math;
using Improbable.Corelibrary.Transforms;
using Improbable.Math;
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
        
        public void OnPositionRelativeUpdate(Vector3f lastPositionRelative, Vector3f nextPositionRelative)
        {
            Console.WriteLine("PositionRelative: " + lastPositionRelative + " -> " + nextPositionRelative);
        }

        public void OnRotationRelativeUpdate(Quaternion lastRotationRelative, Quaternion nextRotationRelative)
        {
            Console.WriteLine("RotationRelative: " + lastRotationRelative + " -> " + nextRotationRelative);
        }

        public void OnRelativeToUpdate(EntityId lastRelativeTo, EntityId nextRelativeTo)
        {
            Console.WriteLine("RelativeTo: " + lastRelativeTo + " -> " + nextRelativeTo);
        }

        public void OnRelativeBiasUpdate(float lastRelativeBias, float nextRelativeBias)
        {
            Console.WriteLine("RelativeBias: " + lastRelativeBias + " -> " + nextRelativeBias);
        }

        public void OnTimestampUpdate(float lastTimestamp, float nextTimestamp)
        {
            Console.WriteLine("Timestamp: " + lastTimestamp + " -> " + nextTimestamp);
        }

        public void OnBoneDataUpdate(byte[] lastBoneData, byte[] nextBoneData)
        {
            Console.WriteLine("BoneData has been updated.");
        }

        public void OnGroundedUpdate(bool lastGrounded, bool nextGrounded)
        {
            Console.WriteLine("Grounded: " + lastGrounded + " -> " + nextGrounded);
        }

        public void OnLastExecutedRequestUpdate(int lastLastExecutedRequest, int nextLastExecutedRequest)
        {
            Console.WriteLine("LastExecutedRequest: " + lastLastExecutedRequest + " -> " + nextLastExecutedRequest);
        }

        public void OnKnockedOutUpdate(bool lastKnockedOut, bool nextKnockedOut)
        {
            Console.WriteLine("KnockedOut: " + lastKnockedOut + " -> " + nextKnockedOut);
        }

        public void OnIsRelativeToShipUpdate(Option<bool> lastIsRelativeToShip, Option<bool> nextIsRelativeToShip)
        {
            Console.WriteLine("IsRelativeToShip: " + lastIsRelativeToShip + " -> " + nextIsRelativeToShip);
        }

        public void OnRelativeToShipUidUpdate(Option<long> lastRelativeToShipUid, Option<long> nextRelativeToShipUid)
        {
            Console.WriteLine("RelativeToShipUid: " + lastRelativeToShipUid + " -> " + nextRelativeToShipUid);
        }
        
        public override void HandleUpdate( ENetPeerHandle player, long entityId, ClientAuthoritativePlayerState.Update clientComponentUpdate, ClientAuthoritativePlayerState.Data serverComponentData)
        {
            if (clientComponentUpdate.positionRelative.HasValue && clientComponentUpdate.positionRelative.Value != serverComponentData.Value.positionRelative)
                OnPositionRelativeUpdate(serverComponentData.Value.positionRelative, clientComponentUpdate.positionRelative.Value);

            if (clientComponentUpdate.rotationRelative.HasValue && clientComponentUpdate.rotationRelative.Value != serverComponentData.Value.rotationRelative)
                OnRotationRelativeUpdate(serverComponentData.Value.rotationRelative, clientComponentUpdate.rotationRelative.Value);

            if (clientComponentUpdate.relativeTo.HasValue && clientComponentUpdate.relativeTo.Value != serverComponentData.Value.relativeTo)
                OnRelativeToUpdate(serverComponentData.Value.relativeTo, clientComponentUpdate.relativeTo.Value);

            if (clientComponentUpdate.relativeBias.HasValue && clientComponentUpdate.relativeBias.Value != serverComponentData.Value.relativeBias)
                OnRelativeBiasUpdate(serverComponentData.Value.relativeBias, clientComponentUpdate.relativeBias.Value);

            if (clientComponentUpdate.timestamp.HasValue && clientComponentUpdate.timestamp.Value != serverComponentData.Value.timestamp)
                OnTimestampUpdate(serverComponentData.Value.timestamp, clientComponentUpdate.timestamp.Value);

            if (clientComponentUpdate.boneData.HasValue) // Not doing deep comparison
                OnBoneDataUpdate(serverComponentData.Value.boneData, clientComponentUpdate.boneData.Value);

            if (clientComponentUpdate.grounded.HasValue && clientComponentUpdate.grounded.Value != serverComponentData.Value.grounded)
                OnGroundedUpdate(serverComponentData.Value.grounded, clientComponentUpdate.grounded.Value);

            if (clientComponentUpdate.lastExecutedRequest.HasValue && clientComponentUpdate.lastExecutedRequest.Value != serverComponentData.Value.lastExecutedRequest)
                OnLastExecutedRequestUpdate(serverComponentData.Value.lastExecutedRequest, clientComponentUpdate.lastExecutedRequest.Value);

            if (clientComponentUpdate.knockedOut.HasValue && clientComponentUpdate.knockedOut.Value != serverComponentData.Value.knockedOut)
                OnKnockedOutUpdate(serverComponentData.Value.knockedOut, clientComponentUpdate.knockedOut.Value);

            if (clientComponentUpdate.isRelativeToShip.HasValue && !clientComponentUpdate.isRelativeToShip.Value.Equals(serverComponentData.Value.isRelativeToShip))
                OnIsRelativeToShipUpdate(serverComponentData.Value.isRelativeToShip, clientComponentUpdate.isRelativeToShip.Value);

            if (clientComponentUpdate.relativeToShipUid.HasValue && !clientComponentUpdate.relativeToShipUid.Value.Equals(serverComponentData.Value.relativeToShipUid))
                OnRelativeToShipUidUpdate(serverComponentData.Value.relativeToShipUid, clientComponentUpdate.relativeToShipUid.Value);

            clientComponentUpdate.ApplyTo(serverComponentData);
            ClientAuthoritativePlayerState.Update serverComponentUpdate = (ClientAuthoritativePlayerState.Update)serverComponentData.ToUpdate();


            var entity = EntityManager.GlobalEntityRealm[entityId];
            var transform = entity.Get<TransformState>().Value.ToUpdate().Get();
            transform.SetTimestamp(serverComponentData.Value.timestamp);

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
