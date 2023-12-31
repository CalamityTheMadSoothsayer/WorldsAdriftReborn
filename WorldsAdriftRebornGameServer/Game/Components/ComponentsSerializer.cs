using System.Runtime.InteropServices;
using Bossa.Travellers.Alliance;
using Bossa.Travellers.Analytics;
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
using Improbable.Math;
using Improbable.Worker.Internal;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game.Entities;
using WorldsAdriftRebornGameServer.Game.Items;
using WorldsAdriftRebornGameServer.Networking.Singleton;

namespace WorldsAdriftRebornGameServer.Game.Components
{
    internal class ComponentsSerializer
    {
        public unsafe static void InitAndSerialize(ENetPeerHandle player, long entityId, uint componentId, byte** buffer, uint* length)
        {
            for(int i = 0; i < ComponentsManager.Instance.ClientComponentVtables.Length; i++)
            {
                if (ComponentsManager.Instance.ClientComponentVtables[i].ComponentId == componentId)
                {
                    ulong refId = 0;
                    object obj = null;

                    switch (componentId)
                    {
                        case 1040:
                            // Used By - InventoryVisualiser (reader)
                            // Known settings are:
                            // serverAuthoritativeInventory (bool) - Unknown repercussions
                            Console.WriteLine("Sending authInv bool!");
                            GamePropertiesState.Data gpData = new GamePropertiesState.Data(new GamePropertiesStateData(new Map<string, string> { ["serverAuthoritativeInventory"] = "true" }));
                            obj = gpData;
                            break;

                        case 1109:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // ControlVehicleType - TODO: Needs to be changed when mounting something
                            PilotState.Data psData = new PilotState.Data(new PilotStateData(new EntityId(-1), new EntityId(-1), ControlVehicleType.None));
                            obj = psData;
                            break;

                        case 1077:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // alive - TODO: Needs to be changed when dead
                            HealthState.Data hData = new HealthState.Data(new HealthStateData(100, 100, true, 0f, true, new Improbable.Collections.List<EntityId> { }, 1f, 1f));
                            obj = hData;
                            break;

                        case 1207:
                            // Used By - PlayerExternalDataVisualizer (reader)
                            // Known settings are:
                            // schematics - TODO: Investigate
                            ShipHullAgentState.Data shData = new ShipHullAgentState.Data(new ShipHullAgentStateData(new Improbable.Collections.List<ShipHullSchematicData> { }, new EntityId(0)));
                            obj = shData;
                            break;

                        case 1073:
                            // relativeBias - Controls interpolation between the player and their relative gameobject's position
                            ClientAuthoritativePlayerState.Data capData = new ClientAuthoritativePlayerState.Data(new ClientAuthoritativePlayerStateData(new Improbable.Math.Vector3f(0f, 0f, 0f),
                                new Improbable.Corelib.Math.Quaternion(0, 0, 0, 0),
                                new EntityId(1),
                                1,
                                0,
                                new byte[] { },
                                true,
                                0,
                                false,
                                false,
                                null));
                            obj = capData;
                            break;
                        case 8065:
                            Blueprint.Data bData = new Blueprint.Data(new BlueprintData("Player"));
                            obj = bData;
                            break;

                        case 190602:
                            var matchingIsland = EntityManager.islandEntities.SingleOrDefault(i => i.EntityId == entityId);
                            if (matchingIsland != null)
                            {
                                TransformStateData tInit = new TransformStateData(new FixedPointVector3(matchingIsland.Position),
                                    new Quaternion32(1),
                                    null,
                                    new Improbable.Math.Vector3d(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    false,
                                    0f);
                                TransformState.Data tData = new TransformState.Data(tInit);
                                obj = tData;
                            }
                            else
                            {
                                Console.WriteLine("[Component " + componentId + "] accessed for [Entity] " + entityId);
                                TransformStateData tInit = new TransformStateData(
                                    new FixedPointVector3(new Improbable.Collections.List<long> { 1000, 1000, 0 }),
                                    new Quaternion32(1),
                                    null,
                                    new Improbable.Math.Vector3d(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    new Improbable.Math.Vector3f(0f, 0f, 0f),
                                    false,
                                    0f);
                                TransformState.Data tData = new TransformState.Data(tInit);
                                obj = tData;
                            }
                            break;

                        case 190601:
                            TransformHierarchyState.Data thData = new TransformHierarchyState.Data(new TransformHierarchyStateData(new Improbable.Collections.List<Child> { }));
                            obj = thData;
                            break;

                        case 1080:
                            SchematicsLearnerGSimState.Data schematicGsimData = new SchematicsLearnerGSimState.Data(new Improbable.Collections.List<string>(), new Map<string, string>(), false, new Improbable.Collections.List<string>());
                            obj = schematicGsimData;
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
                            break;
                        case 1086:
                            PlayerName.Data pData = new PlayerName.Data(new PlayerNameData("sp00ktober", "id", "cUid", "bossaToken", "bossaId"));
                            obj = pData;
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
                            break;

                        case 2105:
                            MultiToolPlayerState.Data mtData = new MultiToolPlayerState.Data(new MultiToolPlayerStateData(true, MultitoolMode.Salvage, 10));
                            obj = mtData;
                            break;

                        case 2106:
                            MultitoolSalvagerState.Data msData = new MultitoolSalvagerState.Data(new MultitoolSalvagerStateData(false, false, false));
                            obj = msData;
                            break;

                        case 2002:
                            MultitoolRepairerState.Data mrData = new MultitoolRepairerState.Data(new MultitoolRepairerStateData(false, false));
                            obj = mrData;
                            break;

                        case 1280:
                            WearableUtilsState.Data wData = new WearableUtilsState.Data(new WearableUtilsStateData(new Improbable.Collections.List<int> { }, new Improbable.Collections.List<float> { }, new Improbable.Collections.List<bool> { }));
                            obj = wData;
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
                            break;

                        case 6924:
                            AllianceNameState.Data anData = new AllianceNameState.Data(new AllianceNameStateData("WA Alliance"));
                            obj = anData;
                            break;

                        case 6925:
                            AllianceAndCrewWorkerState.Data acData = new AllianceAndCrewWorkerState.Data(new AllianceAndCrewWorkerStateData("alliance_invalid_uid", "crew_invalid_uid"));
                            obj = acData;
                            break;

                        case 1082:
                            InventoryModificationState.Data imData = new InventoryModificationState.Data();
                            obj = imData;
                            break;

                        case 1087:
                            PlayerPermissionsState.Data pPData = new PlayerPermissionsState.Data(Role.NonAdmin);
                            obj = pPData;
                            break;

                        case 4444:
                            MountedGunShotState.Data mgData = new MountedGunShotState.Data();
                            obj = mgData;
                            break;

                        case 1071:
                            BuilderServerState.Data bsData = new BuilderServerState.Data(new BuilderServerStateData(new EntityId(0)));
                            obj = bsData;
                            break;
                        case 1131:
                            WorldData.Data woData = new WorldData.Data(new WorldDataData(new EntityId(0), 0.15f, 1f, 1));
                            obj = woData;
                            break;

                        case 1098:
                            RopeControlPoints.Data rcData = new RopeControlPoints.Data(new RopeControlPointsData(new Improbable.Collections.List<Coordinates> { }, new Improbable.Collections.List<DynamicRopePoint> { }, false, 0f));
                            obj = rcData;
                            break;

                        case 2001:
                            PlayerAnalyticsState.Data paData = new PlayerAnalyticsState.Data(new PlayerAnalyticsStateData("someuser_id",
                                "somesession_id",
                                false,
                                "unity",
                                new Improbable.Collections.List<string> { },
                                "defaultPayload",
                                false));
                            obj = paData;
                            break;

                        case 1332:
                            KnowledgeServerState.Data ksData = new KnowledgeServerState.Data(new KnowledgeServerStateData(1000,
                                new Map<string, int> { },
                                1000,
                                new Map<string, int> { }));
                            obj = ksData;
                            break;

                        case 1079:
                            SchematicsLearnerClientState.Data scData = new SchematicsLearnerClientState.Data(new SchematicsLearnerClientStateData(new Improbable.Collections.List<string> { },
                                new Improbable.Collections.List<string> { },
                                10,
                                20,
                                10,
                                10));
                            obj = scData;
                            break;

                        case 190002:
                            Activated.Data aData = new Activated.Data(new ActivatedData(true, true, 0));
                            obj = aData;
                            break;

                        case 190000:
                            EntityLoadingControl.Data elData = new EntityLoadingControl.Data(new EntityLoadingControlData(EntityLoadingControlData.EntityLoadingStates.Idle,
                                0,
                                5,
                                100,
                                false,
                                new Improbable.Collections.List<EntityId> { }));
                            obj = elData;
                            break;
                        case 1150:
                            PlayerActivationState.Data pcData = new PlayerActivationState.Data(new PlayerActivationStateData(true, 12345, 123));
                            obj = pcData;
                            break;

                        case 1219:
                            ShipyardVisitorState.Data svData = new ShipyardVisitorState.Data(new ShipyardVisitorStateData(new EntityId(0), "abcdefg"));
                            obj = svData;
                            break;

                        case 1003:
                            PlayerCraftingInteractionState.Data pcisData = new PlayerCraftingInteractionState.Data(new EntityId(1), true);
                            obj = pcisData;
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
                            break;

                        case 8055:
                            NewPlayerState.Data npData = new NewPlayerState.Data(new NewPlayerStateData(true));
                            obj = npData;
                            break;

                        case 4329:
                            PlayerBuffState.Data pbData = new PlayerBuffState.Data(new PlayerBuffStateData(new Improbable.Collections.List<Buff> { }));
                            obj = pbData;
                            break;

                        case 8060:
                            FeedbackListener.Data fbData = new FeedbackListener.Data();
                            obj = fbData;
                            break;
                        case 1095:
                            FSimTimeState.Data fsData = new FSimTimeState.Data(new FSimTimeStateData(0.15f, "fsimId", 100));
                            obj = fsData;
                            break;

                        case 190300:
                            ClientPhysicsLatency.Data cpData = new ClientPhysicsLatency.Data(new ClientPhysicsLatencyData(250, 100));
                            obj = cpData;
                            break;

                        case 1006:
                            DevelopmentConsoleState.Data dcData = new DevelopmentConsoleState.Data(new DevelopmentConsoleStateData(100, 100, "gsimHostname", new Coordinates(0, 0, 0), "zone"));
                            obj = dcData;
                            break;

                        case 1008:
                            FsimStatus.Data fData = new FsimStatus.Data(new FsimStatusData(60f, 1234, 150, "fsimEngineId", 123));
                            obj = fData;
                            break;

                        case 9005:
                            SocialWorkerId.Data wiData = new SocialWorkerId.Data(new SocialWorkerIdData("workerId"));
                            obj = wiData;
                            break;

                        case 6902:
                            GsimEventAuditState.Data gsData = new GsimEventAuditState.Data(new GsimEventAuditStateData(new Map<string, int> { }));
                            obj = gsData;
                            break;

                        case 1269:
                            RadialStormState.Data rsData = new RadialStormState.Data(new RadialStormStateData(0f));
                            obj = rsData;
                            break;
                        case 1139:
                            WeatherCellState.Data wcData = new WeatherCellState.Data(new WeatherCellStateData(1f, new Vector3f(0f, 0f, 0f)));
                            obj = wcData;
                            break;

                        case 1254:
                            IslandLightningTimerState.Data ilData = new IslandLightningTimerState.Data(new IslandLightningTimerStateData(50 * 1000,
                                0,
                                1234,
                                1234,
                                false,
                                1,
                                new Improbable.Collections.List<EntityId> { new EntityId(2) }));
                            obj = ilData;
                            break;

                        case 1041:
                            IslandState.Data data = new IslandState.Data(new IslandStateData("949069116@Island",
                                new Coordinates(0, 0, 0),
                                1f,
                                new Vector3f(0, 0, 0),
                                new Vector3f(200f, 200f, 200f),
                                new Option<string>("Mr Scronge"),
                                false,
                                new Improbable.Collections.List<IslandDatabank>()
                            ));
                            obj = data;
                            break;

                        case 1042:
                            IslandFabricState.Data fabricData = new IslandFabricState.Data(new IslandFabricStateData(5,
                                0,
                                0,
                                new Improbable.Collections.List<EntityId> { new EntityId(0) },
                                new Option<EntityId>(new EntityId(0)),
                                new Option<string>(""),
                                Bossa.Travellers.Biomes.BiomeType.Biome1,
                                false,
                                new Option<Coordinates>(new Coordinates(0, 0, 0)),
                                new Option<double>(0),
                                new Option<double>(0)));
                            obj = fabricData;
                            break;

                        case 190604:
                            GlobalTransformState.Data globalTransformData = new GlobalTransformState.Data(new GlobalTransformStateData(new Coordinates(0, 0, 0),
                                new Improbable.Corelib.Math.Quaternion(0, 0, 0, 0),
                                new Vector3d(0, 0, 0),
                                0));
                            obj = globalTransformData;
                            break;

                        case 1240:
                            LorePiecesCollectorGsimState.Data loreGsimData = new LorePiecesCollectorGsimState.Data(new Improbable.Collections.List<string>());
                            obj = loreGsimData;
                            break;
                        case 1241:
                            LorePiecesCollectorClientState.Data loreClientData = new LorePiecesCollectorClientState.Data();
                            obj = loreClientData;
                            break;

                        case 1242:
                            LocationState.Data locData = new LocationState.Data(new AbsoluteLocation(new Coordinates(0, 100, 0), new Quaternion(0, 0, 0, 0)),
                                new RelativeLocation(new EntityId(2), new Vector3f(0, 100f, 0), new Quaternion(0, 0, 0, 0)), 0f);
                            obj = locData;
                            break;

                        case 8051:
                            ToolState.Data toolData = new ToolState.Data(new ToolStateData(30));
                            obj = toolData;
                            break;

                        case 8050:
                            ToolRequestState.Data toolRequestData = new ToolRequestState.Data(new ToolRequestStateData());
                            obj = toolRequestData;
                            break;

                        case 6908:
                            ReferenceDataRequestState.Data eeeData = new ReferenceDataRequestState.Data(new ReferenceDataRequestStateData());
                            obj = eeeData;
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
                            obj = referenceData;
                            break;
                        case 1260:
                            SchematicsUnlearnerState.Data susData = new SchematicsUnlearnerState.Data();
                            obj = susData;
                            break;
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

                        // store refId for player and component as we need this to access the component later
                        // this needs to change in the future, we need to make use of the games structures.
                        // noone wants to work with this triple dictionary >.>
                        if (!GameState.Instance.ComponentMap.ContainsKey(player))
                        {
                            GameState.Instance.ComponentMap.Add(player, new Dictionary<long, Dictionary<uint, ulong>> {
                                { entityId, new Dictionary<uint, ulong> {
                                    {
                                        componentId, refId
                                    }
                                } }
                            });
                        }
                        else
                        {
                            if (GameState.Instance.ComponentMap[player].ContainsKey(entityId))
                            {
                                if (GameState.Instance.ComponentMap[player][entityId].ContainsKey(componentId))
                                {
                                    // here we need to decide if we want to update the existing refId with the new one or drop the creation above.
                                    // this case should only happen if the same component is added multiple times to the same entityId and player
                                    GameState.Instance.ComponentMap[player][entityId][componentId] = refId;
                                }
                                else
                                {
                                    GameState.Instance.ComponentMap[player][entityId].Add(componentId, refId);
                                }
                            }
                            else
                            {
                                GameState.Instance.ComponentMap[player].Add(entityId, new Dictionary<uint, ulong> { { componentId, refId } });
                            }
                        }
                    }
                }
            }
        }
    }
}
