using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Game.Instance.ShouldRotate)
        {
            transform.Rotate(Vector3.up, Game.Instance.Speed * Time.deltaTime);
        }
    }
}
