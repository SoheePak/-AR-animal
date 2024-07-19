using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpriteShadow : MonoBehaviour
{
    void Start()
    {
        if (GetComponent<Renderer>())
        {
            GetComponent<Renderer>().receiveShadows = true;
        }
    }

    void Update()
    {
        
    }
}
