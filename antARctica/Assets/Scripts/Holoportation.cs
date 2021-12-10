using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Holoportation : MonoBehaviour
{

    public GameObject Antarctica;

    void Start()
    {
        Antarctica.transform.position = new Vector3(-10.7f, 0f, 150f);
    }

}
