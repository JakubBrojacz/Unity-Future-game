using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NeuralNetwork
{
    public class NEATDrower : MonoBehaviour {

        // Use this for initialization
        void Start() {
            
        }

        // Update is called once per frame
        void Update() {

        }

        public void Init()
        {
            nodes = new List<GameObject>();
            connections = new List<LineRenderer>();
        }

        Vector3 ComputeNeuron(Neuron n, Vector3? nextPosition = null)
        {
            if (n.Done)
                return n.position;
            n.Done = true;
            //create next ball
            var newNode = Instantiate(nodePrefab, new Vector3(0, 0, 0), Quaternion.identity).gameObject;
            newNode.transform.localScale = new Vector3(radius, radius, radius);
            newNode.GetComponent<BallScript>().InnovationNo = n.InnovationNo;
            if (n.OutputValue > 0.5)
                newNode.GetComponent<Renderer>().material.color = new Color(0, 1, 0);
            nodes.Add(newNode);
            //calculate position
            Vector3 position;
            if (nextPosition==null)
            {
                float y_tmp = 0;
                if (n.InputSynapses.Count > 0)
                {
                    foreach (var syn in n.InputSynapses)
                    {
                        newNode.GetComponent<BallScript>().InputInnovationNo.Add(syn.InputNeuron.InnovationNo);
                        y_tmp += ComputeNeuron(syn.InputNeuron, null).y;
                    }
                    if (n.InputSynapses.Count == 0)
                        Debug.Break();
                    y_tmp /= n.InputSynapses.Count;
                }
                else
                    y_tmp = initialPosittion.y;
                position = new Vector3(initialPosittion.x + horizontal * offset * 2, y_tmp, 0);
                horizontal++;
            }
            else
            {
                foreach (var syn in n.InputSynapses)
                {
                    newNode.GetComponent<BallScript>().InputInnovationNo.Add(syn.InputNeuron.InnovationNo);
                    ComputeNeuron(syn.InputNeuron, null);
                }
                position = (Vector3)nextPosition;
            }
            n.position = position;
            newNode.transform.position = position;
            //create new links
            foreach(var syn in n.InputSynapses)
            {
                GameObject gObject = new GameObject("Empty "+i);
                connections.Add(gObject.AddComponent<LineRenderer>());
                gObject.GetComponent<LineRenderer>().positionCount = 2;
                gObject.GetComponent<LineRenderer>().startWidth = lineWidth;
                gObject.GetComponent<LineRenderer>().SetPosition(0, n.position);
                gObject.GetComponent<LineRenderer>().SetPosition(1, syn.InputNeuron.position);
                i += 1;
            };
            return n.position;
        }

        public void Assign(NEAT source)
        {
            foreach (var n in nodes)
                Destroy(n);
            nodes.Clear();
            foreach (var c in connections)
                Destroy(c.gameObject);
            connections.Clear();

            source.InputLayer.ForEach(neuron => neuron.Done = false);
            source.OutputLayer.ForEach(neuron => neuron.Done = false);
            source.HiddenLayers.Values.ToList().ForEach(neuron => neuron.Done = false);
            horizontal = 1;

            i = 0;
            var position = initialPosittion;
            foreach(var neuron in source.InputLayer)
            {
                ComputeNeuron(neuron,position);
                position.y += offset;
            }
            position = initialPosittion;
            position.x += offset * (source.HiddenLayers.Count + 1) * 2;
            position.y += offset * (source.InputLayer.Count - source.OutputLayer.Count) / 2;
            foreach (var neuron in source.OutputLayer)
            {
                ComputeNeuron(neuron, position);
                position.y += offset;
            }
        }

        List<GameObject> nodes;
        List<LineRenderer> connections;

        public Vector3 initialPosittion;
        public float offset = 5;
        public float radius = 0.5f;
        public float lineWidth = 0.1f;
        int horizontal;

        int i;

        public Transform nodePrefab;
    }
}