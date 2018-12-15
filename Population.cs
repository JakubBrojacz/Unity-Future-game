using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using NeuralNetwork;
using System.Linq;




public class Population : MonoBehaviour {

	// Use this for initialization
	void Start () {
        generation = new List<GameObject>();
        for (int i = 0; i < populationSize; i++)
        {
            Transform c = Instantiate(agentPrefab, con.initial_position + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
            c.GetComponent<Agent>().brain = new NeuralNet(8, 8, 4, 1);
            generation.Add(c.gameObject);
        }
        update_time_left = con.seconds_every_tic;
        simulation_ticks_left = con.number_of_ticks_every_simulation;

        generation[0].GetComponent<Agent>().brain.OutputLayer[3].InputSynapses.PrintList(s => s.Weight.ToString());
        generation[1].GetComponent<Agent>().brain.OutputLayer[3].InputSynapses.PrintList(s => s.Weight.ToString());
        generation[0].GetComponent<Agent>().brain.OutputLayer.Crossover(generation[1].GetComponent<Agent>().brain.OutputLayer);
        generation[0].GetComponent<Agent>().brain.OutputLayer[3].InputSynapses.PrintList(s => s.Weight.ToString());
        generation[1].GetComponent<Agent>().brain.OutputLayer[3].InputSynapses.PrintList(s => s.Weight.ToString());
        generation[0].GetComponent<Agent>().brain.OutputLayer.Mutate();
        generation[0].GetComponent<Agent>().brain.OutputLayer[3].InputSynapses.PrintList(s => s.Weight.ToString());

      
    }
	
	// Update is called once per frame
	void Update () {
        if (runSimulation || runSingle)
        {
            update_time_left -= Time.deltaTime;

            if (update_time_left < 0)
            {
                update_time_left = con.seconds_every_tic;
                generation.ForEach(agent => UpdateAgent(agent));
                simulation_ticks_left--;
            }
        }
        if(runSimulation)
        { 
            if(simulation_ticks_left <= 0)
            {
                generation.ForEach(agent =>
                {
                    var position = tilemap.LocalToCell(agent.transform.position);
                    if (tilemap.GetTile(position) != null && tilemap.GetTile(position).name == "empty")
                        agent.GetComponent<Agent>().Score = Mathf.Abs(position.x - con.initial_position.x)-Mathf.Abs(position.y- con.initial_position.y);
                });
                generation[0].GetComponent<SpriteRenderer>().sprite = blueSprite;
                generation[0].transform.position += new Vector3(0, 0, 0.5f);
                generation.Sort((a1,a2) => -a1.GetComponent<Agent>().Score.CompareTo(a2.GetComponent<Agent>().Score));
                Debug.Log("Srednia: " + generation.Sum(a => a.GetComponent<Agent>().Score)/generation.Count);
                Debug.Log("Liczba najlepszych: " + generation.Sum(a => a.GetComponent<Agent>().Score > 5 ? 1:0));
                var tmp = generation[0];
                int i = 0;
                foreach(var agent in generation)
                {
                    if (i == 0)
                    {
                        agent.GetComponent<SpriteRenderer>().sprite = redSprite;
                        agent.transform.position += new Vector3(0, 0, -0.5f);
                    }
                    if(i > 0)
                        agent.GetComponent<Agent>().brain.Mutate();
                    if (i > 3)
                    {
                        agent.GetComponent<Agent>().brain.Crossover(generation[NeuralNet.RandomGenerator.Next(0,i/2)].GetComponent<Agent>().brain);
                    }
                    if (i > 45)
                    {
                        agent.GetComponent<Agent>().brain.Mutate();
                    }
                    if (i>90)
                    {
                        agent.GetComponent<Agent>().brain.Mutate();
                        agent.GetComponent<Agent>().brain.Mutate();
                    }
                    agent.transform.position = new Vector3(con.initial_position.x+0.5f, con.initial_position.y+0.5f, agent.transform.position.z);
                    agent.GetComponent<Agent>().Score = 0;
                    i++;
                    agent.GetComponent<Agent>().finished = false;
                }
                simulation_ticks_left = con.number_of_ticks_every_simulation;
            }
        }
	}

    public Transform agentPrefab;

    public int populationSize=100;

    public bool runSimulation = false;
    public bool runSingle = false;

    public List<GameObject> generation;

    public Tilemap tilemap;

    public Time time;
    public float update_time_left;
    public float simulation_ticks_left;

    public Sprite redSprite;
    public Sprite blueSprite;

    public Constants con;

    public GameObject ttmp;

    public void OnButtonClick()
    {
        runSingle = false;
        if (runSimulation)
        {
            runSimulation = false;
            return;
        }
        runSimulation = true;
        
    }

    public void RunSingle()
    {
        runSimulation = false;
        runSingle = true;
        foreach (var agent in generation)
        {
            agent.GetComponent<SpriteRenderer>().enabled = false;
        }
        generation[0].GetComponent<SpriteRenderer>().enabled = true;
    }

    private void UpdateAgent(GameObject agent)
    {
        if (agent.GetComponent<Agent>().finished)
            return;
        var position = tilemap.LocalToCell(agent.transform.position);
        if (tilemap.GetTile(position) == null)
        {
            //agent.GetComponent<Agent>().Score = 20 - simulation_ticks_left;
            agent.GetComponent<Agent>().finished = true;
            return;
        }
        if (tilemap.GetTile(position).name == "green")
        {
            agent.GetComponent<Agent>().Score = 1000;
            agent.GetComponent<Agent>().finished = true;
            return;
        }
        if (tilemap.GetTile(position).name == "black")
        {
            //agent.GetComponent<Agent>().Score = 0;
            agent.GetComponent<Agent>().Score = Mathf.Abs(position.x - con.initial_position.x) - Mathf.Abs(position.y - con.initial_position.y);
            agent.GetComponent<Agent>().finished = true;
            return;
        }
        //agent.GetComponent<Agent>().Score++;              -> points for time
        List<double> brainInput = new List<double>();
        //Debug.Log(tilemap.GetTile(position));

        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(1, 0, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(-1, 0, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(0, 1, 0), position));
        //brainInput.Add(FindColorInDircetion("green", new Vector3Int(0, -1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, 0, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, 0, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(0, 1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(0, -1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, 1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, 1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(1, -1, 0), position));
        brainInput.Add(FindColorInDircetion("black", new Vector3Int(-1, -1, 0), position));

        

        var brainOutput = agent.GetComponent<Agent>().brain.Predict(brainInput);
        
        int maxIndex = brainOutput.IndexOf(brainOutput.Max());
        switch(maxIndex)
        {
            case 0:
                agent.transform.position += new Vector3(1, 0);
                break;
            case 1:
                agent.transform.position += new Vector3(-1, 0);
                break;
            case 2:
                agent.transform.position += new Vector3(0,1);
                break;
            case 3:
                agent.transform.position += new Vector3(0,-1);
                break;
            default:
                Debug.Log("ERROR, unnown direction");
                break;
        }
    }

    private double FindColorInDircetion(string color,Vector3Int direction, Vector3Int origin)
    {
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
    }
}
