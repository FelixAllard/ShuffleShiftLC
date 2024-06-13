using GameNetcodeStuff;
using HarmonyLib;
using ShuffleShift.ActiveObject;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShuffleShift.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatch
{
    [HarmonyPatch(nameof(StartOfRound.Instance.EndGameServerRpc))]
    [HarmonyPostfix]
    private static void UnloadSceneObjectEarly(RoundManager __instance)
    {
        if (SwapPositionHandler.Instance != null)
        {
            SwapPositionHandler.Instance.DestroyManager();
        }
    }
    [HarmonyPatch(nameof(StartOfRound.Instance.StartGame))]
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