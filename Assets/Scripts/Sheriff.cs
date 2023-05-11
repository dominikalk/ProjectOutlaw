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
    private Crosshair crosshairObject;
    private Vector2 mousePosition;

    private ChatSystem chatSystem;
    private bool joinedChat = false;

    protected override void Start()
    {
        base.Start();

        isSheriff = true;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.green;

        // Hide cursor and instantiate cross hair object
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InstantiateCrosshairClientRpc();

        // Find the ChatSystem object in the scene
        chatSystem = FindObjectOfType<ChatSystem>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && crosshairObject.outlawsInCrosshair.Count > 0 && gameManager.isGamePlaying.Value)
        {
            foreach (Outlaw outlaw in crosshairObject.outlawsInCrosshair.ToArray())
            {
                outlaw.KillOutlawServerRpc();
            }
            gameManager.DecrementBulletsServerRpc();
        }

        if (gameManager.isGamePlaying.Value)
        {
            HandleCursorUpdate();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            chatSystem.SendMessage("Sheriff");
            chatSystem.chatInput.DeactivateInputField();
            isChatInputActive = false;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            chatSystem.chatInput.Select();
            chatSystem.chatInput.ActivateInputField();
            isChatInputActive = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            chatSystem.chatInput.DeactivateInputField();
            isChatInputActive = false;
        }

        // Send an initial message to the chat system if it hasn't been sent before
        if (!joinedChat)
        {
            chatSystem.chatInput.text = "Sheriff joined the game!";
            chatSystem.SendMessage("Sheriff");
            joinedChat = true;
        }
    }

    // Handles logic relating to moving the cursor/ cross hair
    private void HandleCursorUpdate()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSpeed;

        mousePosition += mouseDelta;
        mousePosition = Vector2.ClampMagnitude(mousePosition, mouseRadius);

        crosshairObject.transform.position = new Vector3(mousePosition.x + transform.position.x, mousePosition.y + transform.position.y, -2);
    }

    // Instantiates crosshair only on specific sheriff client
    [ClientRpc]
    private void InstantiateCrosshairClientRpc()
    {
        if (!IsOwner) return;

        crosshairObject = Instantiate(crosshair).GetComponent<Crosshair>();
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