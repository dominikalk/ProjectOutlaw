using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float playerSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float xVelocity = 0;
        float yVelocity = 0;

        if (Input.GetKey(KeyCode.W))
        {
            yVelocity = playerSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            yVelocity = -playerSpeed;
        }

        if (Input.GetKey(KeyCode.A))
        {
            xVelocity = -playerSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            xVelocity = playerSpeed;
        }

        rb.velocity = new Vector2(xVelocity, yVelocity);
    }
}
