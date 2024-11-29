using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints; // Array of spawn points for players

    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // Master client initializes the room properties
            if (PhotonNetwork.IsMasterClient)
            {
                InitializeSpawnPoints();
            }

            // Wait for room properties to sync and then spawn player
            SpawnPlayer();
        }
    }

    void InitializeSpawnPoints()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("OccupiedSpawnPoints"))
        {
            bool[] initialSpawnPointOccupied = new bool[spawnPoints.Length];
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
            {
                { "OccupiedSpawnPoints", initialSpawnPointOccupied }
            });

            Debug.Log("Initialized OccupiedSpawnPoints in room properties.");
        }
    }

    void SpawnPlayer()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("OccupiedSpawnPoints", out object occupiedPoints))
        {
            bool[] spawnPointOccupied = (bool[])occupiedPoints;

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (!spawnPointOccupied[i]) // Find an unoccupied spawn point
                {
                    Transform spawnPoint = spawnPoints[i];
                    spawnPointOccupied[i] = true; // Mark spawn point as occupied

                    // Update the shared room property
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                    {
                        { "OccupiedSpawnPoints", spawnPointOccupied }
                    });

                    // Instantiate the player prefab at the spawn point
                    PhotonNetwork.Instantiate("Blue", spawnPoint.position, spawnPoint.rotation);

                    // Store the spawn index in the player's custom properties
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                    {
                        { "SpawnIndex", i }
                    });

                    return;
                }
            }

            Debug.LogError("No available spawn points!");
        }
        else
        {
            Debug.LogError("OccupiedSpawnPoints not found in room properties. Retrying...");
            Invoke(nameof(SpawnPlayer), 0.5f); // Retry after a short delay
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.CustomProperties.TryGetValue("SpawnIndex", out object index))
        {
            int spawnIndex = (int)index;

            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("OccupiedSpawnPoints", out object occupiedPoints))
            {
                bool[] spawnPointOccupied = (bool[])occupiedPoints;
                if (spawnIndex >= 0 && spawnIndex < spawnPoints.Length)
                {
                    spawnPointOccupied[spawnIndex] = false; // Mark the spawn point as free

                    // Update the shared room property
                    PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
                    {
                        { "OccupiedSpawnPoints", spawnPointOccupied }
                    });
                }
            }
        }
    }
}
