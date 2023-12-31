using System.Runtime.InteropServices;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game;
using WorldsAdriftRebornGameServer.Game.Entities;
using WorldsAdriftRebornGameServer.Game.Components;
using WorldsAdriftRebornGameServer.Networking.Singleton;
using WorldsAdriftRebornGameServer.Networking.Wrapper;

namespace WorldsAdriftRebornGameServer
{
    internal class WorldsAdriftRebornGameServer
    {
        private static bool keepRunning = true;

        [PInvoke(typeof(EnetLayer.ENet_Poll_Callback))]
        private static unsafe void OnNewClientConnected( IntPtr peer )
        {
            ENetPeerHandle ePeer = new ENetPeerHandle(peer, new ENetHostHandle());
            if (!ePeer.IsInvalid)
            {
                Console.WriteLine("[info] got a connection.");
                PeerManager.Instance.playerState.Add(ePeer,
                    new Dictionary<int, PlayerSyncStatus> { { 0, new PlayerSyncStatus() } });
            }
        }

        [PInvoke(typeof(EnetLayer.ENet_Poll_Callback))]
        private unsafe static void OnClientDisconnected( IntPtr peer )
        {
            ENetPeerHandle ePeer = new ENetPeerHandle(peer, new ENetHostHandle());
            if (!ePeer.IsInvalid)
            {
                Console.WriteLine("[info] a client disconnected.");
            }
        }

        private static readonly EnetLayer.ENet_Poll_Callback callbackC =
            new EnetLayer.ENet_Poll_Callback(OnNewClientConnected);

        private static readonly EnetLayer.ENet_Poll_Callback callbackD =
            new EnetLayer.ENet_Poll_Callback(OnClientDisconnected);

        private static List<long> playerEntityIDs = new List<long>();

        private static int IslandsCount = 4;
        private static List<string> IslandNames;
        private static string IslandsPath = "D:\\Steam\\steamapps\\common\\WorldsAdrift\\Assets\\unity";
        public static long TestPlayerId => EntityManager.NextEntityId;


