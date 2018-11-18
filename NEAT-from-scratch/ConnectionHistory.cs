//this is a c# translation of a codebullet script. I first transcribe it to understand the concepts, then I'll write my own version.
using System;
using System.Collections.Generic;

namespace NEAT
{
    public class ConnectionHistory
    {
        public int FromNode;
        public int ToNode;
        public int InnovationNumber;
        private List<int> InnovationNumbers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NEAT.ConnectionHistory"/> class.
        /// </summary>
        public ConnectionHistory(int fromNode, int toNode, int innovationNumber, List<int> innovationNumbers)
        {
            FromNode = fromNode;
            ToNode = toNode;
            InnovationNumber = innovationNumber;
            InnovationNumbers = new List<int>(innovationNumbers);
        }

        public bool Matches(Genome genome, Node fromNode, Node toNode)
        {
            if (genome.Connections.Count == InnovationNumbers.Count && fromNode.Number == FromNode && toNode.Number == ToNode)
            {
                foreach (ConnectionGene connection in genome.Connections)
                    if (!InnovationNumbers.Contains(connection.InnovationNumber))
                        return false;
                return true;
            }
            return false;
        }
    }
}
