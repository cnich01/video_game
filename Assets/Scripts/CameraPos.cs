using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPos : MonoBehaviour
{
    public Transform trackedObject;

    private MeshRenderer _renderer;
    public bool firstPerson = true;
    void Start()
    {
        _renderer = trackedObject.gameObject.GetComponent<MeshRenderer>();
    }

    void Update()
    {
        //switch between first and third person;
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (firstPerson)
            {
                transform.position += new Vector3(0.0f, 1.75f, -15.0f);
            }
            else
            {
                transform.position += new Vector3(0.0f, -1.75f, 15.0f);
                transform.LookAt(trackedObject.position);
            }
            firstPerson = !firstPerson;
        }
        // _renderer.enabled = !firstPerson;
    }
}
