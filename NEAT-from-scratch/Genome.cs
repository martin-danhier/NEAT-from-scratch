using System;
using System.Collections.Generic;

namespace NEAT
{
	public class Genome
	{
		public List<ConnectionGene> Connections { get; private set; }
		public List<Node> Nodes { get; private set; }
		public int Inputs { get; private set; }
		public int Outputs { get; private set; }
		public int Layers { get; private set; }
		public int NextNode { get; private set; }
		public int BiasNode { get; private set; }

		private List<Node> network; //a list of the nodes in the order that they need to be considered by the neural network

		//============CONSTRUCTOR=============
		/// <summary>
		/// Initializes a new instance of the <see cref="NEAT.Genome"/> class.
		/// </summary>
		/// <param name="inputs">Inputs.</param>
		/// <param name="outputs">Outputs.</param>
		public Genome(int inputs, int outputs)
		{
			Layers = 2;
			NextNode = 0;
			Connections = new List<ConnectionGene> ();
			Nodes = new List<Node> ();

			//set input number and output number
			Inputs = inputs;
			Outputs = outputs;

			//create input nodes
			for (int i = 0; i<inputs;i++)
				Nodes.Add (new Node (NextNode++, 0));
			
			//create output nodes
			for (int i = 0; i < outputs; i++) 
				Nodes.Add(new Node (NextNode++,1));

			//create bias node
			Nodes.Add (new Node (NextNode, 0));
			BiasNode = NextNode++;

		}


		//==============MAIN METHODS==============
		/// <summary>
		/// Generate the output values of the NN.
		/// </summary>
		/// <returns>Output values of the NN</returns>
		/// <param name="inputValues">Input values.</param>
		public float[] FeedForward(float[] inputValues)
		{
            GenerateNetwork();
			// set the inputs
			for (int i = 0; i< Inputs; i++)
				Nodes[i].OutputValue = inputValues[i];
			Nodes [BiasNode].OutputValue = 1;

			foreach (Node node in network)
				node.engage (); //engage every node (engage = send its output to the inputs of the nodes it's connected to)

			//the outputs are nodes[inputs] to nodes [inputs+outputs-1]
			float[] outputs = new float[Outputs];
			for (int i = 0; i < Outputs; i++)
				outputs [i] = Nodes [Inputs + i].OutputValue;

			//reset all the nodes for the next feed forward
			foreach (Node node in Nodes)
				node.InputSum = 0;

			return outputs;

		}

		/// <summary>
		/// Adds the connections going out of a node to that node so that it can access the next node during feeding forward.
		/// </summary>
		private void ConnectNodes()
		{
			// Reset connections
			foreach (Node node in Nodes) 
				node.OutputConnections.Clear ();
			// Connect
			foreach (ConnectionGene gene in Connections)
				gene.FromNode.OutputConnections.Add (gene);
		}

		/// <summary>
		/// Sets up the NN as a list of nodes in order to be engaged.
		/// </summary>
		private void GenerateNetwork()
		{
			ConnectNodes ();
			network = new List<Node> ();

			//for each layer add the node in that layer, since layers cannot connect to themselves there is no need to order the nodes within a layer.

				for (int layer = 0; layer < Layers; layer++) //for each layer
					foreach (Node node in Nodes) //for each node
						if (node.Layer == layer) //if that node is in that layer
							network.Add (node); // add it

		}
		//===============USEFUL METHODS==============


		Node GetNode(int nodeNumber)
		{
			foreach (Node n in Nodes)
				if (n.Number == nodeNumber)
					return n;
			return null; //if there isn't any node with that number
		}
	}


}

