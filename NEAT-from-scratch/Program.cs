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
            Genome genome2 = new Genome(3, 2);
            genome2.Connections.Add(new ConnectionGene(genome2.Nodes[0], genome2.Nodes[3], 2.54f, 1));
            genome2.Connections.Add(new ConnectionGene(genome2.Nodes[1], genome2.Nodes[4], 2.4f, 2));
            genome2.Connections[1].Disable();
            genome2.Connections.Add(new ConnectionGene(genome2.Nodes[0], genome2.Nodes[4], 0.005f, 3));
            genome2.Connections.Add(new ConnectionGene(genome2.Nodes[2], genome2.Nodes[3], 1.6f, 4));

            Console.WriteLine("GENOME 1:\n{0}\nGENOME 2:\n{1}\n\n\n\nmutation ........\n\n\n", genome, genome2);
            Genome genome3 = genome2.Crossover(genome);
            Console.WriteLine("GENOME 3:\n{0}\n", genome3);
            //Uncomment this if you are using Visual Studio Community instead of MonoDevelop
            Console.WriteLine("\nPress any key to close the program.");
            Console.ReadKey();
		}
	}
}
