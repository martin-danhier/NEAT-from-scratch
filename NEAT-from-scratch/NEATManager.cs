using System;
using System.Collections.Generic;

namespace NEAT
{
    

    /// <summary>
    /// Thrown when a NEAT method is called but the <see cref="NEATManager"/> object isn't running in the right <see cref="NEATMode"/>.
    /// </summary>
    public class WrongNEATModeException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongNEATModeException"/> class with a specified error message.
        /// </summary>
        /// <param name="message"></param>
        public WrongNEATModeException(string message) : base(message) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="WrongNEATModeException"/> class.
        /// </summary>
        public WrongNEATModeException() : base() { }
    }

    /// <summary>
    /// The modes available to run NEAT.
    /// </summary>
    public enum NEATMode
    {
        /// <summary>
        /// All players of a generation at a time. (harder) 
        /// </summary>
        Simultaneously,
        /// <summary>
        /// One player at a time (easier).
        /// </summary>
        Separately
    }
    /// <summary>
    /// The global manager of the NEAT Algorithm. It allows you to interact with the NEAT process.
    /// </summary>
    public partial class NEATManager
    {
        //======================================== PROPERTIES =================================================
        public List<Player> Players { get; private set; }
        public NEATMode Mode { get; private set; }
        public int PopulationSize { get; private set; }
        public int Generation { get; private set; }

        private List<Species> SpeciesList;
        private Player bestPlayer;
        private List<NeuralNetwork.ConnectionHistory> history;
        private int nextInnovationNumber;

        //background of the property below
        private int _currentPlayerIndex;
        /// <summary>
        /// The index of the currently playing <see cref="Player"/>. Intended for the <see cref="NEATMode.Separately"/> mode.
        /// </summary>
        public int CurrentPlayerIndex
        {
            
            get
            {
                if (Mode == NEATMode.Separately)
                    return _currentPlayerIndex;
                else
                    throw new WrongNEATModeException("The CurrentPlayerIndex value is meant to be used in the NEATMode.Separately mode. ");
            }
            private set { _currentPlayerIndex = value;  }
        }

        

        // ======================================== CONSTRUCTOR ==============================================
        /// <summary>
        /// Instanciate a <see cref="T:NEAT.NEATManager"/>. It sets the configs for it to run correctly.
        /// </summary>
        /// <param name="inputs">The number of inputs neurons of a neural network.</param>
        /// <param name="outputs">The number of outputs of a neural network.</param>
        /// <param name="fitnessMethod">The method that computes the fitness of a dead player.</param>
        /// <param name="size">The size of the population.</param>
        /// <param name="mode">The mode that will be used to run NEAT.</param>
        public NEATManager(int inputs, int outputs, FitnessMethod fitnessMethod, int size = 200 , NEATMode mode = NEATMode.Separately)
        {
            Mode = mode;
            PopulationSize = size;
            if (Mode == NEATMode.Separately)
                CurrentPlayerIndex = 0;
            history = new List<NeuralNetwork.ConnectionHistory>();
            Players = new List<Player>();
            SpeciesList = new List<Species>();
            nextInnovationNumber = 0;
            for (int i = 0; i < size; i++)
            {
                Players.Add(new Player(inputs, outputs, ref fitnessMethod));
                Players[i].Brain.Mutate(history, ref nextInnovationNumber);
            }
            Generation = 1;


        }

        // ================================ SIMULTANEOUSLY MODE METHODS ======================================== 
        /// <summary>
        /// Kills the given player and calculate its fitness. Intended for <see cref="NEATMode.Simultaneously"/> mode.
        /// </summary>
        /// <param name="playerIndex">The index of the player in the list.</param>
        /// <param name="args">Args to calculate fitness.</param>
        public void Kill(int playerIndex, object args)
        {
            if (Mode == NEATMode.Simultaneously)
                Players[playerIndex].Kill(args);
            else
                throw new WrongNEATModeException("Expected NEATMode.Simultaneoulsy, but found NEATMode.Separately. Try to change the mode, or call NEATManager.KillCurrent(args) instead. ");
        }
        /// <summary>
        /// Computes through the neural network the decision of the given player. Intended for <see cref="NEATMode.Simultaneously"/> mode.
        /// </summary>
        /// <param name="playerIndex">The index of the player in the list.</param>
        /// <returns> The decision of the neural network. It's an int between 0 and the max output number. </returns>
        public int Think(int playerIndex, float[] inputs)
        {
            if (Mode == NEATMode.Simultaneously)
                return Players[playerIndex].Think(inputs);
            else
                throw new WrongNEATModeException("Expected NEATMode.Simultaneously, but found NEATMode.Separately. Try to change the mode, or call NEATManager.ThinkCurrent(inputs) instead.");
        }
        /// <summary>
        /// Evolves the population and generate a new one from the previous. Intended for <see cref="NEATMode.Simultaneously"/> mode.
        /// </summary>
        public void NextGeneration()
        {
            if (Mode == NEATMode.Simultaneously) 
               NaturalSelection();
            else
                throw new WrongNEATModeException("Expected NEATMode.Simultaneously, but found NEATMode.Separately. Try to change the mode, or you don't need to call this method.");
        }

        // ================================ SEPARATELY MODE METHODS ======================================== 

        /// <summary>
        /// Computes through the neural network the decision of the current player. Intended for <see cref="NEATMode.Separately"/> mode.
        /// </summary>
        /// <returns></returns>
        public int ThinkCurrent(float[] inputs)
        {
            if (Mode == NEATMode.Separately)
                return Players[_currentPlayerIndex].Think(inputs);
            else
                throw new WrongNEATModeException("Expected NEATMode.Separately, but found NEATMode.Simultaneously. Try to change the mode, or call NEATManager.Think(playerIndex, inputs) instead.");
        }
        /// <summary>
        /// Kills the current player and calculate its fitness. Intended for <see cref="NEATMode.Separately"/> mode.
        /// </summary>
        /// <param name="args">Args to calculate fitness.</param>
        public void KillCurrent(object args)
        {
            if (Mode == NEATMode.Separately)
            {
                Players[_currentPlayerIndex].Kill(args);
                if (_currentPlayerIndex == PopulationSize - 1) //The current generation is complete
                { 
                    NaturalSelection();
                    _currentPlayerIndex = 0;
                }

                else
                    _currentPlayerIndex++;
            }
            else
                throw new WrongNEATModeException("Expected NEATMode.Separately, but found NEATMode.Simultaneoulsy. Try to change the mode, or call NEATManager.Kill(playerIndex, args) instead.");
        }

        // ==================================== GLOBAL =========================================

        /// <summary>
        /// Get the best topology ever generated.
        /// </summary>
        /// <returns>The best topology.</returns>
        public NeuralNetwork ExportBestTopology()
        {
            return bestPlayer.Brain.Clone();
            
        }

        private void Speciate()
        {
            //Reset
            foreach (Species s in SpeciesList)
                s.Players.Clear();
            for (int i = 0; i < PopulationSize; i++)
            {
                bool speciesFound = false;
                foreach (Species s in SpeciesList)
                    if (s.BelongsToTheSameSpecies(Players[i].Brain))
                    {
                        s.AddPlayer(Players[i]);
                        speciesFound = true;
                        break;
                    }
                if (!speciesFound)
                    SpeciesList.Add(new Species(Players[i]));

            }

        }

        private void SortSpecies()
        {
            //sort players
            foreach (Species s in SpeciesList)
                s.SortSpecies();
            //sort species
            SpeciesList.Sort((x, y) => ( x.BestFitness.CompareTo(y.BestFitness) ));
        }

        private void CullSpecies()
        {
            foreach (Species s in SpeciesList)
            {
                s.Cull();
                s.FitnessSharing();
            }
        }

        private void KillBadSpecies()
        {
            float averageSum = 0;
            foreach (Species s in SpeciesList)
                averageSum += s.AverageFitness;

            for (int i = 1; i < SpeciesList.Count; i++)
            {
                if (SpeciesList[i].AverageFitness / averageSum * PopulationSize < 1) //no possible child
                {
                    SpeciesList.RemoveAt(i);
                    i--;
                }
                else if (i > 1 && SpeciesList[i].Staleness >= 15) //no progress
                {
                    SpeciesList.RemoveAt(i);
                    i--;
                }
            }

                        
            
        }

        private bool IsEveryoneDead()
        {
            foreach (Player p in Players)
                if (p.IsAlive)
                    return false;
            return true;
        }

        private void NaturalSelection()
        {
            if (IsEveryoneDead())
            {

                // Speciate and keep the best species
                Speciate(); //Separate the population into species
                SortSpecies(); //sort the species according to fitness
                CullSpecies(); //kill the bottom half of each species
                //get the best player
                Player genBest = SpeciesList[0].Players[0];
                if (bestPlayer == null)
                    bestPlayer = genBest.Clone();
                else if (genBest.Fitness > bestPlayer.Fitness)
                    bestPlayer = genBest.Clone();
                
                KillBadSpecies(); //kill the useless species

                Console.WriteLine("Generation: {0}, Number of mutations: {1}, Species: {2}", Generation, history.Count, SpeciesList.Count);

                //Get average fitness sum
                float averageSum = 0;
                foreach (Species s in SpeciesList)
                    averageSum += s.AverageFitness;

                List<Player> children = new List<Player>(); //Instanciates the next generation
                foreach (Species s in SpeciesList)
                {
                    children.Add(s.Champion);
                    for (int i = 0; i < Math.Floor((s.AverageFitness / averageSum) * PopulationSize - 1); i++) //add more or less babies according to the species
                    {
                        children.Add(s.GetBaby(history, ref nextInnovationNumber));
                    }

                }

                while (children.Count < PopulationSize)
                    children.Add(SpeciesList[0].GetBaby(history, ref nextInnovationNumber));
                Players = new List<Player>(children);
                Generation++;




            }
        }
    }
}
