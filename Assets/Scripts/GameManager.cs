using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

class PlayerRatio
{
    public int sheriffs { get; private set; }
    public int outlaws { get; private set; }
    public int npcs { get; private set; }

    public int bullets { get; private set; }

    public int tasks { get; private set; }

    public PlayerRatio(int sheriffs, int outlaws, int npcs, int bullets, int tasks)
    {
        this.sheriffs = sheriffs;
        this.outlaws = outlaws;
        this.npcs = npcs;
        this.bullets = bullets;
        this.tasks = tasks;
    }
}

public class GameManager : NetworkBehaviour
{
    [SerializeField] private float gameLength = 120f;
    private float gameStartTime = Mathf.Infinity;
    public NetworkVariable<bool> isGamePlaying =
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // TODO: hide in inspector
    public List<GameObject> playerObjects = new List<GameObject>();
    [HideInInspector] public List<Sheriff> sheriffs = new List<Sheriff>();
    [HideInInspector] public List<Outlaw> outlaws = new List<Outlaw>();
    [HideInInspector] public List<NPC> npcs = new List<NPC>();

    // Tasks Vars
    [SerializeField]
    public NetworkVariable<int> noOfTasks =
        new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private int noTasksCompleted = 0;
    [SerializeField] private GameObject tasksScreen;

    // Start Game Vars
    [SerializeField] private GameObject startGameUI;

    // Win Loss Objects
    private enum GameEndEnum
    {
        TimeOut,
        TasksCompleted,
        OutlawsShot,
        BulletsGone,
        NotEnded,
        ClientDisconected
    };
    [SerializeField] private GameObject winLossScreen;
    [SerializeField] private TextMeshProUGUI winLossText;
    [SerializeField] private TextMeshProUGUI winLossDescText;

    // Sheriff Screen Vars
    [HideInInspector] public int bulletsRemaining;
    [SerializeField] private GameObject sheriffScreen;
    [SerializeField] private TextMeshProUGUI bulletsRemainingText;
    [SerializeField] private TextMeshProUGUI outlawsRemainingText;

    [SerializeField] private NPC npcObject;
    [SerializeField] private GameObject chatWindow;
    [SerializeField] private GameObject worldCamera;

    // Sheriffs, Outlaws, NPCs, Bullets, Tasks
    private Dictionary<int, PlayerRatio> playerRatios = new Dictionary<int, PlayerRatio> {
        { 2, new PlayerRatio(1, 1, 3, 2, 3) },
        { 3, new PlayerRatio(1, 2, 4, 3, 5) },
        { 4, new PlayerRatio(1, 3, 6, 4, 7) },
        { 5, new PlayerRatio(2, 3, 7, 5, 7) },
        { 6, new PlayerRatio(2, 4, 8, 6, 9) },
    };

    // Pause game until "Start Game" pressed
    public void Start()
    {
        Time.timeScale = 0f;

        NetworkManager.Singleton.OnClientDisconnectCallback += (_) =>
        {
            if (winLossScreen.activeSelf) return;
            startGameUI.SetActive(false);
            chatWindow.SetActive(false);
            worldCamera.SetActive(true);
            worldCamera.transform.position = Camera.main.transform.position;
            winLossScreen.SetActive(true);
            winLossText.text = "Disconnected";
            winLossDescText.text = "A Client/Host has disconnected from the game.";
            Time.timeScale = 0f;
            tasksScreen.SetActive(false);
            sheriffScreen.SetActive(false);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        };
    }

