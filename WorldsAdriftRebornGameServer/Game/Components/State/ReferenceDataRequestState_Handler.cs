﻿using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Bossa.Travellers.Refdata;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer.Game.Components.Handlers
{
    [ComponentStateHandler]
    internal class ReferenceDataRequestStateHandler : IComponentStateHandler<ReferenceDataRequestState, ReferenceDataRequestState.Update, ReferenceDataRequestState.Data>
    {
        public override uint ComponentId => 6908;
        
        private static byte[] Compress(string input)
        {
            byte[] data = Encoding.ASCII.GetBytes(input);
            using MemoryStream mStream = new();
            using (GZipStream gStream = new(mStream, CompressionMode.Compress, true))
            {
                gStream.Write(data, 0, data.Length);
            }
            return mStream.ToArray();
        }

        public override void HandleUpdate( ENetPeerHandle player, long entityId,
            ReferenceDataRequestState.Update clientComponentUpdate, ReferenceDataRequestState.Data serverComponentData )
        {
            var entity = EntityManager.GlobalEntityRealm[entityId];
            clientComponentUpdate.ApplyTo(serverComponentData);
            ReferenceDataRequestState.Update serverComponentUpdate = (ReferenceDataRequestState.Update)serverComponentData.ToUpdate();

            for (int j = 0; j < clientComponentUpdate.requestReferenceData.Count; j++)
            {
                bool doComp = clientComponentUpdate.requestReferenceData[j].compress;
                Console.WriteLine("[info] game requests reference data, compress: " + doComp);

                var newRefData = entity.Get<ReferenceDataState>().Value.ToUpdate().Get();
                
                var invData = ItemHelper.GetReferenceItems();
                var resDesc = ItemHelper.GetDescriptions(true);
                var scrapDesc = ItemHelper.GetDescriptions();
                var bundleDesc = ItemHelper.BundleDescriptions();
                newRefData.SetInventoryData(invData);
                newRefData.AddInventoryDataSent(new SendInventoryData(invData, doComp ? Compress(invData) : null));
                newRefData.SetResourcesDescriptions(resDesc);
                newRefData.AddResourceDescriptionsSent(new SendResourceDescriptions(resDesc, doComp ? Compress(JsonSerializer.Serialize(resDesc)) : null));
                newRefData.SetScrapItemsDescriptions(scrapDesc);
                newRefData.AddScrapItemDescriptionsSent(new SendScrapItemsDescriptions(scrapDesc, doComp ? Compress(JsonSerializer.Serialize(scrapDesc)) : null));
                newRefData.AddSteamInvBundlesDescriptionsSent(
                    new SendSteamInventoryBundlesDescriptions(bundleDesc, doComp ? Compress(JsonSerializer.Serialize(bundleDesc)) : null));
                var schematicData =
                    "{\"glider\":{\"SchematicType\":0,\"uUID\":\"glider\",\"schematicId\":\"glider\",\"referenceData\":\"glider\",\"category\":\"Personal\",\"title\":\"cool glider\",\"iconId\":\"crafted items/3x4_glider\",\"description\":\"wolo\",\"timeToCraft\":10,\"amountToCraft\":1,\"itemType\":\"hmm\",\"craftingRequirements\":[],\"baseHp\":100.0,\"baseStats\":{},\"rarity\":1,\"cipherSlots\":[],\"unlearnable\":false,\"modules\":{},\"hullData\":\"hullData\",\"OrderedStats\":[],\"UniqueID\":\"glider\",\"CraftingCategoryEnum\":1,\"HumanReadableItemType\":\"Hmm\",\"rarityParsed\":1,\"HullDataBytes\":\"hullData\",\"IsProcedural\":false,\"IsShip\":false,\"cipherSlotParsed\":[]}}";
                newRefData.SetSchematicsData(schematicData);
                newRefData.AddSchematicDataSent(new SendSchematicData(schematicData, doComp ? Compress(schematicData) : null));

                SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { 1097 }, new List<object> { newRefData });

                entity.Update(newRefData);
            }

            SendOPHelper.SendComponentUpdateOp(player, entityId, new List<uint> { ComponentId }, new List<object> { serverComponentUpdate });
        }
    }
}
