using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameNetcodeStuff;
using StaticNetcodeLib;
using Unity.Netcode;
using UnityEngine;

namespace ShuffleShift.ActiveObject
{
    [StaticNetcode]
    public static class PositionHandler
    {
        private static SwapPositionHandler _instance;
        public static SwapPositionHandler Instance => _instance;

        private static int indexOfPick;
        private static bool isInsideFactory;
        private static bool isInHangarShipRoom;
        

        public static void ShufflePlayerTransforms()
        {
            // Get all players
            List<PlayerControllerB> players = GetAllPlayer();
            Plugin.Logger.LogInfo("SWAPPING POSITION OF " + players.Count.ToString() + "!");
            
            // Generate random indexes for shuffling
            int[] possibleIndex = Enumerable.Range(0, players.Count).ToArray();
            Shuffle(possibleIndex);

            // Broadcast indexes to all players
            SendIndexesToAllPlayers(possibleIndex);
            
            // Prepare positions and rotations for teleportation
            List<ulong> clientIds = new List<ulong>();
            List<Vector3> positions = new List<Vector3>();
            List<Quaternion> rotations = new List<Quaternion>();

            // Gather positions and rotations
            foreach (var player in players)
            {
                clientIds.Add(player.actualClientId);
                positions.Add(player.transform.position);
                rotations.Add(player.transform.rotation);
            }

            // Execute teleportation
            FinalizeTeleportClientRpc(clientIds.ToArray(), positions.ToArray(), rotations.ToArray());
        }

        private static void SendIndexesToAllPlayers(int[] indexes)
        {
            List<ulong> clientsToUpdate = new List<ulong>();

            // Find players that need to be updated
            foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (player.isPlayerControlled && !player.isPlayerDead)
                {
                    clientsToUpdate.Add(player.actualClientId);
                }
            }

            // Batch the index updates and send them in a single RPC call
            DefineIndexesClientRpc(clientsToUpdate.ToArray(), indexes);
        }

        [ClientRpc]
        private static void DefineIndexesClientRpc(ulong[] clientIds, int[] indexes)
        {

            for (int i = 0; i < clientIds.Length; i++)
            {
                if (RoundManager.Instance.playersManager.localPlayerController.actualClientId == clientIds[i])
                {
                    indexOfPick = indexes[i];
                }
            }
        }

        

        [ClientRpc]
        private static async void FinalizeTeleportClientRpc(ulong[] clientId, Vector3[] positions, Quaternion[] rotations)
        {
            foreach (var player in GetAllPlayer())
            {
                if (player.actualClientId == clientId[indexOfPick])
                {
                    if (player.actualClientId != RoundManager.Instance.playersManager.localPlayerController.actualClientId)
                    {
                        if (!player.isPlayerDead && player.isPlayerControlled)
                        {
                            isInsideFactory = player.isInsideFactory;
                            isInHangarShipRoom = player.isInHangarShipRoom;
                            await Task.Delay(200);
                            ExecuteTeleport(positions, rotations);
                        }
                    }
                    
                }
            }
        }

        private static void  ExecuteTeleport(Vector3[] positions, Quaternion[] rotations)
        {
            if (Plugin.ShuffleShiftConfig.ENABLE_SCREEN_SHAKE.Value)
            {
                HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            }
            var transform1 = RoundManager.Instance.playersManager.localPlayerController.transform;
            transform1.position = positions[indexOfPick];
            transform1.rotation = rotations[indexOfPick];
            RoundManager.Instance.playersManager.localPlayerController.isInHangarShipRoom = isInsideFactory;
            RoundManager.Instance.playersManager.localPlayerController.isInHangarShipRoom = isInHangarShipRoom;
        }
        /*private static void SendIndexToAllPlayers(int[] indexes)
        {
            int index = 0;
            foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (player.isPlayerControlled && !player.isPlayerDead)
                {
                    DefineIndexClientRpc(player.actualClientId, indexes[index]);
                }

                index++;
            }
        }
        [ClientRpc]
        private static void DefineIndexClientRpc(ulong clientId, int index)
        {
            Plugin.Logger.LogInfo("Ran!");
            if (RoundManager.Instance.playersManager.localPlayerController.actualClientId == clientId)
            {
                indexOfPick = index;
            }
        }*/

        private static List<PlayerControllerB> GetAllPlayer()
        {
            List<PlayerControllerB> allPlayerObject = new List<PlayerControllerB>();
            foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (player.isPlayerControlled && player.isPlayerControlled && !player.isClimbingLadder)
                {
                    allPlayerObject.Add(player);
                }
            }

            return allPlayerObject;
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
