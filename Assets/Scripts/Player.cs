using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float taskRadius;

    private TaskController taskInRadius = null;

    private GameManager gameManager;


    private void Start()
    {
        if (!IsOwner) return;

        // Set Radius Of Trigger When Player Spawns
        gameObject.GetComponent<CircleCollider2D>().radius = taskRadius;
        gameManager = FindObjectOfType<GameManager>();
        transform.position = new Vector3(0, 0, -1f);
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckMovement();
    }

    private void Update()
    {
        if (!IsOwner) return;

        CheckTaskCompleting();
    }

    // Code To Handle Player Movement
    private void CheckMovement()
    {
        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.y += +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y += -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x += -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += +1f;

        Vector3 deltaPos = moveDir.normalized * movementSpeed * Time.deltaTime;
        if (deltaPos != Vector3.zero) MovePlayerServerRpc(deltaPos);
    }

    // Code To Handle Task Completion
    private void CheckTaskCompleting()
    {
        if (!Input.GetKey(KeyCode.E) || !gameManager.isGamePlaying.Value)
        {
            if (taskInRadius != null && taskInRadius.completingStart.Value != Mathf.Infinity) StopTaskServerRpc(taskInRadius.NetworkObjectId);
            return;
        }
        if (taskInRadius == null || taskInRadius.completingStart.Value <= NetworkManager.Singleton.LocalTime.Time) return;
        // Function Called Only if E is pressed, wasn't pressed before, and if within a tasks radius
        StartTaskServerRpc((float)NetworkManager.Singleton.LocalTime.Time, taskInRadius.NetworkObjectId);
    }

    // Set Task When Within Radius
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsOwner || collision.gameObject.GetComponent<TaskController>() == null) return;

        TaskController task = collision.gameObject.GetComponent<TaskController>();
        if (task != null) taskInRadius = task;
    }

    // Remove Task When Not Within Radius
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsOwner || collision.gameObject.GetComponent<TaskController>() == null) return;

        TaskController task = collision.gameObject.GetComponent<TaskController>();
        if (task == null) return;

        StopTaskServerRpc(task.NetworkObjectId);
        taskInRadius = null;
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 deltaPos)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            client.PlayerObject.transform.Translate(deltaPos);
        }
    }


    // Sets value of task competion start time on server
    [ServerRpc]
    public void StartTaskServerRpc(float start, ulong taskId)
    {
        TaskController taskInRadius = FindObjectsOfType<TaskController>().Where(x => x.NetworkObjectId == taskId).FirstOrDefault();
        if (taskInRadius == null) return;
        taskInRadius.completingStart.Value = start;
    }

    // Sets value of task competion start time to infinity (i.e. task no started)
    [ServerRpc]
    public void StopTaskServerRpc(ulong taskId)
    {
        TaskController taskInRadius = FindObjectsOfType<TaskController>().Where(x => x.NetworkObjectId == taskId).FirstOrDefault();
        if (taskInRadius == null) return;
        taskInRadius.completingStart.Value = Mathf.Infinity;
    }
}
