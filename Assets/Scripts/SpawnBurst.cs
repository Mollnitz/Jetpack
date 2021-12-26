using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBurst : MonoBehaviour
{
    [SerializeField]
    GameObject burst;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.gameManager.pointEvent.AddListener(spawnBurst);
    }

    void spawnBurst(GameObject point, int _)
    {
        GameObject burstInstance = GameObject.Instantiate(burst, point.transform.position, Quaternion.identity);
        Destroy(burstInstance, 1.5f);
    }

}
