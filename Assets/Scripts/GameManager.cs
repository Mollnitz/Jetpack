using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PointEvent : UnityEvent<GameObject, int> { }

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    public PointEvent pointEvent;

    private void Awake()
    {
        if(gameManager != null)
        {
            DestroyImmediate(this);
        }
        else
        {
            gameManager = this;
            pointEvent = new PointEvent();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
