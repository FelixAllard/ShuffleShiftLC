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
    public class SwapPositionHandler : MonoBehaviour
    {
        private static SwapPositionHandler _instance;
        public static SwapPositionHandler Instance => _instance;

        private int indexOfPick;
        private bool isInsideFactory;
        private bool isInHangarShipRoom;
        public Coroutine swapCoroutine;

        private IEnumerator ActivationCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(Plugin.ShuffleShiftConfig.TIME_BETWEEN_SWAP.Value);
                ActivateSwap();
            }
        }

        private void Awake()
        {
            // Check if an instance already exists
            if (_instance != null && _instance != this)
            {
                // If an instance already exists, destroy this game object
                Destroy(this.gameObject);
                return;
            }
            // If no instance exists, set this as the instance
            _instance = this;
        }
        private void Start()
        {
            if (Plugin.ShuffleShiftConfig.ENABLE_POSITION_SWAP.Value)
            {
                if (RoundManager.Instance.IsHost)
                {
                    swapCoroutine = StartCoroutine(ActivationCoroutine());
                }
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
        

        public void OnDestroy()
        {
            StopCoroutine(swapCoroutine);
        }

        public void DestroyManager()
        {
            enabled = false;
            StopCoroutine(swapCoroutine);
            Destroy(this.gameObject);
        }
    }
}