    // Called when start game is pressed
    public void OnGameStart()
    {
        if (!IsServer) return;

        int noPlayers = playerObjects.Count();

        noOfTasks.Value = playerRatios[noPlayers].tasks;

        // Remove tasks to leave set remaining number
        List<int> removedTasksIndexs = new List<int>();
        TaskController[] tasks = FindObjectsOfType<TaskController>();
        for (int i = 0; i < tasks.Length - noOfTasks.Value; i++)
        {
            while (true)
            {
                int taskIndex = UnityEngine.Random.Range(0, tasks.Length);
                if (removedTasksIndexs.Contains(taskIndex)) continue;
                removedTasksIndexs.Add(taskIndex);
                DespawnTaskServerRpc(tasks[taskIndex].NetworkObjectId);
                break;
            }
        }

        // Assign player types
        bulletsRemaining = playerRatios[noPlayers].bullets;
        for (int i = 0; i < playerRatios[noPlayers].sheriffs; i++)
        {
            int rand = UnityEngine.Random.Range(0, playerObjects.Count);
            sheriffs.Add(playerObjects[rand].GetComponent<Sheriff>());
            playerObjects.RemoveAt(rand);
        }
        for (int i = 0; i < playerRatios[noPlayers].outlaws; i++)
        {
            int rand = UnityEngine.Random.Range(0, playerObjects.Count);
            outlaws.Add(playerObjects[rand].GetComponent<Outlaw>());
            playerObjects.RemoveAt(rand);
        }
        // Spawn NPCs
        for (int i = 0; i < playerRatios[noPlayers].npcs; i++)
        {
            GameObject npc = Instantiate(npcObject.gameObject, Vector3.zero, Quaternion.identity);
            npcs.Add(npc.GetComponent<NPC>());
            npc.GetComponent<NetworkObject>().Spawn();
        }


        // Sync up player types on all clients
        ulong[] sheriffIds = new ulong[sheriffs.Count];
        for (int i = 0; i < sheriffs.Count; i++)
        {
            sheriffIds[i] = sheriffs[i].NetworkObjectId;
        }
        ulong[] outlawIds = new ulong[outlaws.Count];
        for (int i = 0; i < outlaws.Count; i++)
        {
            outlawIds[i] = outlaws[i].NetworkObjectId;
        }
        SetPlayerTypesClientRpc(sheriffIds, outlawIds);

        // Move Players and NPCs
        Player[] players = sheriffs.Cast<Player>().Concat(outlaws).ToArray();
        List<GameObject> spawnNodes = new List<GameObject>(GameObject.FindGameObjectsWithTag("SpawnNode"));
        int v = 0;
        foreach (Player player in players)
        {
            int rand = UnityEngine.Random.Range(0, spawnNodes.Count() - v);
            player.SetPlayerPosServerRpc(spawnNodes[rand].transform.position, player.NetworkObjectId);
            spawnNodes.RemoveAt(rand);
            v++;
        }
        v = 0;
        foreach (NPC npc in npcs)
        {
            int rand = UnityEngine.Random.Range(0, spawnNodes.Count() - v);
            npc.MoveNPCServerRpc(spawnNodes[rand].transform.position, Vector2.zero, npc.NetworkObjectId);
            spawnNodes.RemoveAt(rand);
            v++;
        }

        ChangeSheriffScreenTextClientRpc(outlaws.Count, bulletsRemaining);

        // Start game time
        gameStartTime = (float)NetworkManager.Singleton.LocalTime.Time;
        StartGameClientRpc();

        StartCoroutine("StartGame");
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(0.5f);

        isGamePlaying.Value = true;
    }

