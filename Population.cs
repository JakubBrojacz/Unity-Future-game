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
        random = new System.Random();
        Constants.Con = con;
        con.visual = visual;
        visual.Init();

        //List<Agent> testl = new List<Agent>();
        //for(int i=0;i<5;i++)
        //{
        //    Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
        //    c.GetComponent<Agent>().Score = i;
        //    testl.Add(c.GetComponent<Agent>());
        //}
        //for(int i=0;i<1000;i++)
        //{
        //    Agent tmpa;
        //    FindParents(testl, out tmpa);
        //    Debug.Log(tmpa.Score);
        //}
        //Debug.Break();

        generation = new List<Agent>();
        for (int i = 0; i < populationSize; i++)
        {
            Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            //c.GetComponent<Agent>().brain = new NeuralNet(con.input_layer, 8, con.output_layer, 1);
            c.GetComponent<Agent>().Init();
            c.GetComponent<Agent>().brain = new NEAT(con.input_layer, con.output_layer, true);
#if true
            c.GetComponent<Agent>().brain.AddSynapse();
            c.GetComponent<Agent>().brain.AddSynapse();
            c.GetComponent<Agent>().brain.Mutate();
            c.GetComponent<Agent>().brain.Mutate();
#endif
            c.GetComponent<Agent>().OnDeath += AgentDied;
            generation.Add(c.GetComponent<Agent>());
        }
        species = new List<Species>();
        update_time_left = con.seconds_every_tic;
        simulation_ticks_left = con.number_of_ticks_every_simulation;
        number_of_alive = populationSize;
    }
	
	// Update is called once per frame
	void Update () {
        if (con.runSimulation)
        {
            update_time_left -= Time.deltaTime;

            if (update_time_left < 0)
            {
                update_time_left = con.seconds_every_tic;
                generation.ForEach(agent => UpdateAgent(agent));
                if (species.Count > con.spexies_drawn)
                    visual.Assign(species[con.spexies_drawn].reprezentative.brain);
                else
                    visual.Assign(generation[0].brain);
                //visual.Assign(generation[con.spexies_drawn].brain);
                simulation_ticks_left--;
            }
        }
        if(con.runSimulation)
        { 
            if(simulation_ticks_left <= 0) //|| number_of_alive==0)
            {
                //generation[0].GetComponent<SpriteRenderer>().sprite = blueSprite;
                //generation[0].transform.position += new Vector3(0, 0, 0.5f);
                generation.Sort((a1,a2) => -a1.Score.CompareTo(a2.Score));
                //generation[0].GetComponent<SpriteRenderer>().sprite = redSprite;
                //generation[0].transform.position += new Vector3(0, 0, -0.5f);
                Debug.Log("Srednia: " + generation.Sum(a => a.Score)/generation.Count);
                Debug.Log("Liczba najlepszych: " + generation.Sum(a => a.Score == generation[0].Score ? 1:0));
                Debug.Log("Najlepszy wynik: " + generation[0].Score);
#if false
                int i = 0;
#endif
                foreach (var agent in generation)
                {
#if true
                    //Spectation
                    //chceck if agent is someones representative
                    bool find_species = false;
                    foreach (var s in species)
                    {
                        if (agent.brain == s.reprezentative.brain)
                        {
                            find_species = true;
                            break;
                        }
                    }
                    if (find_species)
                        continue;
                    //find maching species
                    foreach (var s in species)
                    {
                        if (agent.SameSpecies(s.reprezentative))
                        {
                            find_species = true;
                            s.Add(agent);
                            break;
                        }
                    }
                    if (!find_species)
                    {
                        species.Add(new Species(agent));
                    }
                }

#else
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
#endif
#if true
                //calculate size of spieces in next generation
                foreach (var s in species)
                {
                    if (s.members.Count < 2)
                        s.Dead = true;
                    s.Score = s.members.Sum(a => a.Score)/s.members.Count;
                }
                species.RemoveAll(s => s.Dead);
                var scoresSum = species.Sum(s => s.Score);
                species.ForEach(s => s.Score *= populationSize / scoresSum); //normalization

                //next generation
                generation.Clear();
                string tmp_str = "";
                foreach (var s in species)
                {
                    if(s.Score<1)
                    {
                        s.Dead = true;
                        continue;
                    }
                    s.reprezentative.GetComponent<SpriteRenderer>().sprite = blueSprite;
                    s.reprezentative.transform.position += new Vector3(0, 0, 0.5f);
                    s.members.Sort((a1, a2) => -a1.Score.CompareTo(a2.Score));
                    s.reprezentative.GetComponent<SpriteRenderer>().sprite = redSprite;
                    s.reprezentative.transform.position += new Vector3(0, 0, -0.5f);
                    var rep = s.reprezentative;
                    generation.Add(rep);

                    tmp_str += "Licznosc gatunku " + s.Name + ": " + s.members.Count + "; Najwyzszy wynik: " + rep.Score + Environment.NewLine;

                    for (int i = 0; i < s.Score * 3.0 / 4.0; i++)
                    {
                        Transform offspring = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                        offspring.GetComponent<Agent>().Init();
                        Agent p1, p2;
                        FindParents(s.members, out p1, out p2);
                        if(p1.Score>p2.Score)
                        {
                            Agent tmp = p1;
                            p1 = p2;
                            p2 = tmp;
                        }
                        offspring.GetComponent<Agent>().brain = p1.brain.Crossover(p2.brain);
                        offspring.GetComponent<Agent>().brain.Mutate();
                        offspring.GetComponent<Agent>().OnDeath += AgentDied;
                        generation.Add(offspring.GetComponent<Agent>());
                    }
                    for (int i = 0; i < s.Score * 1.0 / 4.0; i++)
                    {
                        Transform offspring = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                        offspring.GetComponent<Agent>().Init();
                        Agent p1;
                        FindParents(s.members, out p1);
                        offspring.GetComponent<Agent>().brain = p1.brain.Copy();
                        offspring.GetComponent<Agent>().brain.Mutate();
                        offspring.GetComponent<Agent>().OnDeath += AgentDied;
                        generation.Add(offspring.GetComponent<Agent>());
                    }
                    s.members.RemoveAt(0);
                    foreach (var agent in s.members)
                    {
                        Destroy(agent.tail);
                        Destroy(agent.gameObject);
                    }
                    s.members.Clear();
                    s.members.Add(rep);
                }
                spieces_text.text = tmp_str;
                species.RemoveAll(s => s.Dead);
                con.no_species = species.Count;
                //reseting
                foreach(var agent in generation)
                    agent.Reset(new Vector3(con.initial_position.x + 0.5f, con.initial_position.y + 0.5f, agent.transform.position.z));
#endif
                simulation_ticks_left = con.number_of_ticks_every_simulation;
                number_of_alive = populationSize;
            }
        }
	}

    void AgentDied(object o,EventArgs e)
    {
        number_of_alive--;
    }

    //void UpdateFood()
    //{
    //    //apples.RemoveAll(ap => ap.apples_left <= 0);
    //    //int count = number_of_apples - apples.Count;
    //    //for(int i=0;i<count;i++)
    //    //{
    //    //    Transform c = Instantiate(foodPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
    //    //}
    //}

    public Transform agentPrefab;
    public int populationSize=100;
    public List<Agent> generation;
    private int number_of_alive;
    public List<Species> species;

    //public Transform foodPrefab;
    //public List<Food> apples;
    //private int number_of_apples=5;

    public Tilemap tilemap;

    public Time time;
    public float update_time_left;
    public float simulation_ticks_left;

    public Sprite redSprite;
    public Sprite blueSprite;

    public Constants con;
    public NEATDrower visual;
    public Text spieces_text;

    private System.Random random;

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
        //returns 1/distance from tile of that given color (as string)


        //var pos1 = origin;
        //pos1 += direction;
        //int i = 1;
        //while (tilemap.GetTile(pos1) != null)
        //{
        //    if (tilemap.GetTile(pos1).name == color)
        //    {
        //        return 1.0f / i;
        //    }
        //    pos1 += direction;
        //    i++;
        //}
        //return 0;


        if (tilemap.GetTile(origin + direction).name == color)
        {      
            return 1;
        }
        return 0;

    }

    private void FindParents(IEnumerable<Agent> list,out Agent parent1,out Agent parent2)
    {
        //int i = 0;
        //while(i<15)
        //{
        //    i++;
        FindParents(list, out parent1);
        FindParents(list, out parent2);
        //    if (parent1 != parent2)
        //        return;
        //}

        //throw new Exception("Too short list??");
    }
    private void FindParents(IEnumerable<Agent> list, out Agent parent)
    {
        double sum = list.Sum(a => a.Score);
        double run = random.NextDouble() * sum;
        foreach(var agent in list)
        {
            if(run<agent.Score)
            {
                parent = agent;
                return;
            }
            run -= agent.Score;
        }
        throw new Exception("Something gone wrong with finding parent");
    }
}
