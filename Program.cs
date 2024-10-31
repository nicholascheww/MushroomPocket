using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.Formats.Asn1;
using System.Linq;
using System.Xml;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MushroomPocket
{
    class Program : IDesignTimeDbContextFactory<MushroomPocketContext>
    {
        public MushroomPocketContext CreateDbContext(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot configuration = configurationBuilder.Build();
            string connectionString = configuration.GetConnectionString("MushroomPocketDb");
            DbContextOptionsBuilder<MushroomPocketContext> optionsBuilder = new DbContextOptionsBuilder<MushroomPocketContext>()
                .UseSqlite(connectionString);
            return new MushroomPocketContext(optionsBuilder.Options);
        }

        static int GetLatestId(MushroomPocketContext context)
        {
            var characters = context.Characters.ToList();
            if (characters.Count == 0)
            {
                return 0;
            }
            else
            {
                return characters.Max(c => c.Id);
            }
        }

        static void ListCharacters(MushroomPocketContext context)
        {
            var characters = context.Characters.ToList();

            if (characters.Count == 0)
            {
                Console.WriteLine("No character in the pocket.");
            }
            else
            {
                foreach (var character in characters)
                {
                    Console.WriteLine("-------------------------\nID: " + character.Id + "\nCharacter Name: " + character.Name + "\nHP: " + character.hp + "\nEXP: " + character.exp + "\nSkill: " + character.skill + "\nAttack: " + character.atk + "\n-------------------------");
                }
            }
        }

        static void AddCharacter(MushroomPocketContext context, int Id)
        {
            Console.WriteLine("Enter Character's Name: ");
            string charName = Console.ReadLine();
            if (charName != "Daisy" && charName != "Wario" && charName != "Waluigi")
            {
                Console.WriteLine("Invalid Character Name. Please enter Daisy, Wario, or Waluigi");
                return;
            }

            int charHP;
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter Character's HP: ");
                    charHP = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("Invalid HP format. Please enter a number");
                }
            }

            int charEXP;
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter Character's EXP: ");
                    charEXP = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch
                {
                    Console.WriteLine("Invalid EXP format. Please enter a number");
                }
            }
            Id++;
            Character newCharacter = null;
            if (charName == "Daisy")
            {
                newCharacter = new Daisy(Id, charName, charHP, charEXP, "Magic Abilities");
                Console.WriteLine("Daisy has been added.");
            }
            else if (charName == "Wario")
            {
                newCharacter = new Wario(Id, charName, charHP, charEXP, "Strength");
                Console.WriteLine("Wario has been added.");
            }
            else if (charName == "Waluigi")
            {
                newCharacter = new Waluigi(Id, charName, charHP, charEXP, "Agility");
                Console.WriteLine("Waluigi has been added.");
            }

            context.Characters.Add(newCharacter);
            context.SaveChanges();
        }
        static void CheckEvolve(MushroomPocketContext context, List<MushroomMaster> mushroomMasters)
        {
            var characters = context.Characters.ToList();
            if (characters.Count == 0)
            {
                Console.WriteLine("No character in the pocket.");
            }
            else
            {
                List<string> transformedCharacters = new List<string>();
                foreach (MushroomMaster mushroomMaster in mushroomMasters)
                {
                    int count = CountCharactersWithSameName(characters, mushroomMaster.Name);
                    if (count >= mushroomMaster.NoToTransform)
                    {
                        if (!transformedCharacters.Contains(mushroomMaster.Name))
                        {
                            Console.WriteLine($"{mushroomMaster.Name} --> {mushroomMaster.TransformTo}");
                            transformedCharacters.Add(mushroomMaster.Name);
                        }
                    }
                }
                if (transformedCharacters.Count == 0)
                {
                    Console.WriteLine("No character can be transformed.");
                }
            }
            
        }
        static void TransformCharacter(MushroomPocketContext context, List<MushroomMaster> mushroomMasters, int Id)
        {
            var characters = context.Characters.ToList();

            if (characters.Count() == 0)
            {
                Console.WriteLine("No character in the pocket.");
            }
            else
            {
                int initialCount = characters.Count();
                List<Character> charactersToRemove = new List<Character>();
                foreach (Character character in characters.ToList())
                {
                    bool transformed = false;
                    int count = CountCharactersWithSameName(characters, character.Name);
                    foreach (MushroomMaster mushroomMaster in mushroomMasters)
                    {
                        if (character.Name == mushroomMaster.Name && count >= mushroomMaster.NoToTransform)
                        {
                            int counter = 0;
                            foreach (Character c in characters.ToList())
                            {
                                if (c.Name == character.Name)
                                {
                                    charactersToRemove.Add(c);
                                    counter++;
                                    if (counter == mushroomMaster.NoToTransform)
                                    {
                                        transformed = true;
                                        break;
                                    }
                                }
                            }
                            Id++;
                            if (transformed)
                            {
                                if (mushroomMaster.TransformTo == "Peach")
                                {
                                    context.Characters.Add(new Peach(Id, "Peach", 100, 0, "Magic Abilities"));
                                    Console.WriteLine("Daisy has been transformed to Peach.");
                                }
                                else if (mushroomMaster.TransformTo == "Mario")
                                {
                                    context.Characters.Add(new Mario(Id, "Mario", 100, 0, "Strength"));
                                    Console.WriteLine("Wario has been transformed to Mario.");
                                }
                                else if (mushroomMaster.TransformTo == "Luigi")
                                {
                                    context.Characters.Add(new Luigi(Id, "Luigi", 100, 0, "Agility"));
                                    Console.WriteLine("Waluigi has been transformed to Luigi.");
                                }
                                characters = characters.Except(charactersToRemove).ToList();
                                break;
                            }
                        }

                    }
                }
                foreach (Character c in charactersToRemove)
                {
                    context.Characters.Remove(c);
                }
                context.SaveChanges();

                if (characters.Count() == initialCount)
                {
                    Console.WriteLine("No character can be transformed.");
                }
            }
        }
        static void RemoveCharacter(MushroomPocketContext context, ref bool isMega)
        {

            var characters = context.Characters.ToList();

            if (characters.Count == 0)
            {
                Console.WriteLine("No character in the pocket.");
            }
            else
            {
                for (int i = 0; i < characters.Count; i++)
                {
                    Console.WriteLine("Character Name: " + characters[i].Name);
                    Console.WriteLine("HP: " + characters[i].hp);
                    Console.WriteLine("EXP: " + characters[i].exp);
                    Console.WriteLine("Skill: " + characters[i].skill);
                    Console.WriteLine("Attack: " + characters[i].atk);
                    Console.WriteLine("Would you like to delete this character? (Y/N)");
                    string userAnswer = Console.ReadLine();
                    if (userAnswer.ToUpper() == "Y")
                    {
                        if(characters[i].Name.Contains("Mega "))
                        {
                            isMega = false;
                        }
                        context.Characters.Remove(characters[i]);
                        Console.WriteLine("Character has been removed.");
                        context.SaveChanges();
                        break;
                    }
                    else if (userAnswer.ToUpper() == "N")
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Answer. Please enter Y or N.");
                        break;
                    }

                }
            }
        }

        static void TakeSecretMission(MushroomPocketContext context, ref bool Megaowned, ref bool isMega, ref bool Zowned, ref bool Dynaowned)
        {
            Console.WriteLine("Welcome to the secret mission.\nIf you successfully complete the mission, you will be rewarded with a special reward.");
            Console.WriteLine("Depending on the number you choose you will be given a different mission.");
            Console.WriteLine("Please choose a number between 1-3.");
            string userAnswer = Console.ReadLine();
            if (int.TryParse(userAnswer, out int answer))
            {
                if (answer == 1 && Megaowned == false)
                {
                    MegaMisson(context, ref Megaowned, ref isMega);
                }
                else if (answer == 2 && Zowned == false)
                {
                    ZMisson(context, ref Zowned);
                }
                else if (answer == 3 && Dynaowned == false)
                {
                    DynaMisson(context, ref Dynaowned);
                }

                else if (Megaowned == true || Zowned == true || Dynaowned == true)
                {
                    Console.WriteLine("You have already completed the missions.");
                }

                else
                {
                    Console.WriteLine("Invalid Answer. Please enter a number between 1-3.");
                }
            }
            else
            {
                Console.WriteLine("Invalid Answer type. Please enter a number.");
            }
        }
        static void MegaMisson(MushroomPocketContext context, ref bool Megaowned, ref bool isMega)
        {

            Console.WriteLine("This mission will be a math quiz answer all 5 questions correct and you will win a special reward.");
            int correctCounter = 0;
            Random random = new Random();
            for (int i = 0; i < 5; i++)
            {
                int num1 = random.Next(1, 10);
                int num2 = random.Next(1, 10);
                Console.WriteLine("Question " + (i + 1) + ": " + num1 + " + " + num2 + " = ?");
                string userAnswer = Console.ReadLine();
                if (int.TryParse(userAnswer, out int answer))
                {
                    if (answer == num1 + num2)
                    {
                        correctCounter++;
                        Console.WriteLine("Correct Answer. You have answered " + correctCounter + " question(s) correctly.");
                    }
                    else
                    {
                        Console.WriteLine("Incorrect Answer. You have answered " + correctCounter + " question(s) correctly.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Answer type. Please enter a number.");
                }
            }

            if (correctCounter == 5)
            {
                Console.WriteLine("Congratulations! You have successfully completed the mission. You have been rewarded with a special reward.");
                Console.WriteLine("You have obtained a Mega special mushroom stone.");
                Console.WriteLine("You can now mega evolve your character(s) that have been fully evolved.");
                Megaowned = true;
            }
            else
            {
                Console.WriteLine("You have failed the mission. Please try again.");
            }
        }

        static void ZMisson(MushroomPocketContext context, ref bool Zowned)
        {
            Console.WriteLine("This misson will need you to win a game of rock paper scissors against the computer.");
            Console.WriteLine("You will need to win 2 out of 3 games in order to get the reward.");
            int winCounter = 0;
            Random random = new Random();
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine("Please choose Rock, Paper, or Scissors.");
                string userAnswer = Console.ReadLine();
                if (userAnswer.ToUpper() != "ROCK" && userAnswer.ToUpper() != "PAPER" && userAnswer.ToUpper() != "SCISSORS")
                {
                    Console.WriteLine("Invalid Answer. Please enter Rock, Paper, or Scissors.");
                    i -= 1;
                    break;
                }
                string[] choices = { "Rock", "Paper", "Scissors" };
                string computerChoice = choices[random.Next(0, 3)];
                Console.WriteLine("Computer chose: " + computerChoice);
                if (userAnswer.ToUpper() == "ROCK" && computerChoice == "Scissors")
                {
                    winCounter++;
                    Console.WriteLine("You win!");
                }
                else if (userAnswer.ToUpper() == "PAPER" && computerChoice == "Rock")
                {
                    winCounter++;
                    Console.WriteLine("You win!");
                }
                else if (userAnswer.ToUpper() == "SCISSORS" && computerChoice == "Paper")
                {
                    winCounter++;
                    Console.WriteLine("You win!");
                }
                else if (userAnswer.ToUpper() == computerChoice.ToUpper())
                {
                    Console.WriteLine("It's a tie!");
                }
                else
                {
                    Console.WriteLine("You lose!");
                }

                if (winCounter == 2)
                {
                    Console.WriteLine("Congratulations! You have successfully completed the mission. You have been rewarded with a special reward.");
                    Console.WriteLine("You have obtained a special Z mushroom stone.");
                    Console.WriteLine("You can now use a special move on your character(s).");
                    Zowned = true;
                    break;
                }
            }
            if (winCounter < 2)
            {
                Console.WriteLine("You have failed the mission. Please try again.");
            }
        }

        static void DynaMisson(MushroomPocketContext context, ref bool Dynaowned)
        {
            Console.WriteLine("This mission will need you guess a random number between 1-100 you will have 5 tries to get the correct number. If you do you will obtain a special reward.");
            Random random = new Random();
            int correctNumber = random.Next(1, 100);
            int tryCounter = 0;
            while (tryCounter < 5)
            {
                tryCounter++;
                Console.WriteLine($"Please enter a number between 1-100. (Try {tryCounter} of 5)");
                string userAnswer = Console.ReadLine();
                if (int.TryParse(userAnswer, out int answer))
                {
                    if (answer == correctNumber)
                    {
                        Console.WriteLine("Congratulations! You have successfully completed the mission. You have been rewarded with a special reward.");
                        Console.WriteLine("You have obtained a special Dynamax mushroom stone.");
                        Console.WriteLine("You can now Dynamax your character(s).");
                        Dynaowned = true;
                        break;
                    }
                    else if (answer < correctNumber)
                    {
                        Console.WriteLine("Incorrect Answer. The correct number is higher.");

                    }
                    else if (answer > correctNumber)
                    {
                        Console.WriteLine("Incorrect Answer. The correct number is lower.");

                    }
                    else
                    {
                        Console.WriteLine("Incorrect Answer. Please try again.");

                    }
                }
                else
                {
                    Console.WriteLine("Invalid Answer type. Please enter a number.");
                }
            }
            if (tryCounter == 5 && Dynaowned == false)
            {
                Console.WriteLine("You have failed the mission. Please try again.");
                Console.WriteLine("The correct number was: " + correctNumber);
            }
        }

        static void TransformUsingMushroomStone(MushroomPocketContext context, ref bool isMega, ref bool Megaowned, ref bool Zowned, ref bool Dynaowned)
        {
            var characters = context.Characters.ToList();

            if (characters.Count == 0)
            {
                Console.WriteLine("No character in the pocket.");
                return;
            }
            if (Megaowned == false && Zowned == false && Dynaowned == false)
            {
                Console.WriteLine("You do not have any special mushroom stones.");
                return;
            }

            if(isMega == true)
            {
                Console.WriteLine("You have already mega evolved your character(s). You can only mega evolve your character(s) once at a time.");
                return;
            }

            Console.WriteLine("After closing the Mushroom Pocket all characters will lose the special skills that they have gained from the special mushroom stones.");

            if (Megaowned == true && isMega == false)
            {
                Console.WriteLine("You have a special Mega mushroom stone. When used on a fully evolved character, it will mega evolve the character and grant it extra HP and attack points this stone can only be used once at a time.");
                Console.WriteLine("Would you like to use it to mega evolve your character(s)? (Y/N)");
                string userAnswer = Console.ReadLine();

                if (userAnswer.ToUpper() != "Y" && userAnswer.ToUpper() != "N")
                {
                    Console.WriteLine("Invalid Answer. Please enter Y or N.");
                    return;
                }

                if (userAnswer.ToUpper() == "N")
                {
                }

                foreach (var character in characters)
                {
                    if (character.Name == "Peach" || character.Name == "Mario" || character.Name == "Luigi")
                    {
                        Console.WriteLine("Character Name: " + character.Name);
                        Console.WriteLine("HP: " + character.hp);
                        Console.WriteLine("EXP: " + character.exp);
                        Console.WriteLine("Skill: " + character.skill);
                        Console.WriteLine("Would you like to mega evolve this character? (Y/N)");
                        string userAnswer2 = Console.ReadLine();
                        if (userAnswer2.ToUpper() == "Y")
                        {
                            character.Name = "Mega " + character.Name;
                            character.hp += 150;
                            character.skill = character.skill;
                            character.atk += 50;
                            isMega = true;
                            Console.WriteLine("Character has been mega evolved.");
                            break;
                        }
                        else if (userAnswer2.ToUpper() == "N")
                        {
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Invalid Answer. Please enter Y or N.");
                            return;
                        }
                    }
                }
            }

            if (Zowned == true)
            {
                Console.WriteLine("You have a Z special mushroom stone. When used on a character, it will grant the character a special move and extra attack points.");
                Console.WriteLine("Would you like to use it to give a special move to your character(s)? (Y/N)");
                string userAnswer = Console.ReadLine();
                if (userAnswer.ToUpper() == "N")
                {
                }

                foreach (var character in characters)
                {
                    Console.WriteLine("Character Name: " + character.Name);
                    Console.WriteLine("HP: " + character.hp);
                    Console.WriteLine("EXP: " + character.exp);
                    Console.WriteLine("Skill: " + character.skill);
                    Console.WriteLine("Would you like to give a special move to this character? (Y/N)");
                    string userAnswer2 = Console.ReadLine();
                    if (userAnswer2.ToUpper() == "Y")
                    {
                        if (character.Name == "Daisy" || character.Name == "Peach")
                        {
                            character.skill += ", Z Magic Abilities";
                            character.atk += 20;
                            Console.WriteLine("Character has been given a special move.");
                        }
                        else if (character.Name == "Wario" || character.Name == "Mario")
                        {
                            character.skill += ", Z Strength";
                            character.atk += 20;
                            Console.WriteLine("Character has been given a special move.");
                        }
                        else if (character.Name == "Waluigi" || character.Name == "Luigi")
                        {
                            character.skill += ", Z Agility";
                            character.atk += 20;
                            Console.WriteLine("Character has been given a special move.");
                        }
                        break;
                    }
                    else if (userAnswer2.ToUpper() == "N")
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Answer. Please enter Y or N.");
                        break;
                    }
                }
            }

            if (Dynaowned == true)
            {
                Console.WriteLine("You have a Dynamax special mushroom stone. When used on a character, it will Dynamax the character and grant it extra HP.");
                Console.WriteLine("Would you like to use it to Dynamax your character(s)? (Y/N)");
                string userAnswer = Console.ReadLine();
                if (userAnswer.ToUpper() == "N")
                {
                    return;
                }

                foreach (var character in characters)
                {
                    Console.WriteLine("Character Name: " + character.Name);
                    Console.WriteLine("HP: " + character.hp);
                    Console.WriteLine("EXP: " + character.exp);
                    Console.WriteLine("Skill: " + character.skill);
                    Console.WriteLine("Attack: " + character.atk);
                    Console.WriteLine("Would you like to Dynamax this character? (Y/N)");
                    string userAnswer2 = Console.ReadLine();
                    if (userAnswer2.ToUpper() == "Y")
                    {
                        character.Name = "Dynamax " + character.Name;
                        character.hp += 200;
                        Console.WriteLine("Character has been Dynamaxed.");
                    }
                    else if (userAnswer2.ToUpper() == "N")
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Answer. Please enter Y or N");
                    }
                    break;
                }
            }
            context.SaveChanges();
        }
        static int CountCharactersWithSameName(List<Character> characters, string name)
        {
            int count = 0;
            foreach (Character character in characters)
            {
                if (character.Name == name)
                {
                    count++;
                }
            }
            return count;
        }

        static void reset(MushroomPocketContext context)
        {
            var characters = context.Characters.ToList();
            foreach (var character in characters)
            {
                if (character.Name.Contains("Mega "))
                {
                    character.Name = character.Name.Replace("Mega ", "");
                    character.hp -= 150;
                    character.atk -= 50;
                }

                if (character.Name.Contains("Dynamax "))
                {
                    character.Name = character.Name.Replace("Dynamax ", "");
                    character.hp -= 200;
                }
                if (character.skill.Contains("Z Magic Abilities") || character.skill.Contains("Z Strength") || character.skill.Contains("Z Agility"))
                {
                    character.skill = character.skill.Replace(", Z Magic Abilities", "");
                    character.skill = character.skill.Replace(", Z Strength", "");
                    character.skill = character.skill.Replace(", Z Agility", "");
                    character.atk -= 20;
                }
            }
            context.SaveChanges();
        }

        static Character selectRandomCharacter(MushroomPocketContext context)
        {
            var characters = context.Characters.ToList();
            Random random = new Random();
            int index = random.Next(0, characters.Count);
            return characters[index];
        }

        static void Battle(MushroomPocketContext context)
        {
            if(context.Characters.Count() == 0)
            {
                Console.WriteLine("No character in the pocket.");
                return;
            }
            if(context.Characters.Count() == 1)
            {
                Console.WriteLine("You need at least 2 characters to battle.");
                return;
            }
            Console.WriteLine("Choose a character for the battle:");
            var playerCharacters = context.Characters.ToList();
            for (int i = 0; i < playerCharacters.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {playerCharacters[i].Name}"
                    + $" (HP: {playerCharacters[i].hp}, EXP: {playerCharacters[i].exp}, Skill: {playerCharacters[i].skill}, Attack: {playerCharacters[i].atk})");
            }
            string input = Console.ReadLine();
            int playerChoice;
            if(int.TryParse(input, out playerChoice))
            {
                playerChoice -= 1;
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a number");
                return;
            }
            if (playerChoice < 0 || playerChoice >= playerCharacters.Count)
            {
                Console.WriteLine("Invalid input. Please enter a number between 1-" + playerCharacters.Count);
                return;
            }

            Character playerCharacter = playerCharacters[playerChoice];
            Character computerCharacter = selectRandomCharacter(context);
            Console.WriteLine(playerCharacter.hp);
            int initialPlayerHP = playerCharacter.hp;
            
            int initialComputerHP = computerCharacter.hp;

            Console.WriteLine("Player: " + playerCharacter.Name + " vs Computer: " + computerCharacter.Name);

            while(playerCharacter.hp > 0 && computerCharacter.hp > 0)
            {
                Console.WriteLine("Player's Turn");
                Console.WriteLine("Choose an attack: 1. Normal Attack 2. Special Attack");
                int playerAttack = Convert.ToInt32(Console.ReadLine());
                if (playerAttack == 1)
                {
                    computerCharacter.hp -= playerCharacter.atk;
                    Console.WriteLine("Player used Normal Attack. Computer's HP: " + computerCharacter.hp);
                }
                else if (playerAttack == 2)
                {
                    if (playerCharacter.skill == "Magic Abilities")
                    {
                        computerCharacter.hp -= 30;
                        Console.WriteLine("Player used Magic Ability. Computer's HP: " + computerCharacter.hp);
                    }
                    else if (playerCharacter.skill == "Strength")
                    {
                        computerCharacter.hp -= 40;
                        Console.WriteLine("Player used Strength. Computer's HP: " + computerCharacter.hp);
                    }
                    else if (playerCharacter.skill == "Agility")
                    {
                        computerCharacter.hp -= 20;
                        Console.WriteLine("Player used Agility. Computer's HP: " + computerCharacter.hp);
                    }
                    else if (playerCharacter.skill == "Magic Abilities, Z Magic Abilities")
                    {
                        computerCharacter.hp -= 50;
                        Console.WriteLine("Player used Z Magic Ability. Computer's HP: " + computerCharacter.hp);
                    }
                    else if (playerCharacter.skill == "Strength, Z Strength")
                    {
                        computerCharacter.hp -= 60;
                        Console.WriteLine("Player used Z strength. Computer's HP: " + computerCharacter.hp);
                    }
                    else if (playerCharacter.skill == "Agility, Z Agility")
                    {
                        computerCharacter.hp -= 40;
                        Console.WriteLine("Player used Z Agility. Computer's HP: " + computerCharacter.hp);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
                }

                Console.WriteLine("Computer's Turn");
                int computerAttack = new Random().Next(1, 3);
                if (computerAttack == 1){
                    playerCharacter.hp -= computerCharacter.atk;
                    Console.WriteLine("Computer used Normal Attack. Player's HP: " + playerCharacter.hp);
                }
                else if (computerAttack == 2)
                {
                    if (computerCharacter.skill == "Magic Abilities")
                    {
                        playerCharacter.hp -= 30;
                        Console.WriteLine("Computer used Magic Ability. Player's HP: " + playerCharacter.hp);
                    }
                    else if (computerCharacter.skill == "Strength")
                    {
                        playerCharacter.hp -= 40;
                        Console.WriteLine("Computer used Strength. Player's HP: " + playerCharacter.hp);
                    }
                    else if (computerCharacter.skill == "Agility")
                    {
                        playerCharacter.hp -= 20;
                        Console.WriteLine("Computer used Agility. Player's HP: " + playerCharacter.hp);
                    }
                    else if (computerCharacter.skill == "Magic Abilities, Z Magic Abilities")
                    {
                        playerCharacter.hp -= 50;
                        Console.WriteLine("Computer used Z Magic Ability. Player's HP: " + playerCharacter.hp);
                    }
                    else if (computerCharacter.skill == "Strength, Z Strength")
                    {
                        playerCharacter.hp -= 60;
                        Console.WriteLine("Computer used Z strength. Player's HP: " + playerCharacter.hp);
                    }
                    else if (computerCharacter.skill == "Agility, Z Agility")
                    {
                        playerCharacter.hp -= 40;
                        Console.WriteLine("Computer used Z Agility. Player's HP: " + playerCharacter.hp);
                    }
                }
            }
        if(playerCharacter.hp <= 0)
        {
            Console.WriteLine("Computer wins!");
            playerCharacter.hp = initialPlayerHP;
        }
        else if(computerCharacter.hp <= 0)
        {
            Console.WriteLine("Player wins!");
            computerCharacter.hp = initialComputerHP;
        }
        }

        static void Main(string[] args)
        {
            //MushroomMaster criteria list for checking character transformation availability.   
            /*************************************************************************
                PLEASE DO NOT CHANGE THE CODES FROM LINE 15-19
            *************************************************************************/
            List<MushroomMaster> mushroomMasters = new List<MushroomMaster>(){
                    new MushroomMaster("Daisy", 2, "Peach"),
                    new MushroomMaster("Wario", 3, "Mario"),
                    new MushroomMaster("Waluigi", 1, "Luigi")
                    };

            //Use "Environment.Exit(0);" if you want to implement an exit of the console program
            //Start your assignment 1 requirements below.
            //Character list to store the character's information
            Program p = new Program();
            using (MushroomPocketContext context = p.CreateDbContext(null))
            {
                context.Database.Migrate();

                List<Character> characters = context.Characters.ToList();
                bool isContinue = true;
                bool Megaowned = false;
                bool Zowned = false;
                bool Dynaowned = false;
                bool isMega = false;
                int Id = GetLatestId(context);
                while (isContinue == true)
                {
                    Console.WriteLine("********************************\nWelcome to Mushroom Pocket\n********************************\n(1). Add Mushroom's character to my pocket\n(2). List character(s) in my Pocket\n(3). Check if I can transform my characters\n(4). Transform character(s) \n(5). Remove a character from your pocket\n(6). Take on a secret mission \n(7). Use item \n(8). Battle");
                    Console.WriteLine("Please only enter [1,2,3,4,5,6,7,8] or Q to quit: ");
                    string choice = Console.ReadLine();

                    if (choice.ToUpper() == "Q")
                    {
                        reset(context);
                        Environment.Exit(0);
                    }
                    else if (Convert.ToInt32(choice) == 1)
                    {
                        AddCharacter(context, Id);
                    }
                    else if (Convert.ToInt32(choice) == 2)
                    {
                        ListCharacters(context);
                    }
                    else if (Convert.ToInt32(choice) == 3)
                    {
                        CheckEvolve(context, mushroomMasters);
                    }
                    else if (Convert.ToInt32(choice) == 4)
                    {
                        TransformCharacter(context, mushroomMasters, Id);
                    }
                    else if (Convert.ToInt32(choice) == 5)
                    {
                        RemoveCharacter(context, ref isMega);
                    }
                    else if (Convert.ToInt32(choice) == 6)
                    {
                        TakeSecretMission(context, ref Megaowned, ref isMega, ref Zowned, ref Dynaowned);
                    }
                    else if (Convert.ToInt32(choice) == 7)
                    {
                        TransformUsingMushroomStone(context, ref isMega, ref Megaowned, ref Zowned, ref Dynaowned);
                    }
                    else if (Convert.ToInt32(choice) == 8)
                    {
                        Battle(context);
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter a number between 1-8.");
                }
            }
        }
    }
}
}