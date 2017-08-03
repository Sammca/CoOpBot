using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CoOpBot.Modules.Roll
{
    [Name("Dice rolls")]
    public class RollModule : ModuleBase
    {
        Random rng;
        public RollModule()
        {
            // Need to initialise random number generator here]
            // If not, it keeps getting reinitialised with the same seed and always gives the same result
            rng = new Random();
        }

        [Command("Roll")]
        [Summary("Rolls a specified number of dice, with a specified number of sides. Time to Die.")]
        public async Task roll(string diceModifier = "")
        {
            int numberOfDice;
            int sidesOnDice;
            string[] splitInput;
            int rollResult;
            string output;
            
            numberOfDice = 1;
            sidesOnDice = 6;
            rollResult = 0;
            output = "";

            try
            {

                // check if the roll has been modified with an input
                if (diceModifier.Contains("d"))
                {
                    splitInput = diceModifier.Split('d');
                    if (splitInput.Length == 2)
                    {
                        numberOfDice = int.Parse(splitInput[0]);
                        sidesOnDice = int.Parse(splitInput[1]);
                    }
                }


                if (sidesOnDice == 1)
                {
                    output = "Why would you even try to roll a d1?!";
                }
                else if (sidesOnDice == 0)
                {
                    output = "Wow... good try... a 0 sided dice";
                }
                else if (sidesOnDice < 0)
                {
                    output = "Negative sides! Now you're just being silly";
                }
                else
                {
                    rollResult = RollDice(numberOfDice, sidesOnDice);
                    output = string.Format("You rolled {0}d{1} and got {2}", numberOfDice, sidesOnDice, rollResult);
                }
                
                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        [Command("RollToBeat")]
        [Summary("Rolls to match or beat a specified result using a specified number of dice, with a specified number of sides. Time to Die Hard with a Vengence.")]
        public async Task rollToBeat(int targetRoll, int numberOfTries, string diceModifier = "")
        {
            int numberOfDice;
            int sidesOnDice;
            string[] splitInput;
            int curRollResult;
            string output;
            int passCounter;

            try
            {
                numberOfDice = 1;
                sidesOnDice = 6;
                curRollResult = 0;
                passCounter = 0;
                output = "";
                // check if the roll has been modified with an input
                if (diceModifier.Contains("d"))
                {
                    splitInput = diceModifier.Split('d');
                    if (splitInput.Length == 2)
                    {
                        numberOfDice = int.Parse(splitInput[0]);
                        sidesOnDice = int.Parse(splitInput[1]);
                    }
                }

                // do the rolls
                for (int attemptNumber = 1; attemptNumber <= numberOfTries; attemptNumber++)
                {
                    curRollResult = RollDice(numberOfDice, sidesOnDice);

                    if (curRollResult >= targetRoll)
                    {
                        passCounter++;
                    }
                }

                output += string.Format("{0} attempts to roll a {1}+ with {2}d{3}, passed {4} rolls", numberOfTries, targetRoll, numberOfDice, sidesOnDice, passCounter);

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [Command("Rolls")]
        [Summary("Shows a summary of rolls from a specified number of dice, with a specified number of sides.")]
        public async Task rolls(int numberOfDice = 1, int sidesOnDice = 6)
        {
            int[] rollCount = new int[(sidesOnDice + 1)];
            int curRollResult;
            string output;

            try
            {
                curRollResult = 0;
                output = "[Roll] - [Count] \n";
                // Initialise array values
                rollCount[0] = 0;
                for (int i = 1; i <= sidesOnDice; i++)
                {
                    rollCount[i] = 0;
                }

                // do the rolls
                for (int attemptNumber = 1; attemptNumber <= numberOfDice; attemptNumber++)
                {
                    curRollResult = RollDice(1, sidesOnDice);

                    rollCount[curRollResult] += 1;
                }


                for (int j = 1; j <= sidesOnDice; j++)
                {
                    if (rollCount[j] > 0)
                    {
                        output += string.Format("{0} - {1} \n", j, rollCount[j]);
                    }
                }

                await ReplyAsync(output);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private int RollDice(int numberOfDice = 1, int sidesOnDice = 6)
        {
            int totalRoll;

            totalRoll = 0;
            
            if (sidesOnDice <= 1)
            {
                return 0;
            }

            // do the roll
            for (int rollNumber = 1; rollNumber <= numberOfDice; rollNumber++)
            {
                // Add 1 to the sides on dice because upper bound is EXCLUSIVE
                totalRoll += rng.Next(1, (sidesOnDice+1));
            }
            
            return totalRoll;
        }
    };
};

