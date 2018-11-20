using System;

namespace NEAT
{
	public class ConnectionGene
	{
		public Node FromNode { get; private set; }
		public Node ToNode { get; private set; }
		public float Weight { get; private set; }
		public bool IsEnabled { get; set; }
		public int InnovationNumber { get; private set; }

        private static Random randomGenerator = new Random();

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
            if (randomGenerator.NextDouble() < 0.1)
                Weight = Convert.ToSingle((randomGenerator.NextDouble() * 2) - 1);
            else
            {
                Weight += Convert.ToSingle(randomGenerator.NextDouble() / 50);
                if (Weight > 1)
                    Weight = 1;
                if (Weight < -1)
                    Weight = -1;
            }
            
        }
        /// <summary>
        /// Enable this connection.
        /// </summary>
        public void Enable()
        {
            IsEnabled = true;
        }
        /// <summary>
        /// Disable this connection.
        /// </summary>
        public void Disable()
        {
            IsEnabled = false;
        }
        /// <summary>
        /// Toggle this connection.
        /// </summary>
        public void Toggle()
        {
            if (IsEnabled)
                IsEnabled = false;
            else
                IsEnabled = true;
        }

		/// <summary>
		/// Returns a copy of this connectionGene.
		/// </summary>
		public ConnectionGene Clone(Node fromNode, Node toNode)
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

