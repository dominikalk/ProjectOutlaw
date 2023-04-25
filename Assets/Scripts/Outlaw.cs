using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;

public class Outlaw : Player
{
    private TaskController taskInRadius = null;
    private GameObject taskPromptText;

    public bool isAlive = true;

    protected override void Start()
    {
        base.Start();

        isSheriff = false;
        // Set Radius Of Trigger When Player Spawns
        gameObject.GetComponent<CircleCollider2D>().radius = taskRadius;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.red;

        taskPromptText = GameObject.FindGameObjectWithTag("TaskPromptContainer").transform.GetChild(0).gameObject;

        // TODO: Remove this temp fix for outlaws
        MovePlayerServerRpc(new Vector3(0, 2, 0));
    }

    private void Update()
    {
        if (!IsOwner || !isAlive) return;

        CheckTaskCompleting();
        CheckNearTask();
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

    // Code To Handle Task Prompt Text
    private void CheckNearTask()
    {
        if (!!taskInRadius && gameManager.isGamePlaying.Value) taskPromptText.SetActive(true);
        else taskPromptText.SetActive(false);
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

    // Shows crosshair effect for sheriff
    public void ShowCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    // Hides crosshair effect for sheriff
    public void HideCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.red;
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

    // Enables the task UI only for Outlaws
    [ClientRpc]
    public void ShowTaskUIClientRpc()
    {
        if (!IsOwner) return;

        Resources.FindObjectsOfTypeAll<TaskScreenController>().FirstOrDefault()?.gameObject.SetActive(true);
    }

    // Set isAlive to false on server and call client rpc
    [ServerRpc(RequireOwnership = false)]
    public void KillOutlawServerRpc()
    {
        if (!IsServer) return;

        isAlive = false;
        KillOutlawClientRpc();
    }

    // Hide "ghost" for sheriffs but make transparent for outlaws
    [ClientRpc]
    public void KillOutlawClientRpc()
    {
        bool isSheriff = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Sheriff>().enabled;

        if (!isSheriff)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 0, 0, 0.4f);
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.GetComponent<CircleCollider2D>().enabled = false;

        isAlive = false;
    }
}
