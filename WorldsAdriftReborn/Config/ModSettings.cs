﻿using BepInEx;
using BepInEx.Configuration;

namespace WorldsAdriftReborn.Config
{
    internal static class ModSettings
    {
        public static ConfigFile modConfig { get; set; }
        public static ConfigEntry<string> steamUserId { get; set; }
        public static ConfigEntry<string> steamAppId { get; set; }
        public static ConfigEntry<string> steamBranchName { get; set; }
        public static ConfigEntry<string> restServerUrl { get; set; }
        public static ConfigEntry<string> restServerDeploymentUrl { get; set; }
        public static ConfigEntry<string> NTPServerUrl { get; set; }
        public static ConfigEntry<string> localAssetPath { get; set; }
        public static ConfigEntry<string> gameServerHost { get; set; }
        //public static ConfigEntry<string> gameDBUrl { get; set; }

        public static void InitConfig()
        {
            modConfig = new ConfigFile(Paths.ConfigPath + "\\WorldsAdriftReborn.cfg", true);

            steamUserId = modConfig.Bind<string>("Steam",
                                                    "Steam_UserId",
                                                    "steamId",
                                                    "Sets the Steam User ID that the game uses internally. Its not important for the functionality to set this to a specific value.");
            steamAppId = modConfig.Bind<string>("Steam",
                                                    "Steam_AppId",
                                                    "123456789",
                                                    "Sets the Steam App ID that the game uses internally. Its not important for the functionality to set this to a specific value.");
            steamBranchName = modConfig.Bind<string>("Steam",
                                                    "Steam_BranchName",
                                                    "WorldsAdriftRebornBranch",
                                                    "Sets the Steam Branch name that the game uses internally. Its not important for the functionality to set this to a specific value.");

            restServerUrl = modConfig.Bind<string>("REST",
                                                    "REST_ServerUrl",
                                                    "http://127.0.0.1:8080",
                                                    "Sets the URL for the REST server that the game queries once the main menu is reached.");
            restServerDeploymentUrl = modConfig.Bind<string>("REST",
                                                    "REST_ServerDeploymentUrl",
                                                    "http://127.0.0.1:8080/deploymentStatus",
                                                    "Sets the URL for the REST server that the game queries once the main menu is reached. It is the endpoint where server status informations are retrieved from.");

            NTPServerUrl = modConfig.Bind<string>("NTP",
                                                    "NTP_ServerUrl",
                                                    "pool.ntp.org",
                                                    "Set the NTP server that should be used to synchronize time.");

            localAssetPath = modConfig.Bind<string>("AssetLoader",
                                                    "AssetLoader_FilePath",
                                                    "Assets\\",
                                                    "The intermediate part of the Asset folder path. Gets 'unity\\' appended. In some cases the game fails to determine the intermediate path so you can set it here or leave it blank.");

            gameServerHost = modConfig.Bind<string>("GameServer",
                                                    "GameServer_Host",
                                                    "127.0.0.1",
                                                    "The hostname or address of the game server.");
            //gameDBUrl = modConfig.Bind<string>("DBurl", "Url", "https://nichecode.net/clientGameDB.bytes", "gameDBurl");
        }
    }
}
