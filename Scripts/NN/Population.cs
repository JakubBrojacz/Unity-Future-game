#define NEAT

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using NeuralNetwork;
using System.Linq;

//game-object spawning agents and controlling their behaviour
public class Population : MonoBehaviour {

	// Use this for initialization
	void Start () {        
        //random = new System.Random(0);
        Constants.Con = con;
#if NEAT
        con.visual = visual;
        visual.Init();
        species = new List<Species>();
#endif



        //List<Agent> testl = new List<Agent>();
        //for(int i=0;i<5;i++)
        //{
        //    Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        //    c.GetComponent<Agent>().Score = i;
        //    testl.Add(c.GetComponent<Agent>());
        //}
        //for (int i = 0; i < 1000; i++)
        //{
        //    Debug.Log((int)(random.NextGaussian(0,4)));
        //}
        //Debug.Break();

        generation = new List<Agent>();
        for (int i = 0; i < con.populationSize; i++)
        {
            Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            c.GetComponent<Agent>().Init();
#if !NEAT
            c.GetComponent<Agent>().brain = new NeuralNet(con.input_layer, 8, con.output_layer, 1);
#else
            c.GetComponent<Agent>().brain = new NEAT(con.input_layer, con.output_layer, true);
            c.GetComponent<Agent>().brain.AddSynapse();
            c.GetComponent<Agent>().brain.AddSynapse();
            c.GetComponent<Agent>().brain.Mutate();
            c.GetComponent<Agent>().brain.Mutate();
#endif
            //c.GetComponent<Agent>().OnDeath += AgentDied;
            generation.Add(c.GetComponent<Agent>());
        }
        //species.Speciacte(generation);
        update_time_left = con.seconds_every_tic;
        simulation_ticks_left = con.number_of_ticks_every_simulation;
        number_of_alive = con.populationSize;
    }
	
	// Update is called once per frame
	void Update () {
        if (con.runSingleSpecies)
        {
            update_time_left -= Time.deltaTime;

            if (update_time_left < 0)
            {
                update_time_left = con.seconds_every_tic;
                if (species.Count > con.spexies_drawn)
                    UpdateList(species[con.spexies_drawn].members);
                else
                    throw new Exception("Bad species");
                simulation_ticks_left--;
            }
            if (simulation_ticks_left <= 0) //|| number_of_alive==0)
            {
                EndOfSimulation(species[con.spexies_drawn].members);
                simulation_ticks_left = con.number_of_ticks_every_simulation;
                number_of_alive = con.populationSize;
            }
            return;
        }
        if (con.runSimulation)
        {
            update_time_left -= Time.deltaTime;

            if (update_time_left < 0)
            {
                update_time_left = con.seconds_every_tic;
                UpdateList(generation);
                simulation_ticks_left--;
            }
            if(simulation_ticks_left <= 0) //|| number_of_alive==0)
            {
                EndOfSimulation(generation);
                simulation_ticks_left = con.number_of_ticks_every_simulation;
                number_of_alive = con.populationSize;
            }
            return;
        }
	}

    void AgentDied(object o,EventArgs e)
    {
        number_of_alive--;
    }

    public void Reset()
    {
        update_time_left = con.seconds_every_tic;
        simulation_ticks_left = con.number_of_ticks_every_simulation;
        foreach (var agent in generation)
            agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));
    }

    private void EndOfSimulation(List<Agent> alive)
    {
        //generation[0].GetComponent<SpriteRenderer>().sprite = blueSprite;
        //generation[0].transform.position += new Vector3(0, 0, 0.5f);
        alive.Sort((a1, a2) => -a1.Score.CompareTo(a2.Score));
        //generation[0].GetComponent<SpriteRenderer>().sprite = redSprite;
        //generation[0].transform.position += new Vector3(0, 0, -0.5f);
        Debug.Log("Srednia: " + alive.Sum(a => a.Score) / generation.Count);
        Debug.Log("Liczba najlepszych: " + alive.Sum(a => a.Score == generation[0].Score ? 1 : 0));
        Debug.Log("Najlepszy wynik: " + alive[0].Score);
#if !NEAT
                int i = 0;
                foreach (var agent in generation)
                {
                    if (i == 0)
                        agent.GetComponent<Agent>().brain.Mutate();
                    if (i > 3)
                    {
                        agent.brain = agent.brain.Crossover(generation[NeuralNet.RandomGenerator.Next(i/2)].brain);
                        //agent.brain = agent.brain.Crossover(generation[0].brain);
                    }
                    if (i > 50)
                    {
                        agent.brain.Mutate();
                    }
                    //if (i>150)
                    //{
                    //    agent.brain.Mutate();
                    //    agent.brain.Mutate();
                    //}
                    //if (i > 900)
                    //{
                    //    agent.brain.Mutate();
                    //    agent.brain.Mutate();
                    //}
                    i++;
                    agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));

                }
