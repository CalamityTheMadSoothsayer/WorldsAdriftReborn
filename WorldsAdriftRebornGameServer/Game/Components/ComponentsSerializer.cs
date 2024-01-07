using System.Runtime.InteropServices;
using Bossa.Travellers.Alliance;
using Bossa.Travellers.Analytics;
using Bossa.Travellers.Ancientrespawners;
using Bossa.Travellers.Biomes;
using Bossa.Travellers.Clock;
using Bossa.Travellers.Controls;
using Bossa.Travellers.Craftingstation;
using Bossa.Travellers.Devconsole;
using Bossa.Travellers.Ecs;
using Bossa.Travellers.Interact;
using Bossa.Travellers.Inventory;
using Bossa.Travellers.Items;
using Bossa.Travellers.Loot;
using Bossa.Travellers.Misc;
using Bossa.Travellers.Motion;
using Bossa.Travellers.Player;
using Bossa.Travellers.Refdata;
using Bossa.Travellers.Rope;
using Bossa.Travellers.Salvaging;
using Bossa.Travellers.Scanning;
using Bossa.Travellers.Ship.Lock;
using Bossa.Travellers.Social;
using Bossa.Travellers.Weather;
using Bossa.Travellers.World;
using Improbable;
using Improbable.Collections;
using Improbable.Corelib.Math;
using Improbable.Corelib.Metrics;
using Improbable.Corelib.Worker.Checkout;
using Improbable.Corelibrary.Activation;
using Improbable.Corelibrary.Math;
using Improbable.Corelibrary.Transforms;
using Improbable.Corelibrary.Transforms.Global;
using Improbable.Corelibrary.Transforms.Teleport;
using Improbable.Math;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entity;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Singleton;

namespace WorldsAdriftRebornGameServer.Game.Components
{
    internal class ComponentsSerializer
    {
        public static CoreEntity? coreEntityComp { get; set; } 

