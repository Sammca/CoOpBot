using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace CoOpBot.Modules.Roll
{
    public class RollModule : ModuleBase
    {
        Random rng;
        public RollModule()
        {
            // Need to initialise random number generator here]
            // If not, it keeps getting reinitialised with the same seed and always gives the same result
            rng = new Random();
        }

        [Command("roll")]
        [Summary("Rolls a specified number of dice, with a specified number of sides. Time to Die.")]
        public async Task roll(string diceModifier = "", bool outputEachRoll = false)
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
        public async Task rollToBeat(int targetRoll, int numberOfTries, string diceModifier = "", bool outputEachRoll = false)
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

                        if (outputEachRoll)
                        {
                            output += string.Format("Roll {0} result: {1} ", attemptNumber, curRollResult);
                            output += "- PASS \r\n";
                        }
                    }
                    else if(outputEachRoll)
                    {
                        output += string.Format("Roll {0} result: {1} ", attemptNumber, curRollResult);
                        output += "- FAIL \r\n";
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
                totalRoll += rng.Next(1, sidesOnDice);
            }
            
            return totalRoll;
        }
    }
}

