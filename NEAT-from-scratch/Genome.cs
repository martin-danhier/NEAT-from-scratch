using System;
using System.Collections.Generic;

namespace NEAT
{
    public partial class NeuralNetwork
    {
        public List<ConnectionGene> Connections { get; protected set; }
        public List<Node> Nodes { get; protected set; }
        public int Inputs { get; protected set; }
        public int Outputs { get; protected set; }
        public int Layers { get; protected set; }
        public int NextNode { get; protected set; }
        public int BiasNode { get; protected set; }
        protected List<Node> network; //a list of the nodes in the order that they need to be considered by the neural network
        protected static Random randomGenerator = new Random();

        //============CONSTRUCTOR=============
        /// <summary>
        /// Initializes a new instance of the <see cref="NEAT.NeuralNetwork"/> class. A genome countains a neural network topology.
        /// </summary>
        /// <param name="inputs">The number of inputs in the NN.</param>
        /// <param name="outputs">The number of outputs in the NN.</param>
        public NeuralNetwork(int inputs, int outputs)
        {
            Layers = 2;
            NextNode = 0;
            Connections = new List<ConnectionGene>();
            Nodes = new List<Node>();

            //set input number and output number
            Inputs = inputs;
            Outputs = outputs;

            //create input nodes
            for (int i = 0; i < inputs; i++)
                Nodes.Add(new Node(NextNode++, 0));

            //create output nodes
            for (int i = 0; i < outputs; i++)
                Nodes.Add(new Node(NextNode++, 1));

            //create bias node
            Nodes.Add(new Node(NextNode, 0));
            BiasNode = NextNode++;

        }

        // ============ MAIN METHODS ===============
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
        protected void ConnectNodes()
        {
            // Reset connections
            foreach (Node node in Nodes)
                node.OutputConnections.Clear();
            // Connect
            foreach (ConnectionGene gene in Connections)
                gene.FromNode.OutputConnections.Add(gene);
        }

        /// <summary>
		/// Sets up the NN as a list of nodes in order to be engaged.
		/// </summary>
		protected void GenerateNetwork()
        {
            ConnectNodes();
            network = new List<Node>();

            //for each layer add the node in that layer, since layers cannot connect to themselves there is no need to order the nodes within a layer.

            for (int layer = 0; layer < Layers; layer++) //for each layer
                foreach (Node node in Nodes) //for each node
                    if (node.Layer == layer) //if that node is in that layer
                        network.Add(node); // add it

        }

        // ============== USEFUL METHODS =================
        public NeuralNetwork Clone()
        {
            NeuralNetwork clone = new NeuralNetwork(Inputs, Outputs);

            clone.Nodes.Clear();
            clone.Connections.Clear();

            foreach (Node n in Nodes)
                clone.Nodes.Add(n.Clone());

            foreach (ConnectionGene c in Connections)
                clone.Connections.Add(c.Clone(clone.GetNode(c.FromNode.Number), clone.GetNode(c.ToNode.Number)));

            clone.Layers = Layers;
            clone.NextNode = NextNode;
            clone.BiasNode = BiasNode;
            clone.ConnectNodes();
            return clone;

        }

        protected Node GetNode(int nodeNumber)
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
    }





	public class Genome : NeuralNetwork
	{

        //============CONSTRUCTOR=============
        /// <summary>
        /// Initializes a new instance of the <see cref="NEAT.Genome"/> class. A genome countains a neural network topology.
        /// </summary>
        /// <param name="inputs">The number of inputs in the NN.</param>
        /// <param name="outputs">The number of outputs in the NN.</param>
        public Genome(int inputs, int outputs) : base(inputs, outputs)
        {
            //everything is done in the base constructor
        }

        // ================ MUTATIONS ==================
        //already done, cf other repo