        public unsafe static void InitAndSerialize(ENetPeerHandle player, long entityId, uint componentId, byte** buffer, uint* length)
        {
            
            for (int i = 0; i < ComponentsManager.Instance.ClientComponentVtables.Length; i++)
            {
                if (ComponentsManager.Instance.ClientComponentVtables[i].ComponentId == componentId)
                {
                    ulong refId = 0;
                    object obj = null;

                    

                    bool isCoreEntity = EntityManager.GlobalEntityRealm.TryGetValue(entityId, out var coreEntities);
                    if (isCoreEntity)
                    {
                        coreEntityComp = coreEntities;
                    }

                    switch (componentId)
                    {
                        #region 1000s
                        case 1003:
                            PlayerCraftingInteractionState.Data pcisData = new PlayerCraftingInteractionState.Data(new EntityId(1), true);

                            obj = pcisData;

                            EntityManager.GlobalEntityRealm[entityId].Add(pcisData);
                            break;

                        case 1005:
                            CraftingStationClientState.Data csData = new CraftingStationClientState.Data(new CraftingStationClientStateData("schematicId",
                                                                                                                                    "owner",
                                                                                                                                    new Improbable.Collections.List<SlottedMaterial> { },
                                                                                                                                    new Improbable.Collections.List<Cipher> { },
                                                                                                                                    12,
                                                                                                                                    30f,
                                                                                                                                    new Option<PredictedStatDataExtra> { }));
                            obj = csData;

                            EntityManager.GlobalEntityRealm[entityId].Add(csData);
                            break;
                        case 1006:
                            DevelopmentConsoleState.Data dcData = new DevelopmentConsoleState.Data(new DevelopmentConsoleStateData(100, 100, "gsimHostname", new Coordinates(0, 0, 0), "zone"));


                            EntityManager.GlobalEntityRealm[entityId].Add(dcData);
                            obj = dcData;
                            break;

                        case 1008:
                            FsimStatus.Data fData = new FsimStatus.Data(new FsimStatusData(60f, 1234, 150, "fsimEngineId", 123));


                            EntityManager.GlobalEntityRealm[entityId].Add(fData);
                            obj = fData;
                            break;

                        case 1040:
                            // Used By - InventoryVisualiser (reader)
                            // Known settings are:
                            // serverAuthoritativeInventory (bool) - Unknown repercussions
                            Console.WriteLine("Sending authInv bool!");
                            GamePropertiesState.Data gpData = new GamePropertiesState.Data(new GamePropertiesStateData(new Map<string, string> { ["serverAuthoritativeInventory"] = "true" }));

                            obj = gpData;
                            EntityManager.GlobalEntityRealm[entityId].Add(gpData);
                            break;
                        case 1041:
                            var transformTarget = EntityManager.GlobalEntityRealm[entityId];
                            var matchingIsland = transformTarget as Island ?? Island.IslandList[0];
                            var islandPosition = matchingIsland.Position;

                            IslandState.Data data = new IslandState.Data(new IslandStateData(
                                matchingIsland.Key,
                                new Coordinates(islandPosition[0], islandPosition[1] + 300, islandPosition[2]), // should be respawner location
                                1f,
                                new Vector3f(0, 0, 0),
                                new Vector3f(200f, 200f, 200f),
                                new Option<string>("Mr Scronge"),
                                false,
                                new Improbable.Collections.List<IslandDatabank>()
                            ));

                            EntityManager.GlobalEntityRealm[entityId].Add(data);

                            // Set the object to the created data
                            obj = data;
                            break;
                        case 1042:
                            // creates a system.collections list
                            var associatedRespawners = Island.IslandSpawners
                                .Where(association => association.IslandId == entityId)
                                .SelectMany(association => new[] { association.FirstRespawner })
                                .ToList();

                            // need Improbable.Collections.List here (simpler way?)
                            var improbableEntityIdList = new Improbable.Collections.List<EntityId>(associatedRespawners);

                            var islands = Island.IslandList;

                            // todo: Modify the values as needed based on your data structure
                            IslandFabricState.Data edata = new IslandFabricState.Data(new IslandFabricStateData(
                                1,
                                0,
                                0,
                                improbableEntityIdList,
                                new Option<EntityId>(associatedRespawners.FirstOrDefault()),
                                new Option<string>(""),
                                Bossa.Travellers.Biomes.BiomeType.Biome3,
                                false,
                                new Option<Coordinates>(new Coordinates(islands[0].Position[0], islands[0].Position[1], 0)), // seeing if this is what is moving islands to 0,0,0
                                new Option<double>(0),
                                new Option<double>(0)));

                            EntityManager.GlobalEntityRealm[entityId].Add(edata);
                            obj = edata;
                            break;

                        case 1071:
                            BuilderServerState.Data bsData = new BuilderServerState.Data(new BuilderServerStateData(new EntityId(0)));

                            obj = bsData;

                            EntityManager.GlobalEntityRealm[entityId].Add(bsData);
                            break;
                        case 1072:
                            CharacterControlsData.Data capData = new CharacterControlsData.Data(new CharacterControlsDataData());
                            obj = capData;
                            EntityManager.GlobalEntityRealm[entityId].Add(capData);
                            break;
                        case 1073:
                            // relativeBias - Controls interpolation between the player and their relative gameobject's position
                            ClientAuthoritativePlayerState.Data capsData = new ClientAuthoritativePlayerState.Data(new ClientAuthoritativePlayerStateData(new Improbable.Math.Vector3f(0f, 0f, 0f),
                                new Improbable.Corelib.Math.Quaternion(0, 0, 0, 0),
                                new EntityId(2),
                                1,
                                0,
                                new byte[] { },
                                true,
                                0,
                                false,
                                false,
                                null));
                            obj = capsData;
                            EntityManager.GlobalEntityRealm[entityId].Add(capsData);
                            break;

                        case 1077:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // alive - TODO: Needs to be changed when dead
                            HealthState.Data hData = new HealthState.Data(new HealthStateData(100, 100, true, 0f, true, new Improbable.Collections.List<EntityId> { }, 1f, 1f));

                            obj = hData;
                            EntityManager.GlobalEntityRealm[entityId].Add(hData);
                            break;

                        case 1079:
                            SchematicsLearnerClientState.Data scData = new SchematicsLearnerClientState.Data(new SchematicsLearnerClientStateData(new Improbable.Collections.List<string> { },
                                                                                                                                        new Improbable.Collections.List<string> { },
                                                                                                                                        10,
                                                                                                                                        20,
                                                                                                                                        10,
                                                                                                                                        10));
                            obj = scData;

                            EntityManager.GlobalEntityRealm[entityId].Add(scData);
                            break;
                        case 1080:
                            SchematicsLearnerGSimState.Data schematicGsimData = new SchematicsLearnerGSimState.Data(new Improbable.Collections.List<string>(), new Map<string, string>(), false, new Improbable.Collections.List<string>());
                            obj = schematicGsimData;
                            EntityManager.GlobalEntityRealm[entityId].Add(schematicGsimData);
                            break;
                        case 1081:
                            InventoryState.Data iData = new InventoryState.Data(new InventoryStateData(100,
                                                                                            "{}",
                                                                                            ItemHelper.GetDefaultItems(),
                                                                                            ItemHelper.GetStashItems(true, true),
                                                                                            10,
                                                                                            18,
                                                                                            new Improbable.Collections.List<string> { },
                                                                                            true,
                                                                                            3));
                            obj = iData;
                            EntityManager.GlobalEntityRealm[entityId].Add(iData);
                            break;
                        case 1082:
                            InventoryModificationState.Data imData = new InventoryModificationState.Data();

                            obj = imData;

                            EntityManager.GlobalEntityRealm[entityId].Add(imData);
                            break;

                        case 1086:
                            PlayerName.Data pData = new PlayerName.Data(new PlayerNameData("sp00ktober", "id", "cUid", "bossaToken", "bossaId"));

                            obj = pData;
                            EntityManager.GlobalEntityRealm[entityId].Add(pData);
                            break;
                        case 1087:
                            PlayerPermissionsState.Data ppsData = new PlayerPermissionsState.Data(Role.NonAdmin);

                            obj = ppsData;

                            EntityManager.GlobalEntityRealm[entityId].Add(ppsData);
                            break;
                        case 1088:
                            PlayerPropertiesState.Data ppData = new PlayerPropertiesState.Data(new PlayerPropertiesStateData(new Map<string, string> { },
                                                                                                                    new Map<string, string>
                                                                                                                    {
                                                                                                                        {"Head", "hair_dreads" },
                                                                                                                        {"Body", "torso_ponchoVariantB" },
                                                                                                                        {"Feet", "legs_wrap" },
                                                                                                                        {"Face", "face_C" }
                                                                                                                    },
                                                                                                                    new Improbable.Collections.List<string> { },
                                                                                                                    false));
                            obj = ppData;
                            EntityManager.GlobalEntityRealm[entityId].Add(ppData);
                            break;

                        case 1092:
                            var parent = Island.IslandSpawners[0].FirstRespawner;
                            Console.WriteLine("Player will respawn at entityId " + parent.Id);
                            RespawnState.Data rsdata = new RespawnState.Data(true,
                                parent,  // waitingParent 
                                "oof",
                                2,
                                50,
                                5,
                                5,
                                0,
                                new Option<ReviverData>(),
                                null,
                                1,
                                false);

                            EntityManager.GlobalEntityRealm[entityId].Add(rsdata);
                            obj = rsdata;
                            break;
                        case 1093:
                            RespawnClientState.Data rdata = new RespawnClientState.Data();

                            EntityManager.GlobalEntityRealm[entityId].Add(rdata);
                            obj = rdata;
                            break;

                        case 1095:
                            FSimTimeState.Data fsData = new FSimTimeState.Data(new FSimTimeStateData(0.15f, "fsimId", 100));


                            EntityManager.GlobalEntityRealm[entityId].Add(fsData);
                            obj = fsData;
                            break;

                        case 1097:
                            ReferenceDataState.Data referenceData = new ReferenceDataState.Data(new ReferenceDataStateData(
                                new EntityId(-1),
                                "",
                                new Map<string, string>(),
                                new Map<string, string>(),
                                "{}",
                                "{}",
                                "{}",
                                10,
                                new Map<string, string>(),
                                true
                            ));

                            EntityManager.GlobalEntityRealm[entityId].Add(referenceData);
                            obj = referenceData;
                            break;
                        case 1098:
                            RopeControlPoints.Data rcData = new RopeControlPoints.Data(new RopeControlPointsData(new Improbable.Collections.List<Coordinates> { }, new Improbable.Collections.List<DynamicRopePoint> { }, false, 0f));

                            obj = rcData;

                            EntityManager.GlobalEntityRealm[entityId].Add(rcData);
                            break;
                        #endregion

                        #region 1100s
                        case 1109:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // ControlVehicleType - TODO: Needs to be changed when mounting something
                            PilotState.Data psData = new PilotState.Data(new PilotStateData(new EntityId(-1), new EntityId(-1), ControlVehicleType.None));

                            obj = psData;
                            EntityManager.GlobalEntityRealm[entityId].Add(psData);
                            break;

                        case 1131:
                            WorldData.Data woData = new WorldData.Data(new WorldDataData(new EntityId(0), 0.15f, 1f, 1));

                            obj = woData;

                            EntityManager.GlobalEntityRealm[entityId].Add(woData);
                            break;

                        case 1139:
                            WeatherCellState.Data wcData = new WeatherCellState.Data(new WeatherCellStateData(0f, new Vector3f(0f, 0f, 0f)));


                            EntityManager.GlobalEntityRealm[entityId].Add(wcData);
                            obj = wcData;
                            break;

                        case 1150:
                            PlayerActivationState.Data pcData = new PlayerActivationState.Data(new PlayerActivationStateData(true, 12345, 123));

                            obj = pcData;

                            EntityManager.GlobalEntityRealm[entityId].Add(pcData);
                            break;
                        #endregion

                        #region 1200s
                        case 1207:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // schematics - TODO: Investigate
                            ShipHullAgentState.Data shData = new ShipHullAgentState.Data(new ShipHullAgentStateData(new Improbable.Collections.List<ShipHullSchematicData> { }, new EntityId(0)));

                            obj = shData;
                            EntityManager.GlobalEntityRealm[entityId].Add(shData);
                            break;

                        case 1211:
                            InteractAgentState.Data iaData = new InteractAgentState.Data(new InteractAgentStateData(true,
                                                                                                            new EntityId(0),
                                                                                                            new EntityId(0),
                                                                                                            new EntityId(0),
                                                                                                            new Improbable.Math.Vector3f(0f, 0f, 0f),
                                                                                                            new Improbable.Math.Coordinates(),
                                                                                                            ItemHelper.SALVAGE_REPAIR_TOOL,
                                                                                                            1));
                            obj = iaData;
                            EntityManager.GlobalEntityRealm[entityId].Add(iaData);
                            break;
                        case 1212:
                            InteractAgentServerState.Data iasData = new InteractAgentServerState.Data(new InteractAgentServerStateData(new EntityId(0),
                                                                                                                            0,
                                                                                                                            1,
                                                                                                                            new EntityId(0),
                                                                                                                            new EntityId(0),
                                                                                                                            Bossa.Travellers.Items.MultitoolMode.Default,
                                                                                                                            new Option<ScalaSlottedInventoryItem> { }));
                            obj = iasData;
                            EntityManager.GlobalEntityRealm[entityId].Add(iasData);
                            break;

                        case 1219:
                            ShipyardVisitorState.Data svData = new ShipyardVisitorState.Data(new ShipyardVisitorStateData(new EntityId(0), "abcdefg"));

                            obj = svData;

                            EntityManager.GlobalEntityRealm[entityId].Add(svData);
                            break;

                        case 1240:  // reader
                            LorePiecesCollectorGsimState.Data loreGsimData = new LorePiecesCollectorGsimState.Data(new Improbable.Collections.List<string>());
                            obj = loreGsimData;

                            EntityManager.GlobalEntityRealm[entityId].Add(loreGsimData);
                            break;
                        case 1241: // writer
                            LorePiecesCollectorClientState.Data loreClientData = new LorePiecesCollectorClientState.Data();
                            obj = loreClientData;

                            EntityManager.GlobalEntityRealm[entityId].Add(loreClientData);
                            break;
                        case 1242:
                            LocationState.Data locData = new LocationState.Data(new AbsoluteLocation(new Coordinates(0, 100, 0), new Quaternion(0, 0, 0, 0)), new RelativeLocation(new EntityId(2), new Vector3f(0, 100f, 0), new Quaternion(0, 0, 0, 0)), 0f);

                            EntityManager.GlobalEntityRealm[entityId].Add(locData);
                            obj = locData;
                            break;

                        case 1254:
                            IslandLightningTimerState.Data ilData = new IslandLightningTimerState.Data(new IslandLightningTimerStateData(50 * 1000, // must be >= 30  and below must be > 0 to trigger lightning rumbles. multiply by 1000 to actually get the value you want ingame (50 in this case)
                                                                                                                                0, // must be 0 or you will set the island into a storm
                                                                                                                                1234,
                                                                                                                                1234,
                                                                                                                                false,
                                                                                                                                1,
                                                                                                                                new Improbable.Collections.List<EntityId> { new EntityId(2) }));

                            EntityManager.GlobalEntityRealm[entityId].Add(ilData);
                            obj = ilData;
                            break;

                        case 1260:
                            SchematicsUnlearnerState.Data susData = new SchematicsUnlearnerState.Data();
                            EntityManager.GlobalEntityRealm[entityId].Add(susData);

                            obj = susData;
                            break;

                        case 1269:
                            RadialStormState.Data rsData = new RadialStormState.Data(new RadialStormStateData(0f));


                            EntityManager.GlobalEntityRealm[entityId].Add(rsData);
                            obj = rsData;
                            break;

                        case 1280:
                            WearableUtilsState.Data wData = new WearableUtilsState.Data(new WearableUtilsStateData(new Improbable.Collections.List<int> { }, new Improbable.Collections.List<float> { }, new Improbable.Collections.List<bool> { }));

                            obj = wData;
                            EntityManager.GlobalEntityRealm[entityId].Add(wData);
                            break;
                        #endregion

                        #region 1300s
                        case 1332:
                            KnowledgeServerState.Data ksData = new KnowledgeServerState.Data(new KnowledgeServerStateData(1,
                                                                                                                new Map<string, int> { },
                                                                                                                1,
                                                                                                                new Map<string, int> { }));
                            obj = ksData;

                            EntityManager.GlobalEntityRealm[entityId].Add(ksData);
                            break;
                        #endregion

                        #region 2000s
                        case 2001:
                            PlayerAnalyticsState.Data paData = new PlayerAnalyticsState.Data(new PlayerAnalyticsStateData("someuser_id",
                                                                                                                "somesession_id",
                                                                                                                false,
                                                                                                                "unity",
                                                                                                                new Improbable.Collections.List<string> { },
                                                                                                                "defaultPayload",
                                                                                                                false));
                            obj = paData;

                            EntityManager.GlobalEntityRealm[entityId].Add(paData);
                            break;
                        case 2002:
                            MultitoolRepairerState.Data mrData = new MultitoolRepairerState.Data(new MultitoolRepairerStateData(false, false));

                            obj = mrData;
                            EntityManager.GlobalEntityRealm[entityId].Add(mrData);
                            break;
                        #endregion

                        #region 2100
                        case 2105:
                            MultiToolPlayerState.Data mtData = new MultiToolPlayerState.Data(new MultiToolPlayerStateData(true, MultitoolMode.Salvage, 10));

                            obj = mtData;
                            EntityManager.GlobalEntityRealm[entityId].Add(mtData);
                            break;
                        case 2106:
                            MultitoolSalvagerState.Data msData = new MultitoolSalvagerState.Data(new MultitoolSalvagerStateData(false, false, false));

                            obj = msData;
                            EntityManager.GlobalEntityRealm[entityId].Add(msData);
                            break;
                        #endregion

                        #region 4300s
                        case 4329:
                            PlayerBuffState.Data pbData = new PlayerBuffState.Data(new PlayerBuffStateData(new Improbable.Collections.List<Buff> { }));


                            EntityManager.GlobalEntityRealm[entityId].Add(pbData);
                            obj = pbData;
                            break;
                        #endregion

                        #region 4400s
                        case 4444:
                            MountedGunShotState.Data mgData = new MountedGunShotState.Data();

                            obj = mgData;

                            EntityManager.GlobalEntityRealm[entityId].Add(mgData);
                            break;
                        #endregion

                        #region 6900s
                        case 6902:
                            GsimEventAuditState.Data gsData = new GsimEventAuditState.Data(new GsimEventAuditStateData(new Map<string, int> { }));


                            EntityManager.GlobalEntityRealm[entityId].Add(gsData);
                            obj = gsData;
                            break;

                        case 6905:
                            AncientRespawnerState.Data arData = new AncientRespawnerState.Data(new AncientRespawnerStateData(1, BiomeType.Biome1, new Improbable.Collections.List<EntityId>()));
                            obj = arData;
                            EntityManager.GlobalEntityRealm[entityId].Add(arData);
                            break;

                        case 6908:
                            ReferenceDataRequestState.Data eeeData = new ReferenceDataRequestState.Data(new ReferenceDataRequestStateData());

                            EntityManager.GlobalEntityRealm[entityId].Add(eeeData);
                            obj = eeeData;
                            break;
                        
                        case 6924:
                            AllianceNameState.Data anData = new AllianceNameState.Data(new AllianceNameStateData("WA Alliance"));

                            obj = anData;
                            EntityManager.GlobalEntityRealm[entityId].Add(anData);
                            break;
                        case 6925:
                            AllianceAndCrewWorkerState.Data acData = new AllianceAndCrewWorkerState.Data(new AllianceAndCrewWorkerStateData("alliance_invalid_uid", "crew_invalid_uid"));

                            obj = acData;

                            EntityManager.GlobalEntityRealm[entityId].Add(acData);
                            break;
                        #endregion

                        #region 8000s
                        case 8050:
                            ToolRequestState.Data toolRequestData = new ToolRequestState.Data(new ToolRequestStateData());

                            EntityManager.GlobalEntityRealm[entityId].Add(toolRequestData);
                            obj = toolRequestData;
                            break;
                        case 8051:
                            ToolState.Data toolData = new ToolState.Data(new ToolStateData(30));

                            EntityManager.GlobalEntityRealm[entityId].Add(toolData);
                            obj = toolData;
                            break;

                        case 8055:
                            NewPlayerState.Data npData = new NewPlayerState.Data(new NewPlayerStateData(true));

                            EntityManager.GlobalEntityRealm[entityId].Add(npData);
                            obj = npData;
                            break;

                        case 8060:
                            FeedbackListener.Data fbData = new FeedbackListener.Data();


                            EntityManager.GlobalEntityRealm[entityId].Add(fbData);
                            obj = fbData;
                            break;

                        case 8065:
                            if (isCoreEntity)
                            {
                                if (coreEntityComp.Key != "Player")
                                {
                                    Blueprint.Data bData = new Blueprint.Data(new BlueprintData("Player"));
                                    obj = bData;
                                    EntityManager.GlobalEntityRealm[entityId].Add(bData);
                                }
                            }
                            break;
                        #endregion

                        #region 9000s
                        case 9005:
                            SocialWorkerId.Data wiData = new SocialWorkerId.Data(new SocialWorkerIdData("workerId"));


                            EntityManager.GlobalEntityRealm[entityId].Add(wiData);
                            obj = wiData;
                            break;
                        #endregion

                        #region 190000s
                        case 190000:
                            EntityLoadingControl.Data elData = new EntityLoadingControl.Data(new EntityLoadingControlData(EntityLoadingControlData.EntityLoadingStates.Idle,
                                                                                                                0,
                                                                                                                5,
                                                                                                                100,
                                                                                                                false,
                                                                                                                new Improbable.Collections.List<EntityId> { }));
                            obj = elData;

                            EntityManager.GlobalEntityRealm[entityId].Add(elData);
                            break;

                        case 190002:
                            Activated.Data aData = new Activated.Data(new ActivatedData(true, true, 0));

                            obj = aData;

                            EntityManager.GlobalEntityRealm[entityId].Add(aData);
                            break;

                        case 190300:
                            ClientPhysicsLatency.Data cpData = new ClientPhysicsLatency.Data(new ClientPhysicsLatencyData(250, 100));


                            EntityManager.GlobalEntityRealm[entityId].Add(cpData);
                            obj = cpData;
                            break;

                        case 190601:
                            TransformHierarchyState.Data thData = new TransformHierarchyState.Data(new TransformHierarchyStateData(new Improbable.Collections.List<Child> { }));

                            obj = thData;
                            EntityManager.GlobalEntityRealm[entityId].Add(thData);
                            break;

                        case 190602:
                            if (isCoreEntity)
                            {
                                Console.WriteLine("[Component " + componentId + "] accessed for " + coreEntityComp.Key + " " + entityId + "[POSITION] = " + coreEntityComp.Position[0] + " " + coreEntityComp.Position[1] + " " + coreEntityComp.Position[2]);
                                TransformState.Data tData = coreEntityComp.Get<TransformState>().Value.Get();

                                obj = tData;

                                EntityManager.GlobalEntityRealm[entityId].Add(tData);
                            }
                            else
                            {
                                // Default TransformState moved to CoreEntity class
                                Console.WriteLine("[Component " + componentId + "] No existing entry for TransformState, use entity.Position");
                                TransformStateData tInit = new TransformStateData(new FixedPointVector3(new Improbable.Collections.List<long>()),
                                    new Quaternion32(1),
                                    null,
                                    new Improbable.Math.Vector3d(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    false,
                                    0f);
                                TransformState.Data tData = new TransformState.Data(tInit);

                                obj = tData;
                                EntityManager.GlobalEntityRealm[entityId].Add(tData);
                            }
                            break;

                        case 190604:
                            GlobalTransformState.Data gdata = new GlobalTransformState.Data(new GlobalTransformStateData(new Coordinates(0, 0, 0),
                                                                                                                            new Improbable.Corelib.Math.Quaternion(0, 0, 0, 0),
                                                                                                                            new Vector3d(0, 0, 0),
                                                                                                                            0));

                            EntityManager.GlobalEntityRealm[entityId].Add(gdata);
                            obj = gdata;
                            break;

                        case 190607:
                            var entity = EntityManager.GlobalEntityRealm[entityId];
                            TeleportRequestState.Data tdata = new TeleportRequestState.Data(entity.ToVector3d(),
                                new Option<Quaternion>(),
                                new Option<Parent>(new Parent(Island.IslandSpawners[0].FirstRespawner, "Ancient Respawner")),
                                1);
                            entity.Add(tdata);
                            obj = tdata;
                            break;
                        #endregion

                        default:
                            Console.WriteLine("[ToDo] unhandled component id needs investigation: " + componentId);
                            break;
                    }

                    if (obj != null)
                    {
                        
                        refId = ClientObjects.Instance.CreateReference(obj);
                        ComponentProtocol.ClientObject wrapper = new ComponentProtocol.ClientObject();
                        wrapper.Reference = refId;

                        ComponentProtocol.ClientSerialize serialize = Marshal.GetDelegateForFunctionPointer<ComponentProtocol.ClientSerialize>(ComponentsManager.Instance.ClientComponentVtables[i].Serialize);
                        serialize(componentId, 2, &wrapper, buffer, length);
                        
                        
                        // I don't know if removing the refId storage breaks stuff but here goes nothing! :D
                    }
                }
            }
        }
    }
}
