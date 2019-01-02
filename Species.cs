using System.Collections.Generic;
using UnityEngine;
using NeuralNetwork;

public class Species{

    public List<Agent> members;
    private NeuralNetwork.NEAT _reprezentative;
    public NeuralNetwork.NEAT Reprezentative
    {
        get { return _reprezentative; }
        set
        {
            _reprezentative = value;
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
                    Object.Destroy(agent.tail);
                    Object.Destroy(agent.gameObject);
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

    public void PerformMatingRitual()
    {

    }
}

public static class Speciacion
{
    public static void Speciacte(this List<Species> species, List<Agent> population)
    {
        //find new reprezentatives
        foreach (var s in species)
        {
            Agent closest_a = null;
            float min_score = Constants.Con.delta_t;
            int i = 0;
            int mini = 0;
            foreach(var agent in population)
            {
                float score = agent.brain.SameSpecies(s.Reprezentative);
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

        //assign every agent to closest species
        foreach (var agent in population)
        {
            Species closest = null;
            float min_score = Constants.Con.delta_t;
            foreach (var s in species)
            {
                float score = agent.brain.SameSpecies(s.Reprezentative);
                if(score<min_score)
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
}
