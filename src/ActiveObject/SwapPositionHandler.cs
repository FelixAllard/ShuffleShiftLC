using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace ShuffleShift.ActiveObject;



public class SwapPositionHandler : MonoBehaviour
{
    private static SwapPositionHandler _instance;
    public static SwapPositionHandler Instance => _instance;

    private void Awake()
    {
        //Make sure this doesn't already exist
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        InvokeRepeating("ShufflePlayerTransforms", 30f, 45f);

    }
    public void ShufflePlayerTransforms()
    {
        
        // Get all players
        List<GameObject> players = GetAllPlayer();
        Plugin.Logger.LogInfo("SWAPPING POSITION OF " + players.Count.ToString()+ "!");

        // Store original positions and rotations
        List<Vector3> positions = new List<Vector3>();
        List<Quaternion> rotations = new List<Quaternion>();

        foreach (var player in players)
        {
            positions.Add(player.transform.position);
            rotations.Add(player.transform.rotation);
        }

        // Shuffle positions and rotations
        ShuffleList(positions);
        ShuffleList(rotations);

        // Assign shuffled positions and rotations back to players
        for (int i = 0; i < players.Count; i++)
        {
            players[i].transform.position = positions[i];
            players[i].transform.rotation = rotations[i];
        }

        foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
        {
            player.SyncBodyPositionWithClients();
        }
    }
    // Generic method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    private List<GameObject> GetAllPlayer()
    {
        List<GameObject> allPlayerObject = new List<GameObject>();
        foreach (var player in RoundManager.Instance.playersManager.allPlayerScripts)
        {
            if (player.isPlayerControlled && player.isPlayerControlled)
            {
                allPlayerObject.Add(player.gameObject);
            }
        }

        return allPlayerObject;
    }
    
    
    [ClientRpc]
    public void DestroyManagerClientRpc()
    {
        Destroy(gameObject);
    }
    private void OnDestroy()
    {
        StopAllCoroutines();
        CancelInvoke("CheckIfSeen");
        CancelInvoke("Emotehandles");
        CancelInvoke("HonkManager");
    }
}