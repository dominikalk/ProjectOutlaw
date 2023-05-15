using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
using System.Linq;

public class NPC : NetworkBehaviour
{
    [SerializeField] public float speed;
    private System.Random r = new System.Random();
    private GameManager gameManager;

    private Vector2 nextPos = Vector2.zero;
    private int[] angles = new int[] { 0, 45, -45, 90, -90, 135, -135, 180 };

    private bool waiting;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (!gameManager.isGamePlaying.Value) return;

        HandleNPCMovement();
    }

    // Handles client side player movement
    private void HandleNPCMovement()
    {
        Vector2 position = Vector2.MoveTowards(transform.position, nextPos, speed * NetworkManager.Singleton.LocalTime.FixedDeltaTime);
        transform.position = new Vector3(position.x, position.y, -5f);

        if (!IsServer) return;

        if ((Vector2)transform.position == nextPos) FindNewTargetPosition();
    }

    // Finds new location to head towards
    private void FindNewTargetPosition(int angle = 200)
    {
        float dist = speed * UnityEngine.Random.Range(0f, 5f);
        if (angle == 200) angle = angles[r.Next(8)];

        Vector2 deltaPos = new Vector2(dist * Mathf.Cos(Mathf.Deg2Rad * angle), dist * Mathf.Sin(Mathf.Deg2Rad * angle));

        if (waiting == false) StartCoroutine(WaitToMove(transform.position, (Vector2)transform.position + deltaPos, NetworkObjectId));
    }

    // Waits to move if the NPC is meant to wait
    private IEnumerator WaitToMove(Vector2 position, Vector2 nextPos, ulong objectId)
    {
        waiting = true;
        int rInt = r.Next(3);

        if (rInt == 0) yield return new WaitForSeconds(UnityEngine.Random.Range(0, 5));

        MoveNPCServerRpc(position, nextPos, objectId);
        waiting = false;
    }

    // Shows crosshair effect for sheriff
    public void ShowCrosshairHover()
    {
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    // Hides crosshair effect for sheriff
    public void HideCrosshairHover()
    {
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
    public void MoveNPCServerRpc(Vector2 position, Vector2 nextPos, ulong objectId)
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();
        NPC npc = gameManager.npcs.Find(npc => npc.NetworkObjectId == objectId);

        if (npc == null) return;

        npc.gameObject.transform.position = new Vector3(position.x, position.y, -5f);
        npc.nextPos = nextPos;

        MoveNPCClientRpc(position, nextPos);
    }

    // Move NPC on clients
    [ClientRpc]
    private void MoveNPCClientRpc(Vector2 position, Vector2 nextPos)
    {
        transform.position = new Vector3(position.x, position.y, -5f);
        this.nextPos = nextPos;
    }

    // Logic to change direction if NPC collides with another object
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsServer || collision == null || collision.gameObject.CompareTag("Crosshair")) return;

        Vector2 normal = Vector2.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (!collision.gameObject.CompareTag("Crosshair"))
            {
                normal = collision.GetContact(i).normal;
                break;
            }
        }

        float collisionAngle;
        if (normal.x != 0) collisionAngle = 90 + (normal.x * -90);
        else collisionAngle = 90 * normal.y;

        Dictionary<int, int> deltaAngles = new Dictionary<int, int>();

        foreach (int angle in angles)
        {
            deltaAngles[angle] = Mathf.Abs((int)collisionAngle - angle);
        }

        IOrderedEnumerable<KeyValuePair<int, int>> sortedDict = from entry in deltaAngles orderby entry.Value ascending select entry;

        int newangle = sortedDict.ElementAt(r.Next(3)).Key;

        MoveNPCServerRpc(transform.position, transform.position, NetworkObjectId);
        FindNewTargetPosition(newangle);
    }
}
