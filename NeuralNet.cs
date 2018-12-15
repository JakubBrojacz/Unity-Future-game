using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NeuralNetwork
{
    public class NeuralNet
    {
        public List<Neuron> InputLayer { get; set; }
        public List<List<Neuron>> HiddenLayers { get; set; }
        public List<Neuron> OutputLayer { get; set; }

        public static readonly System.Random RandomGenerator = new System.Random();

        public NeuralNet(int inputSize,int hiddenSize,int outputSize,int numHidden)
        {
            InputLayer = new List<Neuron>();
            for (int i = 0; i < inputSize; i++)
                InputLayer.Add(new Neuron());

            HiddenLayers = new List<List<Neuron>>();
            for(int i=0;i<numHidden;i++)
            {
                var tmp = new List<Neuron>();
                for (int j = 0; j < hiddenSize; j++)
                    tmp.Add(new Neuron(i == 0 ? InputLayer : HiddenLayers[i - 1]));
                HiddenLayers.Add(tmp);
            }

            OutputLayer = new List<Neuron>();
            for (int i = 0; i < outputSize; i++)
                OutputLayer.Add(new Neuron(numHidden > 0 ? HiddenLayers[numHidden - 1] : InputLayer));
        }

        public List<double> Predict(IEnumerable<double> inputValues)
        {
            ForwardPropagation(inputValues);
            List<double> outputValues = new List<double>();
            foreach(var neuron in OutputLayer)
            {
                outputValues.Add(neuron.Value);
            }
            return outputValues;
        }

        public void ForwardPropagation(IEnumerable<double> inputValues)
        {
            var inputIenumerator = inputValues.GetEnumerator();
            foreach(var neuron in InputLayer)
            {
                inputIenumerator.MoveNext();
                neuron.Value = inputIenumerator.Current;
            }
            foreach (var layer in HiddenLayers)
                layer.ForEach(neuron => neuron.CalculateValue());
            OutputLayer.ForEach(neuron => neuron.CalculateValue());
        }

        public void Mutate()
        {
            OutputLayer.Mutate();
            foreach (var layer in HiddenLayers)
                layer.Mutate();
        }

        public void Crossover(NeuralNet partner)
        {
            OutputLayer.Crossover(partner.OutputLayer);
            var layerIEnumerator = partner.HiddenLayers.GetEnumerator();
            foreach(var layer in HiddenLayers)
            {
                layerIEnumerator.MoveNext();
                layer.Crossover(layerIEnumerator.Current);
            }
        }
    }
}