using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Crosshair : NetworkBehaviour
{
    public List<Outlaw> outlawsInCrosshair = new List<Outlaw>();
    public List<NPC> npcsInCrosshair = new List<NPC>();

    private GameManager gameManger;

    private void Start()
    {
        gameManger = FindObjectOfType<GameManager>();
    }

    // Add outlaw/ npc to list and add hover effect
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameManger.isGamePlaying.Value || collision.isTrigger) return;

        Outlaw outlaw = collision.gameObject.GetComponent<Outlaw>();
        NPC npc = collision.gameObject.GetComponent<NPC>();

        if (outlaw != null && outlaw.isActiveAndEnabled && !outlawsInCrosshair.Contains(outlaw))
        {
            outlawsInCrosshair.Add(outlaw);
            outlaw.ShowCrosshairHover();
        };

        if (npc != null && !npcsInCrosshair.Contains(npc))
        {
            npcsInCrosshair.Add(npc);
            npc.ShowCrosshairHover();
        }

    }

    // Remove outlaw/ npc from list and add hover effect
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!gameManger.isGamePlaying.Value || collision.isTrigger) return;

        Outlaw outlaw = collision.gameObject.GetComponent<Outlaw>();
        NPC npc = collision.gameObject.GetComponent<NPC>();

        if (outlaw != null && outlaw.isActiveAndEnabled && outlawsInCrosshair.Contains(outlaw))
        {
            outlawsInCrosshair.Remove(outlaw);
            outlaw.HideCrosshairHover();
        };

        if (npc != null && npcsInCrosshair.Contains(npc))
        {
            npcsInCrosshair.Remove(npc);
            npc.HideCrosshairHover();
        }
    }
}
