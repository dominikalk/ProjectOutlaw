using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using Unity.VisualScripting;

public class Sheriff : Player
{

    [SerializeField] private float mouseSpeed;
    [SerializeField] private float mouseRadius;
    [SerializeField] private GameObject crosshair;
    private GameObject crosshairObject;
    private Vector2 mousePosition;

    protected override void Start()
    {
        base.Start();

        isSheriff = true;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.green;

        // Hide cursor and instantiate cross hair object
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        crosshairObject = Instantiate(crosshair);

    }

    private void Update()
    {
        if (!IsOwner) return;

        // Temporary - code to decrement bullets, i.e. shooting
        if (Input.GetKeyDown(KeyCode.E) && gameManager.isGamePlaying.Value)
        {
            gameManager.DecrementBulletsServerRpc();
        }

        HandleCursorUpdate();
    }

    // Handles logic relating to moving the cursor/ cross hair
    private void HandleCursorUpdate()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSpeed;

        mousePosition += mouseDelta;
        mousePosition = Vector2.ClampMagnitude(mousePosition, mouseRadius);

        crosshairObject.transform.position = new Vector3(mousePosition.x + transform.position.x, mousePosition.y + transform.position.y, -2);
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