    private void Update()
    {
        // Check if time has run out
        if (IsServer && isGamePlaying.Value && NetworkManager.Singleton.LocalTime.Time - gameStartTime >= gameLength)
        {
            ShowWinLoss(GameEndEnum.TimeOut);
            Debug.Log("Sherrifs Win - Time Ran Out");
        }

        if (!isGamePlaying.Value || winLossScreen.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Handles play again button logic
    public void OnPlayAgainPressed()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }

    // Contains reused code for win loss screen logic
    private void ShowWinLoss(GameEndEnum gameEndEnum)
    {
        ShowWinLossClientRpc(gameEndEnum);
        isGamePlaying.Value = false;
    }

    // Decrement bullets remaining
    [ServerRpc(RequireOwnership = false)]
    public void DecrementBulletsServerRpc()
    {
        bulletsRemaining--;

        Outlaw[] outlaws = FindObjectsOfType<Outlaw>().Where(outlaw => outlaw.isActiveAndEnabled && outlaw.isAlive).ToArray();

        ChangeSheriffScreenTextClientRpc(outlaws.Length, bulletsRemaining);

        if (outlaws.Length == 0)
        {
            ShowWinLoss(GameEndEnum.OutlawsShot);
            Debug.Log("Sheriffs Win - Sheriffs shot all the outlaws");
        }
        else if (bulletsRemaining <= 0)
        {
            ShowWinLoss(GameEndEnum.BulletsGone);
            Debug.Log("Outlaws Win - Sheriffs ran out of bullets");
        }
    }

    [ClientRpc]
    public void ChangeSheriffScreenTextClientRpc(int outlaws, int bullets)
    {
        outlawsRemainingText.text = $"Outlaws Remaining: {outlaws}";
        bulletsRemainingText.text = $"Bullets Remaining: {bullets}";
    }

    // Increment number of tasks completed on server
    [ServerRpc(RequireOwnership = false)]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= noOfTasks.Value)
        {
            ShowWinLoss(GameEndEnum.TasksCompleted);
            Debug.Log("Outlaws Win - Completed All Tasks");
        }
    }

    // Despawn task with id on server
    [ServerRpc]
    private void DespawnTaskServerRpc(ulong taskId)
    {
        TaskController taskInRadius = FindObjectsOfType<TaskController>().Where(x => x.NetworkObjectId == taskId).FirstOrDefault();
        taskInRadius.GetComponent<NetworkObject>().Despawn();
    }

    // Starts game timescale on all clients
    [ClientRpc]
    private void StartGameClientRpc()
    {
        Time.timeScale = 1f;
        foreach (Outlaw outlaw in outlaws)
        {
            outlaw.HideStartMenuClientRpc();
            outlaw.ShowTaskUIClientRpc();
            outlaw.HandlePlayerCameraClientRpc();
        }
        foreach (Sheriff sheriff in sheriffs)
        {
            sheriff.HideStartMenuClientRpc();
            sheriff.HideTasksClientRpc();
            sheriff.HandlePlayerCameraClientRpc();
            sheriff.ShowSheriffUIClientRpc();
        }
    }

    // Manipulates Win Loss Screen depending on end of game type
    [ClientRpc]
    private void ShowWinLossClientRpc(GameEndEnum gameEndType)
    {
        bool isSheriff = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Sheriff>().enabled;
        chatWindow.SetActive(false);

        worldCamera.SetActive(true);
        worldCamera.transform.position = Camera.main.transform.position;
        Camera.main.gameObject.SetActive(false);

        winLossScreen.SetActive(true);
        switch (gameEndType)
        {
            case GameEndEnum.TimeOut:
                winLossText.text = isSheriff ? "You Win" : "You Lose";
                winLossDescText.text = isSheriff ? "The Outlaws Ran Out Of Time!" : "You Ran Out Of Time!";
                break;
            case GameEndEnum.TasksCompleted:
                winLossText.text = isSheriff ? "You Lose" : "You Win";
                winLossDescText.text = isSheriff ? "The Outlaws Completed All Of The Tasks!" : "You Completed All The Tasks!";
                break;
            case GameEndEnum.OutlawsShot:
                winLossText.text = isSheriff ? "You Win" : "You Lose";
                winLossDescText.text = isSheriff ? "You Shot All Of The Outlaws!" : "All Of The OutLaws Were Shot!";
                break;
            case GameEndEnum.BulletsGone:
                winLossText.text = isSheriff ? "You Lose" : "You Win";
                winLossDescText.text = isSheriff ? "You Have Wasted All Of Your Bullets!" : "The Sheriffs Wasted All Of Their Bullets!";
                break;
            case GameEndEnum.ClientDisconected:
                winLossText.text = "Disconnected";
                winLossDescText.text = "A Client/Host has disconnected from the game.";
                break;
            default:
                break;
        }
        Time.timeScale = 0f;
        tasksScreen.SetActive(false);
        sheriffScreen.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // On Game Start, sync up player types on all clients
    [ClientRpc]
    private void SetPlayerTypesClientRpc(ulong[] sheriffIds, ulong[] outlawIds)
    {
        Sheriff[] sheriffs = FindObjectsOfType<Sheriff>();
        foreach (Sheriff sheriff in sheriffs)
        {
            if (sheriffIds.Contains(sheriff.GetComponent<NetworkObject>().NetworkObjectId))
            {
                sheriff.enabled = true;
            }
        }
        Outlaw[] outlaws = FindObjectsOfType<Outlaw>();
        foreach (Outlaw outlaw in outlaws)
        {
            if (outlawIds.Contains(outlaw.GetComponent<NetworkObject>().NetworkObjectId))
            {
                outlaw.enabled = true;
            }
        }
    }
}
