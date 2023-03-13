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

    protected virtual void Start()
    {
        if (!IsOwner) return;

        gameManager = FindObjectOfType<GameManager>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        CheckMovement();
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

    [ServerRpc]
    private void MovePlayerServerRpc(Vector3 deltaPos)
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
}
