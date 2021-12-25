using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlerpCamera : MonoBehaviour
{
    [SerializeField]
    Transform followTarget;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position + new Vector3(0,0,-10), followTarget.position, 0.1f);
    }
}
