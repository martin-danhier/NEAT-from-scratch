using System;

namespace NEAT
{
	public class ConnectionGene
	{
		public Node FromNode { get; private set; }
		public Node ToNode { get; private set; }
		public float Weight { get; private set; }
		public bool IsEnabled { get; private set; }
		public int InnovationNumber { get; private set; }

		//========= CONSTRUCTOR ==========
		/// <summary>
		/// Initializes a new instance of the <see cref="NEAT.ConnectionGene"/> class.
		/// </summary>
		public ConnectionGene(Node fromNode, Node toNode, float weight, int innovationNumber)
		{
			FromNode = fromNode;
			ToNode = toNode;
			Weight = weight;
			InnovationNumber = innovationNumber;
			IsEnabled = true;
		}

		// ============ METHODS ==========
		/// <summary>
		/// Mutates the weight.
		/// </summary>
		public void MutateWeight()
		{
			
		}

		/// <summary>
		/// Returns a copy of this connectionGene.
		/// </summary>
		public ConnectionGene clone(Node fromNode, Node toNode)
		{
			ConnectionGene clone = new ConnectionGene (fromNode, toNode, Weight, InnovationNumber);
			clone.IsEnabled = true;
			return clone;
		}

        public override string ToString()
        {
            return String.Format("Connection from Node #{0} to Node #{1}, Enabled: {2}, Innovation: {3}, Weight: {4}", FromNode.Number, ToNode.Number, IsEnabled, InnovationNumber, Weight);

        }

    }
}

