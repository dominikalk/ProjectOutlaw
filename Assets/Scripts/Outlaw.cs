using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Outlaw : Player
{
    private TaskController taskInRadius = null;

    protected override void Start()
    {
        base.Start();

        isSheriff = false;
        // Set Radius Of Trigger When Player Spawns
        gameObject.GetComponent<CircleCollider2D>().radius = taskRadius;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    private void Update()
    {
        if (!IsOwner) return;

        CheckTaskCompleting();
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
