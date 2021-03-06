﻿#define EXISTING_REPRESENTATIVE

using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;
using System.Linq;
using System;

public class Species{

    public List<Agent> members;
    private NeuralNetwork.NEAT _reprezentative;
    public NeuralNetwork.NEAT Reprezentative
    {
        get { return _reprezentative; }
        set
        {
            _reprezentative = value.Copy();
        }
    }
    private bool _dead;
    public bool Dead
    {
        get
        {
            return _dead;
        }
        set
        {
            _dead = value;
            if(_dead)
            {
                foreach(var agent in members)
                {
                    UnityEngine.Object.Destroy(agent.tail);
                    UnityEngine.Object.Destroy(agent.gameObject);
                }
                members.Clear();
            }
        }
    }
    private string _name;
    public string Name { get { return _name; } }
    public double Score { set; get; }

    static public int SpeciesNo = 0;
    public Species(Agent rep)
    {
        members = new List<Agent>();
        Reprezentative = rep.brain;
        members.Add(rep);
        Dead = false;
        _name = SpeciesNo.ToString();
        SpeciesNo++;
    }

    public void Add(Agent newMember)
    {
        members.Add(newMember);
    }

}

public static class Speciacion
{
    public static void Speciacte(this List<Species> species, List<Agent> population)
    {
        //find new reprezentatives
#if EXISTING_REPRESENTATIVE
        foreach (var s in species)
        {
            Agent closest_a = null;
            float min_score = Constants.Con.new_representative_max_distance;
            int i = 0;
            int mini = 0;
            foreach(var agent in population)
            {
                float score = agent.brain.Distance(s.Reprezentative);
                if (score < min_score)
                {
                    min_score = score;
                    closest_a = agent;
                    mini = i;
                }
                i++;
            }
            if (closest_a != null)
            {
                s.Reprezentative = closest_a.brain;
                s.Add(closest_a);
                population.RemoveAt(mini);
            }
            else
                s.Dead = true;
        }
        species.RemoveAll(s => s.Dead);
#endif

        //assign every agent to closest species
        foreach (var agent in population)
        {
            Species closest = null;
            float min_score = Constants.Con.delta_t;
            foreach (var s in species)
            {
                float score = agent.brain.Distance(s.Reprezentative);
                if (score < min_score)
                {
                    min_score = score;
                    closest = s;
                }
            }
            if (closest != null)
                closest.Add(agent);
            else
                species.Add(new Species(agent));
        }

    }

    public static List<Agent> PerformMatingRitual(this List<Species> species, Transform agentPrefab, out string description)
    {
        //calculate size of spieces in next generation
        foreach (var s in species)
        {
            if (s.members.Count < 1)
                s.Dead = true;
            s.Score = s.members.Sum(a => a.Score) / s.members.Count;
        }
        species.RemoveAll(s => s.Dead);
        var scoresSum = species.Sum(s => s.Score);
        species.ForEach(s => s.Score *= Constants.Con.populationSize / scoresSum); //normalization

        //next generation
        List<Agent> nextGeneration = new List<Agent>();
        description = "";
        foreach (var s in species)
        {
            //delete bad species
            if (s.Score < 1)
            {
                s.Dead = true;
                continue;
            }

            //mark leader
            //s.members[0].GetComponent<SpriteRenderer>().sprite = blueSprite;
            //s.members[0].transform.position += new Vector3(0, 0, 0.5f);
            s.members.Sort((a1, a2) => -a1.Score.CompareTo(a2.Score));
            //s.members[0].GetComponent<SpriteRenderer>().sprite = redSprite;
            //s.members[0].transform.position += new Vector3(0, 0, -0.5f);

            //update information string
            description += "Licznosc gatunku " + s.Name + ": " + s.members.Count + "; Najwyzszy wynik: " + s.members[0].Score + Environment.NewLine;

            //create next generation
            for (int i = 0; i < s.Score * Constants.Con.crossover_chanse; i++)
            {
                Transform offspring = UnityEngine.Object.Instantiate(agentPrefab, Constants.Con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                offspring.GetComponent<Agent>().Init();
                Agent p1, p2;
                s.members.FindParents(out p1, out p2);
                if (p1.Score > p2.Score)
                {
                    Agent tmp = p1;
                    p1 = p2;
                    p2 = tmp;
                }
                offspring.GetComponent<Agent>().brain = p1.brain.Crossover(p2.brain);
                offspring.GetComponent<Agent>().brain.Mutate();
                nextGeneration.Add(offspring.GetComponent<Agent>());
            }
            for (int i = 0; i < s.Score * (1 - Constants.Con.crossover_chanse); i++)
            {
                Transform offspring = UnityEngine.Object.Instantiate(agentPrefab, Constants.Con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                offspring.GetComponent<Agent>().Init();
                Agent p1;
                s.members.FindParents(out p1);
                offspring.GetComponent<Agent>().brain = p1.brain.Copy();
                offspring.GetComponent<Agent>().brain.Mutate();
                nextGeneration.Add(offspring.GetComponent<Agent>());
            }
            if (s.Score > 4)
            {
                nextGeneration.Add(s.members[0]);
                s.members.RemoveAt(0);
            }
            foreach (var agent in s.members)
            {
                GameObject.Destroy(agent.tail);
                GameObject.Destroy(agent.gameObject);
            }
            s.members.Clear();
        }
        species.RemoveAll(s => s.Dead);
        return nextGeneration;
    }
}
