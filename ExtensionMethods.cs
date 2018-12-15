using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace NeuralNetwork
{
    static public class ExtensionMethods
    {

        static public void Mutate(this List<Neuron> source)
        {
            source.ForEach(neuron => neuron.Mutate());
        }

        static public void Crossover(this List<Neuron> male, List<Neuron> female)
        {
            //they need to be the same lenght
            if (NeuralNet.RandomGenerator.Next(0, 2) == 1)
            {
                int n = male.Count;
                int crossIndex = NeuralNet.RandomGenerator.Next(0, n);
                int i = 0;
                var femaleIEnumerator = female.GetEnumerator();
                foreach (var neuron in male)
                {
                    femaleIEnumerator.MoveNext();
                    if (i < crossIndex)
                    {
                        i++;
                        continue;
                    }
                    var femaleInputSynapsesIEnumerator = femaleIEnumerator.Current.InputSynapses.GetEnumerator();
                    if (i > crossIndex)
                        foreach (var synapse in neuron.InputSynapses)
                        {
                            femaleInputSynapsesIEnumerator.MoveNext();
                            synapse.Weight = femaleInputSynapsesIEnumerator.Current.Weight;
                        }
                    if (i == crossIndex)
                    {
                        int j = 0;
                        int crossSynapseIndex = NeuralNet.RandomGenerator.Next(0, neuron.InputSynapses.Count - 1);
                        foreach (var synapse in neuron.InputSynapses)
                        {
                            femaleInputSynapsesIEnumerator.MoveNext();
                            if (j >= crossSynapseIndex)
                            {
                                synapse.Weight = femaleInputSynapsesIEnumerator.Current.Weight;
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            else
            {
                int n = male.Count;
                int crossIndex = NeuralNet.RandomGenerator.Next(0, n);
                int i = 0;
                var femaleIEnumerator = female.GetEnumerator();
                foreach (var neuron in male)
                {
                    femaleIEnumerator.MoveNext();
                    if (i > crossIndex)
                    {
                        i++;
                        continue;
                    }
                    var femaleInputSynapsesIEnumerator = femaleIEnumerator.Current.InputSynapses.GetEnumerator();
                    if (i < crossIndex)
                        foreach (var synapse in neuron.InputSynapses)
                        {
                            femaleInputSynapsesIEnumerator.MoveNext();
                            synapse.Weight = femaleInputSynapsesIEnumerator.Current.Weight;
                        }
                    if (i == crossIndex)
                    {
                        int j = 0;
                        int crossSynapseIndex = NeuralNet.RandomGenerator.Next(0, neuron.InputSynapses.Count - 1);
                        foreach (var synapse in neuron.InputSynapses)
                        {
                            femaleInputSynapsesIEnumerator.MoveNext();
                            if (j < crossSynapseIndex)
                            {
                                synapse.Weight = femaleInputSynapsesIEnumerator.Current.Weight;
                            }
                            j++;
                        }
                    }
                    i++;
                }
            }
            
        }

        static public void PrintList<T>(this List<T> l)
        {
            string tmp = "";
            foreach (var o in l)
                tmp += o + " ";
            Debug.Log(tmp);
        }
        static public void PrintList<T>(this List<T> l,System.Func<T,string> f)
        {
            string tmp = "";
            foreach (var o in l)
                tmp += f(o) + " ";
            Debug.Log(tmp);
        }
    }
}

