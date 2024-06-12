using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

namespace ShuffleShift.ActiveObject
{
    public class SwapPositionHandler : NetworkBehaviour
    {
        private static SwapPositionHandler _instance;
        public static SwapPositionHandler Instance => _instance;

        private int indexOfPick;
        private bool isInsideFactory;
        private bool isInHangarShipRoom;

        private void Awake()
        {
            
            InstantiateOnClientsClientRpc();
            // Ensure only one instance exists
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            _instance = this;

            // Schedule position shuffling
            if (RoundManager.Instance.IsHost)
            {
                InvokeRepeating("ShufflePlayerTransforms", 30f, 45f);
            }
            // TODO: Implement other configurations
        }
        [ClientRpc]
        private void InstantiateOnClientsClientRpc()
        {
            if (!IsHost)
            {
                if (SwapPositionHandler.Instance == null)
                {
                    GameObject manager = new GameObject("ShuffleSwapHandler");
                    manager.AddComponent<SwapPositionHandler>();
                }
            }
        }
        private void ShufflePlayerTransforms()
        {
            // Get all players
            List<PlayerControllerB> players = GetAllPlayer();
            Plugin.Logger.LogInfo("SWAPPING POSITION OF " + players.Count.ToString() + "!");
            
            // Generate random indexes for shuffling
            int[] possibleIndex = Enumerable.Range(0, players.Count).ToArray();
            Shuffle(possibleIndex);

            // Broadcast indexes to all players
            SendIndexToAllPlayers(possibleIndex);

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

        private void SendIndexToAllPlayers(int[] indexes)
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
        private void FinalizeTeleportClientRpc(ulong[] clientId, Vector3[] positions, Quaternion[] rotations)
        {
            foreach (var player in GetAllPlayer())
            {
                if (player.actualClientId == clientId[indexOfPick])
                {
                    if (!player.isPlayerDead && player.isPlayerControlled)
                    {
                        isInsideFactory = player.isInsideFactory;
                        isInHangarShipRoom = player.isInHangarShipRoom;
                        StartCoroutine(ExecuteTeleport(positions, rotations));
                    }
                }
            }
        }

        private IEnumerator ExecuteTeleport(Vector3[] positions, Quaternion[] rotations)
        {
            yield return new WaitForSeconds(0.2f);
            HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
            var transform1 = RoundManager.Instance.playersManager.localPlayerController.transform;
            transform1.position = positions[indexOfPick];
            transform1.rotation = rotations[indexOfPick];
            RoundManager.Instance.playersManager.localPlayerController.isInHangarShipRoom = isInsideFactory;
            RoundManager.Instance.playersManager.localPlayerController.isInHangarShipRoom = isInHangarShipRoom;
        }

        [ClientRpc]
        private void DefineIndexClientRpc(ulong clientId, int index)
        {
            Plugin.Logger.LogInfo("Ran!");
            if (RoundManager.Instance.playersManager.localPlayerController.actualClientId == clientId)
            {
                indexOfPick = index;
            }
        }

        private List<PlayerControllerB> GetAllPlayer()
        {
            List<PlayerControllerB> allPlayerObject = new List<PlayerControllerB>();
            foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
            {
                if (player.isPlayerControlled && player.isPlayerControlled)
                {
                    allPlayerObject.Add(player);
                }
            }

            return allPlayerObject;
        }

        [ClientRpc]
        public void DestroyManagerClientRpc()
        {
            Destroy(gameObject);
        }

        private void Shuffle(int[] array)
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

        private void OnDestroy()
        {
            StopAllCoroutines();
            CancelInvoke("ShufflePlayerTransforms");

        }
    }
}
