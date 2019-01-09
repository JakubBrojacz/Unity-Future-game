using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace NeuralNetwork
{
    static public class ExtensionMethods
    {
        static public IEnumerable<KeyValuePair<T,Y>> Zip<T,Y>(this IEnumerable<T> en1, IEnumerable<Y> en2)
        {
            var en1Enumerator = en1.GetEnumerator();
            foreach(var obj in en2)
            {
                en1Enumerator.MoveNext();
                yield return new KeyValuePair<T, Y>(en1Enumerator.Current, obj);
            }
        }

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
        
                foreach (var neuron in male.Zip(female))
                {
                    if (i < crossIndex)
                    {
                        i++;
                        continue;
                    }

                    if (i > crossIndex)
                        foreach (var synapse in neuron.Key.InputSynapses.Zip(neuron.Value.InputSynapses))
                        {
                            synapse.Key.Weight = synapse.Value.Weight;
                        }
                    if (i == crossIndex)
                    {
                        int j = 0;
                        int crossSynapseIndex = NeuralNet.RandomGenerator.Next(0, neuron.Key.InputSynapses.Count - 1);
                        foreach (var synapse in neuron.Key.InputSynapses.Zip(neuron.Value.InputSynapses))
                        {
                            if (j >= crossSynapseIndex)
                            {
                                synapse.Key.Weight = synapse.Value.Weight;
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
                foreach (var neuron in male.Zip(female))
                {
                    if (i > crossIndex)
                    {
                        i++;
                        continue;
                    }
                    if (i < crossIndex)
                        foreach (var synapse in neuron.Key.InputSynapses.Zip(neuron.Value.InputSynapses))
                        {
                            synapse.Key.Weight = synapse.Value.Weight;
                        }
                    if (i == crossIndex)
                    {
                        int j = 0;
                        int crossSynapseIndex = NeuralNet.RandomGenerator.Next(0, neuron.Key.InputSynapses.Count - 1);
                        foreach (var synapse in neuron.Key.InputSynapses.Zip(neuron.Value.InputSynapses))
                        {
                            if (j < crossSynapseIndex)
                            {
                                synapse.Key.Weight = synapse.Value.Weight;
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

        static public double NextGaussian(this System.Random rand, double mean=0,double stdDev=1)
        {
            double u1 = 1.0 - rand.NextDouble(); //uniform(0,1] random doubles
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }
    }
}

