using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using BepInEx.Configuration;
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
            if (Plugin.ShuffleShiftConfig.ENABLE_POSITION_SWAP.Value)
            {
                InvokeRepeating("ActivateSwap", Plugin.ShuffleShiftConfig.TIME_BEFORE_FIRST_SWAP.Value, Plugin.ShuffleShiftConfig.TIME_BETWEEN_SWAP.Value);
            }
        }

        public void ActivateSwap()
        {
            if (RandomNumberGenerator.GetInt32(0, 100) <= Plugin.ShuffleShiftConfig.CHANCE_FOR_SWAP_TO_HAPPEN.Value)
            {
                PositionHandler.ShufflePlayerTransforms();
            }
            else
            {
                Plugin.Logger.LogInfo("Teleport was a miss!");
            }
        }
        private void OnDestroy()
        {
            StopAllCoroutines();
            CancelInvoke("ShufflePlayerTransforms");
        }

        public void DestroyManager()
        {
            Destroy(this.gameObject);
        }
    }
}
