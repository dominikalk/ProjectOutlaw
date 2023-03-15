using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sheriff : Player
{
    protected override void Start()
    {
        base.Start();

        isSheriff = true;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.green;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && gameManager.isGamePlaying.Value)
        {
            gameManager.DecrementBullets();
        }
    }

    // Hides tasks for the Sheriffs
    [ClientRpc]
    public void HideTasksClientRpc()
    {
        if (!IsOwner) return;

        foreach (TaskController task in FindObjectsOfType<TaskController>())
        {
            task.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            task.transform.GetChild(1).GetComponent<Renderer>().enabled = false;
        }
    }
}