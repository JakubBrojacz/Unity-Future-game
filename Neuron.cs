using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace NeuralNetwork
{
    public class Neuron
    {
        public List<Synapse> InputSynapses { get; set; }
        public List<Synapse> OutputSynapses { get; set; }
        public double Bias { get; set; }
        public double Value { get; set; }

        public Neuron()
        {
            Bias = (NeuralNet.RandomGenerator.NextDouble()*2-1)/5;
            InputSynapses = new List<Synapse>();
            OutputSynapses = new List<Synapse>();
        }
        public Neuron(IEnumerable<Neuron> PreviousLayer) : this()
        {
            foreach(var inputNeuron in PreviousLayer)
            {
                Synapse syn = new Synapse(inputNeuron, this);
                InputSynapses.Add(syn);
                inputNeuron.OutputSynapses.Add(syn);
            }
        }

        public void CalculateValue()
        {
            Value = Sigmoid.Output(InputSynapses.Sum(syn => syn.Weight * syn.InputNeuron.Value)+Bias);
        }

        public void Mutate()
        {
            int ran = NeuralNet.RandomGenerator.Next(1, 10);
            if (ran == 1)
                return;
            else if (ran > 4)
                InputSynapses.ForEach(syn => 
                {
                    if (NeuralNet.RandomGenerator.Next(1, 2) == 1) syn.Weight *= 1 + (NeuralNet.RandomGenerator.NextDouble() - 0.5) / 2;
                });
            else if (ran == 2)
                InputSynapses.ForEach(syn =>
                {
                    if (NeuralNet.RandomGenerator.Next(1, 5) == 1)
                        syn.Weight *= -1;
                });
            else if (ran == 3)
                Bias *= 1+(NeuralNet.RandomGenerator.NextDouble() - 0.5) / 2;
            else if (ran == 4)
                Bias *= -1;
        }
    }

    public class Synapse
    {
        public Neuron InputNeuron { get; set; }
        public Neuron OutputNeuron { get; set; }
        public double Weight { get; set; }

        public Synapse()
        {
            Weight = (NeuralNet.RandomGenerator.NextDouble()-0.5)/4;
        }

        public Synapse(Neuron input,Neuron output) : this()
        {
            InputNeuron = input;
            OutputNeuron = output;
        }
    }

    public class Sigmoid
    {
        public static double Output(double x)
        {
            if (x > 20)
            {
                //Debug.Log("Dużo: " + x);
                return 1;
            }
                
            if (x < -20)
            {
                //Debug.Log("Mało: " + x);
                return 0;
            }
            return 1.0 / (1.0 + Mathf.Exp((float)-x));
        }
    }
}

