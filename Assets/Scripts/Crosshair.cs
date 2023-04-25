using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Crosshair : NetworkBehaviour
{
    public List<Outlaw> outlawsInCrosshair = new List<Outlaw>();

    // Add outlaw to list and add hover effect
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Outlaw outlaw = collision.gameObject.GetComponent<Outlaw>();

        if (outlaw == null || !outlaw.isActiveAndEnabled || outlawsInCrosshair.Contains(outlaw)) return;

        outlawsInCrosshair.Add(outlaw);
        outlaw.ShowCrosshairHover();
    }

    // Remove outlaw from list and add hover effect
    private void OnTriggerExit2D(Collider2D collision)
    {
        Outlaw outlaw = collision.gameObject.GetComponent<Outlaw>();

        if (outlaw == null || !outlaw.isActiveAndEnabled || !outlawsInCrosshair.Contains(outlaw)) return;

        outlawsInCrosshair.Remove(outlaw);
        outlaw.HideCrosshairHover();
    }
}
