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
		/// Initializes a new instance of the <see cref="NEAT.Genome"/> class. A genome countains a neural network topology.
		/// </summary>
		/// <param name="inputs">The number of inputs in the NN.</param>
		/// <param name="outputs">The number of outputs in the NN.</param>
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
        /// <param name="inputValues">Input values. Must be as long as the number of input nodes.</param>
        /// <exception cref="Exception">Throw when the input array isn't as long as the number of input nodes in the genome.</exception>
        public float[] FeedForward(float[] inputValues)
		{
            if (inputValues.Length == Inputs)
            {
                GenerateNetwork();
                // set the inputs
                for (int i = 0; i < Inputs; i++)
                    Nodes[i].OutputValue = inputValues[i];
                Nodes[BiasNode].OutputValue = 1;

                foreach (Node node in network)
                    node.engage(); //engage every node (engage = send its output to the inputs of the nodes it's connected to)

                //the outputs are nodes[inputs] to nodes [inputs+outputs-1]
                float[] outputs = new float[Outputs];
                for (int i = 0; i < Outputs; i++)
                    outputs[i] = Nodes[Inputs + i].OutputValue;

                //reset all the nodes for the next feed forward
                foreach (Node node in Nodes)
                    node.InputSum = 0;

                return outputs;
            }
            else
            {
                throw new Exception("The input array must be the same size than the number of input nodes.");
            }

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

        // ================ MUTATIONS ==================
        //already done, cf other repo

        /// <summary>
        /// Randomly add a new connection between two nodes.
        /// </summary>
        private void LinkMutate(List<ConnectionHistory> history)
        {
            if (Nodes.Count >= 2 && Connections.Count < GetMaxConnections())
            {
                bool ConnectionAlreadyExists = false;
                int innovation = 1;
                int inNode = 0;
                int outNode = 0;
                Random random = new Random(); //random number generator
                do
                {
                    ConnectionAlreadyExists = false;
                    //Get two values for inNode and outNode that are different and valid
                    bool loop = true;
                    do
                    {
                        int randomIndex = random.Next(0, Nodes.Count);
                        if (Nodes[randomIndex].Layer != 1) //if the chosen node is not an output node
                        {
                            loop = false;
                            inNode = Nodes[randomIndex].Number;
                        }
                    } while (loop);
                    loop = true;
                    do
                    {
                        int randomIndex = random.Next(0, Nodes.Count);
                        if (Nodes[randomIndex].Layer != 0 && Nodes[randomIndex].Number != inNode) //if the chosen node is not an output node
                        {
                            loop = false;
                            outNode = Nodes[randomIndex].Number;
                        }
                    } while (loop);
                    //Check if the connection already exists
                    foreach (ConnectionGene connection in Connections)
                        if (connection.FromNode.Number == inNode && connection.ToNode.Number == outNode) //the node already exists
                            ConnectionAlreadyExists = true;

                } while (ConnectionAlreadyExists);
                //A random new valid generation has been generated.
                //Add it
                Connections.Add(new ConnectionGene(GetNode(inNode), GetNode(outNode), (float)random.NextDouble() * 4 - 2, innovation));
            }
            else
            {
                //can't add a new connection
                Console.WriteLine("The genome has reached its max connection number. Cannot add a new one.")
            }
        }
        /// <summary>
        /// Add a new node by splitting an existing connection into two connections and a new node
        /// </summary>
        private void NodeMutate(List<ConnectionHistory> history)
        {
            
        }
        /// <summary>
        /// Randomly enable/disable an existing connection.
        /// </summary>
        private void EnableDisableMutate()
        {
            if (Connections.Count > 0)
            {
                Connections[new Random().Next(Connections.Count)].Toggle();
            }
        }

        /// <summary>
        /// Randomly mutate this genome.
        /// </summary>
        /// <param name="history">The history of past connections</param>
        public void Mutate(List<ConnectionHistory> history)
        {
            if (Connections.Count == 0)
                LinkMutate(history);
            Random random = new Random();
            //Mutates weights
            if (random.NextDouble() < 0.08)
                foreach (ConnectionGene connection in Connections)
                    connection.MutateWeight();
            //Add a new connection
            if (random.NextDouble() < 0.08)
                LinkMutate(history);
            //Add a new node
            if (random.NextDouble() < 0.02)
                NodeMutate(history);
            //Toggle a connection
            if (random.NextDouble() < 0.01)
                EnableDisableMutate();
        }


        // =============== CROSSOVER ================
        public Genome Crossover (Genome partner)
        {
            Genome child = new Genome(Inputs, Outputs);
            child.Connections.Clear();
            child.Nodes.Clear();
            child.Layers = Layers;
            child.NextNode = NextNode;
            child.BiasNode = BiasNode;
            List<ConnectionGene> childConnections = new List<ConnectionGene>();
            List<bool> isEnabled = new List<bool>();
            //CONNECTIONS
            foreach (ConnectionGene connection in Connections)
            {
                Random random = new Random();
                bool setEnabled = true;
                int partnerConnection = GetMatchingGene(partner, connection.InnovationNumber);
                if (partnerConnection != -1) //if the other parent has the same gene
                {
                    //If one of the connections is disabled, there is 75% chance that the gene will be disabled
                    if ((!connection.IsEnabled || !partner.Connections[partnerConnection].IsEnabled) && random.NextDouble() < 0.75)
                        setEnabled = false;
                    //The gene is transmitted by one of the parents
                    if (random.NextDouble() < 0.5)
                        childConnections.Add(connection);
                    else
                        childConnections.Add(partner.Connections[partnerConnection]);
                }
                else //disjoint or excess gene -> add it
                {
                    childConnections.Add(connection);
                    setEnabled = connection.IsEnabled;
                }
                isEnabled.Add(setEnabled);
            }
            //the child has as many nodes as the fittest parent (this one)
            foreach (Node node in Nodes)
                child.Nodes.Add(node.Clone());
            //clone all connections
            for (int i = 0; i < childConnections.Count; i++)
            {
                child.Connections.Add(childConnections[i].Clone(child.GetNode(childConnections[i].FromNode.Number), child.GetNode(childConnections[i].ToNode.Number)));
                child.Connections[i].IsEnabled = isEnabled[i];
            }
            child.ConnectNodes();
            return child;

        }
        //===============USEFUL METHODS==============

        //TO FACTORIZE (usage of ref)
        /// <summary>
        /// Gets the innovation number.
        /// </summary>
        /// <returns>The innovation number.</returns>
        private int GetInnovationNumber(List<ConnectionHistory> history, ref int nextInnovationNumber, Node fromNode, Node toNode)
        {
            int innovation = nextInnovationNumber;
            bool isNew = true;
            foreach (ConnectionHistory element in history)
                if (element.Matches(this, fromNode, toNode))
                {
                    isNew = false; //mutation already exists
                    innovation = element.InnovationNumber; 
                }
            if (isNew)
            {
                List<int> innovationNumbers = new List<int>();
                foreach (ConnectionGene connection in Connections){
                    innovationNumbers.Add(connection.InnovationNumber);
                }
                history.Add(new ConnectionHistory(fromNode.Number,toNode.Number,innovation, innovationNumbers));
                nextInnovationNumber++;
            }
            return innovation;
        }

        /// <summary>
        /// Check if there is a connection matching the input innovation number.
        /// </summary>
        /// <returns>The matching gene index or -1 if there aren't any.</returns>
        int GetMatchingGene(Genome partner, int innovationNumber)
        {
            for (int i = 0; i < partner.Connections.Count; i++)
                if (partner.Connections[i].InnovationNumber == innovationNumber)
                    return i;
            return -1; //no matching gene found
        }




		Node GetNode(int nodeNumber)
		{
			foreach (Node n in Nodes)
				if (n.Number == nodeNumber)
					return n;
			return null; //if there isn't any node with that number
		}

        public override string ToString()
        {
            string str = "NODES:\n";
            foreach (Node node in Nodes)
                str += " - " + node + "\n";
            
            str += "CONNECTIONS:\n";
            foreach (ConnectionGene connection in Connections)
                str += " - " + connection + "\n";
            
            return str;

        }

        /// <summary>
        /// Computes the max number of connection that this genome may have with the current nodes.
        /// </summary>
        public int GetMaxConnections()
        {
            //the number of hidden nodes
            int hiddens = Nodes.Count - Outputs - Inputs;

            //     /- connections from inputs -\    /-connections from hiddens nodes-\
            return (Inputs * (hiddens + Outputs)) + (hiddens * (hiddens + Outputs - 1));

        }
    }


}

