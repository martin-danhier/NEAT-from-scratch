using System;
using NEAT;

namespace Program
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            //Create a genome
			Genome genome = new Genome (3, 2);
            //Add a connection between two nodes
			genome.Connections.Add (new ConnectionGene (genome.Nodes [0], genome.Nodes [3], -0.6f, 1));
            //Process the neural network and compute the outputs
			float[] outputs = genome.FeedForward(new float[] {3.14f, 2f, -1f });



			foreach (float f in outputs)
				Console.WriteLine (f);
		}
	}
}
