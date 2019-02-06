

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using NeuralNetwork;
using System.Linq;

//game-object spawning agents and controlling their behaviour
public class PopR : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        //random = new System.Random(0);
        Constants.Con = con;

        //con.visual = visual;
        //visual.Init();
        species = new List<Species_NNR>();
      

        generation = new List<Agent_NNR>();
        for (int i = 0; i < con.populationSize; i++)
        {
            Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            c.GetComponent<Agent_NNR>().Init();
            c.GetComponent<Agent_NNR>().brain = new NEAT(5, 3, true);
            c.GetComponent<Agent_NNR>().brain.recurrent = true;
            c.GetComponent<Agent_NNR>().brain.AddSynapse();
            c.GetComponent<Agent_NNR>().brain.AddSynapse();
            c.GetComponent<Agent_NNR>().brain.Mutate();
            c.GetComponent<Agent_NNR>().brain.Mutate();

            generation.Add(c.GetComponent<Agent_NNR>());
        }
        

        simulation_ticks_left = con.number_of_ticks_every_simulation;
        number_of_alive = con.populationSize;
    }

    void FixedUpdate()
    {
        UpdateList(generation);

    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (con.runSingleSpecies)
    //    {
    //        update_time_left -= Time.deltaTime;

    //        if (update_time_left < 0)
    //        {
    //            update_time_left = con.seconds_every_tic;
    //            if (species.Count > con.spexies_drawn)
    //                UpdateList(species[con.spexies_drawn].members);
    //            else
    //                throw new Exception("Bad species");
    //            simulation_ticks_left--;
    //        }
    //        if (simulation_ticks_left <= 0) //|| number_of_alive==0)
    //        {
    //            EndOfSimulation(species[con.spexies_drawn].members);
    //            simulation_ticks_left = con.number_of_ticks_every_simulation;
    //            number_of_alive = con.populationSize;
    //        }
    //        return;
    //    }
    //    if (con.runSimulation)
    //    {
    //        update_time_left -= Time.deltaTime;

    //        if (update_time_left < 0)
    //        {
    //            update_time_left = con.seconds_every_tic;
    //            UpdateList(generation);
    //            simulation_ticks_left--;
    //        }
    //        if (simulation_ticks_left <= 0) //|| number_of_alive==0)
    //        {
    //            EndOfSimulation(generation);
    //            simulation_ticks_left = con.number_of_ticks_every_simulation;
    //            number_of_alive = con.populationSize;
    //        }
    //        return;
    //    }
    //}

    //void AgentDied(object o, EventArgs e)
    //{
    //    number_of_alive--;
    //}

    //public void Reset()
    //{
    //    update_time_left = con.seconds_every_tic;
    //    simulation_ticks_left = con.number_of_ticks_every_simulation;
    //    foreach (var agent in generation)
    //        agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));
    //}

    //private void EndOfSimulation(List<Agent> alive)
    //{
    //    //generation[0].GetComponent<SpriteRenderer>().sprite = blueSprite;
    //    //generation[0].transform.position += new Vector3(0, 0, 0.5f);
    //    alive.Sort((a1, a2) => -a1.Score.CompareTo(a2.Score));
    //    //generation[0].GetComponent<SpriteRenderer>().sprite = redSprite;
    //    //generation[0].transform.position += new Vector3(0, 0, -0.5f);
    //    Debug.Log("Srednia: " + alive.Sum(a => a.Score) / generation.Count);
    //    Debug.Log("Liczba najlepszych: " + alive.Sum(a => a.Score == generation[0].Score ? 1 : 0));
    //    Debug.Log("Najlepszy wynik: " + alive[0].Score);

    //    if (alive == generation)
    //    {
    //        MutateGeneration();
    //        alive = generation;
    //    }

    //    //reseting
    //    foreach (var agent in alive)
    //        agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));
    //}

    //private void MutateGeneration()
    //{
    //    species.Speciacte(generation);
    //    string tmp_str;
    //    generation.Clear();
    //    generation = species.PerformMatingRitual(agentPrefab, out tmp_str);
    //    spieces_text.text = tmp_str;
    //    con.no_species = species.Count;

    //}

    private void UpdateList(List<Agent_NNR> alive)
    {
        alive.ForEach(agent => UpdateAgent(agent));

        //if (con.draw_representative)
        //{
        //    if (species.Count > con.spexies_drawn)
        //        visual.Assign(species[con.spexies_drawn].Reprezentative);
        //    else
        //        visual.Assign(alive[0].brain);
        //}
        //else
        //    if (alive.Count > con.spexies_drawn)
        //    visual.Assign(alive[con.spexies_drawn].brain);
        //else
        //    visual.Assign(alive[0].brain);

    }

    public Transform agentPrefab;
    public Transform treePrefab;

    public List<Agent_NNR> generation;
    private int number_of_alive;
    public List<Species_NNR> species;

    public Tilemap tilemap;

    public Time time;
    public float update_time_left;
    public float simulation_ticks_left;

    public Sprite redSprite;
    public Sprite blueSprite;

    public Constants con;
    public NEATDrower visual;
    public Text spieces_text;


    //public void OnButtonClick()
    //{
    //    if (con.runSimulation)
    //    {
    //        con.runSimulation = false;
    //        return;
    //    }
    //    con.runSimulation = true;
    //}

    private void UpdateAgent(Agent_NNR agent)
    {
        //chceck if agent can move
        if (agent.Dead)
            return;

        //calculate current position and perks
        //on agent side - on collision enter

        //calc input wector
        List<double> brainInput = new List<double>();
        Vector2 front = agent.transform.up;
        brainInput.Add(LookInDirection(Quaternion.Euler(0, 0, -30) * front, agent.transform));
        brainInput.Add(LookInDirection(Quaternion.Euler(0, 0, -15) * front, agent.transform));
        brainInput.Add(LookInDirection(front, agent.transform));
        brainInput.Add(LookInDirection(Quaternion.Euler(0, 0, 15) * front, agent.transform));
        brainInput.Add(LookInDirection(Quaternion.Euler(0, 0, 30) * front, agent.transform));

        //get next move
        var brainOutput = agent.brain.Predict(brainInput);
        int maxIndex = brainOutput.IndexOf(brainOutput.Max());
        switch (maxIndex)
        {
            case 2:
                agent.MoveInDirection(new Vector3(0.03f, 0)*con.speed);
                break;
            case 1:
                agent.Rotate(0.1f*con.speed);
                break;
            case 0:
                agent.Rotate(-0.1f*con.speed);
                break;
            default:
                Debug.Log("ERROR, unnown direction");
                break;
        }
    }

    private double LookInDirection(Vector3 direction, Transform origin, int layer_mask = 9)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin.position, origin.TransformDirection(direction), 5, ~layer_mask);
        if (hit.collider != null)
        {
            Debug.DrawRay(origin.position, origin.TransformDirection(direction) * hit.distance, Color.yellow);
            //Debug.Log("Did Hit");
            return 1 / hit.distance;
        }
        else
        { 
            Debug.DrawRay(origin.position, origin.TransformDirection(direction) * 5, Color.white);
            //Debug.Log("Did not Hit");
            return 0;
        }
    }

}