        public static unsafe void Main( string[] args )
        {
            IslandNames = Directory.GetFiles(IslandsPath)
                                       .Where(fp => !fp.EndsWith(".manifset"))
                                       .Select(fp =>
                                           Path.GetFileNameWithoutExtension(fp).Replace("@island_unityclient", ""))
                                       .ToList();

            if (IslandsCount > IslandNames.Count)
                IslandsCount = IslandNames.Count;

            Console.CancelKeyPress += delegate
            {
                keepRunning = false;
            };

            if (EnetLayer.ENet_Initialize() < 0)
            {
                Console.WriteLine("ERROR - ENet Failed to initialize, server will not run.");
                return;
            }

            ENetHostHandle server = EnetLayer.ENet_Create_Host(7777, 1, 5, 0, 0);

            if (server.IsInvalid)
            {
                EnetLayer.ENet_Deinitialize(new IntPtr(0));
                Console.WriteLine("ERROR - Unable to create host & listen for incoming data, server will not run.");
                return;
            }

            Console.WriteLine("INFO - ENet Started, waiting for connections and data.");
            PeerManager.Instance.SetENetHostHandle(server);

            // define initial world state for first chunk
            GameState.Instance.WorldState[0] = new List<SyncStep>()
            {
                new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, SyncStepLoadPlayer),
                new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, SyncStepLoadIslands),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, SyncStepAddIslands),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, SyncStepAckIsland)
            };

            while (keepRunning)
            {
                EnetLayer.ENetPacket_Wrapper* packet = EnetLayer.ENet_Poll(server, 50,
                    Marshal.GetFunctionPointerForDelegate(callbackC), Marshal.GetFunctionPointerForDelegate(callbackD));

                if (packet == null)
                {
                    SyncPlayers();
                    continue;
                }
                
                SyncHighPriority(packet);
                SyncLowPriority(packet);

                EnetLayer.ENet_Destroy_Packet(new IntPtr(packet));
                SyncPlayers();
            }

            server.Dispose();

            Console.WriteLine("INFO - Shutting down");
        }

        #region Packet Processing
        private static unsafe void SyncHighPriority(EnetLayer.ENetPacket_Wrapper* packet)
        {
            // Work on packets that are relevant to progress in sync state
            foreach (KeyValuePair<ENetPeerHandle, Dictionary<int, PlayerSyncStatus>> keyValuePair in PeerManager.Instance.playerState)
            {
                int currentChunkIndex = 0;
                int currentPlayerSyncIndex = PeerManager.Instance.playerState[keyValuePair.Key][currentChunkIndex].SyncStepPointer;
                if (currentPlayerSyncIndex == GameState.Instance.WorldState[currentChunkIndex].Count - 1)
                {
                    // this player is synced
                    continue;
                }
                GameState.NextStateRequirement nextStateRequirement =
                    GameState.Instance.WorldState[currentChunkIndex][currentPlayerSyncIndex].NextStateRequirement;
                switch (packet->Channel)
                {
                    case (int)EnetLayer.ENetChannel.ASSET_LOAD_REQUEST_OP when
                        nextStateRequirement == GameState.NextStateRequirement.ASSET_LOADED_RESPONSE:
                    case (int)EnetLayer.ENetChannel.ADD_ENTITY_OP when nextStateRequirement ==
                                                                       GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE:
                        // for now set it for every client, but we need to distinguish them by their userData field
                        PeerManager.Instance.playerState[keyValuePair.Key][currentChunkIndex].SyncStepPointer++;
                        break;
                }
            }
        }

        private static unsafe void SyncLowPriority(EnetLayer.ENetPacket_Wrapper* packet)
        {
            foreach (KeyValuePair<ENetPeerHandle, Dictionary<int, PlayerSyncStatus>> keyValuePair in PeerManager
                         .Instance.playerState)
            {
                switch (packet->Channel)
                {
                    case (int)EnetLayer.ENetChannel.SEND_COMPONENT_INTEREST:
                    {
                        long entityId = 0;
                        uint interestCount = 0;
                        Structs.Structs.InterestOverride* interests =
                            (Structs.Structs.InterestOverride*)new IntPtr(0);

                        if (!EnetLayer.PB_EXP_SendComponentInterest_Deserialize(packet->Data,
                                (int)packet->DataLength, &entityId, &interests, &interestCount))
                        {
                            Console.WriteLine("ERROR -Failed to deserialize ComponentInterest message from game.");
                            break;
                        }
                        
                        Console.WriteLine("[info] game requests components for entity id: " + entityId);

                        long entityIdToCheck = entityId; // Assuming entityId is a long

                        bool isEntityInPlayerList =
                            playerEntityIDs.Any(playerEntity => playerEntity == entityIdToCheck);
                        bool isPeerNotInClientSetupState =
                            !PeerManager.Instance.clientSetupState.Contains(keyValuePair.Key);

                        if (isEntityInPlayerList && isPeerNotInClientSetupState)
                        {
                            // a player entity requests components for the first time, we need to setup a few things to make him work properly
                            // some of this might not be needd anymore in the future once we sorted out a few things.
                            //
                            // we can make use of the fact that the game requests components for players in two stages, where the second one will terminate the loading screen of the client.
                            // the second stage needs a few components setup properly, for this we need to inject one component and call auth changed for a few others once.

                            // some components are needed in the first stage and need to be injected.
                            // Send PlayerExternalDataVisualizer required components
                            List<Structs.Structs.InterestOverride> injectedEarly =
                                new uint[] { 1207, 1077, 1109 }
                                    .Select(p => new Structs.Structs.InterestOverride(p, 1)).ToList();
                            if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injectedEarly,
                                    true))
                            {
                                continue;
                            }

                            // then send what the game requested
                            if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, interests,
                                    interestCount, true))
                            {
                                continue;
                            }

                            // TODO: Document these
                            // 1242 - LocationState - Might only be relevant for OTHER players! See RelativeLocationVisualizer for more info.
                            List<uint> authoritativeComponents = new List<uint>
                            {
                                1073,
                                1040,
                                1080,
                                8050,
                                8051,
                                6908,
                                1260,
                                1097,
                                1003,
                                1241,
                                1082,
                                1211,
                                1212,
                                2105,
                                2106,
                                2002
                            }; //190602};  // 1073};
                            List<uint> authoritativeComponentsClearAfter = new List<uint> { 1212 };

                            // for some reason the game does not always request component 1080 (SchematicsLearnerGSimState), but its reader is required in InventoryVisualiser
                            List<Structs.Structs.InterestOverride> injected = authoritativeComponents
                                                                              .Select(p => new Structs.Structs.InterestOverride(p, 1)).ToList();

                            if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injected, true))
                            {
                                continue;
                            }

                            // now send auth change
                            if (!SendOPHelper.SendAuthorityChangeOp(keyValuePair.Key, entityId,
                                    authoritativeComponents))
                            {
                                continue;
                            }

                            // clear auth on select components. some need temporary authority to initialise?
                            if (!SendOPHelper.SendAuthorityChangeOp(keyValuePair.Key, entityId,
                                    authoritativeComponentsClearAfter, false))
                            {
                                continue;
                            }

                            // now add player to clientSetupState
                            PeerManager.Instance.clientSetupState.Add(keyValuePair.Key);
                        }
                        else
                        {
                            // player already setup or another entity requested components, so just process them
                            if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, interests,
                                    interestCount, true))
                            {
                                continue;
                            }
                        }
                        

                        break;
                    }
                    case (int)EnetLayer.ENetChannel.COMPONENT_UPDATE_OP:
                    {
                        long entityId = 0;
                        uint updateCount = 0;
                        Structs.Structs.ComponentUpdateOp* update =
                            (Structs.Structs.ComponentUpdateOp*)new IntPtr(0);

                        if (!EnetLayer.PB_EXP_ComponentUpdateOp_Deserialize(packet->Data, (int)packet->DataLength, &entityId, &update, &updateCount) || updateCount <= 0)
                        {
                            Console.WriteLine("ERROR - Failed to deserialize ComponentUpdate message from game, or empty message.");
                            break;
                        }
                        
                        for (int i = 0; i < updateCount; i++)
                        {
                            var componentId = update[i].ComponentId;
                            ComponentStateManager.Instance.HandleComponentUpdate(keyValuePair.Key, entityId, componentId, update[i].ComponentData, update[i].DataLength);
                            
                            if (componentId is 1073)
                                continue; // You can block more very easily : `componentId is 1073 or 1074 or 1075 or 1075 ...`
                            
                            Console.WriteLine("INFO - Client " + entityId + " requested ComponentUpdate " + componentId);
                        }
                        
                        break;
                    }
                }
            }
        }
        
        // dont wait for GetOplist and then for the Dispatch call as we are the ones who would dispatch the work anyways.
        // sync up players
        private static void SyncPlayers()
        {
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
        
        #endregion
        #region Sync Steps

        #region Asset Loading

        private static void SyncStepLoadPlayer( object peer )
        {
            var result = SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)peer, "notNeeded?", "Traveller", "Player");
            if (result) return;
            Console.WriteLine("ERROR - Unable to send Player AssetLoadRequest");
        }

        private static void SyncStepLoadIslands( object peer )
        {
            var failed = new List<string>();
            for (var i = 0; i < IslandsCount; i++)
            {
                var islandName = IslandNames[i];
                var result = SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)peer, "notNeeded?",
                    islandName + "@Island", "notNeeded?");
                if (result)
                    continue;
                failed.Add(islandName);
            }

            if (failed.Count == 0)
                return;
            Console.WriteLine("ERROR - Unable to send Island AssetLoadRequest for: " + string.Join(", ", failed));
        }

        #endregion

        #region Entity Creation

        // TODO: Refactor to SyncStepAddEntities<T> for globally loaded entities, possibly using some other utility
        private static void SyncStepAddIslands( object peer )
        {
            long entityId = EntityManager.NextEntityId;
            List<string> failed = new List<string>();
            bool generateLocations = EntityManager.islandEntities.Count == 0;
            for (int i = 0; i < IslandsCount; i++)
            {
                string islandName = IslandNames[i];
                if (generateLocations)
                {
                    var pos = i != 0
                        ? new Improbable.Collections.List<long> { i * 10000000, i * 10000000, 0 }
                        : new Improbable.Collections.List<long> { 0, 0, 0 };

                    IslandEntities newIsland = new IslandEntities();
                    newIsland.EntityId = entityId;
                    newIsland.Name = islandName;
                    newIsland.Position = pos;
                    EntityManager.islandEntities.Add(newIsland);

                    if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)peer, entityId, islandName + "@Island", "notNeeded?"))
                    {
                        Console.WriteLine("[ISLAND] " + islandName + " added with entity " + entityId);
                        entityId = EntityManager.NextEntityId;
                        continue;
                    }
                }
                failed.Add(islandName);
            }

            if (failed.Count == 0)
                return;
            Console.WriteLine("ERROR - Unable to send Island AddEntity for: " + string.Join(", ", failed));
        }

        private static void SyncStepAckIsland( object peer )
        {            
            var nextId = TestPlayerId; // NextEntityId
            // client ack'ed island spawning instruction (info by sdk, does not mean it truly spawned).
            Console.WriteLine("INFO - Requesting to spawn player " + nextId);

            // TODO: Add player entity to Entity Manager / Player Manager
            playerEntityIDs.Add(nextId);
            if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)peer, nextId, "Traveller", "Player")) return;
        
            Console.WriteLine("ERROR - Unable to send Player AddEntity for: " + nextId);
        }

        #endregion

        #endregion
    }
}
