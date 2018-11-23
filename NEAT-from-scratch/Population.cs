using System;
using System.Collections.Generic;

namespace NEAT
{
    class Population
    {
        public List<Player> Players { get; private set; }

        public Population(int size, int inputs, int outputs, ref FitnessMethod fitnessMethod)
        {
            Players = new List<Player>();
            for (int i = 0; i < size; i++)
            {
                Players.Add(new Player(inputs, outputs, ref fitnessMethod));
            }
        }
    }
}
