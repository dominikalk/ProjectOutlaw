using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCNode : MonoBehaviour
{
    [SerializeField] public List<NPCNode> adjacentNodes = new List<NPCNode>();
    [SerializeField] public Boolean isTask;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
