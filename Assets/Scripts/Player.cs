using QFSW.QC.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : NetworkBehaviour
{
    [SerializeField] private float movementSpeed;

    protected GameManager gameManager;
    protected bool isSheriff;

    protected bool isChatInputActive = false;
    protected ChatSystem chatSystem;
    protected bool joinedChat = false;

    private Vector2 gMoveDir = Vector2.zero;

    protected virtual void Start()
    {
        if (!IsOwner) return;

        gameManager = FindObjectOfType<GameManager>();

        // Find the ChatSystem object in the scene
        chatSystem = FindObjectOfType<ChatSystem>();
    }

    private void FixedUpdate()
    {
        if (IsServer) MovePlayerServerRpc(gMoveDir);
        if (!IsOwner || isChatInputActive) return;

        CheckMovement();
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
            chatSystem.chatInput.text = $"{roleText} joined!";
            chatSystem.SendMessage("Sheriff");
            chatSystem.chatInput.text = $"{roleText} joined!";
            chatSystem.SendMessage("Outlaw");
            joinedChat = true;
        }
    }

    // Code To Handle Player Movement
    private void CheckMovement()
    {
        Vector2 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.y += +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y += -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x += -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += +1f;

        Vector2 deltaPos = moveDir.normalized * movementSpeed * NetworkManager.Singleton.LocalTime.FixedDeltaTime;
        if (deltaPos != gMoveDir)
        {
            gMoveDir = deltaPos;
            MovePlayerServerRpc(deltaPos);
        }
    }

    // Move Player by difference
    [ServerRpc(RequireOwnership = false)]
    private void MovePlayerServerRpc(Vector2 deltaPos)
    {
        Debug.Log("Here");
        if (NetworkManager.ConnectedClients.ContainsKey(OwnerClientId))
        {
            var client = NetworkManager.ConnectedClients[OwnerClientId];
            client.PlayerObject.transform.SetPositionAndRotation(
                new Vector3(
                    client.PlayerObject.transform.position.x + deltaPos.x,
                    client.PlayerObject.transform.position.y + deltaPos.y,
                    -5f),
                Quaternion.identity);
            gMoveDir = deltaPos;
        }
    }

    // Move player by absolute 
    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerPosServerRpc(Vector3 position, ulong objectId)
    {
        GameManager gameManager = FindObjectOfType<GameManager>();

        Player player = gameManager.outlaws.Find(outlaw => outlaw.NetworkObjectId == objectId);
        if (player == null) player = gameManager.sheriffs.Find(sheriff => sheriff.NetworkObjectId == objectId);

        if (player == null) return;

        player.transform.SetPositionAndRotation(new Vector3(position.x, position.y, -5f), Quaternion.identity);

        PullZPosFrontClientRpc();
    }

    // Brings the users player to the front of all other players
    [ClientRpc]
    public void PullZPosFrontClientRpc()
    {
        if (!IsOwner) return;

        foreach (Player player in FindObjectsOfType<Player>())
        {
            player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -5f);
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    // Disables main camera and enables child camera for each client's player object
    [ClientRpc]
    public void HandlePlayerCameraClientRpc()
    {
        if (!IsOwner) return;

        Camera.main.gameObject.SetActive(false);
        transform.GetChild(0).gameObject.SetActive(true);
    }

    // Hide start menu ui
    [ClientRpc]
    public void HideStartMenuClientRpc()
    {
        if (!IsOwner) return;

        FindObjectOfType<NetworkManagerUI>().gameObject.SetActive(false);
    }
}
