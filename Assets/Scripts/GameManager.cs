using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private float gameLength = 120f;
    private double gameStartTime;

    // Tasks Vars
    private TaskController[] tasks = { };
    [SerializeField] int noOfTasks;
    private int noTasksCompleted = 0;

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
    private GameEndEnum gameEndType = GameEndEnum.NotEnded;
    [SerializeField] private GameObject winLossScreen;
    [SerializeField] private TextMeshProUGUI winLossText;
    [SerializeField] private TextMeshProUGUI winLossDescText;

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
        tasks = FindObjectsOfType<TaskController>();
        for (int i = 0; i < tasks.Length - noOfTasks; i++)
        {
            while (true)
            {
                int taskIndex = Random.Range(0, tasks.Length);
                if (removedTasksIndexs.Contains(taskIndex)) continue;
                removedTasksIndexs.Add(taskIndex);
                DespawnTaskServerRpc(tasks[taskIndex].NetworkObjectId);
                break;
            }
        }

        // Start game time
        gameStartTime = NetworkManager.Singleton.LocalTime.Time;
        StartGameTimeClientRpc();

        // Set start game button active to false
        startGamePressed = true;
        startGameBtn.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.time - gameStartTime >= gameLength)
        {
            gameEndType = GameEndEnum.TimeOut;
            Debug.Log("Sherrifs Win - Time Ran Out");
        }

        CheckGameOver();

        if (!startGamePressed && IsHost) startGameBtn.gameObject.SetActive(true);

        // Check if time has run out
        if (NetworkManager.Singleton.LocalTime.Time - gameStartTime >= gameLength) Debug.Log("Sherrifs Win - Time Ran Out");
    }

    // Checks if the game is over and manipulates win loss screen
    private void CheckGameOver()
    {
        if (gameEndType == GameEndEnum.NotEnded) return;
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
    }

    // Increment number of tasks completed on server
    [ServerRpc(RequireOwnership = false)]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= noOfTasks)
        {
            gameEndType = GameEndEnum.TasksCompleted;
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
    private void StartGameTimeClientRpc()
    {
        Time.timeScale = 1f;
    }
}
