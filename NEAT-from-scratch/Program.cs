using System;
using NEAT;
using System.Collections.Generic;

namespace Program
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            List<ConnectionHistory> history = new List<ConnectionHistory>();
            int nextInnovationNumber = 1000;

            //Create a genome
			Genome genome = new Genome (3, 2);
            //Add connections between two nodes
			genome.Connections.Add (new ConnectionGene (genome.Nodes [0], genome.Nodes [3], -0.6f, 1));
            genome.Connections.Add(new ConnectionGene(genome.Nodes[1], genome.Nodes[4], 1f, 2));
            genome.Connections.Add(new ConnectionGene(genome.Nodes[0], genome.Nodes[4], 0.5f, 3));
            //Process the neural network and compute the outputs
            Genome genome2 = genome.Clone();
            int iterations = 0;
            genome2.Mutate(history, ref nextInnovationNumber);

            Console.WriteLine("GENOME 1:\n{0}\nGENOME 2: (after {2} iterations)\n{1}\n\n\n\n", genome, genome2, iterations);

            


            //Genome genome3 = genome2.Crossover(genome);
            //Console.WriteLine("GENOME 3:\n{0}\n", genome3);
            //Uncomment this if you are using Visual Studio Community instead of MonoDevelop
            Console.WriteLine("\nPress any key to close the program.");
            Console.ReadKey();
		}
	}
}
