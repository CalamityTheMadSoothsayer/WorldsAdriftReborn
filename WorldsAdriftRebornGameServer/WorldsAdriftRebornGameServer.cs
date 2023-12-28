﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Improbable.Worker;
using WorldsAdriftRebornGameServer.DLLCommunication;
using WorldsAdriftRebornGameServer.Game;
using WorldsAdriftRebornGameServer.Game.Components;
using WorldsAdriftRebornGameServer.Game.Components.Update;
using WorldsAdriftRebornGameServer.Networking.Singleton;
using WorldsAdriftRebornGameServer.Networking.Wrapper;
using WorldsAdriftRebornGameServer.Models;
using static WorldsAdriftRebornGameServer.DLLCommunication.EnetLayer;
using System.Net.NetworkInformation;

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
        private static readonly List<uint> authoritativeComponents = new List<uint>{ 8050, 8051, 6908, 1260, 1097, 1003, 1241, 1082};
        public static List<PlayerEntity> playerEntityIDs = new List<PlayerEntity>();
        public static List<IslandEntity> islandEntityIDs = new List<IslandEntity>();
        private static List<string> islands = new List<string>();
        private static int islandsToLoad = 3;
        private static List<Vector3> locationList = new List<Vector3>();

        private static long nextEntityId = 0;
        public static long NextEntityId
        {
            get
            {
                return nextEntityId++;
            }
        }
        
        static unsafe void Main( string[] args )
        {
            locationList.Add(new Vector3(1000000, 1000000, 0));
            locationList.Add(new Vector3(0, 0, 0));
            locationList.Add(new Vector3(3000000, 10000, 0));

            var path = "C:\\Users\\s3v3r\\Desktop\\dd\\depots\\322783\\4112968\\Assets\\unity";


            var files = Directory.GetFiles(path)
                .Where(file => !file.EndsWith(".manifest"))
                .Select(file =>
                {
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    var strippedFileName = fileNameWithoutExtension.EndsWith("@island_unityclient")
                        ? fileNameWithoutExtension.Substring(0, fileNameWithoutExtension.Length - "@island_unityclient".Length)
                        : fileNameWithoutExtension;
                    return new { OriginalFile = file, StrippedFileName = strippedFileName };
                });

            foreach (var file in files)
            {
                islands.Add(file.StrippedFileName);
            }

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
                        Console.WriteLine("[info] successfully serialized and queued AssetLoadRequestOp for PLAYER.");
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AssetLoadRequestOp for PLAYER.");
                    }
                })),
                new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, new Action<object>((object o) =>
                {
                    Console.WriteLine("[info] requesting the game to load the island from its asset bundles...");

                    var i = 0;
                    foreach(var name in islands) {

                        if (SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)o, "notNeeded?", name + "@Island", "notNeeded?"))
                        {
                            Console.WriteLine("[info] successfully serialized and queued AssetLoadRequestOp for ISLAND.");
                        }
                        else
                        {
                            Console.WriteLine("[error] failed to serialize and queue AssetLoadRequestOp for ISLAND.");
                        }
                        i++;
                        if(i == islandsToLoad)
                        {
                            break;
                        }
                    }
                })),
                //new SyncStep(GameState.NextStateRequirement.ASSET_LOADED_RESPONSE, new Action<object>((object o) =>
                //{
                //    Console.WriteLine("[info] requesting the game to load the respawner from its asset bundles...");

                //    var i = 0;
                //    foreach(var name in islands) {

                //        if (SendOPHelper.SendAssetLoadRequestOP((ENetPeerHandle)o, "6905", "AncientRespawner", "6905"))
                //        {
                //            Console.WriteLine("[info] successfully serialized and queued AssetLoadRequestOp.");
                //        }
                //        else
                //        {
                //            Console.WriteLine("[error] failed to serialize and queue AssetLoadRequestOp.");
                //        }
                //        i++;
                //        if(i == islandsToLoad)
                //        {
                //            break;
                //        }
                //    }
                //})),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, new Action<object>((object o) =>
                {
                    Console.WriteLine("[success] island asset loaded. requesting loading of island...");
                    var i = 0;
                    foreach (var name in islands)
                    {
                        // Instantiate an IslandEntity object instead of adding a long to the list
                        var islandEntity = new IslandEntity { EntityId = NextEntityId, Name = name, Position = locationList[i] };

                        if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)o, islandEntity.EntityId, islandEntity.Name + "@Island", "notNeeded?"))
                        {
                            Console.WriteLine("[info] successfully serialized and queued AddEntityOp for ISLAND. [ID] " + islandEntity.EntityId);
                            // Add the IslandEntity object to the list
                            islandEntityIDs.Add(islandEntity);
                        }
                        else
                        {
                            Console.WriteLine("[error] failed to serialize and queue AddEntityOp for ISLAND.");
                        }

                        i++;
                        if (i == islandsToLoad)
                        {
                            break;
                        }
                    }
                })),
                //new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, new Action<object>((object o) =>
                //{
                //    Console.WriteLine("[success] respawner asset loaded. requesting loading of respawner...");

                //        if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)o, nextEntityId, "AncientRespawner", "6905"))
                //        {
                //            Console.WriteLine("[info] successfully serialized respawner and queued AddEntityOp.");
                //        }
                //        else
                //        {
                //            Console.WriteLine("[error] failed to serialize respawner and queue AddEntityOp.");
                //        }

                //})),
                new SyncStep(GameState.NextStateRequirement.ADDED_ENTITY_RESPONSE, new Action<object>((object o) =>
                {
                    Console.WriteLine("[info] client ack'ed island spawning instruction (info by sdk, does not mean it truly spawned). requesting to spawn player...");

                    // Instantiate a new PlayerEntity object
                    var playerEntity = new PlayerEntity { EntityId = NextEntityId };

                    // Add the PlayerEntity object to the list
                    playerEntityIDs.Add(playerEntity);

                    // Send the AddEntityOp using the EntityId from the PlayerEntity object
                    if (SendOPHelper.SendAddEntityOP((ENetPeerHandle)o, playerEntity.EntityId, "Traveller", "Player"))
                    {
                        Console.WriteLine("[info] successfully serialized and queued AddEntityOp for PLAYER. [ID] " + playerEntity.EntityId);
                    }
                    else
                    {
                        Console.WriteLine("[error] failed to serialize and queue AddEntityOp for PLAYER.");
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

                                long entityIdToCheck = entityId; // Assuming entityId is a long

                                bool isEntityInPlayerList = playerEntityIDs.Any(playerEntity => playerEntity.EntityId == entityIdToCheck);
                                bool isPeerNotInClientSetupState = !PeerManager.Instance.clientSetupState.Contains(keyValuePair.Key);

                                if (isEntityInPlayerList && isPeerNotInClientSetupState)
                                {
                                    // a player entity requests components for the first time, we need to setup a few things to make him work properly
                                    // some of this might not be needd anymore in the future once we sorted out a few things.
                                    //
                                    // we can make use of the fact that the game requests components for players in two stages, where the second one will terminate the loading screen of the client.
                                    // the second stage needs a few components setup properly, for this we need to inject one component and call auth changed for a few others once.

                                    // some components are needed in the first stage and need to be injected.
                                    // we also need PilotState since schematics for glider where added, as the game nullrefs in PlayerExternalDataVisualizer.IsDriving() now (1109)
                                    List<Structs.Structs.InterestOverride> injectedEarly = new List<Structs.Structs.InterestOverride> { new Structs.Structs.InterestOverride(1109, 1) };

                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injectedEarly, true))
                                    {
                                        continue;
                                    }

                                    // then send what the game requested
                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, interests, interestCount, true))
                                    {
                                        continue;
                                    }

                                    // for some reason the game does not always request component 1080 (SchematicsLearnerGSimState), but its reader is required in InventoryVisualiser
                                    List<Structs.Structs.InterestOverride> injected = new List<Structs.Structs.InterestOverride> { new Structs.Structs.InterestOverride(1080, 1) };
                                    // also inject other required components for the inventory
                                    injected.AddRange(authoritativeComponents.Select(p => new Structs.Structs.InterestOverride(p, 1)));

                                    if (!SendOPHelper.SendAddComponentOp(keyValuePair.Key, entityId, injected, true))
                                    {
                                        continue;
                                    }

                                    // now send auth change
                                    if(!SendOPHelper.SendAuthorityChangeOp(keyValuePair.Key, entityId, authoritativeComponents))
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
                                Console.WriteLine("[info] game requests " + updateCount + " ComponentUpdate's for entity id " + entityId);

                                for(int i = 0; i < updateCount; i++)
                                {
                                    ComponentUpdateManager.Instance.HandleComponentUpdate(keyValuePair.Key, entityId, update[i].ComponentId, update[i].ComponentData, update[i].DataLength);
                                }
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
