using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections.LowLevel.Unsafe;

public class GameManager : NetworkBehaviour
{
    private float gameLength = 120f;
    private float gameStartTime;
    private TaskController[] tasks = { };
    [SerializeField] int noOfTasks;
    private int noTasksCompleted = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        tasks = FindObjectsOfType<TaskController>();
        for (int i = 0; i < tasks.Length - noOfTasks; i++)
        {
            tasks[i].transform.GetChild(1).GetComponent<NetworkObject>().Despawn();
            tasks[i].NetworkObject.Despawn();
        }
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
        if (noTasksCompleted >= noOfTasks) Debug.Log("Outlaws Win - Completed All Tasks");
    }
}
