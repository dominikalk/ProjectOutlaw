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
    [SerializeField] private Sprite sheriffSprite;
    private Crosshair crosshairObject;
    private Vector2 mousePosition;

    protected override void Start()
    {
        base.Start();

        isSheriff = true;
        GetComponent<SpriteRenderer>().sprite = sheriffSprite;

        // Hide cursor and instantiate cross hair object
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InstantiateCrosshairClientRpc();
    }

    protected override void Update()
    {
        base.Update();

        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && (crosshairObject.outlawsInCrosshair.Count > 0 || crosshairObject.npcsInCrosshair.Count > 0) && gameManager.isGamePlaying.Value)
        {
            foreach (Outlaw outlaw in crosshairObject.outlawsInCrosshair.ToArray())
            {
                outlaw.KillOutlawServerRpc();
            }
            foreach (NPC npc in crosshairObject.npcsInCrosshair.ToArray())
            {
                npc.KillNPCServerRpc();
            }
            gameManager.DecrementBulletsServerRpc();
        }

        if (gameManager.isGamePlaying.Value)
        {
            HandleCursorUpdate();
        }
    }

    // Handles logic relating to moving the cursor/ cross hair
    private void HandleCursorUpdate()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSpeed;

        mousePosition += mouseDelta;
        mousePosition = Vector2.ClampMagnitude(mousePosition, mouseRadius);

        crosshairObject.transform.position = new Vector3(mousePosition.x + transform.position.x, mousePosition.y + transform.position.y, -12f);
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
            for (int i = 0; i < 3; i++)
            {
                task.transform.GetChild(i).GetComponent<Renderer>().enabled = false;
            }
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