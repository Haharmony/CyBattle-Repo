using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DamageAllObject : MonoBehaviourPunCallbacks
{
    public float damageAmount = 0.2f; // Amount of damage to apply
    public float rotationSpeed = 30f; // Speed at which the object rotates

    private static HashSet<int> damagedPlayers = new HashSet<int>();
    private static int currentInstanceID = 0;
    private int instanceID;

    private DmgAllObjSpawner spawner;

    private void Start()
    {
        spawner = FindObjectOfType<DmgAllObjSpawner>();
        instanceID = currentInstanceID++;
        damagedPlayers.Clear();
    }

    private void Update()
    {
        // Rotate the object around its up axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the PhotonView of the player who triggered the damage object
            PhotonView playerPhotonView = other.GetComponent<PhotonView>();
            if (playerPhotonView != null && playerPhotonView.IsMine)
            {
                // Notify all players to apply damage except the triggering player
                photonView.RPC("ApplyDamageToAllExcept", RpcTarget.All, playerPhotonView.ViewID, damageAmount, instanceID);
                // Notify the spawner to start the cooldown
                spawner.StartSpawnCooldown();
                // Destroy the object for all players
                photonView.RPC("DestroyObject", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    void ApplyDamageToAllExcept(int triggeringPlayerID, float damage, int instanceID)
    {
        if (this.instanceID != instanceID) return; // Ensure we're only handling the current instance

        // Find all players in the scene
        DisplayColor[] players = FindObjectsOfType<DisplayColor>();
        string triggeringPlayerName = null;

        // Retrieve the triggering player's name
        foreach (DisplayColor player in players)
        {
            PhotonView playerPhotonView = player.GetComponent<PhotonView>();
            if (playerPhotonView.ViewID == triggeringPlayerID)
            {
                triggeringPlayerName = playerPhotonView.Owner.NickName;
                break;
            }
        }

        if (triggeringPlayerName == null)
        {
            Debug.LogError("Triggering player not found.");
            return;
        }

        // Apply damage to all other players once
        foreach (DisplayColor player in players)
        {
            PhotonView playerPhotonView = player.GetComponent<PhotonView>();
            if (playerPhotonView.ViewID != triggeringPlayerID && !damagedPlayers.Contains(playerPhotonView.ViewID))
            {
                // Apply damage to the player
                player.ReceiveDamage(triggeringPlayerName, damage);
                damagedPlayers.Add(playerPhotonView.ViewID);
            }
        }
    }

    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }
}