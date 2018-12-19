using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;

public class Agent : MonoBehaviour {

	// Use this for initialization
	void Start () {
        tail = Instantiate(tail_prefab, transform.position, Quaternion.identity).gameObject;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void MoveInDirection(Vector3 dir)
    {
        tail.transform.position = transform.position;
        transform.position += dir;
    }

    public IEnumerable<double> LookAtYourself()
    {
        if (tail.transform.position.x - transform.position.x == 1)
            yield return 1;
        else
            yield return 0;
        if (tail.transform.position.x - transform.position.x == -1)
            yield return 1;
        else
            yield return 0;
        if (tail.transform.position.y - transform.position.y == 1)
            yield return 1;
        else
            yield return 0;
        if (tail.transform.position.y - transform.position.y == -1)
            yield return 1;
        else
            yield return 0;
    }

    public NeuralNet brain;

    public double Score = 0;
    public bool finished = false;

    GameObject tail;
    public Transform tail_prefab;
}
