using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Sheriff : Player
{
    protected override void Start()
    {
        base.Start();

        isSheriff = true;
        // TODO: replace with change in sprite
        GetComponent<SpriteRenderer>().color = Color.green;
    }
}
