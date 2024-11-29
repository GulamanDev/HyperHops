using Photon.Pun;
using UnityEngine;

public class OfflineModeManager : MonoBehaviour
{
    private void Start()
    {
        PhotonNetwork.OfflineMode = true;

        PhotonNetwork.JoinOrCreateRoom("OfflineRoom", new Photon.Realtime.RoomOptions(), null);

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));

        PhotonNetwork.Instantiate("Blue", spawnPosition, Quaternion.identity);
    }
}
