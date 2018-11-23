using System;

namespace NEAT
{
    /// <summary>
    /// A method that computes the fitness (score) of the current player. The more the player is good, the higher the fitness.
    /// </summary>
    /// <param name="sender">The player that is calculating its fitness. </param>
    /// <param name="args">The arg(s) passed to calculate the fitness.</param>
    /// <returns></returns>
    public delegate float FitnessMethod(Player sender, object args);

    public class Player
    {
        public Genome Brain { get; private set; }
        public float Fitness { get; private set; }
        public bool IsAlive { get; private set; }

        
        private FitnessMethod fitness;

        public Player(int inputs, int outputs, ref FitnessMethod fitnessMethod)
        {
            Brain = new Genome(inputs, outputs);
            IsAlive = true;
            fitness = fitnessMethod;
        }

        public void Kill(object args)
        {
            if (IsAlive)
            {
                //kill it
                IsAlive = false;
                //calculate fitness
                try
                {
                    Fitness = fitness(this, args);
                }
                catch (Exception e)
                {
                    throw new Exception("Fitness method invalid.", e);
                }
            }
        }

        public int Think(float[] inputs)
        {
            float[] outputs = Brain.FeedForward(inputs);
            //Get max value
            int max = 0;
            for (int i = 0; i < outputs.Length; i++)
                if (outputs[i] > max)
                    max = i;
            return max;
        }           

    }
}
