﻿using System.Collections;
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

        public static readonly System.Random RandomGenerator = new System.Random(0);

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
                outputValues.Add(neuron.OutputValue);
            }
            return outputValues;
        }

        public void ForwardPropagation(IEnumerable<double> inputValues)
        {
            foreach(var neuron in InputLayer.Zip(inputValues))
            {
                neuron.Key.OutputValue = neuron.Value;
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

        public NeuralNet Crossover(NeuralNet partner)
        {
            OutputLayer.Crossover(partner.OutputLayer);
            foreach(var layer in HiddenLayers.Zip(partner.HiddenLayers))
            {
                layer.Key.Crossover(layer.Value);
            }
            return this;
        }
    }
}