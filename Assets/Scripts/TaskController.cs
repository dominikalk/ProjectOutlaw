using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking.Types;

public class TaskController : NetworkBehaviour
{
    [SerializeField] private float timeToComplete = 2f;
    [SerializeField] private String taskName;

    public NetworkVariable<float> completingStart =
        new NetworkVariable<float>(Mathf.Infinity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


    private void Update()
    {
        // Temp Code : Progress Bar For Task Completion
        float timePassed = timeToComplete - (Time.time - completingStart.Value);
        if (timePassed > timeToComplete) timePassed = timeToComplete;
        Transform scale = gameObject.transform.GetChild(1).transform;
        if (scale != null) scale.localScale = new Vector3(timePassed / timeToComplete, 0.2f, 1f);

        // Code When Task Gets Completed
        if (Time.time - completingStart.Value >= timeToComplete)
        {
            FindObjectOfType<GameManager>().IncTasksCompletedServerRpc();
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
