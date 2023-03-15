using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

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

        // Temporary - code to decrement bullets, i.e. shooting
        if (Input.GetKeyDown(KeyCode.E) && gameManager.isGamePlaying.Value)
        {
            gameManager.DecrementBulletsServerRpc();
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

    // Enables the sheriff UI only for sheriffs
    [ClientRpc]
    public void ShowSheriffUIClientRpc()
    {
        if (!IsOwner) return;

        Resources.FindObjectsOfTypeAll<SheriffScreenController>().FirstOrDefault()?.gameObject.SetActive(true);
    }
}