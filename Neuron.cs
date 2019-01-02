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
        public int InnovationNo { get; set; }
        public bool Done { get; set; } //if neuron value was already calculated
        public Vector3 position; //only for drawing... What a shame... If I could delete it


        public Neuron()
        {
            Bias = NeuralNet.RandomGenerator.NextGaussian(0, Constants.Con.init_stdDev_synapse_value); 
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

        public void CalculateValue()
        {
            Value = Sigmoid.Output(InputSynapses.Sum(syn => syn.Weight * syn.InputNeuron.Value)+Bias);
        }

        public void CalculateValue_NEAT()
        {
            foreach(Synapse syn in InputSynapses)
            {
                if (!syn.InputNeuron.Done)
                    syn.InputNeuron.CalculateValue_NEAT();
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
                if (NeuralNet.RandomGenerator.Next(100) < 95)
                    syn.Weight += (NeuralNet.RandomGenerator.NextDouble() - 0.5) / 30;
                else
                    syn.Weight = (NeuralNet.RandomGenerator.NextDouble() - 0.5);
            });
            if (NeuralNet.RandomGenerator.Next(100) < 95)
                Bias += (NeuralNet.RandomGenerator.NextDouble() - 0.5) / 30;
            else
                Bias = (NeuralNet.RandomGenerator.NextDouble() - 0.5);
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
            Weight = NeuralNet.RandomGenerator.NextGaussian(0,Constants.Con.init_stdDev_synapse_value);
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

