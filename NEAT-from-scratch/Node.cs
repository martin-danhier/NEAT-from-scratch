using System;
using System.Collections.Generic;

namespace NEAT
{
	public class Node
	{

		public float InputSum { get; set; }
		public float OutputValue { get; set; }
		public int Layer { get;  set; }
		public int Number { get; private set; }
		public List<ConnectionGene> OutputConnections { get; private set; }

		// ============ CONSTRUCTOR =============
		/// <summary>
		/// Initializes a new instance of the <see cref="NEAT.Node"/> class.
		/// </summary>
		public Node(int number, int layer = 0)
		{
			Number = number;
			Layer = layer;
			OutputConnections = new List<ConnectionGene> ();
		}

		/// <summary>
		/// Sends its output to the inputs of the nodes its connected to.
		/// </summary>
		public void engage()
		{
			//no sigmoid for input layer and bias
			if (Layer != 0)
				OutputValue = sigmoid (InputSum);

			foreach (ConnectionGene connection in OutputConnections)
				if (connection.IsEnabled)
					//add the weighted output to the sum of the inputs of whatever node this node is connected to
					connection.ToNode.InputSum += connection.Weight * OutputValue;
		}

		/// <summary>
		/// Sigmoid activation function
		/// </summary>
		private float sigmoid(float x)
		{
			return 1 / (1 + (float) Math.Pow (Math.E, -4.9f * x));
		}

        public override string ToString()
        {
            return String.Format("Node #{0}, Layer: {1}, {2} output connection(s), Output value: {3} ", Number, Layer, OutputConnections.Count, OutputValue);

        }

        /// <summary>
        /// Clone this node.
        /// </summary>
        public Node Clone()
        {
            Node clone = new Node(Number);
            clone.Layer = Layer;
            return clone;
        }
    }
}

