using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//it was static class with const values, but I found it usefull to modify them during runtime
public class Constants : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float seconds_every_tic = 0.5f;
    public int number_of_ticks_every_simulation = 10;
    public Vector3 initial_position = new Vector3(5, 0, -1); //where to spanw agents
    public bool runSimulation = false;

    public int input_layer = 8;
    public int output_layer = 4;

    public float init_stdDev_synapse = 1f;
    public float init_stdDev_bias = 1f;

    public float crossover_chanse = 0.75f;

    public float mutate_weights_chanse = 0.8f;
    public float mutate_percent_of_synapses_uniform = 0.8f;
    public float mutate_percent_of_synapses_new_values = 0.1f;
    public float mutate_percent_of_biases_uniform = 0.7f;
    public float mutate_percent_of_biases_new_values = 0.1f;
    public float mutate_new_synapse_chanse = 0.05f;
    public float mutate_del_synapse_chanse = 0.05f;
    public float mutate_new_neuron_chanse = 0.03f;
    public float mutate_del_neuron_chanse = 0.03f;
    //public float chanse_that_gene_was_disabled_if_disabled_in_either_parent = 0.75;
    //public float sznasa na krzyżowanie międzygatunkowe 0.1;

    public float mutation_power_synapse = 0.1f;
    public float mutation_power_bias = 0.5f;

    public float c1 = 1;
    public float c2 = 1;
    public float c3 = 0.4f;

    public int no_species = 0;
    public int spexies_drawn = 0;
    public bool draw_representative = true;

    public float delta_t = 3;
    public float new_representative_max_distance = 1;

    public NeuralNetwork.NEATDrower visual;

    public static Constants Con { get; set; }
}
