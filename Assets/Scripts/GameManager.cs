using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private float gameLength = 120f;
    private float gameStartTime;
    private TaskController[] tasks = { };
    private int noTasksCompleted = 0;

    public override void OnNetworkSpawn()
    {
        tasks = FindObjectsOfType<TaskController>();
        gameStartTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - gameStartTime >= gameLength) Debug.Log("Sherrifs Win - Time Ran Out");
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncTasksCompletedServerRpc()
    {
        noTasksCompleted++;
        if (noTasksCompleted >= tasks.Length) Debug.Log("Outlaws Win - Completed All Tasks");
    }
}
