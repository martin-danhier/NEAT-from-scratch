﻿using System;
using NEAT;
using System.Collections.Generic;

namespace Program
{
	class MainClass
	{
        

		public static void Main (string[] args)
		{

            NEATManager manager = new NEATManager(3, 2, CalculateFitness, 200, NEATMode.Separately) ;
            for (int i = 0; i < manager.PopulationSize * 10; i++)
            {
                //Console.WriteLine("Player " + manager.CurrentPlayerIndex);
                //Console.WriteLine("Decision : " + manager.ThinkCurrent(new float[] { 1.5f, 0.72f, -1.618f }));
                manager.KillCurrent(new int[] { i, 2 });
                //Console.WriteLine("Fitness : " + manager.Players[manager.CurrentPlayerIndex - 1].Fitness+"\n");
            }


            //Uncomment this if you are using Visual Studio instead of MonoDevelop
            Console.WriteLine("\nPress any key to close the program.");
            Console.ReadKey();
		}

        //DEFINE YOUR OWN FITNESS METHOD
        private static float CalculateFitness(Player sender, object args)
        {
            int[] data = args as int[];
            return Convert.ToSingle(data[0] * data[0] + data[1]);
        } 
    }
}
