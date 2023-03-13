using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private float gameLength = 120f;
    private float gameStartTime = Mathf.Infinity;
    public NetworkVariable<bool> isGamePlaying =
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Tasks Vars
    [SerializeField] public int noOfTasks;
    private int noTasksCompleted = 0;
    [SerializeField] private GameObject tasksScreen;

    // Start Game Vars
    [SerializeField] private Button startGameBtn;
    private bool startGamePressed = false;

    // Win Loss Objects
    private enum GameEndEnum
    {
        TimeOut,
        TasksCompleted,
        OutlawsShot,
        BulletsGone,
        NotEnded
    };
    [SerializeField] private GameObject winLossScreen;
    [SerializeField] private TextMeshProUGUI winLossText;
    [SerializeField] private TextMeshProUGUI winLossDescText;

    [HideInInspector] public List<GameObject> sheriffs = new List<GameObject>();
    [HideInInspector] public List<GameObject> outlaws = new List<GameObject>();

    // Pause game until "Start Game" pressed
    public void Start()
    {
        Time.timeScale = 0f;
    }

    // Called when start game is pressed
    public void OnGameStart()
    {
        if (!IsServer) return;

        // Remove tasks to leave set remaining number
        List<int> removedTasksIndexs = new List<int>();
        TaskController[] tasks = FindObjectsOfType<TaskController>();
        for (int i = 0; i < tasks.Length - noOfTasks; i++)
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

        // Sync up player types on all clients
        ulong[] sheriffIds = new ulong[sheriffs.Count];
        for (int i = 0; i < sheriffs.Count; i++)
        {
            sheriffIds[i] = sheriffs[i].GetComponent<NetworkObject>().NetworkObjectId;
        }
        ulong[] outlawIds = new ulong[outlaws.Count];
        for (int i = 0; i < outlaws.Count; i++)
        {
            outlawIds[i] = outlaws[i].GetComponent<NetworkObject>().NetworkObjectId;
        }
        SetPlayerTypesClientRpc(sheriffIds, outlawIds);

        // Ensure each player sees their character in the front
        Player[] sheriffPlayers = FindObjectsOfType<Sheriff>();
        Player[] outlawPlayers = FindObjectsOfType<Outlaw>();
        foreach (Player player in sheriffPlayers.Concat(outlawPlayers))
        {
            player.PullZPosFrontClientRpc();
        }

        // Start game time
        gameStartTime = (float)NetworkManager.Singleton.LocalTime.Time;
        isGamePlaying.Value = true;
        StartGameClientRpc();

        // Set start game button active to false
        startGamePressed = true;
        startGameBtn.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Check if time has run out
        if (IsServer && isGamePlaying.Value && NetworkManager.Singleton.LocalTime.Time - gameStartTime >= gameLength)
        {
            ShowWinLoss(GameEndEnum.TimeOut);
            Debug.Log("Sherrifs Win - Time Ran Out");
        }

        if (!startGamePressed && IsHost) startGameBtn.gameObject.SetActive(true);

        // TODO: Dev tool - remove later
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPlayAgainPressed();
        }
    }

    // Handles play again button logic
    public void OnPlayAgainPressed()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Contains reused code for win loss screen logic
    private void ShowWinLoss(GameEndEnum gameEndEnum)
    {
        ShowWinLossClientRpc(gameEndEnum);
        isGamePlaying.Value = false;
    }

    // Increment number of tasks completed on server
    [ServerRpc(RequireOwnership = false)]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= noOfTasks)
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
        tasksScreen.SetActive(true);
    }

    // Manipulates Win Loss Screen depending on end of game type
    [ClientRpc]
    private void ShowWinLossClientRpc(GameEndEnum gameEndType)
    {
        winLossScreen.SetActive(true);
        switch (gameEndType)
        {
            case GameEndEnum.TimeOut:
                winLossText.text = "You Lose";
                winLossDescText.text = "You Ran Out Of Time!";
                break;
            case GameEndEnum.TasksCompleted:
                winLossText.text = "You Win";
                winLossDescText.text = "You Completed All The Tasks";
                break;
            case GameEndEnum.OutlawsShot:
                winLossText.text = "You Lose";
                winLossDescText.text = "All The OutLaws Were Shot!";
                break;
            case GameEndEnum.BulletsGone:
                winLossText.text = "You Win";
                winLossDescText.text = "All The Bullets Are Gone!";
                break;
            default:
                break;
        }
        Time.timeScale = 0f;
        tasksScreen.SetActive(false);
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
