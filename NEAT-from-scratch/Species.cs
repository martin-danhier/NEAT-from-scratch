using System;
using System.Collections.Generic;

namespace NEAT
{
    partial class NEATManager
    {
        class Species
        {
            public List<Player> Players { get; private set; }
            public float BestFitness { get; private set; }
            public Player Champion { get; private set; }
            public float AverageFitness { get; private set; }
            public int Staleness { get; private set; } //how many generations the species has gone without an improvement
            public NeuralNetwork BestTopology { get; private set; }




            public float ExcessCoefficient { get; private set; } = 1;
            public float WeightDiffCoeff { get; private set; } = 0.5f;
            public float CompatibilityThreshold { get; private set; } = 4;

            private static Random randomGenerator = new Random();

            public Species(Player player)
            {
                Players = new List<Player>();
                Staleness = 0;
                Players.Add(player);
                BestFitness = player.Fitness;
                BestTopology = player.Brain.Clone();
                Champion = player.Clone();
            }


            // ==================================== METHODS ==============================================
            /// <summary>
            /// Kill the bottom half of the species
            /// </summary>
            /// <param name="sortBefore">Sort the players before cull</param>
            public void Cull(bool sortBefore = true)
            {
                if (Players.Count > 2)
                {
                    if (sortBefore)
                        SortSpecies();
                    for (int i = Players.Count/2; i < Players.Count; i++)
                    {
                        Players.RemoveAt(i);
                        i--;
                    }
                }
            }

            /// <summary>
            /// Selects a player. The more its fitness is big, the higher are its chances to be selected.
            /// </summary>
            /// <returns>The chosen player.</returns>
            private Player SelectPlayer()
            {
                float fitnessSum = 0;
                foreach (Player p in Players)
                    fitnessSum += p.Fitness;
                float random = Convert.ToSingle(randomGenerator.NextDouble()) * fitnessSum;
                float runningSum = 0;
                foreach (Player p in Players)
                {
                    runningSum += p.Fitness;
                    if (runningSum > random)
                        return p;
                }
                return Players[0];
            }

            /// <summary>
            /// Sort the players according to their fitnesses and update the best player.
            /// </summary>
            public void SortSpecies()
            {
                if (Players.Count > 0)
                {
                    //sort the players according to their fitness
                    Players.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
                    if (Players[0].Fitness > BestFitness)
                    {
                        Staleness = 0;
                        BestFitness = Players[0].Fitness;
                        BestTopology = Players[0].Brain.Clone();
                        Champion = Players[0].Clone();
                    }
                    else
                        Staleness++;
                }
            }

            public Player GetBaby(List<NeuralNetwork.ConnectionHistory> history, ref int NextInnovationNumber)
            {
                Player baby;
                if (randomGenerator.NextDouble() < 0.25) //Clone the player 25% of the time
                    baby = SelectPlayer().Clone();
                else //else crossover
                {
                    Player parent1 = SelectPlayer();
                    Player parent2 = SelectPlayer();
                    if (parent1.Fitness < parent2.Fitness)
                        baby = parent2.Crossover(parent1);
                    else
                        baby = parent1.Crossover(parent2);
                }
                baby.Brain.Mutate(history, ref NextInnovationNumber);
                return baby;
            }
            private void SetAverageFitness()
            {
                if (Players.Count > 0)
                {
                    float fitnessSum = 0;
                    foreach (Player p in Players)
                        fitnessSum += p.Fitness;
                    AverageFitness = fitnessSum / Players.Count;
                }
            }

            public bool BelongsToTheSameSpecies(NeuralNetwork g)
            {
                //inspired by codebullet
                float compatibility;
                float excessAndDisjoint = GetExcessAndDisjoint(g, BestTopology);
                float averageWeightDifference = GetAverageWeightDifference(g, BestTopology);

                float largeGenomeNormaliser = g.Connections.Count - 20;
                if (largeGenomeNormaliser < 1)
                    largeGenomeNormaliser = 1;
                compatibility = (ExcessCoefficient * excessAndDisjoint / largeGenomeNormaliser) + (WeightDiffCoeff * averageWeightDifference);
                return (CompatibilityThreshold > compatibility);
            }

            private static float GetExcessAndDisjoint(NeuralNetwork a, NeuralNetwork b)
            {
                //inspired by codebullet
                float matching = 0f;
                foreach (NeuralNetwork.ConnectionGene connectionA in a.Connections)
                    foreach(NeuralNetwork.ConnectionGene connectionB in b.Connections)
                        if (connectionA.InnovationNumber == connectionB.InnovationNumber)
                        {
                            matching++;
                            break;
                        }
                return a.Connections.Count + b.Connections.Count - (2 * matching);
            }

            private static float GetAverageWeightDifference(NeuralNetwork a, NeuralNetwork b)
            {
                //inspired by codebullet
                if (a.Connections.Count == 0 || b.Connections.Count == 0)
                    return 0;
                float totalWeightDifference = 0;
                float matching = 0;

                foreach (NeuralNetwork.ConnectionGene connectionA in a.Connections)
                    foreach (NeuralNetwork.ConnectionGene connectionB in b.Connections)
                        if (connectionA.InnovationNumber == connectionB.InnovationNumber)
                        {
                            matching++;
                            totalWeightDifference += Math.Abs(connectionA.Weight - connectionB.Weight);
                            break;
                        }
                if (matching == 0)
                    return 100;
                return totalWeightDifference / matching;
            }

            public void AddPlayer(Player p)
            {
                Players.Add(p);

            }

            public void FitnessSharing()
            {
                if (Players.Count > 0)
                    foreach (Player p in Players)
                        p.Fitness /= Players.Count;
                SetAverageFitness();
            }
        }
    }
}
