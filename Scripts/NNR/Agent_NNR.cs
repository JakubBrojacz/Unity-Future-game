using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;
using System.Linq;
using System;

public class Agent_NNR : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Collision");
        if (collision.gameObject.layer == 10)
            Dead = true;
        else if (collision.gameObject.layer == 9)
        {
            collision.gameObject.GetComponent<Tree_NNR>().eaten = true;
            Score += 1;
        }
    }

    public void MoveInDirection(Vector3 dir)
    {
        transform.position += dir;
    }
    public void Rotate(float angle)
    {
        transform.Rotate(0, 0, angle);
    }

    public void Init()
    {
        Score = 0.1;
    }

    public bool Dead = false;
    public NEAT brain;
    public double Score = 0;
}

public static class Agent_NNRList
{
    public static void FindParents(this IEnumerable<Agent_NNR> list, out Agent_NNR parent1, out Agent_NNR parent2)
    {
        list.FindParents(out parent1);
        list.FindParents(out parent2);
    }

    public static void FindParents(this IEnumerable<Agent_NNR> list, out Agent_NNR parent)
    {
        double sum = list.Sum(a => a.Score);
        double run = NEAT.RandomGenerator.NextDouble() * sum;
        foreach (var agent in list)
        {
            if (run < agent.Score)
            {
                parent = agent;
                return;
            }
            run -= agent.Score;
        }
        throw new Exception("Something gone wrong with finding parent");
    }
}