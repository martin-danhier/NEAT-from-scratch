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
    public class NEATManager
    {
        //======================================== PROPERTIES =================================================
        public List<Player> Players { get; private set; }
        public NEATMode Mode { get; private set; }
        public int PopulationSize { get; private set; }

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
            Players = new List<Player>();
            for (int i = 0; i < size; i++)
            {
                Players.Add(new Player(inputs, outputs, ref fitnessMethod));
            }
            Mode = mode;
            PopulationSize = size;
            if (Mode == NEATMode.Separately)
                CurrentPlayerIndex = 0;
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
            throw new NotImplementedException();
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
                     ;//NextGeneration TO DO
                else
                    _currentPlayerIndex++;
            }
            else
                throw new WrongNEATModeException("Expected NEATMode.Separately, but found NEATMode.Simultaneoulsy. Try to change the mode, or call NEATManager.Kill(playerIndex, args) instead.");
        }
    }
}