#else
        if(alive == generation)
        {
            MutateGeneration();
            alive = generation;
        }
        
        //reseting
        foreach (var agent in alive)
            agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));
#endif
    }

    private void MutateGeneration()
    {
        species.Speciacte(generation);
        string tmp_str;
        generation.Clear();
        generation = species.PerformMatingRitual(agentPrefab, out tmp_str);
        spieces_text.text = tmp_str;
        con.no_species = species.Count;
        
    }

    private void UpdateList(List<Agent> alive)
    {
        alive.ForEach(agent => UpdateAgent(agent));
#if NEAT
        if (con.draw_representative)
        {
            if (species.Count > con.spexies_drawn)
                visual.Assign(species[con.spexies_drawn].Reprezentative);
            else
                visual.Assign(alive[0].brain);
        }
        else
            if (alive.Count > con.spexies_drawn)
            visual.Assign(alive[con.spexies_drawn].brain);
        else
            visual.Assign(alive[0].brain);
#endif
    }

    public Transform agentPrefab;
    
    public List<Agent> generation;
    private int number_of_alive;
    public List<Species> species;

    public Tilemap tilemap;

    public Time time;
    public float update_time_left;
    public float simulation_ticks_left;

    public Sprite redSprite;
    public Sprite blueSprite;

    public Constants con;
    public NEATDrower visual;
    public Text spieces_text;


    public void OnButtonClick()
    {
        if (con.runSimulation)
        {
            con.runSimulation = false;
            return;
        }
        con.runSimulation = true;
    }
    
    private void UpdateAgent(Agent agent)
    {  
        //chceck if agent can move
        if (agent.Dead)
            return;
        Vector3Int position = tilemap.LocalToCell(agent.transform.position);
        if (tilemap.GetTile(position).name == "green")
        {
            agent.StepOnTile(position);
            //agent.GetComponent<Agent>().Score = 1000;
        }
        if (tilemap.GetTile(position).name == "black")
        {
            //agent.GetComponent<Agent>().Score = 0;
            //agent.GetComponent<Agent>().Score = Mathf.Abs(position.x - con.initial_position.x) - Mathf.Abs(position.y - con.initial_position.y);
            agent.Dead = true;
            return;
        }
        //agent.GetComponent<Agent>().Score++;              -> points for time
        

        //calc input wector
        List<double> brainInput = new List<double>();
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(1, 0, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(-1, 0, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(0, 1, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(0, -1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, 0, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, 0, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(0, 1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(0, -1, 0), position));
        //brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, 1, 0), position));
        //brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, 1, 0), position));
        //brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, -1, 0), position));
        //brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, -1, 0), position));
        foreach (var inp in agent.GetComponent<Agent>().LookAtYourself())
            brainInput.Add(inp);
        
        //get next move
        var brainOutput = agent.GetComponent<Agent>().brain.Predict(brainInput);
        int maxIndex = brainOutput.IndexOf(brainOutput.Max());
        switch(maxIndex)
        {
            case 0:
                agent.GetComponent<Agent>().MoveInDirection(new Vector3(1, 0));
                break;
            case 1:
                agent.GetComponent<Agent>().MoveInDirection(new Vector3(-1, 0));
                break;
            case 2:
                agent.GetComponent<Agent>().MoveInDirection(new Vector3(0,1));
                break;
            case 3:
                agent.GetComponent<Agent>().MoveInDirection(new Vector3(0,-1));
                break;
            default:
                Debug.Log("ERROR, unnown direction");
                break;
        }
    }

    private double FindColorInDircetion(string color,Vector3Int direction, Vector3Int origin)
    {
#if FAR_SIGHT
        returns 1/distance from tile of that given color (as string)

        var pos1 = origin;
        pos1 += direction;
        int i = 1;
        while (tilemap.GetTile(pos1) != null)
        {
            if (tilemap.GetTile(pos1).name == color)
            {
                return 1.0f / i;
            }
            pos1 += direction;
            i++;
        }
        return 0;
#else
        if (tilemap.GetTile(origin + direction).name == color)
        {      
            return 1;
        }
        return 0;
#endif
    }


}
