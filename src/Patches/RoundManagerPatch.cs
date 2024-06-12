using GameNetcodeStuff;
using HarmonyLib;
using ShuffleShift.ActiveObject;
using UnityEngine;

namespace ShuffleShift.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal class RoundManagerPatch
{
    [HarmonyPatch("UnloadSceneObjectsEarly")]
    [HarmonyPostfix]
    private static void PostFixStart(RoundManager __instance)
    {
        if (SwapPositionHandler.Instance != null)
        {
            SwapPositionHandler.Instance.DestroyManagerClientRpc();
        }
    }
    [HarmonyPatch(nameof(RoundManager.BeginEnemySpawning))]
    [HarmonyPostfix]
    private static void PostfixSpawn()
    {
        if (SwapPositionHandler.Instance == null)
        {
            GameObject manager = new GameObject("ShuffleSwapHandler");
            manager.AddComponent<SwapPositionHandler>();
        }
    }
}