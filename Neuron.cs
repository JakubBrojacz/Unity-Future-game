using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


namespace NeuralNetwork
{
    public class Neuron
    {
        public List<Synapse> InputSynapses { get; set; }
        public List<Synapse> OutputSynapses { get; set; }
        public double Bias { get; set; }
        public double OutputValue { get; set; }
        public int InnovationNo { get; set; }
        public bool Done { get; set; } //if neuron value was already calculated
        public Vector3 position; //only for drawing... What a shame... If I could delete it


        public Neuron()
        {
            Bias = NeuralNet.RandomGenerator.NextGaussian(0, Constants.Con.init_stdDev_bias); 
            InputSynapses = new List<Synapse>();
            OutputSynapses = new List<Synapse>();
            Done = false;
        }
        public Neuron(int innovationno) : this()
        {
            InnovationNo = innovationno;
        }
        public Neuron(IEnumerable<Neuron> PreviousLayer) : this()
        {
            foreach(var inputNeuron in PreviousLayer)
            {
                new Synapse(inputNeuron, this);
            }
        }

        public Neuron(Neuron neuron) : this()
        {
            InnovationNo = neuron.InnovationNo;
            Bias = neuron.Bias;
        }

        public void CalculateValue()
        {
            OutputValue = Sigmoid.Output(InputSynapses.Sum(syn => syn.Weight * syn.InputNeuron.OutputValue)+Bias);
        }

        public void CalculateValue_NEAT_feedforward()
        {
            foreach(Synapse syn in InputSynapses)
            {
                if (!syn.InputNeuron.Done)
                    syn.InputNeuron.CalculateValue_NEAT_feedforward();
            }
            CalculateValue();
            Done = true;
        }

        public void Mutate()
        {
#if false
            int ran = NeuralNet.RandomGenerator.Next(1, 8);
            if (ran == 1)
                return;
            else if (ran > 4)
                InputSynapses.ForEach(syn => 
                {
                    if (NeuralNet.RandomGenerator.Next(1, 2) == 1) syn.Weight *= 1 + (NeuralNet.RandomGenerator.NextDouble() - 0.5) / 10;
                });
            else if (ran == 2)
                InputSynapses.ForEach(syn =>
                {
                    if (NeuralNet.RandomGenerator.Next(1, 5) == 1)
                        syn.Weight *= -1;
                });
            else if (ran == 3)
                Bias *= 1+(NeuralNet.RandomGenerator.NextDouble() - 0.5) / 5;
            else if (ran == 4)
                Bias *= -1;
#else
            InputSynapses.ForEach(syn =>
            {
                var ran = NeuralNet.RandomGenerator.NextDouble();
                if (ran < Constants.Con.mutate_percent_of_synapses_uniform)
                    syn.Weight += NeuralNet.RandomGenerator.NextGaussian(0,Constants.Con.mutation_power_synapse);
                else if (ran < Constants.Con.mutate_percent_of_synapses_uniform+Constants.Con.mutate_percent_of_synapses_new_values)
                    syn.Weight = NeuralNet.RandomGenerator.NextGaussian(0, Constants.Con.init_stdDev_synapse);
                syn.Weight = Math.Max(-30, Math.Min(30, syn.Weight));
            });
            {//bias
                var ran = NeuralNet.RandomGenerator.NextDouble();
                if (ran < Constants.Con.mutate_percent_of_biases_uniform)
                    Bias += NeuralNet.RandomGenerator.NextGaussian(0, Constants.Con.mutation_power_bias);
                else if(ran < Constants.Con.mutate_percent_of_synapses_uniform + Constants.Con.mutate_percent_of_biases_new_values)
                    Bias = NeuralNet.RandomGenerator.NextGaussian(0, Constants.Con.init_stdDev_bias);
                Bias = Math.Max(-30, Math.Min(30, Bias));
            }
            
#endif
        }
    }

    public class Synapse
    {
        public Neuron InputNeuron { get; set; }
        public Neuron OutputNeuron { get; set; }
        public double Weight { get; set; }
        public int InnovationNo { get; set; }

        public Synapse()
        {
            Weight = NeuralNet.RandomGenerator.NextGaussian(0,Constants.Con.init_stdDev_synapse);
        }

        public Synapse(Neuron input, Neuron output,int innovationno) : this(input,output)
        {
            InnovationNo = innovationno;
        }

        public Synapse(Neuron input,Neuron output) : this()
        {
            InputNeuron = input;
            OutputNeuron = output;
            input.OutputSynapses.Add(this);
            output.InputSynapses.Add(this);
        }
    }

    public class Sigmoid
    {
        public static double Output(double x)
        {
            if (x > 20)
            {
                return 1;
            }
                
            if (x < -20)
            {
                return 0;
            }
            return 1.0 / (1.0 + Mathf.Exp((float)(-4.9*x)));
        }
    }
}

