using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValueReader : MonoBehaviour
{
    Slider slider;
    PlayerMovementScript playerMovementScript;
    // Start is called before the first frame update
    void Start()
    {
        slider = GetComponent<Slider>();
        PlayerMovementScript.JetpackEvent.AddListener(SetSlider);
        
    }

    void SetSlider(float value)
    {
        slider.value = value;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
