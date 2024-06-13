using GameNetcodeStuff;
using HarmonyLib;
using ShuffleShift.ActiveObject;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShuffleShift.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal class RoundManagerPatch
{
    [HarmonyPatch("UnloadSceneObjectsEarly")]
    [HarmonyPostfix]
    private static void UnloadSceneObjectEarly(RoundManager __instance)
    {
        foreach (var swapPositionHandler in GameObject.FindObjectsOfType<SwapPositionHandler>())
        {
            if (swapPositionHandler != null)
            {
                if (swapPositionHandler.swapCoroutine != null)
                {
                    swapPositionHandler.StopCoroutine(swapPositionHandler.swapCoroutine);
                }
                Object.Destroy(swapPositionHandler.gameObject);
            }
        }
    }
    [HarmonyPatch(nameof(StartOfRound.Instance.StartGame))]
    [HarmonyPostfix]
    private static void PostfixSpawn()
    {
        foreach (var swapPositionHandler in GameObject.FindObjectsOfType<SwapPositionHandler>())
        {
            if (swapPositionHandler != null)
            {
                if (swapPositionHandler.swapCoroutine != null)
                {
                    swapPositionHandler.StopCoroutine(swapPositionHandler.swapCoroutine);
                }
                Object.Destroy(swapPositionHandler.gameObject);
            }
        }
        if (RoundManager.Instance.IsHost)
        {
            // Specify the scene to search in (replace "YourSceneName" with the actual scene name)
            Scene targetScene = SceneManager.GetSceneByName("SampleSceneRelay");

            if (targetScene.IsValid())
            {
                // Ensure the scene is loaded
                if (targetScene.isLoaded)
                {
                    // Get all root game objects in the specified scene
                    GameObject[] rootObjects = targetScene.GetRootGameObjects();
                    bool shuffleSwapHandlerExists = false;

                    // Check if "ShuffleSwapHandler" exists in the specified scene
                    foreach (GameObject obj in rootObjects)
                    {
                        if (obj.name == "ShuffleSwapHandler")
                        {
                            shuffleSwapHandlerExists = true;
                            break;
                        }
                    }

                    // If SwapPositionHandler.Instance is null and ShuffleSwapHandler does not exist, create it
                    if (SwapPositionHandler.Instance == null && !shuffleSwapHandlerExists)
                    {
                        GameObject manager = new GameObject("ShuffleSwapHandler");
                        manager.AddComponent<SwapPositionHandler>();

                        // Optionally, set the manager to the specified scene
                        SceneManager.MoveGameObjectToScene(manager, targetScene);
                    }
                }
            }
        }

    }
}