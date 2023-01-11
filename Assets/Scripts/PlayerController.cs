using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float PlayerSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xVelocity = 0;
        float yVelocity = 0;

        if (Input.GetKey(KeyCode.W)) {
            yVelocity = PlayerSpeed;
        } else if (Input.GetKey(KeyCode.S)) {
            yVelocity = -PlayerSpeed;
        } 
        
        if (Input.GetKey(KeyCode.A)) {
            xVelocity = -PlayerSpeed;
        } else if (Input.GetKey(KeyCode.D)) {
            xVelocity = PlayerSpeed;
        } 
        
        rb.velocity = new Vector2(xVelocity, yVelocity);
    }
}
