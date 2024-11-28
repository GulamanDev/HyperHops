using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // Array of spawn points for players
    private bool[] spawnPointOccupied; // Tracks whether a spawn point is occupied

    private void Start()
    {
        spawnPointOccupied = new bool[spawnPoints.Length];

        if (PhotonNetwork.IsConnectedAndReady)
        {
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (!spawnPointOccupied[i]) // Find an unoccupied spawn point
            {
                Transform spawnPoint = spawnPoints[i];
                spawnPointOccupied[i] = true; // Mark spawn point as occupied

                // Determine the prefab to use based on the player
                string prefabName = GetPlayerPrefabName();
                PhotonNetwork.Instantiate(prefabName, spawnPoint.position, spawnPoint.rotation);

                PhotonNetwork.LocalPlayer.CustomProperties["SpawnIndex"] = i; // Store player's spawn index
                return;
            }
        }

        Debug.LogError("No available spawn points!");
    }

    string GetPlayerPrefabName()
    {
        // Example logic for four different prefabs based on ActorNumber
        int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        switch (actorNumber % 4)
        {
            case 0:
                return "Blue";
            case 1:
                return "Red";
            case 2:
                return "Green";
            case 3:
                return "Yellow";
            default:
                return "Blue"; // Default to "Blue" if something goes wrong
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Free up the spawn point used by the player who left
        if (otherPlayer.CustomProperties.TryGetValue("SpawnIndex", out object index))
        {
            int spawnIndex = (int)index;
            if (spawnIndex >= 0 && spawnIndex < spawnPoints.Length)
            {
                spawnPointOccupied[spawnIndex] = false;
            }
        }
    }
}
