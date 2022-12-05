using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Status")]
    public bool gameEnded = false;
    [Header("Players")]
    public string playerPrefabLocation;
    public Transform[] spawnPoints;
    public PlayerController[] players;
    private int playersInGame;
    private List<int> pickedSpawnIndex;
    [Header("Reference")]
    public GameObject imageTarget;
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Starting");
        pickedSpawnIndex = new List<int>();
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
        ARPlaceTrackedImages.isTracking = false;
    }

    // Update is called once per frame
    void Update()
    {
        /* Debug.Log("Is tracking " + ARPlaceTrackedImages.isTracking);
        foreach (GameObject gameObject in GameObject.FindObjectsOfType(
            typeof(GameObject)))
        {
            if (gameObject.name == "FBX_Corona Variant(Clone)")
            {
                gameObject.transform.SetParent(imageTarget.transform);
            }
        }
        for (int i = 1; i < imageTarget.transform.childCount; i++)
        {
            imageTarget.transform.GetChild(i).gameObject.SetActive(
                ARPlaceTrackedImages.isTracking);
        } */
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;
        if (playersInGame == PhotonNetwork.PlayerList.Length)
        {
            // SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        int rand = Random.Range(0, spawnPoints.Length);
        while (pickedSpawnIndex.Contains(rand))
        {
            rand = Random.Range(0, spawnPoints.Length);
        }
        pickedSpawnIndex.Add(rand);
        GameObject playerObject = PhotonNetwork.Instantiate(
            playerPrefabLocation, spawnPoints[rand].position,
            Quaternion.identity);
        PlayerController playerScript = playerObject
            .GetComponent<PlayerController>();
        playerScript.photonView.RPC("Initialize", RpcTarget.All,
            PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerID)
    {
        return players.First(x => x.id == playerID);
    }

    public PlayerController GetPlayer(GameObject playerObj)
    {
        return players.First(x => x.gameObject == playerObj);
    }
}
