using UnityEngine;
using System.Collections;

public class WaterFX : MonoBehaviour {

    public float speed_x;
    public float speed_y;

    private Renderer render;
    Vector2 offset = Vector2.zero;

    void Awake () {
        render = GetComponent<Renderer>();
    }
	
	void Update () {
        
        offset += new Vector2(speed_x, speed_y) * Time.deltaTime;
        render.material.mainTextureOffset = offset;
        
    }
}
