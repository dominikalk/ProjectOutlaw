using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking.Types;

public class TaskController : NetworkBehaviour
{
    [SerializeField] private float timeToComplete = 2f;
    [SerializeField] public string taskName;

    public NetworkVariable<float> completingStart =
        new NetworkVariable<float>(Mathf.Infinity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Update()
    {
        // Temp Code : Progress Bar For Task Completion
        float timePassed = timeToComplete - ((float)NetworkManager.Singleton.LocalTime.Time - completingStart.Value);
        if (timePassed > timeToComplete) timePassed = timeToComplete;
        Transform scale = gameObject.transform.GetChild(1).transform;
        if (scale != null) scale.localScale = new Vector3(timePassed / timeToComplete, 0.2f, 1f);

        if (!IsServer) return;

        // Code When Task Gets Completed
        if (NetworkManager.Singleton.LocalTime.Time - completingStart.Value >= timeToComplete)
        {
            FindObjectOfType<GameManager>().IncTasksCompletedServerRpc();
            CallTaskUIClientRpc();
            DespawnServerRpc();
        }
    }

    // Despawn task on server
    [ServerRpc]
    private void DespawnServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
    }

    // Call task ui on all clients
    [ClientRpc]
    private void CallTaskUIClientRpc()
    {
        FindObjectOfType<TaskScreenController>()?.CompleteTask(NetworkObjectId);
    }
}
