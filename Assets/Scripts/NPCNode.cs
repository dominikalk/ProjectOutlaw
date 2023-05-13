using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NPCNode : NetworkBehaviour
{
    [SerializeField] public List<NPCNode> adjacentNodes = new List<NPCNode>();
}
