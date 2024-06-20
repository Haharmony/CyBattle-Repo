using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DmgAllObjSpawner : MonoBehaviourPunCallbacks
{
    public GameObject damageObjectPrefab; // Reference to the DamageObject prefab
    public Vector3 spawnPosition; // Spawn position
    public float spawnCooldown = 60f; // Spawn cooldown in seconds

    private bool isSpawning = false;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnDamageObject();
        }
    }

    public void StartSpawnCooldown()
    {
        if (!isSpawning)
        {
            StartCoroutine(SpawnDamageObjectWithCooldown());
        }
    }

    private void SpawnDamageObject()
    {
        PhotonNetwork.Instantiate(damageObjectPrefab.name, spawnPosition, Quaternion.identity);
    }

    private IEnumerator SpawnDamageObjectWithCooldown()
    {
        isSpawning = true;
        yield return new WaitForSeconds(spawnCooldown);
        SpawnDamageObject();
        isSpawning = false;
    }
}
