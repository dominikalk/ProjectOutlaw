using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class NPCScript : MonoBehaviour
{
    [SerializeField] public NPCNode moveNode;
    [SerializeField] public float speed;
    private System.Random r = new System.Random();
    private int timeSinceTask;


    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, moveNode.transform.position, speed * Time.deltaTime);
        if (transform.position == moveNode.transform.position)
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
}
