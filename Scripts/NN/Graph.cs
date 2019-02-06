using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NeuralNetwork;


static class Graph
{
    public static bool ConnectionWouldMakeCycle(this Neuron input,Neuron output)
    {
        if (input == output)
            return true;

        foreach (var synapse in input.InputSynapses)
        {
            if (synapse.InputNeuron.ConnectionWouldMakeCycle(output))
                return true;
        }

        return false;
    }
}

