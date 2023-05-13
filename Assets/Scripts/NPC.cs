using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Unity.Netcode;
using System.Linq;

public class NPC : NetworkBehaviour
{
    [SerializeField] public NPCNode moveNode;
    [SerializeField] public float speed;
    private System.Random r = new System.Random();
    private int timeSinceTask;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        //GetComponent<SpriteRenderer>().color = Color.gray;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameManager.isGamePlaying.Value) return;

        HandleNPCMovement();
    }

    private void HandleNPCMovement()
    {
        Vector2 position = Vector2.MoveTowards(transform.position, moveNode.transform.position, speed * NetworkManager.Singleton.LocalTime.FixedDeltaTime);
        transform.position = position;

        if (!IsServer) return;

        if (Vector2.Distance(transform.position, moveNode.transform.position) < 0.2f)
        {
            List<NPCNode> currentAdjacentNodes = moveNode.adjacentNodes;

            int rInt = r.Next(0, currentAdjacentNodes.Count());

            MoveNPCServerRpc(position, NetworkObjectId, currentAdjacentNodes[rInt].NetworkObjectId);
        }
    }

    // Shows crosshair effect for sheriff
    public void ShowCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    // Hides crosshair effect for sheriff
    public void HideCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Set isAlive to false on server and call client rpc
    [ServerRpc(RequireOwnership = false)]
    public void KillNPCServerRpc()
    {
        if (!IsServer) return;

        NetworkObject.Despawn();
    }

    // Move NPC on server
    [ServerRpc(RequireOwnership = false)]
    public void MoveNPCServerRpc(Vector2 position, ulong objectId, ulong nodeId)
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        NPC npc = gameManager.npcs.Find(npc => npc.NetworkObjectId == objectId);

        if (npc == null) return;

        npc.gameObject.transform.position = position;
        npc.moveNode = FindObjectsOfType<NPCNode>().Where(node => node.NetworkObjectId == nodeId).FirstOrDefault();

        MoveNPCClientRpc(position, objectId, nodeId);
    }

    // Move NPC on clients
    [ClientRpc]
    private void MoveNPCClientRpc(Vector2 position, ulong objectId, ulong nodeId)
    {
        transform.position = position;
        moveNode = FindObjectsOfType<NPCNode>().Where(node => node.NetworkObjectId == nodeId).FirstOrDefault();
    }
}
