using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using BepInEx;
using BepInEx.Logging;
using System.IO;
using GameNetcodeStuff;
using HarmonyLib;
using ShuffleShift.Configurations;
using ShuffleShift.Patches;

namespace ShuffleShift {
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(StaticNetcodeLib.StaticNetcodeLib.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger = null!;
        public static AssetBundle? ModAssets;
        private readonly Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static new Config ShuffleShiftConfig { get; internal set; }
        
        private void Awake() {
            Logger = base.Logger;
            ShuffleShiftConfig = new(base.Config);
            Logger.LogInfo("Loading : " + PluginInfo.PLUGIN_GUID + " Version : " + PluginInfo.PLUGIN_VERSION);
            harmony.PatchAll(typeof(ConfigurationsPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            Logger.LogInfo("Patch applied and ready to play!");
        }
    }
}