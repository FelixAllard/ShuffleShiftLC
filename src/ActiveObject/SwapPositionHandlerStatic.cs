using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine;

namespace ShuffleShift.ActiveObject
{
    [StaticNetcode]
    public static class PositionHandler
    {
        private static bool isInsideFactory;
        private static bool isInHangarShipRoom;
        private static int indexOfPick;
        private const float Y_OFFSET = 1f;
        //public static SwapPositionHandler Instance;
        
        // Adjust this value as needed

        public static void ShufflePlayerTransforms()
        {
            List<PlayerControllerB> players = GetAllPlayer();
            Plugin.Logger.LogInfo("SWAPPING POSITION OF " + players.Count.ToString() + " players !");

            // Generate random indexes for shuffling
            int[] possibleIndex = Enumerable.Range(0, players.Count).ToArray();
            Shuffle(possibleIndex);

            // Prepare positions and rotations for teleportation
            List<Vector3> positions = players.Select(player => player.transform.position).ToList();
            List<Quaternion> rotations = players.Select(player => player.transform.rotation).ToList();

            // Execute teleportation and synchronize with all players
            FinalizeTeleportClientRpc(possibleIndex, positions.ToArray(), rotations.ToArray());
        }

        /*public static void SetInstance(SwapPositionHandler swapPositionHandler)
        {
            Instance.DestroyManager();
            Instance = swapPositionHandler;
        }*/

        [ClientRpc]
        private static void FinalizeTeleportClientRpc(int[] indexes, Vector3[] positions, Quaternion[] rotations)
        {
            var allPlayers = RoundManager.Instance.playersManager.allPlayerScripts;
    
            for (int i = 0; i < indexes.Length; i++)
            {
                var playerIndex = indexes[i];
                var player = allPlayers[playerIndex];
        
                if (player.isPlayerControlled && !player.isPlayerDead)
                {
                    // Teleport player
                    var transform = player.transform;
                    
                    positions[i].y += Y_OFFSET;
                    transform.position = positions[i];
                    transform.rotation = rotations[i];

                    // Update player state
                    player.isInHangarShipRoom = isInsideFactory;
                    player.isInHangarShipRoom = isInHangarShipRoom;
                    
                }
            }
    
            // Shake camera for local player
            if (Plugin.ShuffleShiftConfig.ENABLE_SCREEN_SHAKE.Value)
            {
                HUDManager.Instance.DisplayTip("SWAP", $"{indexes.Length} players position got swapped!");
            }
        }


        private static List<PlayerControllerB> GetAllPlayer()
        {
            return RoundManager.Instance.playersManager.allPlayerScripts
                .Where(player => 
                    player.isPlayerControlled && 
                    !player.isPlayerDead && 
                    !player.isClimbingLadder && 
                    !player.inTerminalMenu)
                .ToList();
        }

        private static void Shuffle(int[] array)
        {
            System.Random rng = new System.Random();
            int n = array.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                int temp = array[k];
                array[k] = array[n];
                array[n] = temp;
            }
        }
    }
}
