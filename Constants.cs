using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//it was static class with const values, but I found it usefull to modify them during runtime
public class Constants : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float seconds_every_tic = 0.5f;
    public int number_of_ticks_every_simulation = 10;
    public Vector3 initial_position = new Vector3(5, 0, -1); //where to spanw agents
}
