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

    [HideInInspector] public List<Sheriff> sheriffs = new List<Sheriff>();
    [HideInInspector] public List<Outlaw> outlaws = new List<Outlaw>();

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

    // Sheriff Screen Vars
    [SerializeField] public int bulletsRemaining;
    [SerializeField] private GameObject sheriffScreen;
    [SerializeField] private TextMeshProUGUI bulletsRemainingText;
    [SerializeField] private TextMeshProUGUI outlawsRemainingText;

    [SerializeField] private GameObject gameCodeContainer;

    // Pause game until "Start Game" pressed
    public void Start()
    {
        Time.timeScale = 0f;
    }

    // Called when start game is pressed
    public void OnGameStart()
    {
        if (!IsServer) return;

        gameCodeContainer.SetActive(false);

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

        ChangeSheriffScreenTextClientRpc(outlaws.Count, bulletsRemaining);
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
        if (Input.GetKeyDown(KeyCode.Slash))
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
        foreach (Outlaw outlaw in outlaws)
        {
            outlaw.ShowTaskUIClientRpc();
            outlaw.HandlePlayerCameraClientRpc();
        }
        foreach (Sheriff sheriff in sheriffs)
        {
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
