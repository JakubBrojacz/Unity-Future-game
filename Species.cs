using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Species{

    public List<Agent> members;
    public Agent reprezentative
    {
        get
        {
            return members[0];
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
