using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGround : MonoBehaviour
{
    [SerializeField] GameObject originalTile;
    Vector3 originalTilePos;
    [SerializeField] int lineSpawnAmount;
    // [SerializeField] int lines = 2;

    float positionOffset = 0.1f;

    void Start() 
    {
        originalTilePos = originalTile.transform.position;

        for (int i = 0; i < lineSpawnAmount; i++)
        {            
            Vector3 spawnPosition = new Vector3(originalTilePos.x + (i * positionOffset), originalTilePos.y, originalTilePos.z);
            Instantiate(originalTile, spawnPosition, Quaternion.identity);
        }


        for (int i = 0; i < lineSpawnAmount; i++)
        {            
            Vector3 spawnPosition = new Vector3(originalTilePos.x + (i * positionOffset), originalTilePos.y - positionOffset, originalTilePos.z);

            Instantiate(originalTile, spawnPosition, Quaternion.identity);
        }        
    }
}
