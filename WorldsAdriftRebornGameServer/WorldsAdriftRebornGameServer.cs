using System.Runtime.InteropServices;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game;
using WorldsAdriftRebornGameServer.Game.Components.Update;
using WorldsAdriftRebornGameServer.Networking.Singleton;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer
{
    internal class WorldsAdriftRebornGameServer
    {
        private static bool keepRunning = true;
        [PInvoke(typeof(EnetLayer.ENet_Poll_Callback))]
        private unsafe static void OnNewClientConnected(IntPtr peer )
        {
            ENetPeerHandle ePeer = new ENetPeerHandle(peer, new ENetHostHandle());
            if (!ePeer.IsInvalid)
            {
                Console.WriteLine("[info] got a connection.");
                PeerManager.Instance.playerState.Add(ePeer, new Dictionary<int, PlayerSyncStatus> { { 0, new PlayerSyncStatus() } });
            }
        }
        [PInvoke(typeof(EnetLayer.ENet_Poll_Callback))]
        private unsafe static void OnClientDisconnected(IntPtr peer )
        {
            ENetPeerHandle ePeer = new ENetPeerHandle(peer, new ENetHostHandle());
            if (!ePeer.IsInvalid)
            {
                Console.WriteLine("[info] a client disconnected.");
            }
        }

        private static readonly EnetLayer.ENet_Poll_Callback callbackC = new EnetLayer.ENet_Poll_Callback(OnNewClientConnected);
        private static readonly EnetLayer.ENet_Poll_Callback callbackD = new EnetLayer.ENet_Poll_Callback(OnClientDisconnected);
        private static List<long> playerEntityIDs = new List<long>();

        // TODO: Replace with some sort of Entity Manager system. Some code in ComponentsSerializer.cs is hardcoded for
        // TODO: Player=1 and Island=2, so make sure to fix that before removing below
        public const long TestPlayerId = 1;
        public const long TestIslandId = 2;
        private static long nextEntityId = 2;
        public static long NextEntityId
        {
            get
            {
                return ++nextEntityId;
            }
        }
        
        static unsafe void Main( string[] args )
        {
            Console.CancelKeyPress += delegate ( object? sender, ConsoleCancelEventArgs e )
            {
                keepRunning = false;
            };

            if (EnetLayer.ENet_Initialize() < 0)
            {
                Console.WriteLine("[error] failed to initialize ENet.");
                return;
            }

            Console.WriteLine("[info] successfully initialized ENet.");
            ENetHostHandle server = EnetLayer.ENet_Create_Host(7777, 1, 5, 0, 0);

            if (server.IsInvalid)
            {
                Console.WriteLine("[error] failed to create host and listen on network interface.");

                EnetLayer.ENet_Deinitialize(new IntPtr(0));
                return;
            }

            Console.WriteLine("[info] successfully initialized networking, now waiting for connections and data.");
            PeerManager.Instance.SetENetHostHandle(server);

            // define initial world state for first chunk
            GameState.Instance.WorldState[0] = new List<SyncStep>()
            {
                new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, new Action<object>((object o) =>
                {
                    Console.WriteLine("[info] requesting the game to load the player asset...");

                    if (SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)o, "notNeeded?", "Traveller", "Player"))
                    {
                        Console.WriteLine("[info] successfully serialized and queued AssetLoadRequestOp.");
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AssetLoadRequestOp.");
                    }
                })),
                new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, new Action<object>((object o) =>
                {
                    Console.WriteLine("[info] requesting the game to load the island from its asset bundles...");

                    if (SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)o, "notNeeded?", "949069116@Island", "notNeeded?"))
                    {
                        Console.WriteLine("[info] successfully serialized and queued AssetLoadRequestOp.");
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AssetLoadRequestOp.");
                    }
                })),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, new Action<object>((object o) =>
                {
                    var nextId = TestIslandId;  // NextEntityId
                    Console.WriteLine($"[success] island asset loaded. requesting loading of island (ID={nextId}...");

                    if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)o, nextId, "949069116@Island", "notNeeded?"))
                    {
                        Console.WriteLine("[info] successfully serialized and queued AddEntityOp.");
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AddEntityOp.");
                    }
                })),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, new Action<object>((object o) =>
                {
                    var nextId = TestPlayerId;  // NextEntityId
                    Console.WriteLine($"[info] client ack'ed island spawning instruction (info by sdk, does not mean it truly spawned). requesting to spawn player (ID={nextId})...");

                    playerEntityIDs.Add(nextId);
                    if(SendOPHelper.SendAddEntityOP((ENetPeerHandle)o, nextId, "Traveller", "Player"))
                    {
                        Console.WriteLine("[info] successfully serialized and queued AddEntityOp.");
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AddEntityOp.");
                    }
                }))
            };

            while (keepRunning)
            {
                EnetLayer.ENetPacket_Wrapper* packet = EnetLayer.ENet_Poll(server, 50, Marshal.GetFunctionPointerForDelegate(callbackC), Marshal.GetFunctionPointerForDelegate(callbackD));
                if(packet != null)
                {
                    // work on packets that are relevant to progress in sync state
                    foreach (KeyValuePair<ENetPeerHandle, Dictionary<int, PlayerSyncStatus>> keyValuePair in PeerManager.Instance.playerState)
                    {
                        int currentChunkIndex = 0;
                        int currentPlayerSyncIndex = PeerManager.Instance.playerState[keyValuePair.Key][currentChunkIndex].SyncStepPointer;

                        if (currentPlayerSyncIndex == GameState.Instance.WorldState[currentChunkIndex].Count - 1)
                        {
                            // this player is synced
                            continue;
                        }

                        GameState.NextStateRequirement nextStateRequirement = GameState.Instance.WorldState[currentChunkIndex][currentPlayerSyncIndex].NextStateRequirement;

                        if(packet->Channel == (int)EnetLayer.ENetChannel.ASSET_LOAD_REQUEST_OP && nextStateRequirement == GameState.NextStateRequirement.ASSET_LOADED_RESPONSE)
                        {
                            // for now set it for every client, but we need to distinguish them by their userData field
                            PeerManager.Instance.playerState[keyValuePair.Key][currentChunkIndex].SyncStepPointer++;
                        }
                        else if(packet->Channel == (int)EnetLayer.ENetChannel.ADD_ENTITY_OP && nextStateRequirement == GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE)
                        {
                            PeerManager.Instance.playerState[keyValuePair.Key][currentChunkIndex].SyncStepPointer++;
                        }
                    }

                    // work on packets that are not relevant for progress of sync state but need processing for any player
                    foreach (KeyValuePair<ENetPeerHandle, Dictionary<int, PlayerSyncStatus>> keyValuePair in PeerManager.Instance.playerState)
                    {
                        if (packet->Channel == (int)EnetLayer.ENetChannel.SEND_COMPONENT_INTEREST)
                        {
                            long entityId = 0;
                            uint interestCount = 0;
                            Structs.Structs.InterestOverride* interests = (Structs.Structs.InterestOverride*)new IntPtr(0);

                            if (EnetLayer.PB_EXP_SendComponentInterest_Deserialize(packet->Data, (int)packet->DataLength, &entityId, &interests, &interestCount))
                            {
                                Console.WriteLine("[info] game requests components for entity id: " + entityId);

                                if(playerEntityIDs.Contains(entityId) && !PeerManager.Instance.clientSetupState.Contains(keyValuePair.Key))
                                {
                                    // a player entity requests components for the first time, we need to setup a few things to make him work properly
                                    // some of this might not be needd anymore in the future once we sorted out a few things.
                                    //
                                    // we can make use of the fact that the game requests components for players in two stages, where the second one will terminate the loading screen of the client.
                                    // the second stage needs a few components setup properly, for this we need to inject one component and call auth changed for a few others once.

                                    // some components are needed in the first stage and need to be injected.
                                    // Send PlayerExternalDataVisualizer required components
                                    List<Structs.Structs.InterestOverride> injectedEarly = new uint[] {1207, 1077, 1109}.Select(p => new Structs.Structs.InterestOverride(p, 1)).ToList();
                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injectedEarly, true))
                                    {
                                        continue;
                                    }

                                    // then send what the game requested
                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, interests, interestCount, true))
                                    {
                                        continue;
                                    }
                                    
                                    // TODO: Document these
                                    // 1242 - LocationState - Might only be relevant for OTHER players! See RelativeLocationVisualizer for more info.
                                    List<uint> authoritativeComponents = new List<uint>{ 1073, 1040, 1080, 8050, 8051, 6908, 1260, 1097, 1003, 1241, 1082, 1211, 1212, 2105, 2106, 2002, 1242}; //190602};  // 1073};
                                    List<uint> authoritativeComponentsClearAfter = new List<uint>{ 1212, 1242 };
        
                                    // for some reason the game does not always request component 1080 (SchematicsLearnerGSimState), but its reader is required in InventoryVisualiser
                                    List<Structs.Structs.InterestOverride> injected = authoritativeComponents.Select(p => new Structs.Structs.InterestOverride(p, 1)).ToList();

                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injected, true))
                                    {
                                        continue;
                                    }

                                    // now send auth change
                                    if(!SendOPHelper.SendAuthorityChangeOp(keyValuePair.Key, entityId, authoritativeComponents))
                                    {
                                        continue;
                                    }
                                    // clear auth on select components. some need temporary authority to initialise?
                                    if(!SendOPHelper.SendAuthorityChangeOp(keyValuePair.Key, entityId, authoritativeComponentsClearAfter, false))
                                    {
                                        continue;
                                    }

                                    // now add player to clientSetupState
                                    PeerManager.Instance.clientSetupState.Add(keyValuePair.Key);
                                }
                                else
                                {
                                    // player already setup or another entity requested components, so just process them
                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, interests, interestCount, true))
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("[error] failed to deserialize ComponentInterest message from game.");
                            }
                        }
                        else if(packet->Channel == (int)EnetLayer.ENetChannel.COMPONENT_UPDATE_OP)
                        {
                            long entityId = 0;
                            uint updateCount = 0;
                            Structs.Structs.ComponentUpdateOp* update = (Structs.Structs.ComponentUpdateOp*)new IntPtr(0);

                            if (EnetLayer.PB_EXP_ComponentUpdateOp_Deserialize(packet->Data, (int)packet->DataLength, &entityId, &update, &updateCount) && updateCount > 0)
                            {
                                var skipLog = false;
                                for(int i = 0; i < updateCount; i++)
                                {
                                    if (updateCount == 1 && update[i].ComponentId == 1073) skipLog = true;
                                    ComponentUpdateManager.Instance.HandleComponentUpdate(keyValuePair.Key, entityId, update[i].ComponentId, update[i].ComponentData, update[i].DataLength);
                                }
                                if (!skipLog) Console.WriteLine("[info] game requested " + updateCount + " ComponentUpdate's for entity id " + entityId);
                            }
                            else
                            {
                                Console.WriteLine("[error] failed to deserialize ComponentUpdate message from game, or empty message.");
                            }
                        }
                    }

                    EnetLayer.ENet_Destroy_Packet(new IntPtr(packet));
                }

                // dont wait for GetOplist and then for the Dispatch call as we are the ones who would dispatch the work anyways.
                // sync up players
                foreach (KeyValuePair<ENetPeerHandle, Dictionary<int, PlayerSyncStatus>> keyValuePair in PeerManager.Instance.playerState)
                {
                    int currentChunkIndex = 0;
                    PlayerSyncStatus pStatus = keyValuePair.Value[currentChunkIndex];
                    SyncStep step = GameState.Instance.WorldState[currentChunkIndex][pStatus.SyncStepPointer];

                    if (!pStatus.Performed)
                    {
                        step.Step(keyValuePair.Key);
                        pStatus.Performed = true;
                    }
                }
            }

            server.Dispose();

            Console.WriteLine("[info] shutting down.");
        }
    }
}