        /// <summary>
        /// Randomly add a new connection between two nodes.
        /// </summary>
        private void LinkMutate(List<ConnectionHistory> history, ref int nextInnovationNumber)
        {
            if (Nodes.Count >= 2 && Connections.Count < GetMaxConnections())
            {
                bool ConnectionAlreadyExists = false;

                int inNode = 0;
                int outNode = 0; //random number generator
                do
                {
                    ConnectionAlreadyExists = false;
                    //Get two values for inNode and outNode that are different and valid
                    bool loop = true;
                    do
                    {
                        int randomIndex = randomGenerator.Next(0, Nodes.Count);
                        if (Nodes[randomIndex].Layer != Layers-1) //if the chosen node is not an output node
                        {
                            loop = false;
                            inNode = Nodes[randomIndex].Number;
                        }
                    } while (loop);
                    loop = true;
                    do
                    {
                        int randomIndex = randomGenerator.Next(0, Nodes.Count);
                        if (Nodes[randomIndex].Layer != 0 && Nodes[randomIndex].Number != inNode) //if the chosen node is not an input node
                        {
                            loop = false;
                            outNode = Nodes[randomIndex].Number;
                        }
                    } while (loop);

                    //if the first node is after the second, reverse the connection (to have a forward-directed network)
                    if (GetNode(inNode).Layer > GetNode(outNode).Layer)
                    {
                        //swap the values
                        inNode += outNode;
                        outNode = inNode - outNode;
                        inNode = inNode - outNode;
                    }

                    //Check if the connection already exists
                    foreach (ConnectionGene connection in Connections)
                        if (connection.FromNode.Number == inNode && connection.ToNode.Number == outNode) //the node already exists
                            ConnectionAlreadyExists = true;

                } while (ConnectionAlreadyExists);
                //A random new valid generation has been generated.
                //Add it
                int innovation = GetInnovationNumber(history, ref nextInnovationNumber, GetNode(inNode), GetNode(outNode));
                Connections.Add(new ConnectionGene(GetNode(inNode), GetNode(outNode), (float)randomGenerator.NextDouble() * 4 - 2, innovation));
                ConnectNodes();
            }
            else
            {
                //can't add a new connection
                Console.WriteLine("The genome has reached its max connection number. Cannot add a new one.");
            }
        }
        /// <summary>
        /// Add a new node by splitting an existing connection into two connections and a new node
        /// </summary>
        private void NodeMutate(List<ConnectionHistory> history, ref int nextInnovationNumber)
        {
            if (IsThereAnyEnabledConnection())
            {
                //chose a connection
                int randomIndex = 0;
                do
                {
                    randomIndex = randomGenerator.Next(Connections.Count);
                } while (!Connections[randomIndex].IsEnabled && Connections[randomIndex].FromNode.Number == BiasNode);
                // disable it
                Connections[randomIndex].Disable();
                // create a new node
                Nodes.Add(new Node(NextNode, Connections[randomIndex].FromNode.Layer + 1));
                //create connections that have the exact same effect than the first one
                Connections.Add(new ConnectionGene(Connections[randomIndex].FromNode, GetNode(NextNode), 1, GetInnovationNumber(history, ref nextInnovationNumber, Connections[randomIndex].FromNode, GetNode(NextNode))));
                Connections.Add(new ConnectionGene(GetNode(NextNode), Connections[randomIndex].ToNode, Connections[randomIndex].Weight, GetInnovationNumber(history, ref nextInnovationNumber, GetNode(NextNode), Connections[randomIndex].ToNode)));
                //Connect the bias node to the new node
                Connections.Add(new ConnectionGene(GetNode(BiasNode), GetNode(NextNode), 0, GetInnovationNumber(history, ref nextInnovationNumber, GetNode(BiasNode), GetNode(NextNode))));

                //if the layer of the new node doesn't exist, create it (shift every layer positioned after this one)
                if (GetNode(NextNode).Layer == Connections[randomIndex].ToNode.Layer)
                {
                    foreach (Node n in Nodes)
                        if (n.Layer >= GetNode(NextNode).Layer && n.Number != NextNode)
                            n.Layer++;
                    Layers++;
                }
                ConnectNodes();

                NextNode++;
            }
            else
            {
                Console.WriteLine("There are no enabled connection to split.");
            }
        }
        /// <summary>
        /// Randomly enable/disable an existing connection.
        /// </summary>
        private void EnableDisableMutate()
        {
            if (Connections.Count > 0)
            {
                Connections[randomGenerator.Next(Connections.Count)].Toggle();
            }
        }

        /// <summary>
        /// Randomly mutate this genome.
        /// </summary>
        /// <param name="history">The history of past connections</param>
        public void Mutate(List<ConnectionHistory> history, ref int nextInnovationNumber)
        {
            if (Connections.Count == 0)
                LinkMutate(history, ref nextInnovationNumber);
            //Mutates weights
            if (randomGenerator.NextDouble() < 0.8)
                foreach (ConnectionGene connection in Connections)
                    connection.MutateWeight();
            //Add a new connection
            double randomNumber = randomGenerator.NextDouble();
            if ( randomNumber < 0.08)
                LinkMutate(history, ref nextInnovationNumber);
            //Add a new node
            if (randomGenerator.NextDouble() < 0.02)
                NodeMutate(history, ref nextInnovationNumber);
            //Toggle a connection
            if (randomGenerator.NextDouble() < 0.02)
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
                bool setEnabled = true;
                int partnerConnection = GetMatchingGene(partner, connection.InnovationNumber);
                if (partnerConnection != -1) //if the other parent has the same gene
                {
                    //If one of the connections is disabled, there is 75% chance that the gene will be disabled
                    if ((!connection.IsEnabled || !partner.Connections[partnerConnection].IsEnabled) && randomGenerator.NextDouble() < 0.75)
                        setEnabled = false;
                    //The gene is transmitted by one of the parents
                    if (randomGenerator.NextDouble() < 0.5)
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
        private int GetMatchingGene(Genome partner, int innovationNumber)
        {
            for (int i = 0; i < partner.Connections.Count; i++)
                if (partner.Connections[i].InnovationNumber == innovationNumber)
                    return i;
            return -1; //no matching gene found
        }

        /// <summary>
        /// Computes the max number of connection that this genome may have with the current nodes.
        /// </summary>
        private int GetMaxConnections()
        {
            //the number of hidden nodes
            int hiddens = Nodes.Count - Outputs - (Inputs + 1);

            //     /-     connections from input    -\    /-connections from hiddens nodes-\
            return ((Inputs + 1) * (hiddens + Outputs)) + (hiddens * (hiddens + Outputs - 1));

        }



        private bool IsThereAnyEnabledConnection()
        {
            
            //Check if there are enabled connections
            foreach (ConnectionGene connection in Connections)
                if (connection.IsEnabled && connection.FromNode.Number != BiasNode)
                    return true;
            //Can't find any enabled connection
            return false;
        }

        public new Genome Clone()
        {
            Genome clone = new Genome(Inputs, Outputs);

            clone.Nodes.Clear();
            clone.Connections.Clear();

            foreach (Node n in Nodes)
                clone.Nodes.Add(n.Clone());

            foreach (ConnectionGene c in Connections)
                clone.Connections.Add(c.Clone(clone.GetNode(c.FromNode.Number), clone.GetNode(c.ToNode.Number)));

            clone.Layers = Layers;
            clone.NextNode = NextNode;
            clone.BiasNode = BiasNode;
            clone.ConnectNodes();
            return clone;

        }
    }


}

