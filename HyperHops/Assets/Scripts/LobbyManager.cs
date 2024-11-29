using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField roomNameInputField;
    public TextMeshProUGUI statusText;

    private void Start()
    {
        // Connect to Photon Master Server
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "Connecting to Photon...";
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to Photon!";
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "Joined Lobby!";
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInputField.text))
        {
            PhotonNetwork.CreateRoom(roomNameInputField.text, new RoomOptions { MaxPlayers = 4 });
        }
        else
        {
            statusText.text = "Room name cannot be empty!";
        }
    }

    public void JoinRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInputField.text))
        {
            PhotonNetwork.JoinRoom(roomNameInputField.text);
        }
        else
        {
            statusText.text = "Room name cannot be empty!";
        }
    }

    public override void OnCreatedRoom()
    {
        statusText.text = $"Room '{PhotonNetwork.CurrentRoom.Name}' created!";
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"Joined Room: {PhotonNetwork.CurrentRoom.Name}";
        PhotonNetwork.LoadLevel("LevDes"); // Replace with game scene name.
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Create Room Failed: {message}";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = $"Join Room Failed: {message}";
    }
}
