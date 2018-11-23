using System;

namespace NEAT
{
    public delegate float FitnessMethod();

    class Player
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

        public void Kill()
        {
            //kill it
            IsAlive = false;
            //calculate fitness
            try
            {
                Fitness = fitness();
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
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
