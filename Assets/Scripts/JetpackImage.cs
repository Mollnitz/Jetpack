using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JetpackImage : MonoBehaviour
{
    public Sprite max;
    public Sprite fire;
    public Sprite min;

    Image sr;
    RectTransform rect;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        PlayerMovementScript.JetpackEvent.AddListener(SetImage);
        StartCoroutine(WigglePicture());
    }

    private void SetImage(float arg0)
    {
        if(arg0 >= 1f)
        {
            sr.sprite = max;
        }
        else if( arg0 <= 0f)
        {
            sr.sprite = min;
        }
        else
        {
            sr.sprite = fire;
            
        }
    }

    IEnumerator WigglePicture()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            rect.Rotate(new Vector3(0,0, 0.05f * (float)(Math.Sin(Time.timeSinceLevelLoad))));


        }
    }
}
