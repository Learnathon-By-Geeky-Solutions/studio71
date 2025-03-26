using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AdvancedNetworkManager : MonoBehaviourPunCallbacks
{
    public static AdvancedNetworkManager Instance;

    [Header("Connection UI")]
    public InputField playerNameInput;
    public Text connectionStatusText;
    public Button connectButton;
    public GameObject lobbyPanel;
    public GameObject gamePanel;

    [Header("Room Settings")]
    public int maxPlayers = 5;
    public string gameVersion = "1.0";

    [Header("Spawn Settings")]
    public List<Transform> playerSpawnPoints;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Configure Photon settings
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        
        // Connect to Photon servers
        ConnectToPhotonServers();
    }

    public void ConnectToPhotonServers()
    {
        connectionStatusText.text = "Connecting to Servers...";
        PhotonNetwork.ConnectUsingSettings();
    }

    public void CreateOrJoinRoom()
    {
        // Set player nickname
        string playerName = string.IsNullOrEmpty(playerNameInput.text) 
            ? "Player_" + Random.Range(1000, 9999) 
            : playerNameInput.text;
        
        PhotonNetwork.NickName = playerName;

        // Room options
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayers,
            PublishUserId = true,
            CleanupCacheOnLeave = false
        };

        // Join or create room
        PhotonNetwork.JoinOrCreateRoom("CoopStage", roomOptions, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        connectionStatusText.text = "Connected";
        connectButton.interactable = true;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        connectionStatusText.text = "Room Joined";
        
        // Hide lobby, show game panel
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(true);

        // Spawn local player
        SpawnPlayer();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Join Room Failed: {message}");
        connectionStatusText.text = "Join Failed: " + message;
    }

    void SpawnPlayer()
    {
        // Select random spawn point
        if (playerSpawnPoints.Count > 0)
        {
            Transform spawnPoint = playerSpawnPoints[Random.Range(0, playerSpawnPoints.Count)];
            
            // Instantiate networked player prefab
            GameObject player = PhotonNetwork.Instantiate("PlayerPrefab", spawnPoint.position, spawnPoint.rotation);
            
            // Optional: Set player's name
            //player.GetComponent<PlayerController>().playerName = PhotonNetwork.NickName;
        }
        else
        {
            Debug.LogError("No spawn points available!");
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        // Return to lobby
        lobbyPanel.SetActive(true);
        gamePanel.SetActive(false);
    }
}