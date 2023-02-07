using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float taskRadius;

    private TaskController taskInRadius = null;

    private void Start()
    {
        // Set Radius Of Trigger When Player Spawns
        gameObject.GetComponent<CircleCollider2D>().radius = taskRadius;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckMovement();
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
        if (!Input.GetKey(KeyCode.E))
        {
            if (taskInRadius != null) taskInRadius.StopTaskServerRpc();
            return;
        }
        if (taskInRadius == null || taskInRadius.completingStart <= Time.time) return;
        // Function Called Only if E is pressed, wasn't pressed before, and if within a tasks radius
        taskInRadius.StartTaskServerRpc(Time.time);
    }

    // Set Task When Within Radius
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TaskController task = collision.gameObject.GetComponent<TaskController>();
        if (task != null) taskInRadius = task;
    }

    // Remove Task When Not Within Radius
    private void OnTriggerExit2D(Collider2D collision)
    {
        TaskController task = collision.gameObject.GetComponent<TaskController>();
        if (task != null)
        {
            if (taskInRadius) taskInRadius.StopTaskServerRpc();
            taskInRadius = null;
        }
    }

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 deltaPos)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            Debug.Log("Owner Id: " + OwnerClientId);
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            client.PlayerObject.transform.Translate(deltaPos);
        }
    }
}
