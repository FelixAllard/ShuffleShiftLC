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

        private IEnumerator ActivationCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Plugin.ShuffleShiftConfig.TIME_BETWEEN_SWAP.Value);
                ActivateSwap();
            }
        }

        private void Start()
        {
            if (Plugin.ShuffleShiftConfig.ENABLE_POSITION_SWAP.Value)
            {
                StartCoroutine(ActivationCoroutine());
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
        

        public override void OnDestroy()
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
