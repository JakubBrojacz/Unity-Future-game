﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using NeuralNetwork;
using System.Linq;

public class Agent : MonoBehaviour {

    // Use this for initialization
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {

    }
    public void Init()
    {
        tail = Instantiate(tail_prefab, transform.position, Quaternion.identity).gameObject;
        Score = 0.1;
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

    public void StepOnTile(Vector3Int t)
    {
        if (tiles.Contains(t))
            return;
        tiles.Add(t);
        Score++;
    }

    public void Reset(Vector3 pos)
    {
        tiles.Clear();
        Score = 0.1;
        Dead = false;
        transform.position = pos;
        tail.transform.position = pos;
    }

    public bool Dead
    {
        get
        {
            return dead;
        }
        set
        {
            dead = value;
            if (dead && OnDeath != null)
                OnDeath.Invoke(this, null);
        }
    }

    private HashSet<Vector3Int> tiles = new HashSet<Vector3Int>();

    public NEAT brain;

    public double Score = 0;
    private bool dead = false;

    public GameObject tail;
    public Transform tail_prefab;

    public event EventHandler OnDeath;
}

public static class AgentList
{
    public static void FindParents(this IEnumerable<Agent> list, out Agent parent1, out Agent parent2)
    {
        list.FindParents(out parent1);
        list.FindParents(out parent2);
    }

    public static void FindParents(this IEnumerable<Agent> list, out Agent parent)
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
