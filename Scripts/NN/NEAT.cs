using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace NeuralNetwork
{
    public class NEAT
    {
        public List<Neuron> InputLayer { get; set; }
        public Dictionary<int,Neuron> HiddenLayers { get; set; }
        public List<Neuron> OutputLayer { get; set; }

        public Dictionary<int,Synapse> AllSynapses { get; set; }

        public static readonly System.Random RandomGenerator = new System.Random(0);
        private static int _neuronInnovationNo = 0;
        private static int _synapseInnovationNo = 0;
        public bool recurrent = false;

        //TODO
        //List<Innovations> during_one_generation
        //to make sure that the same innovations during one generation will get the same innovationNo

        //TODO 
        //disabled genes

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

        

        public NEAT(int inputSize, int outputSize, bool without_synapses = false)
        {
            AllSynapses = new Dictionary<int, Synapse>();

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

            int temporarySynapseInnovationNumber = 0;
            foreach (var neuron in OutputLayer)
                foreach(var syn in neuron.InputSynapses)
                {
                    temporarySynapseInnovationNumber++;
                    syn.InnovationNo = temporarySynapseInnovationNumber;
                    AllSynapses.Add(syn.InnovationNo,syn);
                }
                    

            if (_synapseInnovationNo < temporarySynapseInnovationNumber)
                _synapseInnovationNo = temporarySynapseInnovationNumber;

            HiddenLayers = new Dictionary<int, Neuron>();
        }

        public NEAT Copy()
        {
            NEAT c = new NEAT(InputLayer.Count, OutputLayer.Count, true);
            foreach(var synapse in AllSynapses)
            {
                c.AddSynapse(synapse.Value);
            }
            foreach (var neuronpair in OutputLayer.Zip(c.OutputLayer))
                neuronpair.Value.Bias = neuronpair.Key.Bias;
            return c;
        }

        public List<double> Predict(IEnumerable<double> inputValues)
        {
            foreach (var neuron in InputLayer.Zip(inputValues))
                neuron.Key.OutputValue = neuron.Value;

            if (recurrent)
                RecurrentPropagation();
            else
                ForwardPropagation();
            List<double> outputValues = new List<double>();
            foreach (var neuron in OutputLayer)
            {
                outputValues.Add(neuron.OutputValue);
            }
            return outputValues;
        }

        private void ForwardPropagation()
        {
            HiddenLayers.Values.ToList().ForEach(neuron => neuron.Done = false);

            foreach (var neuron in OutputLayer)
                neuron.CalculateValue_NEAT_feedforward();
        }

        private void RecurrentPropagation()
        {
            HiddenLayers.Values.ToList().ForEach(neuron => neuron.LastValue = neuron.OutputValue);
            OutputLayer.ForEach(neuron => neuron.LastValue = neuron.OutputValue);

            HiddenLayers.Values.ToList().ForEach(neuron => neuron.CalculateValueRecurrent());
            OutputLayer.ForEach(neuron => neuron.CalculateValueRecurrent());
        }

        public void Mutate()
        {
            if (RandomGenerator.NextDouble() < Constants.Con.mutate_weights_chanse)
            {
                //Debug.Log("mutate");
                OutputLayer.Mutate();
                foreach (var neuron in HiddenLayers)
                    neuron.Value.Mutate();
            }
            if(RandomGenerator.NextDouble() < Constants.Con.mutate_new_neuron_chanse)
            {
                //Debug.Log("mutate");
                if (AllSynapses.Count == 0)
                    return;
                AddNeuron();
            }
            if (RandomGenerator.NextDouble() < Constants.Con.mutate_del_neuron_chanse)
            {
                //Debug.Log("mutate");
                if (AllSynapses.Count == 0)
                    return;
                DelNeuron();
            }
            if (RandomGenerator.NextDouble() < Constants.Con.mutate_new_synapse_chanse)
            {
                //Debug.Log("mutate");
                AddSynapse();
            }
            if (RandomGenerator.NextDouble() < Constants.Con.mutate_del_synapse_chanse)
            {
                //Debug.Log("mutate");
                DelSynapse();
            }
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
                neuron1 = HiddenLayers.Values.ToArray()[ineur1 - InputLayer.Count];
            int ineur2 = RandomGenerator.Next(OutputLayer.Count + HiddenLayers.Count);
            Neuron neuron2;
            if (ineur2 < OutputLayer.Count)
                neuron2 = OutputLayer[ineur2];
            else
            {
                neuron2 = HiddenLayers.Values.ToArray()[ineur2 - OutputLayer.Count];
                if (!recurrent && neuron1.ConnectionWouldMakeCycle(neuron2)) //lets not make cycles
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
                if (synapse.Value.InputNeuron==neuron1 && synapse.Value.OutputNeuron==neuron2)
                {
                    return;
                }


            Synapse syn = new Synapse(neuron1, neuron2, SynapseInnovationNo);
            AllSynapses.Add(syn.InnovationNo,syn);
        }

        public void AddNeuron()
        {
            if(!AllSynapses.Any())
            {
                AddSynapse();
                return;
            }
            //Debug.Log("Poczatek add neuron");
            int tmp = RandomGenerator.Next(AllSynapses.Count);
            Synapse oldSyn = AllSynapses.ToList()[tmp].Value;
            AllSynapses.Remove(oldSyn.InnovationNo);
            oldSyn.InputNeuron.OutputSynapses.Remove(oldSyn);
            oldSyn.OutputNeuron.InputSynapses.Remove(oldSyn);
            Neuron neuron = new Neuron(NeuronInnovationNo);


            Synapse newSyn1 = new Synapse(oldSyn.InputNeuron, neuron, SynapseInnovationNo);
            newSyn1.Weight = 1;
            Synapse newSyn2 = new Synapse(neuron, oldSyn.OutputNeuron, SynapseInnovationNo);
            newSyn2.Weight = oldSyn.Weight;

            HiddenLayers.Add(neuron.InnovationNo,neuron);
            AllSynapses.Add(newSyn1.InnovationNo,newSyn1);
            AllSynapses.Add(newSyn2.InnovationNo,newSyn2);
        }

        private void DelNeuron()
        {
            if (!HiddenLayers.Any())
                return;
            int tmp = RandomGenerator.Next(HiddenLayers.Count);
            Neuron oldNeu = HiddenLayers.Values.ToList()[tmp];
            foreach(var synapse in oldNeu.InputSynapses)
            {
                synapse.InputNeuron.OutputSynapses.Remove(synapse);
                AllSynapses.Remove(synapse.InnovationNo);
            }
            foreach (var synapse in oldNeu.OutputSynapses)
            {
                synapse.OutputNeuron.InputSynapses.Remove(synapse);
                AllSynapses.Remove(synapse.InnovationNo);
            }
            HiddenLayers.Remove(oldNeu.InnovationNo);
        }

        private void DelSynapse()
        {
            if (!AllSynapses.Any())
                return;
            int tmp = RandomGenerator.Next(AllSynapses.Count);
            Synapse oldSyn = AllSynapses.Values.ToList()[tmp];
            oldSyn.InputNeuron.OutputSynapses.Remove(oldSyn);
            oldSyn.OutputNeuron.InputSynapses.Remove(oldSyn);
            AllSynapses.Remove(oldSyn.InnovationNo);
        }

        public NEAT Crossover(NEAT partner)
        {
            //assume: partner has higher score
            NEAT offspring = partner.Copy();
            foreach (var synapse in AllSynapses)
            {
                if(offspring.AllSynapses.ContainsKey(synapse.Key) && RandomGenerator.Next(2) == 0)
                {
                    offspring.AllSynapses[synapse.Key].Weight = synapse.Value.Weight;
                }
            }
            return offspring;
        }

        void AddSynapse(Synapse template)
        {
            if(AllSynapses.ContainsKey(template.InnovationNo))
            {
                Debug.Log("wtf??");
                Debug.Break();
            }
            Neuron neuron1=null, neuron2=null;
            if (template.InputNeuron.InnovationNo <= InputLayer.Count)
                neuron1 = InputLayer[template.InputNeuron.InnovationNo-1];
            else
            {
                if (HiddenLayers.ContainsKey(template.InputNeuron.InnovationNo))
                    neuron1 = HiddenLayers[template.InputNeuron.InnovationNo];
                else
                { 
                    neuron1 = new Neuron(template.InputNeuron);
                    HiddenLayers.Add(neuron1.InnovationNo,neuron1);
                }
            }
            if (template.OutputNeuron.InnovationNo <= InputLayer.Count+OutputLayer.Count)
                neuron2 = OutputLayer[template.OutputNeuron.InnovationNo - InputLayer.Count-1];
            else
            {
                if (HiddenLayers.ContainsKey(template.OutputNeuron.InnovationNo))
                    neuron2 = HiddenLayers[template.OutputNeuron.InnovationNo];
                else
                {
                    neuron2 = new Neuron(template.OutputNeuron);
                    HiddenLayers.Add(neuron2.InnovationNo, neuron2);
                }
            }

            Synapse newSyn = new Synapse(neuron1, neuron2, template.InnovationNo);
            newSyn.Weight = template.Weight;
            AllSynapses.Add(newSyn.InnovationNo,newSyn);
            //Debug.Log("Koniec add synapse template");
        }

        public float Distance(NEAT partner)
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
            if(AllSynapses.Any() || partner.AllSynapses.Any())
            {
                int disjointGenes = 0;
                int excessGenes = 0;
                float weightDifference = 0;
                float N = Math.Max(AllSynapses.Count, partner.AllSynapses.Count);
                disjointGenes = 2 * AllSynapses.Keys.Union(partner.AllSynapses.Keys).Count() - AllSynapses.Count - partner.AllSynapses.Count;
                foreach (var synapseInnovationNo in AllSynapses.Keys.Intersect(partner.AllSynapses.Keys))
                {
                    weightDifference += Math.Abs((float)(AllSynapses[synapseInnovationNo].Weight - partner.AllSynapses[synapseInnovationNo].Weight));
                    //if (Math.Abs((float)(AllSynapses[synapseInnovationNo].Weight - partner.AllSynapses[synapseInnovationNo].Weight)) > Constants.Con.synapse_difference_threshold)
                    //    disjointGenes++;
                }

                result += (Constants.Con.c1 * excessGenes + Constants.Con.c2 * disjointGenes + Constants.Con.c3 * weightDifference)/N;
            }
            //if(HiddenLayers.Any() || partner.HiddenLayers.Any())      OutputLayer.Count > 0 always
            {
                int disjointGenes = 0;
                int excessGenes = 0;
                float weightDifference = 0;
                float N = Math.Max(HiddenLayers.Count, partner.HiddenLayers.Count)+OutputLayer.Count;
                foreach (var neuronPair in OutputLayer.Zip(partner.OutputLayer))
                {
                    weightDifference += Mathf.Abs((float)(neuronPair.Key.Bias - neuronPair.Value.Bias));
                    //if (Mathf.Abs((float)(neuronPair.Key.Bias - neuronPair.Value.Bias)) > Constants.Con.bias_difference_threshold)
                    //    disjointGenes++;
                }
                    
                disjointGenes = 2 * HiddenLayers.Keys.Union(partner.HiddenLayers.Keys).Count() - HiddenLayers.Count - partner.HiddenLayers.Count;
                foreach (var neuronInnovationNo in HiddenLayers.Keys.Intersect(partner.HiddenLayers.Keys))
                {
                    weightDifference += Math.Abs((float)(HiddenLayers[neuronInnovationNo].Bias - partner.HiddenLayers[neuronInnovationNo].Bias));
                    //if (Math.Abs((float)(HiddenLayers[neuronInnovationNo].Bias - partner.HiddenLayers[neuronInnovationNo].Bias)) > Constants.Con.bias_difference_threshold)
                    //    disjointGenes++;
                }
                result += (Constants.Con.c1 * excessGenes + Constants.Con.c2 * disjointGenes + Constants.Con.c3 * weightDifference) / N;
            }
            
            
            return result;
            //if (result < Constants.Con.delta_t)
            //    return true;
            //return false;
        }
    }
}