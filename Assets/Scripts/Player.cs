using QFSW.QC.Utilities;
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
    [SerializeField] protected float taskRadius;

    protected GameManager gameManager;
    protected bool isSheriff;

    protected bool isChatInputActive = false;
    protected ChatSystem chatSystem;
    protected bool joinedChat = false;

    protected virtual void Start()
    {
        if (!IsOwner) return;

        gameManager = FindObjectOfType<GameManager>();

        // Find the ChatSystem object in the scene
        chatSystem = FindObjectOfType<ChatSystem>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        if (!isChatInputActive)
        {
            CheckMovement();
        }
    }

    protected virtual void Update()
    {
        if (!IsOwner) return;

        CheckPlayerMessaging();
    }

    private void CheckPlayerMessaging()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            chatSystem.SendMessage(isSheriff ? "Sheriff" : "Outlaw");
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
            string roleText = isSheriff ? "Sheriff" : "Outlaw";
            chatSystem.chatInput.text = $"{roleText} joined the game!";
            chatSystem.SendMessage(isSheriff ? "Sheriff" : "Outlaw");
            joinedChat = true;
        }
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

    // TODO: make protected private and make requires ownership true
    [ServerRpc(RequireOwnership = false)]
    protected void MovePlayerServerRpc(Vector3 deltaPos)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            client.PlayerObject.transform.Translate(deltaPos);
        }
    }

    // Brings the users player to the front of all other players
    [ClientRpc]
    public void PullZPosFrontClientRpc()
    {
        if (!IsOwner) return;
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
    }

    // Disables main camera and enables child camera for each client's player object
    [ClientRpc]
    public void HandlePlayerCameraClientRpc()
    {
        if (!IsOwner) return;

        Camera.main.gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
