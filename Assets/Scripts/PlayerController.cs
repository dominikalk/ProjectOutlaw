using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float playerSpeed;

    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (int previousValue, int newValue) =>
        {
            Debug.Log(OwnerClientId + "; randomNumber:" + randomNumber.Value);
        };
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKey(KeyCode.T))
        {
            randomNumber.Value = Random.Range(0, 100);
        }
        Vector2 playerVelocity = new Vector2(0, 0);

        if (Input.GetKey(KeyCode.W)) playerVelocity.y = playerSpeed;
        else if (Input.GetKey(KeyCode.S)) playerVelocity.y = -playerSpeed;
        if (Input.GetKey(KeyCode.A)) playerVelocity.x = -playerSpeed;
        else if (Input.GetKey(KeyCode.D)) playerVelocity.x = playerSpeed;

        rb.velocity = playerVelocity;
    }
}
