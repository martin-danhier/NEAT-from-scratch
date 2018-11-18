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
            //Add connections between two nodes
			genome.Connections.Add (new ConnectionGene (genome.Nodes [0], genome.Nodes [3], -0.6f, 1));
            genome.Connections.Add(new ConnectionGene(genome.Nodes[1], genome.Nodes[4], 2f, 2));
            genome.Connections.Add(new ConnectionGene(genome.Nodes[0], genome.Nodes[4], 0.5f, 3));
            //Process the neural network and compute the outputs
            float[] outputs = genome.FeedForward(new float[] {3.14f, 2f, -1f });

            Console.WriteLine("GENOME:\n-------\n{0}\n\nOUTPUTS\n-------", genome);

            
			foreach (float f in outputs)
				Console.WriteLine (f);


            //Uncomment this if you are using Visual Studio Community instead of MonoDevelop
            //Console.WriteLine("\nPress any key to close the program.");
            //Console.ReadKey();
		}
	}
}
