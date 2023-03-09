using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections.LowLevel.Unsafe;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class GameManager : NetworkBehaviour
{
    private float gameLength = 120f;
    private double gameStartTime;
    private TaskController[] tasks = { };
    [SerializeField] int noOfTasks;
    private int noTasksCompleted = 0;
    [SerializeField] private Button startGameBtn;
    private bool startGamePressed = false;
    [SerializeField] private TextMeshProUGUI text;

    public void Start()
    {
        Time.timeScale = 0f;
    }

    public void OnGameStart()
    {
        if (!IsServer) return;

        // Remove tasks to leave set remaining number
        tasks = FindObjectsOfType<TaskController>();
        for (int i = 0; i < tasks.Length - noOfTasks; i++)
        {
            DespawnTaskServerRpc(tasks[i].NetworkObjectId);
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
        if (!startGamePressed && IsHost) startGameBtn.gameObject.SetActive(true);
        if (NetworkManager.Singleton.LocalTime.Time - gameStartTime >= gameLength) Debug.Log("Sherrifs Win - Time Ran Out");
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= noOfTasks) Debug.Log("Outlaws Win - Completed All Tasks");
    }

    [ServerRpc]
    private void DespawnTaskServerRpc(ulong taskId)
    {
        TaskController taskInRadius = FindObjectsOfType<TaskController>().Where(x => x.NetworkObjectId == taskId).FirstOrDefault();
        taskInRadius.GetComponent<NetworkObject>().Despawn();
    }

    [ClientRpc]
    private void StartGameTimeClientRpc()
    {
        Time.timeScale = 1f;
    }
}
