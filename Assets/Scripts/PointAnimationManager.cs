using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointAnimationManager : MonoBehaviour
{
    int childindex = 0;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.gameManager.pointEvent.AddListener(PointAnimation);

    }

    private void PointAnimation(GameObject arg0, int arg1)
    {
        arg0.GetComponent<Collider2D>().enabled = false;
        StartCoroutine(SmoothMove(arg0, transform.GetChild(childindex++)));

        if(childindex == gameObject.transform.childCount)
        {
            GetComponent<Animator>().enabled = true;
            GetComponent<Collider2D>().enabled = true;
        }

    }

    IEnumerator SmoothMove(GameObject mover, Transform location)
    {
        Vector3 speed = Vector3.zero;
        do
        {

            mover.transform.position = Vector3.SmoothDamp(mover.transform.position, location.position, ref speed, 0.5f);

            yield return new WaitForEndOfFrame();

        } while (Vector3.Distance(mover.transform.position, location.position) > 0.01f);
        
    }

}
