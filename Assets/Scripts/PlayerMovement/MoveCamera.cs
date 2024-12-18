using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MoveCamera : MonoBehaviour
{
    public Transform target; // The target the camera should follow

    void Update()
    {
        // Finds the player object by tag
        if (target == null)
        {
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in allPlayers)
            {
                if (player.GetComponent<NetworkObject>()?.IsOwner == true)
                {
                    target = player.transform;
                }
            }
        }
        else
        {
            transform.position = target.position;
        }
    }
}
