using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NeuralNetwork
{
    public class NEAT
    {
        public List<Neuron> InputLayer { get; set; }
        public List<Neuron> HiddenLayers { get; set; }
        public List<Neuron> OutputLayer { get; set; }

        public static int NeuronInnovationNo
        {
            get
            {
                _neuronInnovationNo++;
                return _neuronInnovationNo;
            }

            set
            {
                _neuronInnovationNo = value;
            }
        }

        public static int SynapseInnovationNo
        {
            get
            {
                _synapseInnovationNo++;
                return _synapseInnovationNo;
            }

            set
            {
                _synapseInnovationNo = value;
            }
        }

        public List<Synapse> AllSynapses { get; set; }

        public static readonly System.Random RandomGenerator = new System.Random();
        private static int _neuronInnovationNo = 0;
        private static int _synapseInnovationNo = 0;

        //TODO
        //List<Innovations> during_one_generation
        //to make sure that the same innovations during one generation will get the same innovationNo
        
        //TODO 
        //disabled genes

        public NEAT(int inputSize, int outputSize, bool without_synapses = false)
        {
            AllSynapses = new List<Synapse>();

            int temporaryNeuronInnovationNumber = 0;
            InputLayer = new List<Neuron>();
            for (int i = 0; i < inputSize; i++)
            {
                temporaryNeuronInnovationNumber++;
                var tmp = new Neuron(temporaryNeuronInnovationNumber);
                InputLayer.Add(tmp);
            }

            InputLayer.ForEach(neuron => neuron.Done = true);

            OutputLayer = new List<Neuron>();
            for (int i = 0; i < outputSize; i++)
            {
                temporaryNeuronInnovationNumber++;
                Neuron tmp;
                if(without_synapses)
                    tmp = new Neuron();
                else
                    tmp = new Neuron(InputLayer);
                tmp.InnovationNo = temporaryNeuronInnovationNumber;
                OutputLayer.Add(tmp);
            }

            if (_neuronInnovationNo < temporaryNeuronInnovationNumber)
                _neuronInnovationNo = temporaryNeuronInnovationNumber;

            foreach (var neuron in OutputLayer)
                AllSynapses.AddRange(neuron.InputSynapses);
            int temporarySynapseInnovationNumber = 0;
            foreach(var syn in AllSynapses)
            {
                temporarySynapseInnovationNumber++;
                syn.InnovationNo = temporarySynapseInnovationNumber;
            }

            if (_synapseInnovationNo < temporarySynapseInnovationNumber)
                _synapseInnovationNo = temporarySynapseInnovationNumber;

            HiddenLayers = new List<Neuron>();
        }

        public NEAT Copy()
        {
            NEAT c = new NEAT(InputLayer.Count, OutputLayer.Count, true);
            foreach(var synapse in AllSynapses)
            {
                c.AddSynapse(synapse);
            }
            return c;
        }

        public List<double> Predict(IEnumerable<double> inputValues)
        {
            ForwardPropagation(inputValues);
            List<double> outputValues = new List<double>();
            foreach (var neuron in OutputLayer)
            {
                outputValues.Add(neuron.Value);
            }
            return outputValues;
        }

        public void ForwardPropagation(IEnumerable<double> inputValues)
        {
            //Debug.Log("poczatek forward prop");
            foreach (var neuron in InputLayer.Zip(inputValues))
                neuron.Key.Value = neuron.Value;

            HiddenLayers.ForEach(neuron => neuron.Done = false);

            foreach (var neuron in OutputLayer)
                neuron.CalculateValue_NEAT();
            //Debug.Log("koniec forward prop");
        }

        public void Mutate()
        {
            //Debug.Log("Poczatek mutate");
            if (RandomGenerator.Next(100) < Constants.Con.mutate_weights_chanse)
            {
                //Debug.Log("mutate: weights");
                OutputLayer.Mutate();
                foreach (var neuron in HiddenLayers)
                    neuron.Mutate();
            }
            if(RandomGenerator.Next(100) < Constants.Con.mutate_new_neuron_chanse)
            {
                //Debug.Log("mutate: new neuron");
                if (AllSynapses.Count == 0)
                    return;
                AddNeuron();
            }
            if (RandomGenerator.Next(100) < Constants.Con.mutate_new_synapse_chanse) //according to offical NEAT document 5 in smaler species
            {
                //Debug.Log("mutate: new synapse");
                AddSynapse();
            }
            //Debug.Log("Koniec mutate");
        }
        public void AddSynapse()
        {
            //Debug.Log("Poczatek add synapse");
            //get 2 random neurons and connect them
            int ineur1 = RandomGenerator.Next(InputLayer.Count + HiddenLayers.Count);
            Neuron neuron1;
            if (ineur1 < InputLayer.Count)
                neuron1 = InputLayer[ineur1];
            else
                neuron1 = HiddenLayers[ineur1 - InputLayer.Count];
            int ineur2 = RandomGenerator.Next(OutputLayer.Count + HiddenLayers.Count);
            Neuron neuron2;
            if (ineur2 < OutputLayer.Count)
                neuron2 = OutputLayer[ineur2];
            else
            {
                neuron2 = HiddenLayers[ineur2 - OutputLayer.Count];
                if (neuron1.ConnectionWouldMakeCycle(neuron2)) //lets not make cycles
                {
                    Neuron tmp = neuron1;
                    neuron1 = neuron2;
                    neuron2 = tmp;
                }
            }
                
            //check if it already exist
            if(neuron1==neuron2)
            {
                return;
            }
            foreach (var synapse in AllSynapses)
                if (synapse.InputNeuron==neuron1 && synapse.OutputNeuron==neuron2)
                {
                    return;
                }


            Synapse syn = new Synapse(neuron1, neuron2, SynapseInnovationNo);
            AllSynapses.Add(syn);
        }

        public void AddNeuron()
        {
            //Debug.Log("Poczatek add neuron");
            int tmp = RandomGenerator.Next(AllSynapses.Count);
            Synapse oldSyn = AllSynapses[tmp];
            AllSynapses.RemoveAt(tmp);
            oldSyn.InputNeuron.OutputSynapses.Remove(oldSyn);
            oldSyn.OutputNeuron.InputSynapses.Remove(oldSyn);
            Neuron neuron = new Neuron(NeuronInnovationNo);


            Synapse newSyn1 = new Synapse(oldSyn.InputNeuron, neuron, SynapseInnovationNo);
            newSyn1.Weight = 1;
            Synapse newSyn2 = new Synapse(neuron, oldSyn.OutputNeuron, SynapseInnovationNo);
            newSyn2.Weight = oldSyn.Weight;

            HiddenLayers.Add(neuron);
            AllSynapses.Add(newSyn1);
            AllSynapses.Add(newSyn2);
        }

        public NEAT Crossover(NEAT partner)
        {
            //assume: partner has higher score
            NEAT offspring = partner.Copy();
            foreach (var synapse in AllSynapses)
            {
                foreach (var offspringSynapse in offspring.AllSynapses)
                {
                    if (synapse.InnovationNo == offspringSynapse.InnovationNo)
                    {
                        if (RandomGenerator.Next(2) == 0)
                            offspringSynapse.Weight = synapse.Weight;
                        break;
                    }
                }
            }
            return offspring;
        }
        private void AddSynapse(Synapse template)
        {
            //Debug.Log("Poczatek add synapse template");
            Neuron neuron1=null, neuron2=null;
            if (template.InputNeuron.InnovationNo <= InputLayer.Count)
                neuron1 = InputLayer[template.InputNeuron.InnovationNo-1];
            else
            {
                foreach(var neuron in HiddenLayers)
                    if(neuron.InnovationNo==template.InputNeuron.InnovationNo)
                    {
                        neuron1 = neuron;
                        break;
                    }
                if(neuron1==null)
                {
                    neuron1 = new Neuron(template.InputNeuron.InnovationNo);
                    neuron1.Bias = template.InputNeuron.Bias;
                    HiddenLayers.Add(neuron1);
                }
            }
            if (template.OutputNeuron.InnovationNo <= InputLayer.Count+OutputLayer.Count)
                neuron2 = OutputLayer[template.OutputNeuron.InnovationNo - InputLayer.Count-1];
            else
            {
                foreach (var neuron in HiddenLayers)
                    if (neuron.InnovationNo == template.OutputNeuron.InnovationNo)
                    {
                        neuron2 = neuron;
                        break;
                    }
                if (neuron2==null)
                {
                    neuron2 = new Neuron(template.OutputNeuron.InnovationNo);
                    neuron2.Bias = template.OutputNeuron.Bias;
                    HiddenLayers.Add(neuron2);
                }
            }

            Synapse newSyn = new Synapse(neuron1, neuron2, template.InnovationNo);
            newSyn.Weight = template.Weight;
            AllSynapses.Add(newSyn);
            //Debug.Log("Koniec add synapse template");
        }

        public float SameSpecies(NEAT partner)
        {
            if(partner==null)
            {
                Debug.Log("NULL partner");
                Debug.Break();
            }
            //var synapseIenumerator = AllSynapses.GetEnumerator();
            //var partnerSynapseIenumerator = partner.AllSynapses.GetEnumerator();
            //synapseIenumerator.MoveNext();
            float result = 0;
            int disjointGenes = 0;
            int excessGenes = 0;
            float weightDifference = 0;
            float N = Mathf.Max(AllSynapses.Count, partner.AllSynapses.Count);
            foreach(var synapse in AllSynapses)
                if (!partner.AllSynapses.Any(s1 => s1.InnovationNo == synapse.InnovationNo))
                    disjointGenes++;
            foreach(var synapse in partner.AllSynapses)
            {
                var syn1 = AllSynapses.Find(s1 => s1.InnovationNo == synapse.InnovationNo);
                if (syn1 == null)
                    disjointGenes++;
                else
                    weightDifference += Mathf.Abs((float)(syn1.Weight - synapse.Weight));
            }
            result = Constants.Con.c1 * excessGenes / N + Constants.Con.c2 * disjointGenes / N + Constants.Con.c3 * weightDifference;

            disjointGenes = 0;
            weightDifference = 0;
            N = Mathf.Max(HiddenLayers.Count, partner.HiddenLayers.Count)+OutputLayer.Count;
            foreach (var neuronPair in OutputLayer.Zip(partner.OutputLayer))
                weightDifference+= Mathf.Abs((float)(neuronPair.Key.Bias - neuronPair.Value.Bias));
            foreach (var neuron in HiddenLayers)
                if (!partner.HiddenLayers.Any(n1 => n1.InnovationNo == neuron.InnovationNo))
                    disjointGenes++;
            foreach (var neuron in partner.HiddenLayers)
            {
                var neu1 = HiddenLayers.Find(n1 => n1.InnovationNo == neuron.InnovationNo);
                if (neu1 == null)
                    disjointGenes++;
                else
                    weightDifference += Mathf.Abs((float)(neu1.Bias - neuron.Bias));
            }
            result += Constants.Con.c1 * excessGenes / N + Constants.Con.c2 * disjointGenes / N + Constants.Con.c3 * weightDifference;

            return result;
            //if (result < Constants.Con.delta_t)
            //    return true;
            //return false;
        }
    }
}