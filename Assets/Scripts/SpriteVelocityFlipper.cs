using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteVelocityFlipper : MonoBehaviour
{
    SpriteRenderer sr;
    Rigidbody2D rb2d;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        sr.flipX = rb2d.velocity.x > 0f;
    }
}
