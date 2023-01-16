using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [Header("Menus")]
    public GameObject mainMenu;
    public GameObject lobbyMenu;
    [Header("Main menu")]
    public Button createRoomButton;
    public Button joinRoomButton;
    [Header("Lobby menu")]
    public TextMeshProUGUI roomName;
    public TextMeshProUGUI playerList;
    public Button startGameButton;

    // Start is called before the first frame update
    void Start()
    {
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    public void SetMenu(GameObject menu)
    {
        mainMenu.SetActive(false);
        lobbyMenu.SetActive(false);
        menu.SetActive(true);
    }

    public void OnCreateRoomButton(TextMeshProUGUI roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
        roomName.SetText(roomNameInput.text);
    }

    public void OnJoinRoomButton(TextMeshProUGUI roomNameInput)
    {
        NetworkManager.instance.JoinRoom(roomNameInput.text);
        roomName.SetText(roomNameInput.text);
    }

    public void OnPlayerNameUpdate(TextMeshProUGUI playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text;
    }

    public override void OnJoinedRoom()
    {
        SetMenu(lobbyMenu);
        photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateLobbyUI();
    }

    [PunRPC]
    public void UpdateLobbyUI()
    {
        playerList.SetText("");
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                playerList.text += player.NickName + " (Host) \n";
            }
            else
            {
                playerList.text += player.NickName + "\n";
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable = true;
        }
        else
        {
            startGameButton.interactable = false;
        }
    }

    public void OnLeaveLobbyButton()
    {
        PhotonNetwork.LeaveRoom();
        SetMenu(mainMenu);
    }

    public void OnStartGameButton()
    {
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");
    }
}
