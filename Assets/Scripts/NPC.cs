using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Unity.Netcode;

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
        GetComponent<SpriteRenderer>().color = Color.gray;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!IsServer || !gameManager.isGamePlaying.Value) return;

        HandleNPCMovement();
    }

    private void HandleNPCMovement()
    {
        Vector2 position = Vector3.MoveTowards(transform.position, moveNode.transform.position, speed * Time.deltaTime);
        MoveNPCServerRpc(position, NetworkObjectId);
        if (Vector3.Distance(transform.position, moveNode.transform.position) < 0.2f)
        {
            if (moveNode.isTask == true)
            {
                int wait = r.Next(0, 2);
                if (wait == 1)
                {
                    StartCoroutine(waitOnTask());
                }
            }
            List<NPCNode> currentAdjacentNodes = moveNode.adjacentNodes;

            int rInt = r.Next(0, currentAdjacentNodes.Count);
            moveNode = currentAdjacentNodes[rInt];
        }
    }

    IEnumerator waitOnTask()
    {

        yield return new WaitForSeconds(7);

    }

    // Shows crosshair effect for sheriff
    public void ShowCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.cyan;
    }

    // Hides crosshair effect for sheriff
    public void HideCrosshairHover()
    {
        // TODO: replace with glow effect
        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    // Set isAlive to false on server and call client rpc
    [ServerRpc(RequireOwnership = false)]
    public void KillNPCServerRpc()
    {
        if (!IsServer) return;

        NetworkObject.Despawn();
    }

    // Move NPC on server
    [ServerRpc]
    private void MoveNPCServerRpc(Vector3 position, ulong objectId)
    {
        NPC npc = gameManager.npcs.Find(npc => npc.NetworkObjectId == objectId);

        if (npc == null) return;

        npc.gameObject.transform.position = position;

        MoveNPCClientRpc(position, objectId);
    }

    // Move NPC on clients
    [ClientRpc]
    private void MoveNPCClientRpc(Vector3 position, ulong objectId)
    {
        NPC npc = gameManager.npcs.Find(npc => npc.NetworkObjectId == objectId);

        if (npc == null) return;

        npc.gameObject.transform.position = position;
    }
}
