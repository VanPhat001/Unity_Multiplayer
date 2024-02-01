using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] List<Vector3> spawnPoints;
    [SerializeField] float moveSpeed = 5f;

    void Start()
    {
        Debug.Log("Server | Host " + IsNetworkHostOrServer());
        Debug.Log("Client " + IsNetworkClient());


        this.transform.position = spawnPoints[(int)OwnerClientId];
    }

    public bool IsNetworkHostOrServer()
    {
        return IsServer || IsHost;
    }

    public bool IsNetworkClient()
    {
        return !IsNetworkHostOrServer();
    }

    void HandleMovement()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * Time.deltaTime * moveSpeed);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * Time.deltaTime * moveSpeed);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * Time.deltaTime * moveSpeed);
        }
    }

    private void Update() {
        HandleMovement();
    }
}
