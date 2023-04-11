using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MathNet.Numerics.Random;
using Handelabra;
using Boomlagoon.JSON;
using System.Xml.XPath;
using System.Globalization;

namespace Handelabra.MyModConsole // this has to be this way to work around an EngineCommon issue, will be fixed soon.
{
    /*
    * In order to use the Console version of Sentinels, you must first register your assemblies:
    *    Find the MainClass.ModAssemblies field just below and replace the information with your namespace & type
    * 
    * You can play the game in one of two ways, either interactive (basically the normal game) or testing (where you have complete control over everything
    *  To toggle between the two, add the -i parameter to the project properties (in the Debug tab)
    *  To play a set game, fill in the ConfigureGameForTesting() function with whoever you want to play with
    *  
    *  Improvements to Console Application made by wrhyme29
    */

    // Loading a game in "friendly mode" throws an exception so we can get out of wherever we were.
    class LoadGameException : Exception
    {
        public string GameName { get; private set; }

        public LoadGameException(string gameName)
            : base()
        {
            this.GameName = gameName;
        }
    }

    public enum GameSpeed
    {
        VerySlow,
        Slow,
        Medium,
        Fast
    };

    public enum GameMode
    {
        Classic,
        Team,
        OblivAeon
    };

    class MainClass
    {
        public static Dictionary<string, Assembly> ModAssemblies = new Dictionary<string, Assembly>
        {
            { "Workshopping", typeof(Workshopping.MigrantCoder.MigrantCoderCharacterCardController).Assembly } // replace with your own namespace and type
        };

        // LoadAllModContentOfKind() tries to guess the right identifiers - use this to override ones that don't fit
        public static Dictionary<string, string> ModIdentifierOverrides = new Dictionary<string, string>
        {
            { "Cauldron.Phase", "Cauldron.PhaseVillain" }
        };

        public static string GameNameToLoad = null;

        private static Game ConfigureGameForTesting()
        {
            Game game = null;
            bool advanced = false;
            bool challenge = false;
            Dictionary<string, string> promos = new Dictionary<string, string>();

            // Set up a game how you want, stack decks, etc.
            game = new Game(new string[] { "BaronBlade", "Bunker", "Legacy", "TheWraith", "PikeIndustrialComplex" }, advanced, promos, isChallenge: challenge);

            return game;
        }

        private void ConfigurePostGameStartTesting()
        {
            // Do whatever you want after the game starts.
            RunCoroutine(GameController.SetActiveTurnTaker("Legacy"));
        }

        private static void SetHitPoints(Game game, string identifier, int amount)
        {
            if (game.FindTurnTaker(identifier) != null)
            {
                foreach (var cc in game.FindTurnTaker(identifier).CharacterCards)
                {
                    cc.SetHitPoints(amount);
                }
            }
            else
            {
                var cards = game.GetAllCards().Where(c => c.IsInPlayAndNotUnderCard && c.Identifier == identifier);
                foreach (var card in cards)
                {
                    card.SetHitPoints(amount);
                }
            }
        }

        private static void PutIntoPlay(Game game, string identifier, string[] cards)
        {
            if (game.FindTurnTaker(identifier) != null)
            {
                game.FindTurnTaker(identifier).PutIntoPlay(cards);
            }
            else
            {
                Console.WriteLine("WARNING: " + identifier + " not found in game.");
            }
        }

        private static void PutInHand(Game game, string identifier, string[] cards)
        {
            game.FindTurnTaker(identifier).ToHero().PutInHand(cards);
        }

        private static void PutInHand(Card card)
        {
            card.Owner.ToHero().PutInHand(card);
        }

        private static void PutInTrash(Game game, string identifier, string[] cardIdentifiers)
        {
            game.FindTurnTaker(identifier).PutInTrash(cardIdentifiers);
        }

        private static void StackDeck(Game game, string ttIdentifier, string[] cards, bool takeFromAnywhere = false)
        {
            game.FindTurnTaker(ttIdentifier).StackDeck(cards, false, takeFromAnywhere);
        }

        private static void MoveCard(Game game, string ttIdentifier, string cardIdentifier, Location location)
        {
            game.FindTurnTaker(ttIdentifier).GetCardsByIdentifier(cardIdentifier).Where(c => c.IsInDeck).FirstOrDefault().Location = location;
        }

        private static void MoveAllCardsToTrash(Game game, string identifier)
        {
            var tt = game.FindTurnTaker(identifier);
            tt.PutInTrash(tt.GetAllCards().Select(c => c.Identifier).ToArray());
        }

        private static Game ConfigureGameInteractively()
        {
            List<string> availableVillains = new List<string>(DeckDefinition.AvailableVillains);
            string villain = null;
            DeckDefinition villainDeck = null;
            List<string> availableVillainTeams = new List<string>(DeckDefinition.AvailableVillainTeamMembers);
            List<DeckDefinition> villainTeam = new List<DeckDefinition>();
            List<string> availableEnvironments = new List<string>(DeckDefinition.AvailableEnvironments);
            string environment = null;
            List<DeckDefinition> environments = new List<DeckDefinition>();
            List<string> heroes = new List<string>();
            List<string> availableHeroes = new List<string>(DeckDefinition.AvailableHeroes);

            Dictionary<string, DeckDefinition> modHeroData = LoadAllModContentOfKind(DeckDefinition.DeckKind.Hero);
            Dictionary<string, DeckDefinition> modVillainData = LoadAllModContentOfKind(DeckDefinition.DeckKind.Villain);
            Dictionary<string, DeckDefinition> modVillainTeamData = LoadAllModContentOfKind(DeckDefinition.DeckKind.VillainTeam);
            Dictionary<string, DeckDefinition> modEnvironmentData = LoadAllModContentOfKind(DeckDefinition.DeckKind.Environment);

            List<string> fullHeroList = new List<string>();
            List<string> fullVillainList = new List<string>();
            List<string> fullVillainTeamList = new List<string>();
            List<string> fullEnvironmentList = new List<string>();

            fullHeroList.AddRange(availableHeroes);
            fullHeroList.AddRange(modHeroData.Keys);

            fullVillainList.AddRange(availableVillains);
            fullVillainList.AddRange(modVillainData.Keys);

            fullVillainTeamList.AddRange(availableVillainTeams);
            fullVillainTeamList.AddRange(modVillainTeamData.Keys);

            fullEnvironmentList.AddRange(availableEnvironments);
            fullEnvironmentList.AddRange(modEnvironmentData.Keys);

            Dictionary<string, string> promoIdentifiers = new Dictionary<string, string>();

            RandomSource rng = new MersenneTwister();
            GameMode gameMode = GameMode.Classic;

            string input = GetInput("Do you want to try a random scenario? (y/n)");
            if (input != null && input.ToLower().StartsWith("y"))
            {
                bool decided = false;

                while (!decided)
                {
                    heroes.Clear();
                    villainTeam.Clear();
                    environments.Clear();
                    promoIdentifiers.Clear();
                    fullHeroList.Clear();
                    fullVillainList.Clear();
                    fullVillainTeamList.Clear();
                    fullEnvironmentList.Clear();

                    availableHeroes = new List<string>(DeckDefinition.AvailableHeroes);
                    availableVillainTeams = new List<string>(DeckDefinition.AvailableVillainTeamMembers);
                    availableEnvironments = new List<string>(DeckDefinition.AvailableEnvironments);

                    fullHeroList.AddRange(availableHeroes);
                    fullHeroList.AddRange(modHeroData.Keys);

                    fullVillainList.AddRange(availableVillains);
                    fullVillainList.AddRange(modVillainData.Keys);

                    fullVillainTeamList.AddRange(availableVillainTeams);
                    fullVillainTeamList.AddRange(modVillainTeamData.Keys);

                    fullEnvironmentList.AddRange(availableEnvironments);
                    fullEnvironmentList.AddRange(modEnvironmentData.Keys);

                    // Choose a villain or villain(s)
                    int modeRNG = rng.Next(0, 5);

                    if (modeRNG == 0)
                    {
                        gameMode = GameMode.Team;
                    }
                    if (modeRNG == 1)
                    {
                        gameMode = GameMode.OblivAeon;
                    }

                    if (gameMode == GameMode.Classic)
                    {
                        int villainIndex = rng.Next(fullVillainList.Count);
                        villain = fullVillainList.ElementAt(villainIndex);
                        IEnumerable<CardDefinition> villainPromoDefinitions = null;

                        if (availableVillains.Contains(villain))
                        {
                            villainDeck = DeckDefinitionCache.GetDeckDefinition(villain);
                            villainPromoDefinitions = villainDeck.PromoCardDefinitions.Where(d => DeckDefinition.AvailablePromos.Contains(d.PromoIdentifier));
                        } else
                        {
                            villainDeck = modVillainData[villain];
                            villainPromoDefinitions = villainDeck.PromoCardDefinitions;
                        }
                        var villainName = villainDeck.Name;

                        // If there is a promo, maybe choose it
                        int villainPromoIndex = rng.Next(1 + villainPromoDefinitions.Count());
                        if (villainPromoIndex > 0)
                        {
                            var promo = villainPromoDefinitions.ElementAt(villainPromoIndex - 1);
                            promoIdentifiers[villain] = promo.PromoIdentifier;
                            villainName = promo.PromoTitle;
                        }

                        Console.WriteLine(villainName + " threatens the Multiverse!");
                    }

                    int numVillains = 1;
                    if (gameMode == GameMode.Team)
                    {
                        numVillains = rng.Next(3, 6);
                        while (villainTeam.Count < numVillains)
                        {
                            int index = rng.Next(fullVillainTeamList.Count);
                            var identifier = fullVillainTeamList.ElementAt(index);
                            DeckDefinition definition = null;
                            if (availableVillainTeams.Contains(identifier))
                            {
                                definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            }
                            else
                            {
                                //mod team villain
                                definition = modVillainTeamData[identifier];
                            }
                            string name = definition.Name;
                            villainTeam.Add(definition);
                            fullVillainTeamList.Remove(identifier);
                        }
                        Console.WriteLine(villainTeam.Select(dd => dd.Name).ToCommaList(useWordAnd: true) + " threaten the Multiverse!");
                    }

                    if (gameMode == GameMode.OblivAeon)
                    {
                        villain = "OblivAeon";
                        villainDeck = DeckDefinitionCache.GetDeckDefinition(villain);
                        Console.WriteLine("OblivAeon threatens the Multiverse!");
                    }

                    if (gameMode != GameMode.OblivAeon)
                    {
                        // Choose an environment
                        int environmentIndex = rng.Next(fullEnvironmentList.Count);
                        environment = fullEnvironmentList.ElementAt(environmentIndex);
                        string envName = "";
                        if (availableEnvironments.Contains(environment))
                        {
                            envName = DeckDefinitionCache.GetDeckDefinition(environment).Name;
                        }
                        else
                        {
                            envName = modEnvironmentData[environment].Name;
                        }
                        Console.WriteLine(envName + " is the location of the conflict.");
                    }
                    else
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            int environmentIndex = rng.Next(fullEnvironmentList.Count);
                            string environmentIdentifier = fullEnvironmentList.ElementAt(environmentIndex);
                            DeckDefinition currentEnvironment = null;
                            if (availableEnvironments.Contains(environmentIdentifier))
                            {
                                currentEnvironment = DeckDefinitionCache.GetDeckDefinition(environmentIdentifier);
                            }
                            else
                            {
                                currentEnvironment = modEnvironmentData[environmentIdentifier];
                            }
                            environments.Add(currentEnvironment);
                            fullEnvironmentList.RemoveAt(environmentIndex - 1);
                            Console.WriteLine(currentEnvironment.Name + " is one of the locations threatened by OblivAeon.");
                        }
                    }

                    // Choose heroes
                    int numHeroes = rng.Next(3, 6);
                    if (gameMode == GameMode.Team)
                    {
                        numHeroes = numVillains;
                    }
                    while (heroes.Count < numHeroes)
                    {
                        int index = rng.Next(fullHeroList.Count);
                        var identifier = fullHeroList.ElementAt(index);
                        DeckDefinition definition = null;
                        IEnumerable<CardDefinition> promoDefinitions = null;
                        if (availableHeroes.Contains(identifier))
                        {
                            definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            promoDefinitions = definition.PromoCardDefinitions.Where(d => DeckDefinition.AvailablePromos.Contains(d.PromoIdentifier));
                        }
                        else
                        {
                            definition = modHeroData[identifier];
                            promoDefinitions = definition.PromoCardDefinitions;

                        }
                        var name = definition.Name;

                        // If there is a promo, maybe choose it
                        if (promoDefinitions.Count() > 0)
                        {
                            int promoIndex = rng.Next(1 + promoDefinitions.Count());
                            if (promoIndex > 0)
                            {
                                var promo = promoDefinitions.ElementAt(promoIndex - 1);
                                promoIdentifiers[identifier] = promo.PromoIdentifier;
                                name = promo.PromoTitle;
                            }
                        }

                        Console.WriteLine(name + " joins the team!");
                        heroes.Add(identifier);
                        fullHeroList.Remove(identifier);
                    }

                    input = GetInput("Do you want to play this scenario? (y/n)");
                    if (input != null && input.ToLower().StartsWith("y"))
                    {
                        decided = true;
                    }
                }
            }
            else
            {
                // Choose a villain
                Console.WriteLine();
                if (fullVillainList.Count > 1)
                {
                    while (villain == null && villainTeam.Count < 3)
                    {
                        Console.WriteLine("Choose Villain:");

                        Console.WriteLine("0: Team Mode");

                        for (int i = 0; i < fullVillainList.Count; i++)
                        {
                            var identifier = fullVillainList.ElementAt(i);
                            DeckDefinition definition = null;
                            if (availableVillains.Contains(identifier))
                            {
                                definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            }
                            else
                            {
                                definition = modVillainData[identifier];
                            }

                            var name = definition.Name;
                            Console.WriteLine((i + 1) + ": " + name);

                        }

                        Console.WriteLine("TIMCOSING: OblivAeon");

                        input = GetInput("r: random choice");
                        int index = -1;
                        if (input != null)
                        {
                            if (input == "r")
                            {
                                index = rng.Next(fullVillainList.Count) + 1;
                            }
                            else
                            {
                                index = input.ToInt(-1);
                            }
                        }

                        if (index > 0 && index <= fullVillainList.Count)
                        {
                            villain = fullVillainList.ElementAt(index - 1);
                            IEnumerable<CardDefinition> promoDefinitions = new List<CardDefinition>();
                            if (availableVillains.Contains(villain))
                            {
                                villainDeck = DeckDefinitionCache.GetDeckDefinition(villain);
                                promoDefinitions = villainDeck.PromoCardDefinitions.Where(d => DeckDefinition.AvailablePromos.Contains(d.PromoIdentifier));
                            }
                            else
                            {
                                villainDeck = modVillainData[villain];
                                promoDefinitions = villainDeck.PromoCardDefinitions.ToList();
                            }

                            var name = villainDeck.Name;
                            if (promoDefinitions.Count() > 0)
                            {
                                // Ask which version
                                Console.WriteLine("Choose version:");
                                Console.WriteLine("1: " + villainDeck.Name);

                                for (int i = 0; i < promoDefinitions.Count(); i++)
                                {
                                    var promo = promoDefinitions.ElementAt(i);
                                    Console.WriteLine((i + 2) + ": " + promo.PromoTitle);
                                }

                                input = GetInput("r: random choice");
                                index = -1;
                                if (input != null)
                                {
                                    if (input == "r")
                                    {
                                        index = rng.Next(promoDefinitions.Count()) + 1;
                                    }
                                    else
                                    {
                                        index = input.ToInt(-1);
                                    }

                                    if (index >= 2)
                                    {
                                        var promo = promoDefinitions.ElementAtOrDefault(index - 2);
                                        if (promo != null)
                                        {
                                            promoIdentifiers[villain] = promo.PromoIdentifier;
                                            name = promo.PromoTitle;
                                        }
                                    }
                                }
                            }

                            Console.WriteLine(name + " threatens the Multiverse!");
                        }
                        else if (index == 0)
                        {
                            gameMode = GameMode.Team;

                            Console.WriteLine("\nChoose Villain Team Members:");

                            while (villainTeam.Count < 5 && fullVillainTeamList.Count > 0)
                            {
                                if (villainTeam.Count >= 3)
                                {
                                    Console.WriteLine("0: No more villains!");
                                }

                                for (int i = 0; i < fullVillainTeamList.Count; i++)
                                {
                                    var identifier = fullVillainTeamList.ElementAt(i);
                                    DeckDefinition definition = null;
                                    if (availableVillainTeams.Contains(identifier))
                                    {
                                        definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                                    }
                                    else
                                    {
                                        definition = modVillainTeamData[identifier];
                                    }
                                    var name = definition.Name;
                                    Console.WriteLine((i + 1) + ": " + name);
                                }

                                input = GetInput("r: random choice");
                                index = -1;
                                if (input != null)
                                {
                                    if (input == "r")
                                    {
                                        index = rng.Next(fullVillainTeamList.Count) + 1;
                                    }
                                    else
                                    {
                                        index = input.ToInt(-1);
                                    }
                                }

                                if (index > 0 && index <= fullVillainTeamList.Count)
                                {
                                    var id = fullVillainTeamList.ElementAt(index - 1);
                                    DeckDefinition definition = null;
                                    if (availableVillainTeams.Contains(id))
                                    {
                                        definition = DeckDefinitionCache.GetDeckDefinition(id);
                                    }
                                    else
                                    {
                                        definition = modVillainTeamData[id];
                                    }
                                    Console.WriteLine(definition.Name + " seeks vengeance!\n");
                                    villainTeam.Add(definition);
                                    fullVillainTeamList.Remove(id);
                                }
                                else if (index == 0)
                                {
                                    break;
                                }
                            }

                            Console.WriteLine(villainTeam.Select(dd => dd.Name).ToCommaList(useWordAnd: true) + " threaten the Multiverse!");
                        }
                        else if (input.ToLower() == "timcosing")
                        {
                            gameMode = GameMode.OblivAeon;
                            villain = "OblivAeon";
                            villainDeck = DeckDefinitionCache.GetDeckDefinition(villain);
                            Console.WriteLine("OblivAeon threatens the Multiverse!");
                        }
                    }
                }

                // Choose an environment
                Console.WriteLine();
                if (fullEnvironmentList.Count > 1 && gameMode != GameMode.OblivAeon)
                {
                    while (environment == null)
                    {
                        Console.WriteLine("Choose Environment:");

                        for (int i = 0; i < fullEnvironmentList.Count; i++)
                        {
                            var identifier = fullEnvironmentList.ElementAt(i);
                            DeckDefinition definition = null;
                            if (availableEnvironments.Contains(identifier))
                            {
                                definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            }
                            else
                            {
                                definition = modEnvironmentData[identifier];
                            }
                            var name = definition.Name;
                            Console.WriteLine((i + 1) + ": " + name);
                        }

                        input = GetInput("r: random choice");
                        int index = -1;
                        if (input != null)
                        {
                            if (input == "r")
                            {
                                index = rng.Next(fullEnvironmentList.Count) + 1;
                            }
                            else
                            {
                                index = input.ToInt(-1);
                            }
                        }

                        if (index > 0 && index <= fullEnvironmentList.Count)
                        {
                            environment = fullEnvironmentList.ElementAt(index - 1);
                            DeckDefinition definition = null;
                            if (availableEnvironments.Contains(environment))
                            {
                                definition = DeckDefinitionCache.GetDeckDefinition(environment);
                            }
                            else
                            {
                                definition = modEnvironmentData[environment];
                            }
                            var name = definition.Name;
                            Console.WriteLine(name + " is the location of the conflict.");
                        }
                    }
                }
                else if (gameMode == GameMode.OblivAeon)
                {
                    // We need to choose 5 environments for OblivAeon mode
                    while (environments.Count() < 5)
                    {
                        Console.WriteLine("Choose Environment:");

                        for (int i = 0; i < fullEnvironmentList.Count; i++)
                        {
                            var identifier = fullEnvironmentList.ElementAt(i);
                            DeckDefinition definition = null;
                            if (availableEnvironments.Contains(identifier))
                            {
                                definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            }
                            else
                            {
                                definition = modEnvironmentData[identifier];
                            }
                            var name = definition.Name;
                            Console.WriteLine((i + 1) + ": " + name);
                        }

                        input = GetInput("r: random choice");
                        int index = -1;
                        if (input != null)
                        {
                            if (input == "r")
                            {
                                index = rng.Next(fullEnvironmentList.Count) + 1;
                            }
                            else
                            {
                                index = input.ToInt(-1);
                            }
                        }

                        if (index > 0 && index <= fullEnvironmentList.Count)
                        {
                            var identifier = fullEnvironmentList.ElementAt(index - 1);
                            DeckDefinition currentEnvironment = null;
                            if (availableEnvironments.Contains(identifier))
                            {
                                currentEnvironment = DeckDefinitionCache.GetDeckDefinition(fullEnvironmentList.ElementAt(index - 1));
                            }
                            else
                            {
                                currentEnvironment = modEnvironmentData[identifier];
                            }
                            environments.Add(currentEnvironment);
                            fullEnvironmentList.RemoveAt(index - 1);
                            Console.WriteLine(currentEnvironment.Name + " is one of the locations threatened by OblivAeon.");
                        }
                    }
                }

                // Choose heroes
                while (heroes.Count < 5)
                {
                    Console.WriteLine();
                    Console.WriteLine("Choose Hero #" + (heroes.Count + 1) + ":");

                    if (heroes.Count >= 3 && gameMode != GameMode.Team)
                    {
                        Console.WriteLine("0: No more heroes!");
                    }

                    for (int i = 0; i < fullHeroList.Count; i++)
                    {
                        var identifier = fullHeroList.ElementAt(i);
                        DeckDefinition definition = null;
                        if (availableHeroes.Contains(identifier))
                        {
                            definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                        }
                        else
                        {
                            definition = modHeroData[identifier];
                        }
                        var name = definition.Name;
                        Console.WriteLine((i + 1) + ": " + name);
                    }

                    input = GetInput("r: random choice");
                    int index = -1;
                    if (input != null)
                    {
                        if (input == "r")
                        {
                            index = rng.Next(fullHeroList.Count) + 1;
                        }
                        else
                        {
                            index = input.ToInt(-1);
                        }
                    }

                    if (index == 0 && heroes.Count >= 3)
                    {
                        // done
                        break;
                    }
                    else if (index > 0 && index <= fullHeroList.Count)
                    {
                        var identifier = fullHeroList.ElementAt(index - 1);
                        DeckDefinition definition = null;
                        List<CardDefinition> promoDefinitions = null;
                        if (availableHeroes.Contains(identifier))
                        {
                            definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                            promoDefinitions = definition.PromoCardDefinitions.Where(d => d.ParentDeck.SupportsPromoSwapping || DeckDefinition.AvailablePromos.Contains(d.PromoIdentifier)).ToList();
                        }
                        else
                        {
                            definition = modHeroData[identifier];
                            promoDefinitions = definition.PromoCardDefinitions.ToList();
                        }
                        var name = definition.Name;

                        if (promoDefinitions.Count() > 0)
                        {
                            bool promoAgain = false;
                            do
                            {
                                // Ask which version
                                Console.WriteLine("Choose version:");
                                if (promoAgain)
                                {
                                    Console.WriteLine("0: Done swapping out");
                                }
                                if (availableHeroes.Contains(identifier))
                                {
                                    var note = "";
                                    if (definition.SupportsPromoSwapping)
                                    {
                                        note = " (choose all)";
                                    }

                                    Console.WriteLine("1: " + definition.Name + note);

                                    for (int i = 0; i < promoDefinitions.Count(); i++)
                                    {
                                        var promo = promoDefinitions.ElementAt(i);
                                        if (definition.SupportsPromoSwapping)
                                        {
                                            note = promo.IsRealCard ? " (swap individual)" : " (choose all)";
                                        }

                                        Console.WriteLine((i + 2) + ": " + (promo.PromoTitle ?? promo.Title) + note);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("1: " + definition.Name);

                                    for (int i = 0; i < promoDefinitions.Count(); i++)
                                    {
                                        var promo = promoDefinitions.ElementAt(i);
                                        Console.WriteLine((i + 2) + ": " + (promo.PromoTitle ?? promo.Title));
                                    }
                                }

                                input = GetInput("r: random choice");
                                index = -1;
                                if (input != null)
                                {
                                    if (input == "r")
                                    {
                                        index = rng.Next(promoDefinitions.Count() + 2);
                                    }
                                    else
                                    {
                                        index = input.ToInt(-1);
                                    }

                                    if (index == 0)
                                    {
                                        promoAgain = false;
                                        break;
                                    }

                                    if (index >= 2)
                                    {
                                        var promo = promoDefinitions.ElementAtOrDefault(index - 2);
                                        if (promo != null)
                                        {
                                            if (availableHeroes.Contains(identifier) && definition.SupportsPromoSwapping)
                                            {
                                                if (!promo.IsRealCard)
                                                {
                                                    // They chose Adamant Sentinels, use them all
                                                    foreach (var c in promoDefinitions.Where(x => x.PromoIdentifier != null))
                                                    {
                                                        promoIdentifiers[c.Identifier] = c.PromoIdentifier;
                                                    }

                                                    promoIdentifiers[identifier] = promo.PromoIdentifier;
                                                    name = promo.PromoTitle;
                                                }
                                                else
                                                {
                                                    CardDefinition before;

                                                    if (promo.PromoIdentifier == null)
                                                    {
                                                        // It's the original card, put it back
                                                        promoIdentifiers.Remove(promo.Identifier);

                                                        before = promo.ParentDeck.PromoCardDefinitions.FirstOrDefault(c => c.Identifier == promo.Identifier);
                                                    }
                                                    else
                                                    {
                                                        // It's a promo, put the original card in the promo list
                                                        promoIdentifiers[promo.Identifier] = promo.PromoIdentifier;

                                                        before = promo.ParentDeck.CardDefinitions.FirstOrDefault(c => c.Identifier == promo.Identifier);
                                                    }

                                                    promoDefinitions.Replace(promo, before);
                                                    promoAgain = true;

                                                    var beforeTitle = before.PromoTitle ?? before.Title;
                                                    var afterTitle = promo.PromoTitle ?? promo.Title;

                                                    Console.WriteLine(beforeTitle + " swaps out for " + afterTitle + "!");
                                                }
                                            }
                                            else
                                            {
                                                promoIdentifiers[identifier] = promo.PromoIdentifier;
                                                name = promo.PromoTitle;
                                            }
                                        }
                                    }
                                }
                            } while (promoAgain);
                        }

                        Console.WriteLine(name + " joins the team!");
                        heroes.Add(identifier);
                        fullHeroList.Remove(identifier);

                        if (gameMode == GameMode.Team && villainTeam.Count == heroes.Count)
                        {
                            Console.WriteLine("All heroes have been selected!");
                            break;
                        }
                    }

                }
            }

            // Choose incapacitated heroes
            List<string> incappedHeroes = new List<string>();
            List<string> normalHeroes = heroes.ToList();
            Console.WriteLine();
            input = GetInput("Would you like to start any heroes incapacitated, to test their abilities? (y/n)");
            if (input != null && input.ToLower().StartsWith("y"))
            {
                while (normalHeroes.Count > 1)
                {
                    Console.WriteLine("0: No more incapacited heroes!");
                    for (int i = 0; i < normalHeroes.Count; i++)
                    {
                        var identifier = normalHeroes.ElementAt(i);
                        var definition = DeckDefinitionCache.GetDeckDefinition(identifier);
                        var name = definition.Name;
                        Console.WriteLine((i + 1) + ": " + name);
                    }

                    input = GetInput("");
                    int index = -1;
                    if (input != null)
                    {
                        index = input.ToInt(-1);
                    }

                    if (index == 0)
                    {
                        // done
                        break;
                    }
                    else if (index > 0 && index <= normalHeroes.Count)
                    {
                        string identifier = normalHeroes[index - 1];
                        incappedHeroes.Add(identifier);
                        normalHeroes.Remove(identifier);
                    }
                }
            }

            // Choose advanced and challenge
            bool advanced = false;
            bool challenge = false;
            string shieldIdentifier = null;
            List<string> scionIdentifiers = null;

            Console.WriteLine();
            var identifiers = new List<string>();
            if (gameMode != GameMode.Team)
            {
                identifiers.Add(villain);
                identifiers.AddRange(heroes);
            }
            else
            {
                // Alternate between villains and heroes.
                for (int i = 0; i < villainTeam.Count; i++)
                {
                    identifiers.Add(villainTeam.ElementAt(i).Identifier);
                    identifiers.Add(heroes.ElementAt(i));
                }
            }

            if (gameMode != GameMode.OblivAeon)
            {
                identifiers.Add(environment);
            }
            else
            {
                identifiers.AddRange(environments.Select(dd => dd.Identifier));
            }

            var tempGame = new Game(identifiers, true, promoIdentifiers, isChallenge: true);
            var advancedInfo = "";
            var challengeInfo = "";

            if (gameMode != GameMode.Team)
            {
                var card = tempGame.FindTurnTaker(villain).CharacterCard;
                advancedInfo = GetAdvancedString(tempGame, card, true);
                challengeInfo = GetChallengeString(card, card.Definition.FlippedChallengeText != null && card.Definition.FlippedChallengeText.Count() > 0);
            }
            else
            {
                foreach (var villainT in villainTeam)
                {
                    var card = tempGame.FindTurnTaker(villainT.Identifier).CharacterCard;
                    advancedInfo += "--- " + villainT.Name + " ---\n";
                    advancedInfo += GetAdvancedString(tempGame, card, false);
                    advancedInfo += "\n\n";

                    challengeInfo += "--- " + villainT.Name + " ---\n";
                    challengeInfo += GetChallengeString(card, false);
                    challengeInfo += "\n\n";
                }
            }

            input = GetInput("Would you like to use Advanced difficulty mode? (y/n)\n\n" + advancedInfo + "\n");

            var teamAdvancedDeck = new List<DeckDefinition>();
            List<string> teamAdvanced = null;

            if (input != null && input.ToLower().StartsWith("y"))
            {
                if (tempGame.IsVillainTeamMode)
                {
                    teamAdvanced = new List<string>();
                    teamAdvancedDeck.AddRange(villainTeam);

                    bool done = false;
                    while (!done)
                    {
                        Console.WriteLine("Which villain team members would you like to use Advanced difficulty mode for?");

                        int i = 1;
                        Console.WriteLine("0: All of them! Bring it on!");
                        foreach (var villainous in teamAdvancedDeck)
                        {
                            Console.WriteLine((i++) + ": " + villainous.Name);
                        }
                        Console.WriteLine("w: No more! Too difficult!");

                        input = GetInput("");
                        var index = -1;

                        if (input != null)
                        {
                            index = input.ToInt(-1);

                            if (index == 0)
                            {
                                foreach (var villainous in teamAdvancedDeck)
                                {
                                    teamAdvanced.Add(villainous.Identifier);
                                }

                                done = true;
                            }
                            else if (index > 0 && index <= teamAdvancedDeck.Count())
                            {
                                var villainous = teamAdvancedDeck.ElementAt(index - 1);

                                teamAdvanced.Add(villainous.Identifier);
                                teamAdvancedDeck.Remove(villainous);
                            }
                            else if (index > teamAdvancedDeck.Count())
                            {
                                Console.WriteLine("That number is too high! Please try another one.");
                            }
                            else
                            {
                                done = true;
                            }
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Advanced mode is on. Good luck!");
                    advanced = true;
                }
            }
            else
            {
                Console.WriteLine("Advanced mode is off. There's no shame in that.");
            }

            Console.WriteLine();

            input = GetInput("Would you like to use Challenge difficulty mode? (y/n)\n\n" + challengeInfo + "\n");

            var teamChallengeDeck = new List<DeckDefinition>();
            List<string> teamChallenge = null;

            if (input != null && input.ToLower().StartsWith("y"))
            {
                if (tempGame.IsVillainTeamMode)
                {
                    teamChallenge = new List<string>();
                    teamChallengeDeck.AddRange(villainTeam);

                    bool done = false;
                    while (!done)
                    {
                        Console.WriteLine("Which villain team members would you like to use Challenge difficulty mode for?");

                        int i = 1;
                        Console.WriteLine("0: All of them! I am invincible!");
                        foreach (var villainous in teamChallengeDeck)
                        {
                            Console.WriteLine((i++) + ": " + villainous.Name);
                        }
                        Console.WriteLine("w: No more! Too extreme!");

                        input = GetInput("");
                        var index = -1;

                        if (input != null)
                        {
                            index = input.ToInt(-1);

                            if (index == 0)
                            {
                                foreach (var villainous in teamChallengeDeck)
                                {
                                    teamChallenge.Add(villainous.Identifier);
                                }

                                done = true;
                            }
                            else if (index > 0 && index <= teamChallengeDeck.Count())
                            {
                                var villainous = teamChallengeDeck.ElementAt(index - 1);

                                teamChallenge.Add(villainous.Identifier);
                                teamChallengeDeck.Remove(villainous);
                            }
                            else if (index > teamChallengeDeck.Count())
                            {
                                Console.WriteLine("That number is too high! Please try another one.");
                            }
                            else
                            {
                                done = true;
                            }
                        }
                        else
                        {
                            done = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Challenge mode is on. Good luck, you'll need it!");
                    challenge = true;
                }
            }
            else
            {
                Console.WriteLine("Challenge mode is off. There's definitely no shame in that.");
            }

            if (villain == "OblivAeon")
            {
                Console.WriteLine("Choose a Shield card for OblivAeon:");
                var shields = villainDeck.CardDefinitions.Where(cd => cd.Keywords.Contains("oblivaeon shield phase 1"));
                int i = 1;
                foreach (var shield in shields)
                {
                    Console.WriteLine((i++) + ": " + shield.Title);
                }

                input = GetInput("r: random choice");
                int index = -1;
                if (input != null)
                {
                    if (input == "r")
                    {
                        index = rng.Next(shields.Count()) + 1;
                    }
                    else
                    {
                        index = input.ToInt(-1);
                    }
                }

                if (index > 0 && index <= shields.Count())
                {
                    shieldIdentifier = shields.ElementAt(index - 1).Identifier;
                }

                var scions = villainDeck.SubDecks.Where(dd => dd.Identifier == "ScionDeck").FirstOrDefault().CardDefinitions.Where(cd => cd.Keywords.Contains("scion"));
                var unselectedScions = scions.ToList();
                for (int j = 0; j < scions.Count(); j++)
                {
                    Console.WriteLine("Choose the Scion order:");
                    i = 1;
                    foreach (var scion in unselectedScions)
                    {
                        Console.WriteLine((i++) + ": " + scion.Title);
                    }

                    input = GetInput("r: randomize remaining Scion order");
                    index = -1;
                    if (input != null && input != "r")
                    {
                        index = input.ToInt(-1);
                    }

                    if (index > 0 && index <= scions.Count())
                    {
                        if (scionIdentifiers == null)
                        {
                            scionIdentifiers = new List<string>();
                        }

                        var selectedScion = unselectedScions.ElementAt(index - 1);
                        scionIdentifiers.Add(selectedScion.Identifier);
                        unselectedScions.Remove(selectedScion);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Game speed options:");
            Console.WriteLine("Add the letter 'c' before the speed to stop for input at the start of every turn.");
            Console.WriteLine("f: Fast - pauses when a decision is needed" + (Speed == GameSpeed.Fast ? " (default)" : ""));
            Console.WriteLine("m: Medium - pauses every villain & environment phase" + (Speed == GameSpeed.Medium ? " (default)" : ""));
            Console.WriteLine("s: Slow - pauses every phase" + (Speed == GameSpeed.Slow ? " (default)" : ""));
            input = GetInput("v: Very Slow - pauses every action" + (Speed == GameSpeed.VerySlow ? " (default)" : ""));
            if (input != null)
            {
                if (input.StartsWith("c"))
                {
                    CheatStops = true;
                    input = input.Replace("c", "");
                }
                else
                {
                    CheatStops = false;
                }

                if (input.StartsWith("f"))
                {
                    Speed = GameSpeed.Fast;
                }
                if (input.StartsWith("m"))
                {
                    Speed = GameSpeed.Medium;
                }
                else if (input.StartsWith("s"))
                {
                    Speed = GameSpeed.Slow;
                }
                else if (input.StartsWith("v"))
                {
                    Speed = GameSpeed.VerySlow;
                }
            }

            // Save speed preference
            SaveSpeedAndCheatPrefs();

            var game = new Game(identifiers, advanced, promoIdentifiers, advancedIdentifiers: teamAdvanced, isChallenge: challenge, challengeIdentifiers: teamChallenge, shieldIdentifier: shieldIdentifier, scionIdentifiers: scionIdentifiers);

            foreach (var identifier in incappedHeroes)
            {
                // Start the hero out incapacitated.
                var tt = game.FindTurnTaker(identifier).ToHero();
                foreach (var cc in tt.CharacterCards)
                {
                    cc.SetFlipped(true);
                    cc.RemoveTarget();
                }

                // Make sure all relevant Sky-Scraper character cards are also incapped
                if (tt.CharacterCard.SharedIdentifier != null)
                {
                    var sharedCharacters = tt.GetCardsWhere(c => c.SharedIdentifier != null && c.SharedIdentifier == tt.CharacterCard.SharedIdentifier && c != tt.CharacterCard);
                    foreach (var shared in sharedCharacters)
                    {
                        shared.SetFlipped(true);
                        shared.RemoveTarget();
                    }
                }

                tt.MoveAllCards(tt.Hand, tt.OutOfGame);
                tt.MoveAllCards(tt.Deck, tt.OutOfGame);
                tt.UpdateTurnPhases(true);
            }

            return game;
        }

        public static void SaveSpeedAndCheatPrefs()
        {
            var data = LoadUserData();
            var speedValue = new JSONValue(Speed.ToString());
            data["speed"] = speedValue;
            var cheatValue = new JSONValue(CheatStops);
            data["cheatStops"] = cheatValue;
            SaveUserData(data);
        }

        public static void Main(string[] args)
        {
            LoadModAssemblies();

            bool interactiveMode = args.Contains("-i");
            if (interactiveMode)
            {
                Log.ToggleLogName(LogName.MegaComputer, true);
                Console.Clear();

                Console.WriteLine("Welcome to the Mega Computer battle simulation.");
                Console.WriteLine();
                Console.WriteLine("During the simulation, you can type 'help' to see a list of commands.");
                Console.WriteLine();
            }

            bool first = true;
            int sanity = 1000;

            while (true)
            {
                QuitRequested = false;
                MainClass main = new MainClass();

                Game game = null;
                bool continueGame = false;
                bool replayGame = false;

                if (interactiveMode)
                {
                    // Load previous speed value if saved
                    var data = LoadUserData();
                    if (data.ContainsKey("speed"))
                    {
                        Speed = (GameSpeed)Enum.Parse(typeof(GameSpeed), data["speed"].Str);
                    }
                    if (data.ContainsKey("cheatStops"))
                    {
                        CheatStops = data["cheatStops"].Boolean;
                    }

                    // Interactive game setup and play, rules will be on
                    if (first)
                    {
                        if (data.Count() > 0)
                        {
                            Console.WriteLine("STORED USER DATA:");
                            foreach (var kvp in data)
                            {
                                Console.WriteLine(kvp.Key + ": " + kvp.Value);
                            }
                            Console.WriteLine();
                        }

                        string input;

                        // Try seeing if there's a game to load
                        try
                        {
                            game = MainClass.LoadGameFile("continue");
                        }
                        catch
                        {
                            // No worries, just ditch it.
                            game = null;
                        }

                        if (game != null)
                        {
                            input = GetInput("Do you want to continue your previous session? (y/n/l/r)");
                            if (input != null && input.ToLower().StartsWith("y"))
                            {
                                continueGame = true;
                            }
                            else if (input != null && input.ToLower().StartsWith("l"))
                            {
                                GameNameToLoad = GetInput("Type the name of the saved game you wish to load...");
                            }
                            else if (input != null && input.ToLower().StartsWith("r"))
                            {
                                GameNameToLoad = GetInput("Type the name of the saved game you wish to replay...");
                                replayGame = true;
                            }
                            else
                            {
                                // clear it out
                                game = null;
                            }
                        }
                        else
                        {
                            input = GetInput("Do you want to load a checkpoint? (y/n)");
                            if (input != null && input.ToLower().StartsWith("y") || input.ToLower().StartsWith("l"))
                            {
                                GameNameToLoad = GetInput("Type the name of the saved game you wish to load...");
                            }
                            else
                            {
                                // clear it out
                                game = null;
                            }
                        }
                    }
                    else
                    {
                        // Just clear the console.
                        Console.Clear();
                    }

                    if (GameNameToLoad != null && GameNameToLoad.Trim() != "")
                    {
                        // The user asked to load a game, try to do that.
                        string name = GameNameToLoad;
                        GameNameToLoad = null;

                        try
                        {
                            game = MainClass.LoadGameFile("save-" + name);

                            if (game != null)
                            {
                                Console.WriteLine("Loaded checkpoint: " + name);
                                if (!replayGame)
                                {
                                    continueGame = true;
                                }
                            }
                            else
                            {
                                Console.WriteLine("Could not load checkpoint: " + name);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Sorry, we could not load the saved checkpoint: " + name + ". Please email john@handelabra.com. Thanks!" + Environment.NewLine + "Technical details: " + e);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No checkpoint name provided.");
                    }

                    if (game == null)
                    {
                        game = ConfigureGameInteractively();

                        Console.WriteLine();
                        Console.WriteLine(">>>>>> Sentinels, Assemble! <<<<<<");
                        Console.WriteLine();
                    }

                    main.EnforceRules = true;
                    main.UserFriendly = true;
                }
                else
                {
                    // Code setup, rules will be off initially
                    game = ConfigureGameForTesting();
                }

                if (!replayGame)
                {
                    main.SetupGameController(game);
                }
                else
                {
                    main.ReplayGameController(game);
                }

                bool gameOver = main.RunLoop(continueGame);

                main.TeardownGameController();

                first = false;

                sanity -= 1;
                if (sanity <= 0)
                {
                    Console.WriteLine("Infinite loop detected in Main()");
                    Environment.Exit(1);
                }

                if ((gameOver || QuitRequested) && interactiveMode)
                {
                    // Delete the continue.dat
                    try
                    {
                        File.Delete("continue.dat");
                    }
                    catch
                    {
                        // oh well
                    }

                    // See if they want to play again
                    var input = GetInput("Do you want to start a new simulation? (y/n)");
                    if (input == null || !input.ToLower().StartsWith("y"))
                    {
                        // We're done
                        Environment.Exit(0);
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (GameNameToLoad != null)
                {
                    // Go back up and load the game
                    continue;
                }
                else
                {
                    // We're done
                    break;
                }
            }
        }

        private static void LoadModAssemblies()
        {
            if (NamespaceList is null)
            {
                NamespaceList = new List<string>();
            }

            foreach (var kvp in ModAssemblies)
            {
                ModHelper.AddAssembly(kvp.Key, kvp.Value);
                NamespaceList.Add(kvp.Key);
            }
        }

        bool UserFriendly = false;
        bool Verbose = false;
        bool EnforceRules = false;
        static List<string> NamespaceList { get; set; }

        GameController GameController { get; set; }

        System.IO.StreamWriter FileOutput = null;
        string OutputFileDate;
        string OutputFileName;
        string DataFileName;
        static GameSpeed Speed = GameSpeed.Fast;
        static bool CheatStops = false;
        Card _lastCardPrinted = null;
        const string UserDataFileName = "userdata.json";

        public void SetupGameController(Game game)
        {
            this.GameController = new GameController(game);
            GameController.StartCoroutine = this.StartCoroutine;
            GameController.ExhaustCoroutine = this.RunCoroutine;

            GameController.OnWillPerformAction += this.WillPerformAction;
            GameController.OnWillApplyActionChanges += this.WillApplyActionChanges;
            GameController.OnDidPerformAction += this.DidPerformAction;

            GameController.OnMakeDecisions += this.MakeDecisions;
            GameController.OnSendMessage += this.ReceiveMessage;
            GameController.OnGetPersistentValueFromView += GetPersistentValueFromView;
            GameController.OnSetPersistentValueInView += SetPersistentValueInView;
            GameController.OnGetHeroCardsInBox += HandleGetHeroCardsInBoxRequest;

            // Save all output to a file as well.
            DateTime now = DateTime.Now;
            this.OutputFileDate = string.Format("{0}-{1}-{2}-{3}-{4}-{5}", now.Year.ToString("D4"), now.Month.ToString("D2"), now.Day.ToString("D2"), now.Hour.ToString("D2"), now.Minute.ToString("D2"), now.Second.ToString("D2"));
            this.OutputFileName = string.Format("output-{0}.txt", this.OutputFileDate);
            this.DataFileName = string.Format("output-{0}.dat", this.OutputFileDate);
            Log.DebugDelegate += OutputToConsoleAndFileLine;
            Log.WarningDelegate += OutputToConsoleAndFileLine;
            Log.ErrorDelegate += OutputToConsoleAndFileLine;
        }

        public void ReplayGameController(Game existingGame)
        {
            // Get the information from the copied game, but not the state of it.
            var turnTakerIds = existingGame.StartingTurnTakerIdentifiers;
            var isAdvanced = existingGame.IsAdvanced;
            var isChallenge = existingGame.IsChallenge;
            var teamAdvanced = existingGame.TurnTakers.Where(tt => tt.IsAdvanced).Select(tt => tt.Identifier);
            if (teamAdvanced.Count() == 0)
            {
                teamAdvanced = null;
            }
            var teamChallenge = existingGame.TurnTakers.Where(tt => tt.IsChallenge).Select(tt => tt.Identifier);
            if (teamChallenge.Count() == 0)
            {
                teamChallenge = null;
            }
            var promoIds = new Dictionary<string, string>();
            foreach (var ttWithPromo in existingGame.TurnTakers.Where(tt => tt.PromoIdentifier != null))
            {
                promoIds.Add(ttWithPromo.Identifier, ttWithPromo.PromoIdentifier);
            }
            int? randomSeed = null;
            if (existingGame.RandomSeed.HasValue)
            {
                randomSeed = existingGame.RandomSeed.Value;
            }
            var isMultiplayer = existingGame.IsMultiplayer;
            var randomizer = existingGame.InitialRNG;
            var shieldIdentifier = existingGame.OblivAeonStartingShield;
            var scionIdentifiers = existingGame.OblivAeonScionOrder;

            var game = new Game(turnTakerIds, isAdvanced, promoIds, randomSeed, isMultiplayer, randomizer, advancedIdentifiers: teamAdvanced, isChallenge: isChallenge, challengeIdentifiers: teamChallenge, shieldIdentifier: shieldIdentifier, scionIdentifiers: scionIdentifiers);

            _replayDecisionAnswers = existingGame.Journal.DecisionAnswerEntries(e => true);
            _replayingGame = true;

            SetupGameController(game);
        }

        private static JSONObject LoadUserData()
        {
            JSONObject data = null;
            if (File.Exists(UserDataFileName))
            {
                string json = File.ReadAllText(UserDataFileName);
                data = JSONObject.Parse(json);
            }
            else
            {
                data = new JSONObject();
            }

            return data;
        }

        private static void SaveUserData(JSONObject data)
        {
            string json = data.ToString();
            File.WriteAllText(UserDataFileName, json);
        }

        private object GetPersistentValueFromView(string key, Type type)
        {
            if (!string.IsNullOrEmpty(key))
            {
                // Allow promo cards to always be unlocked.
                if (key.Contains(GameController.HasPromoCardBeenUnlockedString))
                {
                    return false;
                }

                // Otherwise try looking it up.
                JSONObject data = LoadUserData();
                if (data != null)
                {
                    object result = null;
                    if (data.ContainsKey(key))
                    {
                        var value = data[key];
                        if (value.Type == JSONValueType.Boolean && type == typeof(bool))
                        {
                            result = value.Boolean;
                        }
                        else if (value.Type == JSONValueType.Number && type == typeof(int) || type == typeof(float) || type == typeof(double))
                        {
                            result = value.Number;
                        }
                        else if (value.Type == JSONValueType.String && type == typeof(string))
                        {
                            // Unescape strings, since they can be JSON
                            var str = value.Str.Replace("\\\"", "\"");
                            result = str;
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("User data type mismatch for key: " + key);
                        }
                    }

                    //OutputToConsoleAndFileLine ("GetPersistentValueFromView: " + key + " = " + result);

                    return result;
                }
                else
                {
                    OutputToConsoleAndFileLine("ERROR: GetPersistentValueFromView got null from LoadUserData! Returning null.");
                    return null;
                }
            }
            else
            {
                OutputToConsoleAndFileLine("ERROR: GetPersistentValueFromView called with empty key! Returning null.");
                return null;
            }
        }

        private void SetPersistentValueInView(string key, object value)
        {
            JSONObject data = LoadUserData();

            JSONValue jsonValue = null;
            if (value is bool)
            {
                jsonValue = new JSONValue((bool)value);
            }
            else if (value is double || value is float || value is int)
            {
                jsonValue = new JSONValue((double)value);
            }
            else if (value is string)
            {
                // Need to escape strings since they can be JSON
                var str = value as string;
                str = str.Replace("\"", "\\\"");
                jsonValue = new JSONValue(str);
            }
            else
            {
                OutputToConsoleAndFileLine("Unhandled data type for user data key " + key + ": " + value);
            }

            if (value != null)
            {
                data[key] = jsonValue;
                SaveUserData(data);
            }
        }

        IEnumerable<KeyValuePair<string, string>> HandleGetHeroCardsInBoxRequest(Func<string, bool> identifierCriteria, Func<string, bool> turnTakerCriteria)
        {
            var result = new List<KeyValuePair<string, string>>();

            // Find all the playable hero character cards in the box (including other sizes of Sky-Scraper)
            var availableHeroes = DeckDefinition.AvailableHeroes;
            Dictionary<string, DeckDefinition> modHeroData = LoadAllModContentOfKind(DeckDefinition.DeckKind.Hero);
            List<string> fullHeroList = new List<string>();

            fullHeroList.AddRange(availableHeroes);
            fullHeroList.AddRange(modHeroData.Keys);
            DeckDefinition heroDefinition = null;

            foreach (var heroTurnTaker in availableHeroes.Where(turnTakerCriteria))
            {
                if (availableHeroes.Contains(heroTurnTaker))
                {
                    heroDefinition = DeckDefinitionCache.GetDeckDefinition(heroTurnTaker);
                }
                else
                {
                    heroDefinition = modHeroData[heroTurnTaker];
                }

                var defs = heroDefinition.GetAllCardDefinitions();

                foreach (var cardDef in defs)
                {
                    // Ignore non-real cards (Sentinels Intructions) and cards that do not start in play (Sky-Scraper sizes)
                    if (cardDef.IsCharacter
                        && cardDef.IsRealCard
                        && identifierCriteria(cardDef.QualifiedPromoIdentifierOrIdentifier))
                    {
                        // It's in the box!
                        var kvp = new KeyValuePair<string, string>(heroTurnTaker, cardDef.QualifiedPromoIdentifierOrIdentifier);
                        //Debug.LogFormat("In the box {0}: {1}", result.Count, kvp.Value);
                        result.Add(kvp);
                    }
                }
            }

            return result;
        }

        private static Dictionary<string, DeckDefinition> LoadAllModContentOfKind(DeckDefinition.DeckKind deckKind)
        {
            Dictionary<string, DeckDefinition> modsDictionary = new Dictionary<string, DeckDefinition>();
            foreach (string space in NamespaceList)
            {
                Assembly assembly = ModHelper.GetAssemblyForNamespace(space);
                if (assembly == null) continue;

                foreach (var res in assembly.GetManifestResourceNames())
                {
                    var stream = assembly.GetManifestResourceStream(res);
                    if (stream is null) continue;
                    JSONObject jsonObject;
                    string text;
                    using (var sr = new StreamReader(stream))
                    {
                        text = sr.ReadToEnd();
                    }
                    if (text is null) continue;
                    jsonObject = JSONObject.Parse(text);
                    if (jsonObject is null) continue;
                    string name = jsonObject.GetString("name");
                    TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
                    name = myTI.ToTitleCase(name);
                    name = name.Replace(" ", string.Empty);
                    name = Regex.Replace(name, "[^a-zA-Z0-9]", String.Empty);
                    var kind = jsonObject.GetString("kind");
                    if (kind != null && kind != deckKind.ToString())
                        continue;

                    string qualifiedIdentifier = $"{space}.{name}";
                    if (kind == DeckDefinition.DeckKind.VillainTeam.ToString())
                    {
                        qualifiedIdentifier += "Team";
                    }

                    // Sometimes we don't guess right.
                    if (ModIdentifierOverrides.ContainsKey(qualifiedIdentifier))
                    {
                        qualifiedIdentifier = ModIdentifierOverrides[qualifiedIdentifier];
                    }

                    jsonObject.Add("identifier", new JSONValue(qualifiedIdentifier));
                    modsDictionary.Add(qualifiedIdentifier, new DeckDefinition(jsonObject, space));
                }
            }
            return modsDictionary;
        }

        public void TeardownGameController()
        {
            GameController.StartCoroutine -= this.StartCoroutine;
            GameController.ExhaustCoroutine -= this.RunCoroutine;
            GameController.OnWillPerformAction -= this.WillPerformAction;
            GameController.OnDidPerformAction -= this.DidPerformAction;
            GameController.OnMakeDecisions -= this.MakeDecisions;
            GameController.OnGetPersistentValueFromView -= this.GetPersistentValueFromView;
            GameController.OnSetPersistentValueInView -= this.SetPersistentValueInView;
            GameController.OnGetHeroCardsInBox -= this.HandleGetHeroCardsInBoxRequest;
            GameController = null;
            this.OutputFileName = null;
            Log.DebugDelegate -= OutputToConsoleAndFileLine;
            Log.WarningDelegate -= OutputToConsoleAndFileLine;
            Log.ErrorDelegate -= OutputToConsoleAndFileLine;
        }

        public bool RunLoop(bool continuingGame = false)
        {
            try
            {
                return InnerRunLoop(continuingGame);
            }
            catch (LoadGameException ex)
            {
                // Stop and load the game they want.
                GameNameToLoad = ex.GameName;
                return false;
            }
            catch (Exception ex)
            {
                if (this.UserFriendly && !(ex is IOException))
                {
                    if (!_ignoreException)
                    {
                        OutputToConsoleAndFileLine("");
                        OutputToConsoleAndFileLine("***** ERROR: UNHANDLED EXCEPTION *****");
                        OutputToConsoleAndFileLine(ex.ToString());
                    }
                }
                else
                {
                    // Developer mode, we want to see it locally.
                    throw ex;
                }

                return false;
            }
        }

        private bool InnerRunLoop(bool continuingGame = false)
        {
            bool gameOver = false;
            _ignoreException = false;

            if (!this.UserFriendly)
            {
                // Code playground area
                PrintMenu();
            }

            if (!continuingGame)
            {
                RunCoroutine(GameController.StartGame());

                RunCoroutine(GameController.EnterNextTurnPhase());
            }
            else
            {
                GameController.ResumeGame();

                OutputToConsoleAndFileLine("Resuming simulation...");
            }

            var santa = this.GameController.PromoCardManager.FindPromoCardUnlockController("SantaGuiseCharacter");
            if (santa != null)
            {
                (santa as Handelabra.Sentinels.Engine.Controller.PromoCardUnlockControllers.SantaGuiseCharacterPromoCardUnlockController).ShowGiftOutput = true;
            }

            var pwcc = this.GameController.PromoCardManager.FindPromoCardUnlockController("PrimeWardensCaptainCosmicCharacter");
            if (pwcc != null)
            {
                (pwcc as Handelabra.Sentinels.Engine.Controller.PromoCardUnlockControllers.PrimeWardensCaptainCosmicCharacterPromoCardUnlockController).ShowPointOutput = true;
            }

            if (!this.UserFriendly)
            {
                // Testing stuff
                ConfigurePostGameStartTesting();
            }

            string input = "";
            int sanity = 10000;
            while (!QuitRequested)
            {
                if (this.GameController.IsGameOver)
                {
                    Console.WriteLine("\n\n\n-------------------- G A M E   O V E R --------------------\n\n\n");
                    gameOver = true;
                    break;
                }

                if (!this.EnforceRules)
                {
                    string cannot = "";

                    if (GameController.ActiveTurnTaker.IsHero)
                    {
                        HeroTurnTakerController hero = GameController.ActiveTurnTakerController.ToHero();
                        if (!CanUsePowers(hero))
                        {
                            cannot += " (Cannot Use Powers) ";
                        }
                        if (!CanDrawCards(hero))
                        {
                            cannot += " (Cannot Draw Cards) ";
                        }
                    }

                    if (!GameController.ActiveTurnTakerController.CanPlayCards)
                    {
                        cannot += " (Cannot Play Cards) ";
                    }

                    OutputToConsoleAndFile(GameController.ActiveTurnTaker.DeckDefinition.Identifier + cannot + "> ");
                    input = GetInput();
                    if (input.Equals("stats"))
                    {
                        this.PrintStats();
                    }
                    else if (input.StartsWith("a"))
                    {
                        if (GameController.ActiveTurnTakerController.IsHero)
                        {
                            HeroTurnTakerController controller = (HeroTurnTakerController)GameController.ActiveTurnTakerController;
                            this.DrawCards(controller, this.GetNumber(input));
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("Non-heroes may not draw cards.");
                        }
                    }
                    else if (input.StartsWith("s"))
                    {
                        this.PlayCards(GameController.ActiveTurnTakerController, this.GetNumber(input));
                    }
                    else if (input.StartsWith("d"))
                    {
                        this.DiscardCardsFromHand(GameController.ActiveTurnTakerController, this.GetNumber(input));
                    }
                    else if (input.StartsWith("f"))
                    {
                        this.DestroyCards(GameController.ActiveTurnTakerController, this.GetNumber(input));
                    }
                    else if (input.StartsWith("g"))
                    {
                        IEnumerable<Card> targets = this.GameController.FindTargetsInPlay();
                        SelectCardDecision decision = new SelectCardDecision(this.GameController, null, SelectionType.SelectTarget, targets);

                        var sourceIndex = PrintAndSelectOption("Damage Source?", decision, false);

                        var targetIndex = PrintAndSelectOption("Damage Target?", decision, false);

                        if (sourceIndex.HasValue && targetIndex.HasValue)
                        {
                            var source = new DamageSource(this.GameController, targets.ElementAt(sourceIndex.Value - 1));
                            decision = new SelectCardDecision(this.GameController, null, SelectionType.SelectTarget, targets);
                            Card target = targets.ElementAt(targetIndex.Value - 1);
                            //string[] options = { "0", "1", "2", "3", "4", "5" };
                            //int amount = PrintAndSelectOption("Amount?", options, false) - 1;
                            IEnumerable<DamagePreviewResult> results = this.GameController.GetDamagePreviewResults(source, target, card => 3, DamageType.Melee);
                            DamagePreviewResult first = results.First();
                            foreach (GameAction action in first.OrderedGameActions)
                            {
                                string doWhat = "";
                                if (action is IncreaseDamageAction)
                                {
                                    doWhat = "+" + (action as IncreaseDamageAction).AmountToIncrease;
                                }
                                Console.WriteLine(action.CardSource.Card.Title + " " + doWhat);
                            }
                            Console.WriteLine(first.DealDamageAction.Amount);
                        }
                    }
                    else if (input.StartsWith("u"))
                    {
                        this.UsePower(GameController.ActiveTurnTakerController);
                    }
                    else if (input.StartsWith("i"))
                    {
                        this.UseIncapacitatedAbility(GameController.ActiveTurnTakerController);
                    }
                    else if (input.StartsWith("v"))
                    {
                        if (input.Substring(1).StartsWith("a"))
                        {
                            this.PrintDeck(GameController.ActiveTurnTakerController, this.GetNumber(input, 2));
                        }
                        else if (input.Substring(1).StartsWith("s"))
                        {
                            this.PrintHand(GameController.ActiveTurnTakerController);
                        }
                        else if (input.Substring(1).StartsWith("d"))
                        {
                            this.PrintPlayArea(GameController.ActiveTurnTakerController);
                        }
                        else if (input.Substring(1).StartsWith("f"))
                        {
                            this.PrintTrash(GameController.ActiveTurnTakerController);
                        }
                    }
                    else if (input.Equals("ntt"))
                    {
                        this.NextTurn(GameController, GameController.ActiveTurnTakerController);
                    }
                    else if (input.StartsWith("np"))
                    {
                        this.NextTurnPhase(this.GetNumber(input, 2));
                    }
                    else if (input.StartsWith("nt"))
                    {
                        this.NextTurn(GameController, this.GetNumber(input, 2));
                    }
                    else if (input.Equals("p"))
                    {
                        // Print the game state
                        OutputToConsoleAndFileLine(GameController.Game.ToString());
                    }
                    else if (input.Equals("ps"))
                    {
                        // Print the game state
                        OutputToConsoleAndFileLine(GameController.Game.ToStateString());
                    }
                    else if (input.Equals("pf"))
                    {
                        PrintFriendlyGame();
                    }
                    else if (input.Equals("j"))
                    {
                        // Print the game journal
                        OutputToConsoleAndFileLine(GameController.Game.Journal.Entries.ToRecursiveString("\n"));
                    }
                    else if (input.Equals("m"))
                    {
                        this.PrintMenu();
                    }
                    else if (input.Equals("1"))
                    {
                        // Toggle verbose
                        this.Verbose = !this.Verbose;
                        OutputToConsoleAndFileLine("Verbose now set to: " + this.Verbose);
                    }
                    else if (input.StartsWith("2"))
                    {
                        // Save
                        this.SaveGame(input.Substring(1));
                    }
                    else if (input.StartsWith("3"))
                    {
                        // Load
                        this.LoadGame(input.Substring(1));
                    }
                    else if (input.Equals("4"))
                    {
                        // Toggle enforce rules
                        this.EnforceRules = !this.EnforceRules;
                        OutputToConsoleAndFileLine("Enforce Rules now set to: " + this.EnforceRules);
                        Log.ToggleLogName(LogName.MegaComputer, true);
                    }
                    else if (input.Equals("q"))
                    {
                        QuitRequested = true;
                        this.GameController.IsRealGameOver = true;
                    }
                }
                else
                {
                    SaveForContinue();

                    if (this.GameController.ActiveTurnPhase == null)
                    {
                        this.GameController.Game.SetActiveTurnPhase(this.GameController.FindNextTurnPhase(this.GameController.ActiveTurnPhase));
                    }

                    if (this.RunPhase(this.GameController.ActiveTurnTakerController, this.GameController.ActiveTurnPhase))
                    {
                        this.NextTurnPhase();
                    }
                }

                sanity -= 1;
                if (sanity <= 0)
                {
                    OutputToConsoleAndFileLine("Infinite loop detected in RunLoop()");
                    Environment.Exit(1);
                }
            }

            if (gameOver)
            {
                // Print out the stats
                PrintStats();

                OutputToConsoleAndFileLine("");
            }

            CloseFileOutput();

            return gameOver;
        }

        private bool RunPhase(TurnTakerController controller, TurnPhase phase)
        {
            int sanity = 1000;

            OutputToConsoleAndFileLine("");

            if (phase.IsPlayCard)
            {
                if (phase.TurnTaker.IsHero && !phase.TurnTaker.ToHero().HasCardsInHand)
                {
                    Log.Debug(phase.TurnTaker.Name + " has no cards in their hand to play.");
                }
                else if (phase.TurnTaker.IsScion && phase.TurnTaker.PlayArea.Cards.Where(c => c.IsInPlayAndHasGameText && c.IsScionCharacter).Count() > 0)
                {
                    // Play top card of the Scion deck
                    var oblivTT = this.GameController.FindTurnTakersWhere(tt => tt.Identifier == "OblivAeon").FirstOrDefault();
                    RunCoroutine(this.GameController.PlayTopCardOfLocation(null,
                                                                           oblivTT.FindSubDeck("ScionDeck"),
                                                                           responsibleTurnTaker: phase.TurnTaker,
                                                                           overridePlayLocation: phase.TurnTaker.PlayArea));
                }
                else
                {
                    while (this.GameController.CanPerformPhaseAction(phase))
                    {
                        if (this.GameController.IsGameOver)
                        {
                            return false;
                        }

                        this.PlayCards(controller, 1);

                        sanity -= 1;
                        if (sanity <= 0)
                        {
                            OutputToConsoleAndFileLine("Infinite loop detected in RunPhase() - PlayCard");
                            Environment.Exit(1);
                        }
                    }
                }
            }
            else if (phase.IsAfterEnd)
            {
                RunCoroutine(this.GameController.RunAfterEndOfTurn());
            }
            else if (controller is HeroTurnTakerController)
            {
                if (phase.IsUsePower)
                {
                    while (this.GameController.CanPerformPhaseAction(phase))
                    {
                        if (this.GameController.IsGameOver)
                        {
                            return false;
                        }

                        this.UsePower(controller);

                        sanity -= 1;
                        if (sanity <= 0)
                        {
                            OutputToConsoleAndFileLine("Infinite loop detected in RunPhase() - UsePower");
                            Environment.Exit(1);
                        }
                    }
                }
                else if (phase.IsDrawCard)
                {
                    while (this.GameController.CanPerformPhaseAction(phase))
                    {
                        if (this.GameController.IsGameOver)
                        {
                            return false;
                        }

                        this.DrawCards(controller.ToHero(), 1, true);

                        sanity -= 1;
                        if (sanity <= 0)
                        {
                            OutputToConsoleAndFileLine("Infinite loop detected in RunPhase() - DrawCard");
                            Environment.Exit(1);
                        }
                    }
                }
                else if (phase.IsUseIncapacitatedAbility)
                {
                    while (this.GameController.CanPerformPhaseAction(phase))
                    {
                        if (this.GameController.IsGameOver)
                        {
                            return false;
                        }

                        this.UseIncapacitatedAbility(controller.ToHero(), 1, true);

                        sanity -= 1;
                        if (sanity <= 0)
                        {
                            OutputToConsoleAndFileLine("Infinite loop detected in RunPhase() - UseIncapacitatedAbility");
                            Environment.Exit(1);
                        }
                    }
                }
                else if (phase.IsBeforeStart)
                {
                    RunCoroutine(this.GameController.RunBeforeStartOfTurn(controller.ToHero()));
                }
            }

            return true;
        }

        private void SaveForContinue()
        {
            bool success = SaveGame("continue", true);
            if (success)
            {
                // Copy the file to a dated one as well
                File.Copy("continue.dat", this.DataFileName, true);
            }
        }

        private bool SaveGame(string name, bool quiet = false)
        {
            bool success = false;

            BinaryFormatter formatter = new BinaryFormatter();
            formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            FileStream stream = null;
            try
            {
                stream = new FileStream(name.Trim() + ".dat", FileMode.Create, FileAccess.Write, FileShare.None);
                formatter.Serialize(stream, this.GameController.Game);

                success = true;

                if (!quiet)
                {
                    OutputToConsoleAndFileLine("Serialized game is " + stream.Length + " bytes");
                    OutputToConsoleAndFileLine("Successfully saved game to " + stream.Name);
                }
            }
            catch (SerializationException e)
            {
                OutputToConsoleAndFileLine("Failed to serialize. Reason: " + e.Message + " Please report this.");
            }
            catch (IOException e)
            {
                OutputToConsoleAndFileLine("Failed to create file. Reason: " + e.Message);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return success;
        }

        public static Game LoadGameFile(string name)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            string path = name.Trim() + ".dat";
            if (File.Exists(path))
            {
                FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
                    Game game = (Game)formatter.Deserialize(stream);

                    if (game.SavedModelVersion >= Game.CurrentModelVersion)
                    {
                        return game;
                    }
                    else
                    {
                        Console.WriteLine("The previous session file is incompatible with the new version of Mega Computer. You must start a new session.");
                        return null;
                    }
                }
                finally
                {
                    AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
                    stream.Close();
                }
            }
            else
            {
                return null;
            }
        }

        private bool LoadGame(string name, bool quiet = false)
        {
            try
            {
                Game game = LoadGameFile(name);
                this.TeardownGameController();
                this.SetupGameController(game);

                if (!quiet)
                {
                    OutputToConsoleAndFileLine("Successfully loaded game " + name);
                }

                return true;
            }
            catch (SerializationException e)
            {
                OutputToConsoleAndFileLine("Failed to load game. Reason: " + e.Message);
                throw;
            }
        }

        private static System.Reflection.Assembly OnAssemblyResolve(System.Object sender, System.ResolveEventArgs reArgs)
        {
            Console.WriteLine("OnAssemblyResolve: " + reArgs.Name);
            foreach (System.Reflection.Assembly assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                System.Reflection.AssemblyName assemblyName = assembly.GetName();
                if (reArgs.Name.StartsWith(assemblyName.Name))
                {
                    Console.WriteLine("Returning " + assembly);
                    return (assembly);
                }
            }

            return null;
        }

        public void PutCardsOnDeck(TurnTakerController taker, int numberOfCards)
        {
            OutputToConsoleAndFileLine("JMG: THIS IMPLEMENTATION IS OUTDATED");
            for (int i = 0; i < numberOfCards; i++)
            {
                Card card = SelectACard(taker, taker.TurnTaker.PlayArea, "Select a card from the play area to put in the deck.", "There are no cards in the play area to put in the deck.");
                if (card != null)
                {
                    int topOrBottom = 0;
                    while (topOrBottom == 0)
                    {
                        OutputToConsoleAndFileLine("\tPlace card on top or bottom of deck?\n\t1. Top\n\t2. Bottom");
                        OutputToConsoleAndFile("\t> ");
                        topOrBottom = GetCleanInput().ToInt(0);
                    }
                    bool toBottom = topOrBottom == 1 ? false : true;
                    taker.PutOnDeck(card, toBottom);
                    OutputToConsoleAndFileLine(card.Definition.Title + " put back in deck.");
                }
            }
        }



        public void PrintMenu()
        {
            OutputToConsoleAndFileLine(
                "a:   Draw Card, a#:  Draw # Cards\n" +
                "s:   Play Card, s#:  Play # Cards\n" +
                "u:   Use a Power\n" +
                "i:   Use an Incapacitated Ability\n" +
                "d:   Discard Card, d#:  Discard # Cards\n" +
                "f:   Destroy Card, f#:  Destroy # Cards\n" +
                "g:   Put Card on Deck, g#:  Put # Cards on Deck\n" +
                "va:  View Top Card of Deck, va#: View Top # Cards of Deck\n" +
                "vs:  View Hand\n" +
                "vd:  View Play Area\n" +
                "vf:  View Trash\n" +
                "np:  Next Phase, np#: Go through # phases\n" +
                "nt:  Next Player's Turn, nt#: Go trhough # turns\n" +
                "ntt: Skip turns until it is the start of the current player's turn.\n" +
                "p:   Print the game state\n" +
                "j:   Print the game journal\n" +
                "m:   Print this menu\n" +
                "1:   Toggle verbose (currently " + this.Verbose + ")\n" +
                "2 <name>: Save game\n" +
                "3 <name>: Load game\n" +
                "4:   Toggle enforce rules\n" +
                "q:   Quit");
        }

        public void DrawCards(HeroTurnTakerController taker, int numberOfCards = 1, bool optional = false)
        {
            for (int i = 0; i < numberOfCards; i++)
            {
                RunCoroutine(this.GameController.DrawCard(taker.HeroTurnTaker, optional));
            }
        }

        public void PlayCards(TurnTakerController taker, int numberOfCards)
        {
            // If hero, player decides
            if (taker is HeroTurnTakerController)
            {
                for (int i = 0; i < numberOfCards; i++)
                {
                    RunCoroutine(this.GameController.SelectAndPlayCardFromHand(taker.ToHero(), true));
                }
            }
            else
            {
                // Otherwise play the top card of the deck, reshuffling if necessary.
                for (int i = 0; i < numberOfCards; i++)
                {
                    this.RunCoroutine(this.GameController.PlayTopCard(null, taker, false, numberOfCards));
                }
            }
        }


        public void DestroyCards(TurnTakerController taker, int numberOfCards)
        {
            if (taker.IsHero)
            {
                for (int i = 0; i < numberOfCards; i++)
                {
                    this.RunCoroutine(this.GameController.SelectAndDestroyCard(taker.ToHero(), new LinqCardCriteria(c => c.IsInPlay), true));
                }
            }
            else
            {
                OutputToConsoleAndFileLine("Please destroy cards on a hero's turn.");
            }
        }

        public void DiscardCardsFromHand(TurnTakerController taker, int numberOfCards)
        {
            OutputToConsoleAndFileLine("JMG: THIS IMPLEMENTATION IS OUTDATED");

            if (taker.TurnTaker is HeroTurnTaker)
            {
                for (int i = 0; i < numberOfCards; i++)
                {
                    Card card = this.SelectACard(taker, ((HeroTurnTaker)taker.TurnTaker).Hand, "Which card would you like to discard from your hand?", "You have no cards in your hand to discard!");
                    if (card != null)
                    {
                        taker.ToHero().HeroTurnTaker.DiscardCard(card);
                        OutputToConsoleAndFileLine("You just discarded " + card.Definition.Title + "!");
                    }
                }
            }
            else
            {
                OutputToConsoleAndFileLine("This turn taker does not have a hand of cards.");
            }
        }

        public void UsePower(TurnTakerController taker)
        {
            if (taker.IsHero)
            {
                this.RunCoroutine(this.GameController.SelectAndUsePower(taker.ToHero()));
            }
            else
            {
                OutputToConsoleAndFileLine("Non-heros may not use powers.");
            }
        }

        public void UseIncapacitatedAbility(TurnTakerController taker, int numberOfTimes = 1, bool optional = true)
        {
            if (taker.IsHero)
            {
                for (int i = 0; i < numberOfTimes; i++)
                {
                    this.RunCoroutine(this.GameController.SelectIncapacitatedHeroAndUseAbility(taker.ToHero()));
                }
            }
            else
            {
                OutputToConsoleAndFileLine("Non-heros may not use incapacitated abilities.");
            }
        }

        public void PrintDeck(TurnTakerController taker, int numberOfCards)
        {
            IEnumerable<Card> deck = taker.TurnTaker.Deck.GetTopCards(numberOfCards);
            PrintCardList(taker, deck, "Top card(s) of your deck:", "You have no cards in your deck.");
        }

        public void PrintHand(TurnTakerController taker)
        {
            if (taker.TurnTaker is HeroTurnTaker)
            {
                IEnumerable<Card> hand = taker.GetCardsAtLocation(((HeroTurnTaker)taker.TurnTaker).Hand);
                PrintCardList(taker, hand, "Cards in your hand:", "You have no cards in your hand.");
            }
            else
            {
                OutputToConsoleAndFileLine("This turn taker does not have a hand of cards.");
            }
        }

        public void PrintPlayArea(TurnTakerController taker)
        {
            IEnumerable<Card> hand = taker.GetCardsAtLocation(taker.TurnTaker.PlayArea);
            PrintCardList(taker, hand, "Cards in your play area:", "You have no cards in your play area.");
        }

        public void PrintTrash(TurnTakerController taker)
        {
            IEnumerable<Card> hand = taker.GetCardsAtLocation(taker.TurnTaker.Trash);
            PrintCardList(taker, hand, "Cards in your trash:", "There are no cards in your trash.");
        }


        public Card SelectACard(TurnTakerController taker, Location location, string introText, string noCardsText)
        {
            string input = "";
            IEnumerable<Card> hand = taker.GetCardsAtLocation(location);

            if (hand.Count() == 0)
            {
                OutputToConsoleAndFileLine(noCardsText);
            }
            else
            {
                PrintCardList(taker, hand, introText, "No Cards");
                int choice = -1;
                while (choice == -1)
                {
                    OutputToConsoleAndFile("\t> ");
                    input = GetCleanInput();
                    try
                    {
                        choice = Convert.ToInt32(input) - 1;
                        Card card = hand.ElementAt(choice);
                        if (card == null)
                        {
                            OutputToConsoleAndFileLine("\tInvalid number... try a more valid one next time.");
                            choice = -1;
                        }
                        else
                        {
                            return card;
                        }
                    }
                    catch
                    {
                        OutputToConsoleAndFileLine("\tPsst... You're supposed to press a number!...");
                    }
                }
            }
            return null;
        }

        public void PrintCardList(TurnTakerController taker, IEnumerable<Card> cards, string introText, string noCardsText)
        {
            OutputToConsoleAndFileLine(cards.PrintElements((card, i) =>
            {
                CardController controller = taker.FindCardController(card);
                string imp = controller.IsImplemented ? " (Implemented)" : "";
                string flipped = controller.Card.IsFlipped ? " (Flipped)" : "";
                return "\n\t" + (i + 1) + ": " + card.Definition.Title + imp + flipped;
            },
                    introText, noCardsText));
        }

        public void NextTurnPhase(int numberOfPhases = 1)
        {
            for (int i = 0; i < numberOfPhases; i++)
            {
                this.RunCoroutine(this.GameController.EnterNextTurnPhase());
            }
        }

        public void NextTurn(GameController gameController, int numberOfTurns)
        {
            this.RunCoroutine(gameController.CyclePhasesToNextTurn(numberOfTurns));
        }

        public void NextTurn(GameController gameController, TurnTakerController ttController)
        {
            this.RunCoroutine(gameController.SkipToTurnTakerTurn(ttController));
        }

        public int GetNumber(string input, int startIndex = 1)
        {
            return input.Substring(startIndex, input.Length - startIndex).ToInt(onFail: 1);
        }


        private IEnumerator WillEnterTurnPhase(TurnPhase turnPhase)
        {
            OutputToConsoleAndFileLine("WillEnterTurnPhase: " + turnPhase);
            yield return 0;
        }

        private IEnumerator DidEnterTurnPhase(TurnPhase turnPhase)
        {
            OutputToConsoleAndFileLine("DidEnterTurnPhase: " + turnPhase);
            yield return 0;
        }


        private IEnumerator WillPerformAction(GameAction action)
        {
            if (this.Verbose)
            {
                OutputToConsoleAndFileLine("WillPerformAction: " + action);
            }

            yield return 0;
        }

        private IEnumerator WillApplyActionChanges(GameAction action)
        {
            if (this.Verbose)
            {
                OutputToConsoleAndFileLine("WillApplyActionChanges: " + action);
            }

            if (this.UserFriendly && action.IsSuccessful)
            {
                if (action is PhaseChangeAction)
                {
                    var phaseChange = action as PhaseChangeAction;
                    var message = String.Format("**** {0} - {1} ***** {0} - {1} ****", phaseChange.ToPhase.TurnTaker.Name.ToUpper(), phaseChange.ToPhase.FriendlyPhaseName.ToUpper());
                    int numStars = Console.WindowWidth - message.Length + 5;
                    if (numStars > 0)
                    {
                        var stars = new String('*', numStars);
                        message = message.Replace("*****", stars);
                    }

                    OutputToConsoleAndFileLine(Environment.NewLine + message);

                    if (CheatStops && phaseChange.ToPhase.IsStart)
                    {
                        PressEnterOrCheat(phaseChange.ToPhase.TurnTaker);
                    }
                    else if (Speed <= GameSpeed.Medium)
                    {
                        if (!phaseChange.ToPhase.TurnTaker.IsHero || Speed <= GameSpeed.Slow)
                        {
                            PressEnterToContinue();
                        }
                    }
                }
                else if (action is PlayCardAction)
                {
                    PlayCardAction play = action as PlayCardAction;
                    if (play.IsSuccessful)
                    {
                        PrintCard(play.CardToPlay);

                        bool anyMissing = false;
                        string output = "";
                        foreach (var quote in play.CardToPlay.Definition.FlavorQuotes)
                        {
                            // See if a turn taker or card is in play matching it
                            string name = null;
                            var turnTaker = this.GameController.Game.FindTurnTaker(quote.CardOrDeckIdentifier);
                            if (turnTaker != null)
                            {
                                name = turnTaker.Name;
                            }
                            else
                            {
                                var card = this.GameController.FindCardsWhere(c => c.Identifier == quote.CardOrDeckIdentifier && c.IsInPlay).FirstOrDefault();
                                if (card != null)
                                {
                                    name = card.Title;
                                }
                            }

                            if (name != null)
                            {
                                var text = ReplaceTemplates(play.CardToPlay, quote.Text).Replace(Environment.NewLine, " ").Replace("[b]", "").Replace("[/b]", "");
                                output += name + " says, \"" + text + "\"" + Environment.NewLine;
                            }
                            else
                            {
                                anyMissing = true;
                                break;
                            }
                        }

                        if (!anyMissing && output.Length > 0)
                        {
                            OutputToConsoleAndFileLine(output);
                        }
                    }
                }
            }

            yield return null;
        }

        private IEnumerator DidPerformAction(GameAction action)
        {
            if (this.Verbose)
            {
                OutputToConsoleAndFileLine("DidPerformAction: " + action);
            }

            if (action.IsSuccessful)
            {
                if (action is DrawCardAction)
                {
                    DrawCardAction draw = action as DrawCardAction;
                    if (draw.DrawnCard != null)
                    {
                        PrintCard(draw.DrawnCard);
                    }
                }
                else if (action is RevealCardsAction)
                {
                    RevealCardsAction reveal = action as RevealCardsAction;
                    foreach (var card in reveal.RevealedCards)
                    {
                        PrintCard(card);
                        _lastCardPrinted = card;
                    }
                    _lastCardPrinted = null;
                }
                else if (action is AddTokensToPoolAction)
                {
                    // Show the token count.
                    var addToken = action as AddTokensToPoolAction;
                    addToken.TokenPool.CardWithTokenPool.TokenPools.ForEach(tp => OutputToConsoleAndFileLine("*** " + tp.Name + " Total: " + tp.CurrentValue + " ***"));
                    OutputToConsoleAndFileLine("");
                }

                if (!((action is PhaseChangeAction) || (action is MakeDecisionAction) || (action is MakeDecisionsAction)) && Speed <= GameSpeed.VerySlow)
                {
                    PressEnterToContinue();
                }
            }

            yield return 0;
        }

        private IEnumerator ReceiveMessage(MessageAction message)
        {
            string context = "";
            if (message is StatusEffectMessageAction)
            {
                StatusEffectMessageAction effect = (message as StatusEffectMessageAction);
                if (effect.State == StatusEffectMessageAction.StatusEffectState.Adding)
                {
                    context = "Applying effect: ";
                }
                else
                {
                    context = "Effect expired: ";
                }
            }

            string source = "";
            if (message.CardSource != null)
            {
                source = message.CardSource.Card.Title + ": ";
            }
            string output = source + context + message.Message;

            output = output.Replace("[i]", "").Replace("[/i]", "");

            if (message.Priority == Priority.High)
            {
                OutputToConsoleAndFileLine("*****===== " + output + " =====*****");
                PressEnterToContinue();
            }
            else if (message.Priority == Priority.Medium)
            {
                OutputToConsoleAndFileLine("----- " + output + " -----");
            }
            else
            {
                OutputToConsoleAndFileLine("- " + output + " -");
            }
            yield return null;
        }

        private IEnumerator MakeDecisions(IDecision decision)
        {
            //Log.Debug("MakeDecisions");

            if (!QuitRequested)
            {
                IEnumerator decisionE = MakeDecision(decision);
                RunCoroutine(decisionE);

                yield return 0;
            }
        }

        private IEnumerator MakeDecision(IDecision decision)
        {
            //Log.Debug("MakeDecision");

            // If replaying a game, check if the decision has already been answered.
            DecisionAnswerJournalEntry answer = null;
            if (_replayingGame && _replayDecisionAnswers != null)
            {
                answer = _replayDecisionAnswers.Where(d => d.DecisionIdentifier == decision.DecisionIdentifier).FirstOrDefault();
                if (answer != null)
                {
                    if (answer.Skipped)
                    {
                        Console.WriteLine("Replay skipping decision: " + decision.ToStringForMultiplayerDebugging());
                        decision.Skip();
                    }

                    else if (answer.AutoDecided)
                    {
                        Console.WriteLine("Replay auto deciding decision: " + decision.ToStringForMultiplayerDebugging());
                        decision.AutoDecide();
                    }
                    else if (answer.AnswerIndex.HasValue)
                    {
                        Console.WriteLine("Replay choosing index " + answer.AnswerIndex + " for decision: " + decision.ToStringForMultiplayerDebugging());
                        decision.ChooseIndex(answer.AnswerIndex.Value);

                        if (decision is SelectCardDecision)
                        {
                            (decision as SelectCardDecision).FinishedSelecting = true;
                        }
                    }

                    // If this was the very last decision choice, we are no longer able to continue replaying the game.
                    if (answer == _replayDecisionAnswers.LastOrDefault())
                    {
                        _replayingGame = false;
                    }
                }
                else
                {
                    Console.WriteLine("\tReplayDecisionAnswers did not have an entry for this decision: " + decision.ToStringForMultiplayerDebugging());
                }
            }

            if (answer == null)
            {
                HeroTurnTakerController hero = decision.HeroTurnTakerController;
                string address = "";
                if (hero != null)
                {
                    address = hero.Name + ", ";
                }

                if (decision.ExtraInfo != null)
                {
                    OutputToConsoleAndFileLine(decision.ExtraInfo());
                }

                var cardSource = decision.CardSource != null ? "(" + decision.CardSource.Card.Title + ")" : "";
                string question = cardSource + address + " Do you want to do a thing? (" + decision.ToStringForMultiplayerDebugging() + ". Please report this.)";
                if (decision.CardSource != null)
                {
                    address = "(" + decision.CardSource.Card.Title + ") " + address;

                    // Print out the card
                    // or not, it's noisy.
                    //PrintCard(decision.CardSource.Card);
                }

                string sequence = "a";
                if (decision.SequenceIndex.HasValue)
                {
                    sequence = decision.SequenceIndex.Value.ToOrdinalString().ToLower();
                }

                bool done = QuitRequested;
                while (!decision.Completed && !done && !QuitRequested && CheatEnding == null)
                {
                    if (decision is YesNoDecision)
                    {
                        YesNoDecision yesNo = (YesNoDecision)decision;

                        switch (yesNo.SelectionType)
                        {
                            case SelectionType.ActivateCrocodileEffect:
                                {
                                    question = address + "Do you want to activate the Crocodile effect on " + yesNo.CardSource.Card.Title + "?";
                                    break;
                                }
                            case SelectionType.ActivateGazelleEffect:
                                {
                                    question = address + "Do you want to activate the Gazelle effect on " + yesNo.CardSource.Card.Title + "?";
                                    break;
                                }
                            case SelectionType.ActivateRhinocerosEffect:
                                {
                                    question = address + "Do you want to activate the Rhinoceros effect on " + yesNo.CardSource.Card.Title + "?";
                                    break;
                                }
                            case SelectionType.DealDamage:
                                {
                                    DealDamageAction dealDamage = (DealDamageAction)yesNo.GameAction;
                                    if (dealDamage != null)
                                    {
                                        var target = dealDamage.Target != null ? dealDamage.Target.Title : "a target";
                                        question = address + "Do you want " + dealDamage.DamageSource.TitleOrName + " to deal " + target + " " + dealDamage.Amount + " " + dealDamage.DamageType + " damage?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to deal damage? (" + yesNo.CardSource.Card.Title + ")";
                                    }
                                    break;
                                }
                            case SelectionType.DealDamageSelf:
                                {
                                    DealDamageAction dealDamage = (DealDamageAction)yesNo.GameAction;
                                    if (dealDamage != null)
                                    {
                                        question = address + "Do you want " + dealDamage.DamageSource.TitleOrName + " to deal themselves " + dealDamage.Amount + " " + dealDamage.DamageType + " damage?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to deal yourself damage? (" + yesNo.CardSource.Card.Title + ")";
                                    }
                                    break;
                                }
                            case SelectionType.DestroyCard:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        // Print out the card
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        PrintCard(yesNoCard.Card);

                                        question = address + "Do you want to destroy " + yesNoCard.Card.Title + "?";
                                    }
                                    else if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;
                                        var cards = "cards";
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            if (yesNoAmount.Amount.Value == 1)
                                            {
                                                cards = "1 card";
                                            }
                                            else
                                            {
                                                cards = yesNoAmount.Amount.Value + " cards";
                                            }
                                        }
                                        question = address + "Do you want to destroy " + cards + "?";
                                    }
                                    break;
                                }
                            case SelectionType.DiscardCard:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;

                                        if (yesNoAmount.HeroTurnTakerController == null && yesNoAmount.RequireUnanimous)
                                        {
                                            if (yesNoAmount.Amount.Value == 1)
                                            {
                                                question = address + "Do ALL players agree to discard a card?";
                                            }
                                            else
                                            {
                                                question = address + "Do ALL players agree to discard " + yesNoAmount.Amount.Value + " cards?";
                                            }
                                        }
                                        else if (yesNoAmount.Amount == 1)
                                        {
                                            question = address + "Do you want to discard a card?";
                                        }
                                        else
                                        {
                                            question = address + "Do you want to discard " + (yesNoAmount.UpTo ? "up to " : "") + yesNoAmount.Amount + " cards?";
                                        }
                                    }
                                    else if (yesNo is YesNoCardDecision)
                                    {
                                        // Print out the card
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        PrintCard(yesNoCard.Card);

                                        question = address + "Do you want to discard " + yesNoCard.Card.Title + "?";
                                    }

                                    break;
                                }
                            case SelectionType.DiscardCardsToNoEffect:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;

                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            question = address + "You do not have enough cards to discard " + yesNoAmount.Amount.Value + " cards. Discard anyway to no effect?";
                                        }
                                    }

                                    break;
                                }
                            case SelectionType.DiscardFromDeck:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        // Print out the card
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        PrintCard(yesNoCard.Card);

                                        question = address + "Do you want to discard " + yesNoCard.Card.Title + " from the top of " + yesNoCard.Card.Owner.Name + "'s deck?";
                                    }
                                    else if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = (YesNoAmountDecision)yesNo;
                                        var cards = "cards";
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            if (yesNoAmount.Amount.Value == 1)
                                            {
                                                cards = "card";
                                            }
                                            else
                                            {
                                                cards = yesNoAmount.Amount.Value + " cards";
                                            }
                                        }

                                        if (yesNoAmount.HeroTurnTakerController == null && yesNoAmount.RequireUnanimous)
                                        {
                                            question = address + "Do ALL players agree to discard the top " + cards + " of their deck?";
                                        }
                                        else
                                        {
                                            question = address + "Do you want to discard the top " + cards + " of your deck?";
                                        }
                                    }

                                    break;
                                }
                            case SelectionType.DiscardFromEnvironmentDeck:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = (YesNoAmountDecision)yesNo;
                                        var cards = "cards";
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            if (yesNoAmount.Amount.Value == 1)
                                            {
                                                cards = "card";
                                            }
                                            else
                                            {
                                                cards = yesNoAmount.Amount.Value + " cards";
                                            }
                                        }

                                        question = address + "Do you want to discard the top " + cards + " of the environment deck?";
                                    }

                                    break;
                                }
                            case SelectionType.DrawCard:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;
                                        string willCauseReshuffle = this.GameController.WillDrawCauseReshuffle(yesNoAmount.HeroTurnTakerController.HeroTurnTaker, yesNoAmount.Amount.Value) ? " (This will cause the trash to be reshuffled into the deck)" : "";
                                        if (yesNoAmount.Amount == 1)
                                        {
                                            question = address + "Do you want to draw a card?" + willCauseReshuffle;
                                        }
                                        else
                                        {
                                            question = address + "Do you want to draw " + (yesNoAmount.UpTo ? "up to" : "") + yesNoAmount.Amount + " cards?" + willCauseReshuffle;
                                        }
                                    }
                                    else
                                    {
                                        string willCauseReshuffle = this.GameController.WillDrawCauseReshuffle(yesNo.HeroTurnTakerController.HeroTurnTaker, 1) ? " (This will cause the trash to be reshuffled into the deck)" : "";
                                        question = address + "Do you want to draw a card?" + willCauseReshuffle;
                                    }

                                    break;
                                }
                            case SelectionType.GainHP:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision amountDecision = (YesNoAmountDecision)yesNo;
                                        string upTo = amountDecision.UpTo ? "up to " : "";
                                        question = address + "Do you want to gain " + upTo + amountDecision.Amount + " HP?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to gain HP?";
                                    }
                                    break;
                                }
                            case SelectionType.GetWhatYouWereLookingFor:
                                {
                                    question = address + "Did you get what you were looking for?";
                                    break;
                                }
                            case SelectionType.HandsUpYellWoo:
                                {
                                    question = address + "Do you put your hands up and yell \"WOO!\"?";

                                    break;
                                }
                            case SelectionType.HighestHP:
                            case SelectionType.LowestHP:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;

                                        string damageInfo = "";
                                        if (yesNoCard.GameAction != null && yesNoCard.GameAction is DealDamageAction)
                                        {
                                            DealDamageAction dealDamage = (yesNoCard.GameAction as DealDamageAction);
                                            int amount = dealDamage.Amount;
                                            DamageType type = dealDamage.DamageType;
                                            if (dealDamage.DamageSource != null)
                                            {
                                                damageInfo = " (To be dealt " + amount + " " + type + " damage)";
                                            }
                                            else
                                            {
                                                damageInfo = " (To deal " + amount + " " + type + " damage)";
                                            }
                                        }

                                        if (yesNo.SelectionType == SelectionType.LowestHP)
                                        {
                                            question = address + "Is " + yesNoCard.Card.Title + " considered to have the lowest HP?" + damageInfo;
                                        }
                                        else if (yesNo.SelectionType == SelectionType.HighestHP)
                                        {
                                            question = address + "Is " + yesNoCard.Card.Title + " considered to have the highest HP?" + damageInfo;
                                        }
                                    }
                                    break;
                                }
                            case SelectionType.MoveCard:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision amountDecision = (YesNoAmountDecision)yesNo;
                                        string upTo = amountDecision.UpTo ? "up to " : "";
                                        question = address + "Do you want to move " + upTo + amountDecision.Amount + " cards?";
                                    }
                                    else if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision cardDecision = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to move " + cardDecision.Card.Title + "?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to move a card?";
                                    }
                                    break;
                                }
                            case SelectionType.MoveCardToEnvironmentTrash:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to move " + yesNoCard.Card.Title + " to the environment trash?";
                                    }
                                    break;
                                }
                            case SelectionType.MoveCardOnBottomOfDeck:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to move " + yesNoCard.Card.Title + " to the bottom of its deck?";
                                    }
                                    break;
                                }
                            case SelectionType.MoveCardToHand:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to move " + yesNoCard.Card.Title + " to your hand?";
                                    }
                                    break;
                                }
                            case SelectionType.MoveCardToHandFromTrash:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = (YesNoAmountDecision)yesNo;
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            question = address + "Do you want to move " + yesNoAmount.Amount.Value + " cards from your trash to your hand?";
                                        }
                                    }
                                    break;
                                }
                            case SelectionType.MoveCardToUnderCard:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to put " + yesNoCard.Card.Title + " beneath " + yesNoCard.CardSource.Card.Title + "?";
                                    }
                                    break;
                                }
                            case SelectionType.MoveUnderThisCard:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        var yesNoAmount = yesNo as YesNoAmountDecision;
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            var upto = yesNoAmount.UpTo ? "up to " : "";
                                            question = "Do you want to move " + upto + yesNoAmount.Amount.Value + " cards under this card?";
                                        }
                                    }

                                    break;
                                }
                            case SelectionType.PlayCard:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want to play " + yesNoCard.Card.Title + "?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to play a card?";
                                    }

                                    break;
                                }
                            case SelectionType.PlayBottomCard:
                                {
                                    question = address + "Do you want to play the bottom card of your deck?";
                                    break;
                                }
                            case SelectionType.PlayTopCard:
                                {
                                    question = address + "Do you want to play the top card?";
                                    break;
                                }
                            case SelectionType.PlayTopCardOfVillainDeck:
                                {
                                    question = address + "Do you want to play the top card of the villain deck?";
                                    break;
                                }
                            case SelectionType.PlayTopCardOfEnvironmentDeck:
                                {
                                    question = address + "Do you want to play the top card of the environment deck?";
                                    break;
                                }
                            case SelectionType.PlayTopCardOfBothEnvironmentDecks:
                                {
                                    question = address + "Do you want to play the top card of both environment decks?";
                                    break;
                                }
                            case SelectionType.PlayTopCardOfVillainDeckAndEnvironmentDeck:
                                {
                                    question = address + "Do you want to play both the top card of the Villain deck and the top card of the Environment deck?";
                                    break;
                                }
                            case SelectionType.PreventDamage:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want to prevent damage using " + yesNoCard.Card.Title + "?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to prevent damage?";
                                    }
                                    break;
                                }
                            case SelectionType.PreventDestruction:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want to prevent destruction using " + yesNoCard.Card.Title + "?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to prevent destruction?";
                                    }
                                    break;
                                }
                            case SelectionType.PutIntoPlay:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want put " + yesNoCard.Card.Title + " into play?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to put a card into play?";
                                    }

                                    break;
                                }
                            case SelectionType.PutUnderCardIntoPlay:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want to put a card under " + yesNoCard.Card.Title + " into play?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to put a card into play?";
                                    }
                                    break;
                                }
                            case SelectionType.RedirectDamage:
                                {
                                    DealDamageAction damage = (yesNo.GameAction as DealDamageAction);
                                    var source = damage.DamageSource;
                                    Card target = damage.Target;
                                    int amount = damage.Amount;
                                    DamageType type = damage.DamageType;
                                    Card newTarget = (yesNo as YesNoCardDecision).Card;

                                    question = address + "Do you want to redirect " + amount + " " + type + " damage aimed at " + target.Title + " to " + newTarget.Title + "?";
                                    break;
                                }
                            case SelectionType.ReduceDamageTaken:
                            case SelectionType.ReduceNextDamageTaken:
                                {
                                    DealDamageAction damage = (yesNo.GameAction as DealDamageAction);
                                    Card target = damage.Target;
                                    question = address + "Do you want to reduce damage to " + target.Title + "?";
                                    break;
                                }
                            case SelectionType.RemoveCardFromGame:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (YesNoCardDecision)yesNo;
                                        question = address + "Do you want to remove " + yesNoCard.Card.Title + " from the game?";
                                    }
                                    break;
                                }
                            case SelectionType.RemoveTokens:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            question = address + "Do you want to remove " + yesNoAmount.Amount.Value + " tokens?";
                                        }
                                    }
                                    break;
                                }
                            case SelectionType.RemoveTokensAndRedirectDamage:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        var yesNoAmount = yesNo as YesNoAmountDecision;
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            if (yesNo.GameAction != null)
                                            {
                                                var damage = (yesNo.GameAction as DealDamageAction);
                                                Card target = damage.Target;
                                                int damageAmount = damage.Amount;
                                                DamageType type = damage.DamageType;
                                                question = address + "Do you want to remove " + yesNoAmount.Amount.Value + " tokens and redirect the " + damageAmount + " " + type + " damage aimed at " + target.Title + "?";
                                            }
                                            else
                                            {
                                                question = address + "Do you want to remove " + yesNoAmount.Amount.Value + " tokens and redirect damage?";
                                            }
                                        }
                                    }
                                    break;
                                }
                            case SelectionType.RemoveTokensToNoEffect:
                                {
                                    if (yesNo is YesNoAmountDecision)
                                    {
                                        YesNoAmountDecision yesNoAmount = yesNo as YesNoAmountDecision;
                                        if (yesNoAmount.Amount.HasValue)
                                        {
                                            question = address + "You do not have " + yesNoAmount.Amount.Value + " to remove. Remove as many as possible to no effect?";
                                        }
                                    }
                                    break;
                                }
                            case SelectionType.RevealCardsFromDeck:
                                {
                                    question = address + "Do you want to reveal cards from your deck?";
                                    break;
                                }
                            case SelectionType.RevealBottomCardOfDeck:
                                {
                                    question = address + "Do you want to reveal the bottom card of your deck?";
                                    break;
                                }
                            case SelectionType.RevealTopCardOfDeck:
                                {
                                    question = address + "Do you want to reveal the top card of your deck?";
                                    break;
                                }
                            case SelectionType.SelectTarget:
                                {
                                    question = address + "Do you want to select a target?";
                                    break;
                                }
                            case SelectionType.ShuffleCardIntoDeck:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = yesNo as YesNoCardDecision;
                                        question = address + "Do you want to shuffle " + yesNoCard.Card.Title + " into your deck?";
                                    }
                                    break;
                                }
                            case SelectionType.ShuffleDeck:
                                {
                                    question = address + "Do you want to shuffle your deck?";
                                    break;
                                }
                            case SelectionType.ShuffleEnvironmentDeck:
                                {
                                    question = address + "Do you want to shuffle the environment deck?";
                                    break;
                                }
                            case SelectionType.ShuffleTargetDeck:
                                {
                                    question = address + "Do you want to shuffle this deck?";
                                    break;
                                }
                            case SelectionType.ShuffleTrashIntoDeck:
                                {
                                    if (yesNo.HeroTurnTakerController == null && yesNo.RequireUnanimous)
                                    {
                                        question = address + "Do ALL players want to shuffle their trash into their deck?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to shuffle your trash into your deck?";
                                    }

                                    break;
                                }
                            case SelectionType.SkipTurn:
                                {
                                    question = address + "Do you want to skip the rest of your turn?";
                                    break;
                                }
                            case SelectionType.ShuffleVillainDeck:
                                {
                                    question = address + "Do you want to shuffle the villain deck?";
                                    break;
                                }
                            case SelectionType.SwitchBattleZone:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (yesNo as YesNoCardDecision);
                                        question = address + "Do you want to switch " + yesNoCard.Card.Title + "'s battle zone?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to switch battle zones?";
                                    }
                                    break;
                                }
                            case SelectionType.TakeFullTurnNow:
                                {
                                    question = address + "Do you want to take a full turn now?";
                                    break;
                                }
                            case SelectionType.UsePower:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (yesNo as YesNoCardDecision);
                                        question = address + "Do you want to use the power on " + yesNoCard.Card.Title + "?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to use a power now?";
                                    }
                                    break;
                                }
                            case SelectionType.UsePowerAgain:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (yesNo as YesNoCardDecision);
                                        question = address + "Do you want to use the power on " + yesNoCard.Card.Title + " again?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to use a power again now?";
                                    }
                                    break;
                                }
                            case SelectionType.UsePowerTwice:
                                {
                                    if (yesNo is YesNoCardDecision)
                                    {
                                        YesNoCardDecision yesNoCard = (yesNo as YesNoCardDecision);
                                        question = address + "Do you want to use the power on " + yesNoCard.Card.Title + " twice?";
                                    }
                                    else
                                    {
                                        question = address + "Do you want to use a power twice now?";
                                    }
                                    break;
                                }
                                // DO NOT ADD NEW SELECTION TYPES HERE. Put them in alphabetical order!
                        }

                        var choice = PrintAndSelectOption(question, new string[] { "Yes", "No" }, false, false, true);

                        if (choice.HasValue)
                        {
                            if (choice.Value == 1)
                            {
                                yesNo.Answer = true;
                            }
                            else
                            {
                                yesNo.Answer = false;
                            }
                        }
                    }
                    else if (decision is SelectCardDecision)
                    {
                        SelectCardDecision selectCard = (SelectCardDecision)decision;
                        if (selectCard.Choices.Count() > 0)
                        {
                            switch (selectCard.SelectionType)
                            {
                                case SelectionType.AmbiguousDecision:
                                    {
                                        if (!selectCard.SequenceIndex.HasValue)
                                        {
                                            sequence = "first";
                                        }

                                        question = address + "Which card should take effect " + sequence + "?";

                                        if (selectCard.SecondarySelectionType != null)
                                        {
                                            if (selectCard.SecondarySelectionType == SelectionType.RedirectDamage)
                                            {
                                                question = address + "Which redirect should take effect " + sequence + "?";
                                            }
                                            else if (selectCard.SecondarySelectionType == SelectionType.ModifyDamageAmount)
                                            {
                                                question = address + "Which damage amount modification should take effect " + sequence + "?";
                                            }
                                            else if (selectCard.SecondarySelectionType == SelectionType.DamageType)
                                            {
                                                question = address + "Which damage type modification should take effect " + sequence + "?";
                                            }
                                        }
                                        break;
                                    }
                                case SelectionType.ActivateFilter:
                                    {
                                        question = address + "Select " + sequence + " card with a {filter} ability to activate:";
                                        break;
                                    }
                                case SelectionType.CardFromHand:
                                    {
                                        question = address + "Select " + sequence + " card from your hand:";
                                        break;
                                    }
                                case SelectionType.CharacterCard:
                                    {
                                        question = address + "Select " + sequence + " hero:";
                                        break;
                                    }
                                case SelectionType.DealDamage:
                                    {
                                        question = address + "Select " + sequence + " card to deal damage:";
                                        break;
                                    }
                                case SelectionType.DealDamageAllMagicNumber:
                                    {
                                        question = address + "Select " + sequence + " card's magic number to deal damage to all targets:";
                                        break;
                                    }
                                case SelectionType.DealDamageSelf:
                                    {
                                        question = address + "Select " + sequence + " target to deal itself damage:";
                                        break;
                                    }
                                case SelectionType.DiscardCard:
                                    {
                                        question = address + "Select " + sequence + " card to discard:";
                                        break;
                                    }
                                case SelectionType.DiscardCardToRedirectToCardSource:
                                    {
                                        question = address + "Select " + sequence + " card to discard to redirect damage to " + selectCard.CardSource.Card.Title + ":";
                                        break;
                                    }
                                case SelectionType.DestroyCard:
                                    {
                                        question = address + "Select " + sequence + " card to destroy:";
                                        break;
                                    }
                                case SelectionType.FlipCardFaceDown:
                                    {
                                        question = address + "Select " + sequence + " card to flip face down:";
                                        break;
                                    }
                                case SelectionType.FlipCardFaceUp:
                                    {
                                        question = address + "Select " + sequence + " card to flip face up:";
                                        break;
                                    }
                                case SelectionType.GainHP:
                                    {
                                        question = address + "Select " + sequence + " target to gain HP:";
                                        break;
                                    }
                                case SelectionType.GainHPAndUsePower:
                                    {
                                        question = address + "Select " + sequence + " target to gain HP and use a power:";
                                        break;
                                    }
                                case SelectionType.HeroToDealDamage:
                                    {
                                        question = address + "Select " + sequence + " hero to deal damage:";
                                        break;
                                    }
                                case SelectionType.HighestHP:
                                case SelectionType.LowestHP:
                                    {
                                        string damageInfo = "";
                                        if (selectCard.GameAction != null && selectCard.GameAction is DealDamageAction)
                                        {
                                            // We have one instance of incoming damage...
                                            DealDamageAction dealDamage = (selectCard.GameAction as DealDamageAction);
                                            int amount = dealDamage.Amount;
                                            DamageType type = dealDamage.DamageType;
                                            if (dealDamage.DamageSource != null)
                                            {
                                                damageInfo = " (To be dealt " + amount + " " + type + " damage)";
                                            }
                                            else
                                            {
                                                damageInfo = " (To deal " + amount + " " + type + " damage)";
                                            }
                                        }
                                        else if (selectCard.DealDamageInfo != null)
                                        {
                                            // We have multiple instances of incoming damage...
                                            var output = new List<string>();
                                            foreach (var info in selectCard.DealDamageInfo)
                                            {
                                                int amount = info.Amount;
                                                DamageType type = info.DamageType;
                                                output.Add(amount + " " + type + " damage");
                                            }
                                            if (!selectCard.DealDamageInfo.All(dd => dd.DamageSource != null))
                                            {
                                                damageInfo = " (To deal " + output.ToCommaList(useWordAnd: true) + ")";
                                            }
                                            else
                                            {
                                                damageInfo = " (To be dealt " + output.ToCommaList(useWordAnd: true) + ")";
                                            }
                                        }

                                        var ordinal = "";

                                        if (selectCard.SelectionTypeOrdinal.HasValue && selectCard.SelectionTypeOrdinal.Value > 1)
                                        {
                                            ordinal = selectCard.SelectionTypeOrdinal.Value.ToOrdinalString() + " ";
                                        }

                                        if (selectCard.SelectionType == SelectionType.LowestHP)
                                        {
                                            question = address + "Which of these is considered to have the " + ordinal + "lowest HP?" + damageInfo;
                                        }
                                        else if (selectCard.SelectionType == SelectionType.HighestHP)
                                        {
                                            question = address + "Which of these is considered to have the " + ordinal + "highest HP?" + damageInfo;
                                        }
                                        break;
                                    }
                                case SelectionType.IncreaseGainHP:
                                    {
                                        question = address + "Select " + sequence + " card to increase their next HP gain:";
                                        break;
                                    }
                                case SelectionType.IncreaseDamage:
                                    {
                                        question = address + "Select " + sequence + " card to increase their next damage:";
                                        break;
                                    }
                                case SelectionType.MakeTarget:
                                    {
                                        question = address + "Select " + sequence + " target to enter play:";
                                        break;
                                    }
                                case SelectionType.MoveCard:
                                    {
                                        question = address + "Select " + sequence + " card to move:";
                                        break;
                                    }
                                case SelectionType.MoveCardBelowCard:
                                    {
                                        question = address + "Select " + sequence + " card to move under:";
                                        break;
                                    }
                                case SelectionType.MoveCardFromUnderCard:
                                    {
                                        question = address + "Select " + sequence + " card to move from under this card:";
                                        break;
                                    }
                                case SelectionType.MoveCardNextToCard:
                                    {
                                        question = address + "Select " + sequence + " card to move next to:";
                                        break;
                                    }
                                case SelectionType.MoveCardOnBottomOfDeck:
                                    {
                                        var ordinal = "very bottom";
                                        if (selectCard.SelectionTypeOrdinal.HasValue)
                                        {
                                            ordinal = selectCard.SelectionTypeOrdinal.Value.ToOrdinalString() + " from the bottom";
                                        }
                                        question = address + "Select " + sequence + " card to put on the " + ordinal + " of the deck:";
                                        break;
                                    }
                                case SelectionType.MoveCardOnDeck:
                                    {
                                        if (selectCard.SelectionTypeOrdinal.HasValue)
                                        {
                                            var offset = selectCard.SelectionTypeOrdinal.Value + 1;
                                            if (offset == 1)
                                            {
                                                question = address + "Select card to put on top of the deck.";
                                            }
                                            else
                                            {
                                                question = address + "Select card to put " + offset.ToOrdinalString() + " from the top of the deck.";
                                            }
                                        }
                                        else
                                        {
                                            question = address + "Select " + sequence + " card to put on top of the deck:";
                                        }
                                        break;
                                    }
                                case SelectionType.MoveCardToHand:
                                case SelectionType.MoveCardToHandFromTrash:
                                    {
                                        question = address + "Select " + sequence + " card to put in hand:";
                                        break;
                                    }
                                case SelectionType.MoveCardToPlayArea:
                                    {
                                        question = address + "Select " + sequence + " card to move:";
                                        break;
                                    }
                                case SelectionType.MoveCardToTrash:
                                    {
                                        question = address + "Select " + sequence + " card to move to the trash:";
                                        break;
                                    }
                                case SelectionType.MoveCardToUnderCard:
                                    {
                                        question = address + "Select " + sequence + " card to move under the other card:";
                                        break;
                                    }
                                case SelectionType.MoveCardUnderTopCardOfDeck:
                                    {
                                        question = address + "Select " + sequence + " card to move under the top card of its deck:";
                                        break;
                                    }
                                case SelectionType.PlayCard:
                                case SelectionType.PutIntoPlay:
                                    {
                                        string actionsLeft = null;
                                        if (selectCard.HeroTurnTakerController != null)
                                        {
                                            actionsLeft = " " + GetPhaseActionRemainder(selectCard.HeroTurnTakerController.TurnTaker, Phase.PlayCard, this.GameController.Game.ActiveTurnPhase);
                                        }
                                        if (selectCard.SelectionType == SelectionType.PutIntoPlay)
                                        {
                                            question = address + "Select " + sequence + " card to put into play:";
                                        }
                                        else
                                        {
                                            question = address + "Select " + sequence + " card to play:" + actionsLeft;
                                        }
                                        break;
                                    }
                                case SelectionType.RedirectDamage:
                                    {
                                        string damageInfoString = "";
                                        if (selectCard.GameAction != null)
                                        {
                                            DealDamageAction dealDamage = (selectCard.GameAction as DealDamageAction);
                                            int amount = dealDamage.Amount;
                                            DamageType type = dealDamage.DamageType;
                                            damageInfoString = amount + " " + type + " ";
                                        }

                                        question = address + "Select " + sequence + " target to redirect " + damageInfoString + "damage to:";

                                        break;
                                    }
                                case SelectionType.RedirectNextDamage:
                                    {
                                        question = address + "Select " + sequence + " target and redirect the next damage that they deal:";
                                        break;
                                    }
                                case SelectionType.RedirectDamageDirectedAtTarget:
                                    {
                                        question = address + "Select " + sequence + " target and redirect damage that would be dealt to them:";
                                        break;
                                    }
                                case SelectionType.ReduceDamageTaken:
                                    {
                                        question = address + "Select " + sequence + " card to reduce the damage they take:";
                                        break;
                                    }
                                case SelectionType.ReduceNextDamageTaken:
                                    {
                                        question = address + "Select " + sequence + " card to reduce the next damage they take:";
                                        break;
                                    }
                                case SelectionType.ReturnToHand:
                                    {
                                        question = address + "Select " + sequence + " card to return to hand:";
                                        break;
                                    }
                                case SelectionType.ReturnToDeck:
                                    {
                                        question = address + "Select " + sequence + " card to move to the top of its deck:";
                                        break;
                                    }
                                case SelectionType.SearchDeck:
                                    {
                                        question = address + "Select " + sequence + " card from the deck:";
                                        break;
                                    }
                                case SelectionType.SearchLocation:
                                    {
                                        question = address + "Select " + sequence + " card:";
                                        break;
                                    }
                                case SelectionType.SearchTrash:
                                    {
                                        question = address + "Select " + sequence + " card from the trash:";
                                        break;
                                    }
                                case SelectionType.SelectFunction:
                                    {
                                        question = address + " Select " + sequence + " who will choose what to do:";
                                        break;
                                    }
                                case SelectionType.SelectTarget:
                                    {
                                        question = address + "Select " + sequence + " target to be dealt damage:";
                                        break;
                                    }
                                case SelectionType.SelectTargetNoDamage:
                                case SelectionType.SelectTargetFriendly:
                                    {
                                        question = address + "Select " + sequence + " target:";
                                        break;
                                    }
                                case SelectionType.SetHP:
                                    {
                                        question = address + "Select " + sequence + " target to restore HP:";
                                        break;
                                    }
                                case SelectionType.ShuffleCardFromTrashIntoDeck:
                                    {
                                        question = address + "Select " + sequence + " card from the trash to shuffle into that trash's deck:";
                                        break;
                                    }
                                case SelectionType.SwitchSize:
                                    {
                                        question = address + "Select " + sequence + " size to switch to:";
                                        break;
                                    }
                                case SelectionType.UsePower:
                                case SelectionType.UsePowerOnCard:
                                    {
                                        question = address + "Select " + sequence + " a hero character card to use a power:";
                                        break;
                                    }
                                case SelectionType.UnincapacitateHero:
                                    {
                                        question = address + "Select " + sequence + " which hero character card to revive:";
                                        break;
                                    }
                                // DO NOT ADD NEW SELECTION TYPES HERE. Put them in alphabetical order!
                                default:
                                    {
                                        question = address + "Select " + sequence + " card [" + selectCard.SelectionType + "]:";
                                        break;
                                    }
                            }

                            var choice = PrintAndSelectOption(question, selectCard, selectCard.IsOptional, selectCard.AllowAutoDecide);

                            if (choice.HasValue)
                            {
                                if (choice == 0 && selectCard.IsOptional)
                                {
                                    selectCard.Skip();
                                }
                                else if (choice == -1 && selectCard.AllowAutoDecide)
                                {
                                    selectCard.AutoDecide();
                                }
                                else if (choice >= 1 && choice <= selectCard.Choices.Count())
                                {
                                    //Card card = selectCard.Choices.ElementAt(choice.Value - 1);
                                    selectCard.ChooseIndex(choice.Value - 1);
                                }
                            }
                        }
                        else
                        {
                            OutputToConsoleAndFileLine(address + "There are no valid cards to select.");
                            selectCard.Skip();
                        }
                    }
                    else if (decision is SelectCardsDecision)
                    {
                        SelectCardsDecision selectCardsDecision = (SelectCardsDecision)decision;
                        YesNoDecision yesNo = null;
                        if (selectCardsDecision.IsOptional)
                        {
                            if (selectCardsDecision.NumberOfCards.HasValue)
                            {
                                bool upTo = false;
                                if (selectCardsDecision.RequiredDecisions.HasValue && selectCardsDecision.RequiredDecisions.Value <= 0)
                                {
                                    upTo = true;
                                }
                                yesNo = new YesNoAmountDecision(this.GameController, selectCardsDecision.HeroTurnTakerController, selectCardsDecision.SelectionType, selectCardsDecision.NumberOfCards, upTo);
                            }
                            else
                            {
                                yesNo = new YesNoDecision(this.GameController, selectCardsDecision.HeroTurnTakerController, selectCardsDecision.SelectionType);
                            }
                            this.RunCoroutine(this.MakeDecision(yesNo));
                        }

                        // If selecting all cards at once, the view handles creating and storing the decisions.
                        if (selectCardsDecision.AllAtOnce)
                        {
                            // If a number is provided, select that many cards.
                            if (selectCardsDecision.NumberOfCards.HasValue)
                            {
                                bool finished = false;
                                for (int i = 0; i < selectCardsDecision.NumberOfCards.Value && !finished && !QuitRequested; i++)
                                {
                                    SelectCardDecision selectCardDecision = selectCardsDecision.GetNextSelectCardDecision();

                                    // If there are no choices, we're done.
                                    if (selectCardDecision.Choices.Count() == 0)
                                    {
                                        finished = true;
                                    }
                                    else
                                    {
                                        this.RunCoroutine(this.MakeDecision(selectCardDecision));
                                    }

                                    // If there was no card selected, we're done.
                                    if (selectCardDecision.SelectedCard == null)
                                    {
                                        finished = true;
                                    }
                                }
                            }
                        }

                        if (!selectCardsDecision.IsOptional || (selectCardsDecision.IsOptional && yesNo.Answer.Value == true))
                        {
                            selectCardsDecision.ReadyForNext = true;
                            done = true;
                        }
                        else
                        {
                            selectCardsDecision.Skip();
                        }
                    }
                    else if (decision is MoveCardDecision)
                    {
                        MoveCardDecision moveCardDecision = (MoveCardDecision)decision;
                        question = (String.Format(address + "Where would you like to move the card {0}?", moveCardDecision.CardToMove.Title));
                        var choice = PrintAndSelectOption(question, moveCardDecision.PossibleDestinations.Select(d => d.ToString()).ToArray(), false);
                        if (choice.HasValue)
                        {
                            moveCardDecision.Destination = moveCardDecision.PossibleDestinations.ElementAt(choice.Value - 1);
                        }
                    }
                    else if (decision is SelectLocationDecision)
                    {
                        SelectLocationDecision selectLocationDecision = (SelectLocationDecision)decision;
                        question = (address + "Select a location:");
                        if (selectLocationDecision.SelectionType == SelectionType.SearchLocation)
                        {
                            question = address + "Select a location to search:";
                        }
                        var choice = PrintAndSelectOption(question, selectLocationDecision.Choices.Select(d => d.ToString()).ToArray(), selectLocationDecision.IsOptional);
                        if (choice.HasValue)
                        {
                            if (choice.Value == 0)
                            {
                                selectLocationDecision.FinishedSelecting = true;
                            }
                            else
                            {
                                selectLocationDecision.SelectedLocation = selectLocationDecision.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is UsePowerDecision)
                    {
                        UsePowerDecision usePowerDecision = (UsePowerDecision)decision;
                        if (usePowerDecision.Choices.Count() > 0)
                        {
                            string remainder = "";
                            if (usePowerDecision.HeroTurnTakerController != null)
                            {
                                remainder = " " + GetPhaseActionRemainder(usePowerDecision.HeroTurnTakerController.TurnTaker, Phase.UsePower, GameController.Game.ActiveTurnPhase);
                            }
                            question = address + "Which power would you like to use?" + remainder;

                            var options = new string[usePowerDecision.Choices.Count()];

                            for (int i = 0; i < usePowerDecision.Choices.Count(); i++)
                            {
                                var power = usePowerDecision.Choices.ElementAt(i);
                                var via = "";
                                if (power.CardSource.Card != power.CardController.Card)
                                {
                                    via = " (via " + power.CardSource.Card.Title + ")";
                                }
                                options[i] = power.CardController.Card.Title + via + ": " + ReplaceTemplates(power.CardController.Card, power.Description);
                            }

                            string reprint = PrintOptions(question, options, usePowerDecision.IsOptional);
                            options = usePowerDecision.Choices.Select(power => power.CardController.Card.Title).ToArray();
                            var choice = SelectOption(question, options, usePowerDecision.IsOptional, reprint: reprint, decision: decision);

                            if (choice.HasValue)
                            {
                                if (choice >= 1 && choice <= usePowerDecision.Choices.Count())
                                {
                                    usePowerDecision.SelectedPower = usePowerDecision.Choices.ElementAt(choice.Value - 1);
                                }
                                else
                                {
                                    usePowerDecision.Skip();
                                }
                            }
                        }
                    }
                    else if (decision is UseIncapacitatedAbilityDecision)
                    {
                        UseIncapacitatedAbilityDecision useAbilityDecision = (UseIncapacitatedAbilityDecision)decision;
                        if (useAbilityDecision.Choices.Count() > 0)
                        {
                            question = (address + "Which ability would you like to use?");
                            var choice = PrintAndSelectOption(question, useAbilityDecision.Choices.Select(a => a.Description).ToArray(), true);
                            if (choice.HasValue)
                            {
                                if (choice >= 1 && choice <= useAbilityDecision.Choices.Count())
                                {
                                    useAbilityDecision.SelectedAbility = useAbilityDecision.Choices.ElementAt(choice.Value - 1);
                                }
                                else
                                {
                                    useAbilityDecision.Skip();
                                }
                            }
                        }
                        else
                        {
                            OutputToConsoleAndFileLine(address + "There are no abilities that can be used this turn.");
                        }
                    }
                    else if (decision is SelectDamageTypeDecision)
                    {
                        SelectDamageTypeDecision selectDamage = (SelectDamageTypeDecision)decision;

                        if (decision.SelectionType == SelectionType.FinalDamageType)
                        {
                            question = address + "Multiple cards affect the damage type. Select final damage type desired:";
                        }
                        else
                        {
                            question = (address + "Select a damage type:");
                        }

                        string[] options = new string[selectDamage.Choices.Count()];
                        for (int i = 0; i < selectDamage.Choices.Count(); i++)
                        {
                            options[i] = selectDamage.Choices.ElementAt(i).ToString();
                        }

                        var choice = PrintAndSelectOption(question, options, false);
                        if (choice.HasValue)
                        {
                            selectDamage.SelectedDamageType = selectDamage.Choices.ElementAt(choice.Value - 1);
                        }
                    }
                    else if (decision is SelectTurnTakerDecision)
                    {
                        SelectTurnTakerDecision selectTurnTaker = (SelectTurnTakerDecision)decision;
                        var cards = selectTurnTaker.NumberOfCards.HasValue && selectTurnTaker.NumberOfCards.Value > 1 ? "cards" : "a card";

                        switch (selectTurnTaker.SelectionType)
                        {
                            case SelectionType.CannotPlayCards:
                                {
                                    question = address + " Select which deck cannot play cards:";
                                    break;
                                }
                            case SelectionType.DealDamageAfterUsePower:
                                {
                                    question = address + "Select " + sequence + " player to deal damage after they use a power:";
                                    break;
                                }
                            case SelectionType.DealDamageSelf:
                                {
                                    question = address + " Select " + sequence + " hero to deal themself damage:";
                                    break;
                                }
                            case SelectionType.DestroyCard:
                                {
                                    question = address + " Select " + sequence + " hero to destroy a card:";
                                    break;
                                }
                            case SelectionType.DiscardCard:
                                {
                                    question = address + "Select " + sequence + " player to discard a card:";
                                    break;
                                }
                            case SelectionType.DiscardAndDrawCard:
                                {
                                    question = address + "Select " + sequence + " player to discard and draw a card:";
                                    break;
                                }
                            case SelectionType.DiscardCardToRedirectToCardSource:
                                {
                                    question = address + "Select " + sequence + " player to discard a card to redirect damage to " + selectTurnTaker.CardSource.Card.Title + ":";
                                    break;
                                }
                            case SelectionType.DiscardFromDeck:
                                {
                                    question = address + " Select " + sequence + " player to discard from their deck.";
                                    break;
                                }
                            case SelectionType.DiscardHand:
                                {
                                    question = address + "Select " + sequence + " player to discard their hand:";
                                    break;
                                }
                            case SelectionType.DrawCard:
                                {
                                    question = address + "Select " + sequence + " player to draw a card:";
                                    break;
                                }
                            case SelectionType.DrawACardAndUseAPower:
                                {
                                    question = address + "Select " + sequence + " player to draw a card and use a power:";
                                    break;
                                }
                            case SelectionType.DrawExtraCard:
                                {
                                    question = address + "Select " + sequence + " player to draw an extra card during their draw phase:";
                                    break;
                                }
                            case SelectionType.FewestCardsInPlay:
                                {
                                    question = address + "Which player is considered to have the fewest cards in play?:";
                                    break;
                                }
                            case SelectionType.FewestCardsInPlayArea:
                                {
                                    question = address + "Which player is considered to have the fewest cards in their play area?:";
                                    break;
                                }
                            case SelectionType.GiveHighFive:
                                {
                                    question = address + " Select " + sequence + " hero to give a high five to!";
                                    break;
                                }
                            case SelectionType.MostCardsInHand:
                                {
                                    question = address + "Which player is considered to have the most cards in hand?:";
                                    break;
                                }
                            case SelectionType.MostCardsInPlay:
                                {
                                    question = address + "Which player is considered to have the most cards in play?:";
                                    break;
                                }
                            case SelectionType.MostOngoingCardsInPlay:
                                {
                                    question = address + "Which player is considered to have the most ongoing cards in play?:";
                                    break;
                                }
                            case SelectionType.MostRewardsInPlay:
                                {
                                    question = address + "Which player is considered to have the most rewards in play?:";
                                    break;
                                }
                            case SelectionType.MoveCard:
                            case SelectionType.MoveCardToUnderCard:
                                {
                                    question = address + "Select " + sequence + " player to move " + cards + ":";
                                    break;
                                }
                            case SelectionType.MoveCardToHandFromBottomOfDeck:
                                {
                                    question = address + "Select " + sequence + " player to move " + cards + " from the bottom of their deck to their hand:";
                                    break;
                                }
                            case SelectionType.MoveCardFaceDownToYourPlayArea:
                                {
                                    question = address + " Select " + sequence + " hero to put a card face-down in their play area:";
                                    break;
                                }
                            case SelectionType.MoveCardFaceDownToVillainPlayArea:
                                {
                                    question = address + " Select " + sequence + " hero to put a card face-down in the villain play area:";
                                    break;
                                }
                            case SelectionType.MoveCardOnDeck:
                                {
                                    question = address + " Select " + sequence + " hero to move " + cards + " on their deck:";
                                    break;
                                }
                            case SelectionType.MoveCardToPlayArea:
                                {
                                    question = address + "Select " + sequence + " player's play area:";
                                    break;
                                }
                            case SelectionType.MoveCardUnderTopCardOfDeck:
                                {
                                    question = address + "Select " + sequence + " hero to move this card under the top card of their deck:";
                                    break;
                                }
                            case SelectionType.MoveTrashToDeck:
                                {
                                    question = address + " Select " + sequence + " villain to move their trash to the bottom of their deck.";
                                    break;
                                }
                            case SelectionType.MoveDeckToTrash:
                                {
                                    question = address + " Select " + sequence + " deck to move to its trash:";
                                    break;
                                }
                            case SelectionType.PlayCard:
                                {
                                    question = address + "Select " + sequence + " player to play a card:";
                                    break;
                                }
                            case SelectionType.PlayExtraCard:
                                {
                                    question = address + "Select " + sequence + " player to play an extra card during their play phase:";
                                    break;
                                }
                            case SelectionType.RemoveEnvironmentFromGame:
                                {
                                    question = address + "Select " + sequence + " environment to remove from the game:";
                                    break;
                                }
                            case SelectionType.RevealCardsFromDeck:
                                {
                                    question = address + "Select " + sequence + " player to reveal " + cards + ":";
                                    break;
                                }
                            case SelectionType.RevealTopCardOfDeck:
                                {
                                    question = address + "Select " + sequence + " player to reveal the top card:";
                                    break;
                                }
                            case SelectionType.RevealBottomCardOfDeck:
                                {
                                    question = address + "Select " + sequence + " player to reveal the bottom card:";
                                    break;
                                }
                            case SelectionType.SelectFunction:
                                {
                                    question = address + " Select " + sequence + " hero to choose what to do:";
                                    break;
                                }
                            case SelectionType.ShuffleTopCardOfDeckIntoVillainDeck:
                                {
                                    question = address + " Select " + sequence + " hero to shuffle their top card into the villain deck:";
                                    break;
                                }
                            case SelectionType.ShuffleTrashIntoDeck:
                                {
                                    question = address + " Select " + sequence + " deck to shuffle their trash into their deck:";
                                    break;
                                }
                            case SelectionType.UnincapacitateHero:
                                {
                                    question = address + " Select " + sequence + " hero to revive:";
                                    break;
                                }
                            case SelectionType.UsePower:
                                {
                                    question = address + "Select " + sequence + " player to use a power:";
                                    break;
                                }
                            // DO NOT ADD NEW SELECTION TYPES HERE. Put them in alphabetical order!
                            default:
                                {
                                    question = (address + "Select " + sequence + " player:");
                                    break;
                                }
                        }

                        string[] options = new string[selectTurnTaker.Choices.Count()];
                        for (int i = 0; i < selectTurnTaker.Choices.Count(); i++)
                        {
                            TurnTaker turnTaker = selectTurnTaker.Choices.ElementAt(i);
                            options[i] = turnTaker.Name;
                        }

                        var choice = PrintAndSelectOption(question, options, selectTurnTaker.IsOptional, selectTurnTaker.AllowAutoDecide);

                        if (choice.HasValue)
                        {
                            if (choice == 0 && selectTurnTaker.IsOptional)
                            {
                                selectTurnTaker.Skip();
                            }
                            else if (choice == -1 && selectTurnTaker.AllowAutoDecide)
                            {
                                selectTurnTaker.AutoDecide();
                            }
                            else if (choice >= 1 && choice <= selectTurnTaker.Choices.Count())
                            {
                                selectTurnTaker.SelectedTurnTaker = selectTurnTaker.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is SelectFunctionDecision)
                    {
                        SelectFunctionDecision selectFunctionDecision = (SelectFunctionDecision)decision;

                        question = (address + "What would you like to do?");

                        string[] options = new string[selectFunctionDecision.Choices.Count()];
                        for (int i = 0; i < selectFunctionDecision.Choices.Count(); i++)
                        {
                            Function function = selectFunctionDecision.Choices.ElementAt(i);
                            options[i] = function.DisplayText;
                        }

                        var choice = PrintAndSelectOption(question, options, selectFunctionDecision.IsOptional);
                        if (choice.HasValue)
                        {
                            if (choice == 0)
                            {
                                selectFunctionDecision.Skip();
                            }
                            else if (choice >= 1 && choice <= selectFunctionDecision.Choices.Count())
                            {
                                selectFunctionDecision.SelectedFunction = selectFunctionDecision.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is SelectNumberDecision)
                    {
                        SelectNumberDecision selectNumberDecision = (SelectNumberDecision)decision;

                        if (selectNumberDecision.SelectionType == SelectionType.DealDamage)
                        {
                            question = (address + "How much damage would you like to deal?");
                        }
                        else if (selectNumberDecision.SelectionType == SelectionType.RemoveTokens)
                        {
                            question = address + "How many tokens would you like to remove?";
                        }
                        else if (selectNumberDecision.SelectionType == SelectionType.DiscardFromDeck)
                        {
                            question = address + "How many cards would you like to discard from this deck?";
                        }
                        else
                        {
                            question = (address + "Pick " + sequence + " number:");
                        }

                        bool treatAsOptional = selectNumberDecision.Minimum == 0 || selectNumberDecision.IsOptional ? true : false;

                        string[] options = null;
                        if (selectNumberDecision.Minimum == 0)
                        {
                            options = new string[selectNumberDecision.Choices.Count() - 1];
                        }
                        else
                        {
                            options = new string[selectNumberDecision.Choices.Count()];
                        }

                        int arrayIndex = 0;
                        for (int i = 0; i < selectNumberDecision.Choices.Count(); i++)
                        {
                            if (selectNumberDecision.Choices.ElementAt(i) != 0)
                            {
                                options[arrayIndex] = "" + selectNumberDecision.Choices.ElementAt(i);
                                arrayIndex++;
                            }
                        }

                        var choice = PrintAndSelectOption(question, options, treatAsOptional);
                        if (choice.HasValue)
                        {
                            if (choice == 0 && selectNumberDecision.IsOptional)
                            {
                                selectNumberDecision.Skip();
                            }
                            else
                            {
                                int adjust = selectNumberDecision.Minimum == 0 ? 0 : -1;
                                selectNumberDecision.SelectedNumber = selectNumberDecision.Choices.ElementAt(choice.Value + adjust);
                            }
                        }
                    }
                    else if (decision is SelectWordDecision)
                    {
                        SelectWordDecision selectWord = (SelectWordDecision)decision;

                        if (selectWord.SelectionType == SelectionType.SelectKeyword)
                        {
                            question = (address + "Select a keyword:");
                        }
                        else if (selectWord.SelectionType == SelectionType.HarpyTokenType)
                        {
                            question = (address + "Select a token type to flip:");
                        }
                        else
                        {
                            question = (address + "Select a word:");
                        }

                        var choice = PrintAndSelectOption(question, selectWord.Choices, selectWord.IsOptional);
                        if (choice.HasValue)
                        {
                            if (choice.Value == 0)
                            {
                                selectWord.Skip();
                            }
                            else
                            {
                                selectWord.SelectedWord = selectWord.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is ActivateAbilityDecision)
                    {
                        ActivateAbilityDecision activate = decision as ActivateAbilityDecision;

                        question = address + "Which " + activate.AbilityKey + " text would you like to activate?";

                        string[] options = new string[activate.Choices.Count()];
                        for (int i = 0; i < activate.Choices.Count(); i++)
                        {
                            ActivatableAbility ability = activate.Choices.ElementAt(i);
                            options[i] = ability.CardController.Card.Title + ": " + ReplaceTemplates(ability.CardController.Card, ability.Description);
                        }

                        var choice = PrintAndSelectOption(question, options, activate.IsOptional, activate.AllowAutoDecide);
                        if (choice.HasValue)
                        {
                            if (choice == 0)
                            {
                                if (activate.IsOptional)
                                {
                                    activate.Skip();
                                }
                                else if (activate.AllowAutoDecide)
                                {
                                    activate.AutoDecide();
                                }
                            }
                            else if (choice >= 1 && choice <= activate.Choices.Count())
                            {
                                activate.SelectedAbility = activate.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is SelectTurnPhaseDecision)
                    {
                        SelectTurnPhaseDecision selectTurnPhase = (SelectTurnPhaseDecision)decision;
                        var phases = selectTurnPhase.Choices.Select(p => p.FriendlyPhaseName);

                        question = (address + "Which phase of this turn would like to occur now?");

                        var choice = PrintAndSelectOption(question, phases, selectTurnPhase.IsOptional);
                        if (choice.HasValue)
                        {
                            if (choice == 0)
                            {
                                selectTurnPhase.Skip();
                            }
                            else if (choice >= 1 && choice <= selectTurnPhase.Choices.Count())
                            {
                                selectTurnPhase.SelectedPhase = selectTurnPhase.Choices.ElementAt(choice.Value - 1);
                            }
                        }
                    }
                    else if (decision is SelectFromBoxDecision)
                    {
                        SelectFromBoxDecision selectFromBox = decision as SelectFromBoxDecision;
                        question = (address + "Select a hero from the box: ");
                        var availableHeroes = selectFromBox.Choices.Select(c => c.Key).Distinct();
                        var heroNames = availableHeroes.Select(s => DeckDefinitionCache.GetDeckDefinition(s).Name);

                        var turnTakerChoice = PrintAndSelectOption(question, heroNames, false, decision.AllowAutoDecide);
                        if (turnTakerChoice.HasValue)
                        {
                            string identifier = null;
                            if (turnTakerChoice <= availableHeroes.Count() && turnTakerChoice > 0)
                            {
                                identifier = availableHeroes.ElementAt(turnTakerChoice.Value - 1);
                            }
                            else
                            {
                                selectFromBox.AutoDecide();
                            }

                            if (!selectFromBox.AutoDecided)
                            {
                                selectFromBox.SelectedTurnTakerIdentifier = identifier;
                                var deckDef = DeckDefinitionCache.GetDeckDefinition(identifier);
                                var promoQuestion = (address + "Select a promo character card from the box: ");
                                var promoIdentifiers = selectFromBox.Choices.Where(c => c.Key == identifier).Select(c => c.Value);

                                var allCharacterDefs = deckDef.CardDefinitions.Where(cd => cd.IsCharacter).ToList();
                                allCharacterDefs.AddRange(deckDef.PromoCardDefinitions);
                                var promoTitles = allCharacterDefs.Where(cd => promoIdentifiers.Contains(cd.PromoIdentifierOrIdentifier)).Select(cd => cd.PromoTitleOrTitle);

                                var promoChoice = PrintAndSelectOption(promoQuestion, promoTitles, false, decision.AllowAutoDecide);
                                if (promoChoice.HasValue && promoChoice <= promoIdentifiers.Count() && promoChoice > 0)
                                {
                                    selectFromBox.SelectedIdentifier = promoIdentifiers.ElementAt(promoChoice.Value - 1);
                                }
                                else if (promoChoice.HasValue)
                                {
                                    selectFromBox.AutoDecide();
                                }
                            }
                        }
                    }
                    else
                    {
                        OutputToConsoleAndFileLine(address + "No view implemented for decision type. Add it to Program.MakeDecision!");
                        break;
                    }
                }
            }

            if (decision.HeroTurnTakerController != null && decision.HeroTurnTakerController.IsActiveTurnTakerController && this.GameController.ActiveTurnPhase.CanPerformAction)
            {
                var actionCount = this.GameController.Game.ActiveTurnPhase.GetPhaseActionCount();
                if (decision is SelectCardDecision)
                {
                    var select = decision as SelectCardDecision;
                    if (this.GameController.ActiveTurnPhase.IsPlayCard && (select.SelectionType == SelectionType.PlayCard || select.SelectionType == SelectionType.PutIntoPlay) && select.SelectedCard == null && actionCount.HasValue)
                    {
                        // If we pass on playing a card, assume no more cards are to be played this phase.                        
                        this.GameController.Game.ActiveTurnPhase.SkipPhase();
                    }
                }
                else if (decision is UsePowerDecision)
                {
                    var usePower = decision as UsePowerDecision;
                    if (this.GameController.Game.ActiveTurnPhase.IsUsePower && usePower.SelectedPower == null)
                    {
                        // If we pass on using a power, assume no more powers are to be used this phase.                        
                        this.GameController.Game.ActiveTurnPhase.SkipPhase();
                    }
                }
                else if (decision is YesNoDecision)
                {
                    var yesNo = decision as YesNoDecision;
                    if (this.GameController.Game.ActiveTurnPhase.IsDrawCard && yesNo.SelectionType == SelectionType.DrawCard && yesNo.Answer == false)
                    {
                        // If we pass on drawing a card, assume no more cards are to be drawn this phase.
                        this.GameController.Game.ActiveTurnPhase.SkipPhase();
                    }
                }
                else if (decision is UseIncapacitatedAbilityDecision)
                {
                    var useAbility = decision as UseIncapacitatedAbilityDecision;
                    if (this.GameController.Game.ActiveTurnPhase.IsUseIncapacitatedAbility && useAbility.SelectedAbility == null)
                    {
                        // If we pass on using an ability, assume no more abilities are to be used this phase.                        
                        this.GameController.Game.ActiveTurnPhase.SkipPhase();
                    }
                }
            }

            yield return 0;
        }

        private string PrintOptions(string question, IEnumerable<string> options, bool optional, bool autoDecide = false)
        {
            string result = "";
            result += Environment.NewLine;
            result += question + Environment.NewLine;

            for (int i = 0; i < options.Count(); i++)
            {
                result += ("\t" + (i + 1) + ": " + options.ElementAt(i)) + Environment.NewLine;
            }

            result += ("\t----------") + Environment.NewLine;

            if (autoDecide)
            {
                result += ("\tq 0: Choose for me") + Environment.NewLine;
            }

            if (optional)
            {
                result += ("\tw .: None") + Environment.NewLine;
            }

            if (this.EnforceRules)
            {
                result += ("\thelp: Show Help") + Environment.NewLine;
            }

            OutputToConsoleAndFileLine(result);
            return result;
        }

        protected int? PrintAndSelectOption(string question, SelectCardDecision decision, bool optional, bool allowAutoDecide = false, bool isYesNo = false)
        {
            if (decision.SelectionType == SelectionType.DestroyCard && decision.Choices.All(c => c.PlayIndex.HasValue))
            {
                decision.Choices = decision.Choices.OrderBy(c => c.Title).ThenBy(c => c.PlayIndex);
            }
            var choices = decision.Choices;
            int numberOfChoices = choices.Count();
            string[] options = new string[numberOfChoices];
            for (int i = 0; i < numberOfChoices; i++)
            {
                Card card = choices.ElementAt(i);
                var wasFlipped = card.IsFlipped;
                options[i] = GetCardListString(card, decision.HeroTurnTakerController, i + 1, decision);
            }

            string reprint = PrintOptions(question, options, optional, allowAutoDecide);
            options = decision.Choices.Select(d => d.Title).ToArray();
            return SelectOption(question, options, optional, allowAutoDecide, isYesNo, reprint, decision);
        }

        protected string GetCardListString(Card card, HeroTurnTakerController hero = null, int? index = null, SelectCardDecision decision = null)
        {
            if (!card.IsFaceDownNonCharacter)
            {
                string hp = card.HitPoints.HasValue && card.Location.IsInPlayAndNotUnderCard ? " (" + card.HitPoints.Value + " HP)" : "";
                string unplayable = "";
                string locationInfo = "";
                var extraInfo = "";
                string cannotDealDamage = (this.GameController.CanDealDamage(card, false) == null) ? "" : " (Cannot Deal Damage)";

                CardController cc = this.GameController.FindCardController(card);

                if (hero != null && card.Location == hero.HeroTurnTaker.Hand)
                {
                    unplayable = this.GameController.GetLimitedCardsInPlay(hero).Contains(card) ? " (Limited & In Play)" : "";
                    unplayable += cc.CanBePlayedNow ? "" : " (Cannot Be Played Now)";
                }
                if (card.Location.IsNextToCard)
                {
                    locationInfo = " (Next to " + card.Location.OwnerCard.Title + ")";
                }
                if (card.Location.IsUnderCard)
                {
                    locationInfo = " (Under " + card.Location.OwnerCard.Title + ")";
                }
                if (card.Owner.Identifier == "TheArgentAdept")
                {
                    extraInfo += card.IsMelody ? " (M)" : "";
                    extraInfo += card.IsRhythm ? " (R)" : "";
                    extraInfo += card.IsHarmony ? " (H)" : "";
                }

                extraInfo += card.MagicNumber.HasValue ? " [" + card.MagicNumber.Value + "]" : "";

                if (decision != null && index.HasValue)
                {
                    var associatedCard = decision.GetAssociatedCard(index.Value - 1);
                    if (associatedCard != null)
                    {
                        extraInfo += " (" + associatedCard.Title + ")";
                    }
                }

                var title = card.Title;

                if (card.IsLink)
                {
                    title += " (Link)";
                }

                title += card.Title == "Sky-Scraper" ? " (" + card.Definition.Keywords.First() + ")" : "";

                return title + hp + locationInfo + unplayable + cannotDealDamage + extraInfo;
            }
            else
            {
                return "Back of a " + card.Owner.Name + " card";
            }
        }

        protected int? PrintAndSelectOption(string question, IEnumerable<string> options, bool optional, bool allowAutoDecide = false, bool isYesNo = false)
        {
            PrintOptions(question, options, optional, allowAutoDecide);
            var choice = SelectOption(question, options, optional, allowAutoDecide, isYesNo);
            return choice;
        }

        protected int? SelectOption(string question, IEnumerable<string> options, bool optional, bool allowAutoDecide = false, bool isYesNo = false, string reprint = null, IDecision decision = null, TurnTaker toTurnTaker = null)
        {
            int? choice = null;
            OutputToConsoleAndFile("\t> ");
            bool inputHandled = false;
            string input = GetCleanInput();

            // See if it's a number we are interested in
            int number;
            bool isNumber = Int32.TryParse(input, out number);

            if (options != null)
            {
                if (isNumber)
                {
                    if (number == 0)
                    {
                        if (allowAutoDecide)
                        {
                            choice = -1;
                        }
                        else
                        {
                            // Let's skip
                            choice = 0;
                        }

                        inputHandled = true;
                    }
                    else if (number > options.Count() || number < 0)
                    {
                        OutputToConsoleAndFileLine("\"" + number + "\" is not a valid option number, please try again.");
                        inputHandled = true;
                    }
                    else if (number >= 1)
                    {
                        // Good number!
                        choice = number;
                        inputHandled = true;
                    }
                }
                else
                {
                    // See if it's a shortcut
                    if (allowAutoDecide && input == "q")
                    {
                        inputHandled = true;
                        choice = -1;
                    }
                    else if (optional && (input == "w" || input == "."))
                    {
                        inputHandled = true;
                        choice = 0;
                    }
                    else if (isYesNo)
                    {
                        if (input.ToLower() == "yes" || input.ToLower() == "y")
                        {
                            inputHandled = true;
                            choice = 1;
                        }
                        else if (input.ToLower() == "no" || input.ToLower() == "n")
                        {
                            inputHandled = true;
                            choice = 2;
                        }
                    }
                }
            }

            // Extra commands available when rules are enabled, so you can do things
            if (this.EnforceRules && !inputHandled)
            {
                if (input == "?" || input.ToLower() == "help" || input.ToLower() == "h")
                {
                    // Help
                    inputHandled = true;
                    PrintFriendlyMenu();
                }
                else if (input.ToLower().StartsWith("v"))
                {
                    // View turn taker or game
                    inputHandled = true;

                    var commandLength = 1;
                    if (input.ToLower().StartsWith("view"))
                    {
                        commandLength = 4;
                    }

                    if (input.Length > commandLength)
                    {
                        // Individual turn taker, see if it's a number or "me" first
                        string param = input.Substring(commandLength + 1);
                        int index = param.ToInt(-1);
                        if (index >= 1)
                        {
                            PrintFriendlyTurnTaker(index - 1);
                        }
                        else if (param == "me")
                        {
                            // Active turn taker
                            PrintFriendlyTurnTaker(this.GameController.ActiveTurnTaker);
                        }
                        else if (param == "e")
                        {
                            // Environment
                            PrintFriendlyTurnTaker(this.GameController.FindEnvironmentTurnTakerController().TurnTaker);
                        }
                        else if (param == "v")
                        {
                            // Villain
                            foreach (var villain in this.GameController.FindVillainTurnTakerControllers(false))
                            {
                                PrintFriendlyTurnTaker(villain.TurnTaker);
                            }
                        }
                        else if (param == "hp")
                        {
                            // View just targets and their HP, from highest to lowest.
                            PrintTargets();
                        }
                        else if (this.GameController.IsOblivAeonMode && param == "bz1")
                        {
                            // BattleZone One
                            foreach (var tt in this.GameController.FindTurnTakersWhere(tt => tt.BattleZone.Identifier == "BattleZoneOne"))
                            {
                                PrintFriendlyTurnTaker(tt);
                            }
                        }
                        else if (this.GameController.IsOblivAeonMode && param == "bz2")
                        {
                            foreach (var tt in this.GameController.FindTurnTakersWhere(tt => tt.BattleZone.Identifier == "BattleZoneTwo"))
                            {
                                PrintFriendlyTurnTaker(tt);
                            }
                        }
                        else
                        {
                            PrintFriendlyTurnTaker(param);
                        }
                    }
                    else
                    {
                        // Whole game
                        PrintFriendlyGame();
                    }
                }
                else if (input.ToLower().StartsWith("r"))
                {
                    // Read cards
                    inputHandled = true;

                    ReadCommand(input, options);
                }
                else if (input.ToLower().StartsWith("i"))
                {
                    inputHandled = true;

                    var commandLength = 1;
                    if (input.ToLower().StartsWith("info"))
                    {
                        commandLength = 4;
                    }

                    if (input.Length > commandLength)
                    {
                        // Print the Special Strings of a card.
                        string param = input.Substring(commandLength + 1);
                        int infoIndex = param.ToInt(-1);
                        if (infoIndex >= 1 && infoIndex <= options.Count())
                        {
                            PrintSpecialStringsForCard(options.ElementAt(infoIndex - 1));
                        }
                        else
                        {
                            PrintSpecialStringsForCard(param);
                        }
                    }
                    else
                    {
                        // Read all cards in the options
                        // Keep track of which instance of the card we want to look at in case there are two different ones.
                        var instances = new List<string>();
                        for (int i = 0; i < options.Count(); i++)
                        {
                            var card = options.ElementAt(i);
                            PrintSpecialStringsForCard(card, i + 1);
                            instances.Add(card);
                        }
                    }
                }
                else if (input.ToLower().StartsWith("l"))
                {
                    // Load game
                    inputHandled = true;

                    var commandLength = 1;
                    if (input.ToLower().StartsWith("load"))
                    {
                        commandLength = 4;
                    }

                    if (input.Length > commandLength)
                    {
                        string name = input.Substring(commandLength + 1);
                        var newName = String.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                        string fileName = "save-" + newName + ".dat";

                        if (File.Exists(fileName))
                        {
                            // Throw an exception so we can get out and load the game. Not the best flow control but it'll do for this.
                            throw new LoadGameException(newName);
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("Could not find a saved checkpoint named " + newName);
                        }
                    }
                    else
                    {
                        PrintSavedCheckpoints();
                        OutputToConsoleAndFileLine("To load a checkpoint, enter the name on the same line after the 'load' command.");
                    }
                }
                else if (input.ToLower().StartsWith("s"))
                {
                    // Save game
                    inputHandled = true;

                    var commandLength = 1;
                    if (input.ToLower().StartsWith("save"))
                    {
                        commandLength = 4;
                    }

                    if (input.Length > commandLength)
                    {
                        string name = input.Substring(commandLength + 1);
                        var newName = String.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

                        // Copy the continue.dat to the new name instead of saving directly, because we cannot safely save at any point.
                        string fileName = "save-" + newName + ".dat";
                        try
                        {
                            File.Copy("continue.dat", fileName, true);

                            if (File.Exists(fileName))
                            {
                                OutputToConsoleAndFileLine("Saved checkpoint named " + newName);
                            }
                        }
                        catch (Exception ex)
                        {
                            OutputToConsoleAndFileLine("Error saving checkpoint: " + ex);
                        }
                    }
                    else
                    {
                        OutputToConsoleAndFileLine("Please enter a checkpoint name on the same line after the 'save' command.");
                    }
                }
                else if (input.ToLower().StartsWith("delete"))
                {
                    // Delete saved game
                    inputHandled = true;

                    var commandLength = 6;

                    if (input.Length > commandLength)
                    {
                        string name = input.Substring(commandLength + 1);
                        var newName = String.Join("_", name.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                        string fileName = "save-" + newName + ".dat";

                        if (File.Exists(fileName))
                        {
                            // Delete the file
                            File.Delete(fileName);
                            OutputToConsoleAndFileLine("Saved checkpoint " + newName + " was deleted.");
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("Could not find a saved checkpoint named " + newName);
                        }
                    }
                    else
                    {
                        PrintSavedCheckpoints();
                        OutputToConsoleAndFileLine("To delete a checkpoint, enter the name on the same line after the 'delete' command.");
                    }
                }
                else if (input.ToLower().StartsWith("j"))
                {
                    // Show journal
                    inputHandled = true;

                    var commandLength = 1;
                    if (input.ToLower().StartsWith("journal"))
                    {
                        commandLength = 7;
                    }

                    int numRounds = 0;
                    if (input.Length > commandLength)
                    {
                        // Number of rounds
                        string param = input.Substring(commandLength + 1);
                        int index = param.ToInt(-1);
                        if (index >= 1)
                        {
                            numRounds = index;
                        }
                    }

                    int minRound = Math.Max(0, this.GameController.Game.Round - numRounds);
                    var entries = this.GameController.Game.Journal.Entries.Where(e => e.Round >= minRound);
                    OutputToConsoleAndFileLine("---- Journal entries since Round " + minRound + " ----");
                    OutputToConsoleAndFileLine(entries.ToRecursiveString(Environment.NewLine));
                }
                else if (input.ToLower().StartsWith("x"))
                {
                    // Print out stats
                    inputHandled = true;

                    PrintStats();
                }
                else if (input.ToLower().StartsWith("cheat "))
                {
                    input = input.ToLower().Replace("cheat ", "");
                    inputHandled = true;

                    if (input.ToLower().StartsWith("grab"))
                    {
                        // Grab a card from the hero's deck or trash and put it into their hand.
                        var tt = this.GameController.ActiveTurnTaker;
                        if (toTurnTaker != null)
                        {
                            tt = toTurnTaker;
                        }
                        var selected = false;
                        if (tt.IsHero)
                        {
                            while (!selected)
                            {
                                var cards = tt.Deck.Cards.Union(tt.Trash.Cards);
                                var cardOptions = cards.OrderBy(c => c.Identifier).Select(c => FriendlyCardString(c, false)).Distinct();
                                PrintOptions("Which card do you want to put in " + tt.Name + "'s hand?", cardOptions, true);
                                OutputToConsoleAndFile("\t> ");
                                var grabInput = GetCleanInput();
                                if (grabInput.StartsWith("r"))
                                {
                                    ReadCommand(grabInput, cardOptions);
                                }
                                else if (IsSkipInput(grabInput))
                                {
                                    // None
                                    return null;
                                }
                                else if (Int32.TryParse(grabInput, out number))
                                {
                                    number--;
                                    if (number >= 0 && number < cardOptions.Count())
                                    {
                                        var card = cards.Where(c => FriendlyCardString(c, false).Equals(cardOptions.ElementAt(number))).FirstOrDefault();
                                        if (card != null)
                                        {
                                            PutInHand(card);
                                            OutputToConsoleAndFileLine(card.Title + " was moved to " + tt.Name + "'s hand.");
                                            selected = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("Please use the 'grab' command during a hero turn.");
                        }
                    }
                    else if (input.ToLower().StartsWith("stops"))
                    {
                        CheatStops = !CheatStops;
                        SaveSpeedAndCheatPrefs();
                        OutputToConsoleAndFileLine("Cheat stops now set to: " + CheatStops);
                    }
                    else if (input.ToLower().StartsWith("count"))
                    {
                        var phase = this.GameController.ActiveTurnPhase;
                        OutputToConsoleAndFileLine("Set " + phase.FriendlyPhaseName + " to what count? (Min: 1, 0 to cancel)");
                        OutputToConsoleAndFile("\t> ");
                        var stackInput = GetCleanInput();
                        int count = stackInput.ToInt();
                        if (count > 0)
                        {
                            phase.PhaseActionCountRoot = count;
                            phase.PhaseActionCountUsed = 0;
                            phase.PhaseActionCountModifiers = null;
                        }
                    }
                    else if (input.ToLower().StartsWith("tokens"))
                    {
                        var cardsWithTokenPools = this.GameController.FindCardsWhere(c => c.IsInPlayAndHasGameText && c.TokenPools.Count() > 0);
                        TokenPool poolToSet = null;
                        if (cardsWithTokenPools.Count() == 0)
                        {
                            OutputToConsoleAndFileLine("There are no cards with tokens pools in this game.");
                        }
                        else if (cardsWithTokenPools.Count() == 1 && cardsWithTokenPools.First().TokenPools.Count() == 1)
                        {
                            poolToSet = cardsWithTokenPools.First().TokenPools.First();
                        }
                        else
                        {
                            bool selected = false;
                            while (!selected)
                            {
                                var pools = cardsWithTokenPools.SelectMany(c => c.TokenPools);
                                var poolNames = pools.Select(tp => tp.Name);
                                var whichOne = PrintAndSelectOption("Which token pool would you like to set?", poolNames, true) - 1;

                                if (whichOne.HasValue)
                                {
                                    if (whichOne >= 0 && whichOne < pools.Count())
                                    {
                                        poolToSet = pools.ElementAt(whichOne.Value);
                                        selected = true;
                                    }
                                    else if (whichOne == -1)
                                    {
                                        selected = true;
                                    }
                                }
                                else
                                {
                                    OutputToConsoleAndFileLine("Invalid input.");
                                }
                            }
                        }

                        if (poolToSet != null)
                        {
                            OutputToConsoleAndFileLine("Set " + poolToSet.Name + " to what count? (Min: 0, non-number to cancel)");
                            OutputToConsoleAndFile("\t> ");
                            var stackInput = GetCleanInput();
                            int count = stackInput.ToInt(-1);
                            if (count >= 0)
                            {
                                poolToSet.SetNumberOfTokens(count);
                                OutputToConsoleAndFileLine(poolToSet.Name + "'s token count has been set to " + poolToSet.CurrentValue + ".");
                            }
                        }
                    }
                    else if (input.ToLower().StartsWith("stack"))
                    {
                        bool toBottom = false;

                        if (input.ToLower().EndsWith("bottom"))
                        {
                            toBottom = true;
                        }

                        // Stack which card goes on top of a deck.
                        var selected = false;

                        while (!selected)
                        {
                            var tts = this.GameController.TurnTakerControllers;
                            var locations = tts.Select(ttc => ttc.TurnTaker.Deck).ToList();
                            locations.AddRange(tts.SelectMany(tt => tt.TurnTaker.SubDecks));
                            var locationNames = locations.Where(l => !l.IsSubDeck).Select(l => l.OwnerTurnTaker.Name).ToList();
                            locationNames.AddRange(locations.Where(l => l.IsSubDeck).Select(l => l.SubDeckName));
                            var whoIndex = PrintAndSelectOption("Which deck do you want to stack?", locationNames, true, false, false) - 1;

                            if (whoIndex >= 0 && whoIndex < locations.Count())
                            {
                                Location location = locations.ElementAt(whoIndex.Value);

                                while (!selected)
                                {
                                    var cards = location.Cards;
                                    var tt = location.OwnerTurnTaker;
                                    if (!location.IsSubDeck)
                                    {
                                        cards = cards.Union(tt.Trash.Cards);
                                    }
                                    var cardOptions = cards.OrderBy(c => c.Identifier).Select(c => FriendlyCardString(c, false)).Distinct();
                                    var topBottom = toBottom ? "bottom" : "top";
                                    PrintOptions("Which card do you want to put on " + topBottom + " of " + location.GetFriendlyName() + "?", cardOptions, true);
                                    OutputToConsoleAndFile("\t> ");
                                    var stackInput = GetCleanInput();
                                    if (stackInput == "w")
                                    {
                                        // None
                                        return null;
                                    }
                                    else if (stackInput.StartsWith("r"))
                                    {
                                        ReadCommand(stackInput, cardOptions);
                                    }
                                    else if (int.TryParse(stackInput, out number))
                                    {
                                        number--;
                                        if (number >= 0 && number < cardOptions.Count())
                                        {
                                            var card = cards.Where(c => FriendlyCardString(c, false).Equals(cardOptions.ElementAt(number))).FirstOrDefault();
                                            if (card != null)
                                            {
                                                tt.MoveCard(card, location, toBottom);
                                                OutputToConsoleAndFileLine(card.Title + " was moved to the " + topBottom + " of " + location.GetFriendlyName() + ".");
                                                selected = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whoIndex == -1)
                            {
                                // No one
                                return null;
                            }
                        }

                        return null;
                    }
                    else if (input.ToLower().StartsWith("play"))
                    {
                        // Stack which card goes on top of a deck.
                        var done = false;

                        while (!done)
                        {
                            var tts = this.GameController.TurnTakerControllers;
                            var ttsNames = tts.Select(ttc => ttc.Name);
                            var whoIndex = PrintAndSelectOption("Who would you like to play a card for?", ttsNames, true, false, false) - 1;

                            if (whoIndex >= 0 && whoIndex < ttsNames.Count())
                            {
                                var tt = tts.ElementAt(whoIndex.Value).TurnTaker;

                                while (!done)
                                {
                                    var cards = tt.GetAllCards().Where(c => !c.IsInPlayAndNotUnderCard);
                                    var cardOptions = cards.OrderBy(c => c.Identifier).Select(c => FriendlyCardString(c, false)).Distinct();
                                    PrintOptions("Which card do you want to play?", cardOptions, true);
                                    OutputToConsoleAndFile("\t> ");
                                    var stackInput = GetCleanInput();
                                    if (stackInput == "w")
                                    {
                                        // None
                                        return null;
                                    }
                                    else if (stackInput.StartsWith("r"))
                                    {
                                        ReadCommand(stackInput, cardOptions);
                                    }
                                    else if (int.TryParse(stackInput, out number))
                                    {
                                        number--;
                                        if (number >= 0 && number < cardOptions.Count())
                                        {
                                            var ttc = this.GameController.FindTurnTakerController(tt);
                                            var card = cards.Where(c => c.IsInHand && FriendlyCardString(c, false).Equals(cardOptions.ElementAt(number))).FirstOrDefault();
                                            if (card != null)
                                            {
                                                RunCoroutine(this.GameController.PlayCard(ttc, card, true));
                                                done = true;
                                            }
                                            else
                                            {
                                                card = cards.Where(c => c.IsInDeck && FriendlyCardString(c, false).Equals(cardOptions.ElementAt(number))).FirstOrDefault();
                                                if (card != null)
                                                {
                                                    RunCoroutine(this.GameController.PlayCard(ttc, card, true));
                                                    done = true;
                                                }
                                                else
                                                {
                                                    card = cards.Where(c => FriendlyCardString(c, false).Equals(cardOptions.ElementAt(number))).FirstOrDefault();
                                                    if (card != null)
                                                    {
                                                        RunCoroutine(this.GameController.PlayCard(ttc, card, true));
                                                    }
                                                    else
                                                    {
                                                        Log.Warning("Couldn't find a copy of " + FriendlyCardString(card) + " that wasn't already in play!");
                                                    }

                                                    done = true;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (whoIndex == -1)
                            {
                                // No one
                                return null;
                            }
                        }

                        return null;
                    }
                    else if (input.ToLower().StartsWith("hp"))
                    {
                        // Set the HP of a target.
                        var selected = false;

                        while (!selected)
                        {
                            var targets = this.GameController.FindTargetsInPlay();
                            var targetNames = new List<string>();
                            targets.ForEach(t => targetNames.Add(t.Title + " (" + t.HitPoints.Value + " HP)"));
                            PrintOptions("Set the HP of which target?", targetNames, true);
                            var hpInput = GetCleanInput();

                            if (hpInput.StartsWith("r"))
                            {
                                ReadCommand(hpInput, targets.Select(t => t.Title));
                            }
                            else if (IsSkipInput(hpInput))
                            {
                                // None
                                return null;
                            }
                            if (Int32.TryParse(hpInput, out number) && number >= 1 && number <= targets.Count())
                            {
                                var target = targets.ElementAt(number - 1);
                                OutputToConsoleAndFileLine("Set " + target.Title + "'s HP to what number? (Min: 1, Max: " + target.MaximumHitPoints.Value + ", 0 to cancel)");
                                OutputToConsoleAndFile("\t> ");
                                var stackInput = GetCleanInput();
                                int hp = stackInput.ToInt();
                                if (hp > 0)
                                {
                                    if (hp > target.MaximumHitPoints.Value)
                                    {
                                        OutputToConsoleAndFileLine(hp + " is too high! Setting to max HP of " + target.MaximumHitPoints.Value + ".");
                                        hp = target.MaximumHitPoints.Value;
                                    }
                                    target.SetHitPoints(hp);
                                    OutputToConsoleAndFileLine(target.Title + "'s HP set to " + hp + ".");
                                    selected = true;
                                }
                            }
                        }

                        return null;
                    }
                    else if (input.ToLower().StartsWith("destroy"))
                    {
                        // Destroy a card in play.
                        var selected = false;

                        while (!selected)
                        {
                            var cards = this.GameController.FindCardControllersWhere(c => c.IsInPlay).OrderBy(cc => cc.Card.Title);
                            var cardNames = cards.Select(cc => cc.Card.Title);
                            PrintOptions("Destroy which card in play?", cardNames, true);
                            var destroyInput = GetCleanInput();

                            if (destroyInput.StartsWith("r"))
                            {
                                ReadCommand(destroyInput, cardNames);
                            }
                            else if (destroyInput == "w")
                            {
                                // None
                                return null;
                            }
                            else if (Int32.TryParse(destroyInput, out number) && number >= 1 && number <= cardNames.Count())
                            {
                                var card = cards.ElementAt(number - 1);
                                RunCoroutine(this.GameController.DestroyCard(null, card.Card));
                                selected = true;
                            }
                        }

                        return null;
                    }
                    else if (input.ToLower().StartsWith("win"))
                    {
                        var c = PrintAndSelectOption("Do you really want to win the game?", new string[] { "Yes", "No" }, false, false, true);

                        if (c.HasValue && c.Value == 1)
                        {
                            CheatEnding = EndingResult.AlternateVictory;
                            RunCoroutine(this.GameController.GameOver(CheatEnding.Value, "The heroes cheated and won the game!"));
                            return null;
                        }
                    }
                    else if (input.ToLower().StartsWith("lose"))
                    {
                        var c = PrintAndSelectOption("Do you really want to lose the game?", new string[] { "Yes", "No" }, false, false, true);

                        if (c.HasValue && c.Value == 1)
                        {
                            CheatEnding = EndingResult.AlternateDefeat;
                            RunCoroutine(this.GameController.GameOver(CheatEnding.Value, "The heroes cheated and lost the game!"));
                            _ignoreException = true;
                            return null;
                        }
                    }
                    else if (input.ToLower().StartsWith("switch"))
                    {
                        if (this.GameController.IsTurnTakerInGame("SkyScraper"))
                        {
                            var cards = this.GameController.FindCardsWhere(c => c.Owner.Identifier == "SkyScraper" && c.IsCharacter && (c.Location.IsOffToTheSide || c.Location.IsOutOfGame));
                            var cardOptions = cards.Select(c => FriendlyCardString(c, false));
                            PrintOptions("Which character card would you like to switch to?", cardOptions, true);
                            OutputToConsoleAndFile("\t> ");
                            var stackInput = GetCleanInput();
                            if (stackInput == "w")
                            {
                                // None
                                return null;
                            }
                            else if (stackInput.StartsWith("r"))
                            {
                                ReadCommand(stackInput, cardOptions);
                            }
                            else if (int.TryParse(stackInput, out number))
                            {
                                var current = this.GameController.FindTurnTakerController("SkyScraper").CharacterCard;
                                var other = cards.ElementAt(number - 1);
                                RunCoroutine(this.GameController.SwitchCards(current, other));
                            }
                        }
                        else
                        {
                            OutputToConsoleAndFileLine("Only Sky-Scraper can use the 'cheat switch' command.");
                        }
                    }
                }
                else if (input.ToLower().StartsWith("cleardata"))
                {
                    // Clear user data
                    if (File.Exists(UserDataFileName))
                    {
                        File.Delete(UserDataFileName);
                    }

                    OutputToConsoleAndFileLine("All stored user data has been cleared.");
                }
                else if (input.ToLower().StartsWith("quit"))
                {
                    var c = PrintAndSelectOption("Do you really want to quit the game?", new string[] { "Yes", "No" }, false, false, true);

                    if (c.HasValue && c.Value == 1)
                    {
                        QuitRequested = true;
                        this.GameController.IsRealGameOver = true;
                        return null;
                    }
                }
            }


            if (input == "vp")
            {
                inputHandled = true;
                // View the percentages of the next card to be played in the villain deck.
                foreach (var villain in this.GameController.FindVillainTurnTakerControllers(false))
                {
                    var percentages = this.GameController.GetCardPercentages(card => card.Location == villain.TurnTaker.Deck);
                    var ordered = percentages.Keys.OrderByDescending(card => percentages[card]);
                    foreach (Card card in ordered)
                    {
                        OutputToConsoleAndFileLine("(" + percentages[card] + "%) " + card.Title);
                    }
                }
            }
            else if (input == "ep")
            {
                inputHandled = true;
                // View the percentages of the next card to be played in the environment deck.
                var percentages = this.GameController.GetCardPercentages(card => card.Location == this.GameController.FindEnvironmentTurnTakerController().TurnTaker.Deck);
                var ordered = percentages.Keys.OrderByDescending(card => percentages[card]);
                foreach (Card card in ordered)
                {
                    OutputToConsoleAndFileLine("(" + percentages[card] + "%) " + card.Title);
                }
            }

            return choice;
        }

        private void PrintStats(TurnTakerController ttc)
        {
            var journal = this.GameController.Game.Journal;

            if (ttc.CharacterCards != null && ttc.CharacterCards.Count() > 0 && (ttc.IsHero || ttc.IsVillain || ttc.IsVillainTeam))
            {
                foreach (var cc in ttc.CharacterCards)
                {
                    var sourceDamages = journal.QueryJournalEntries<DealDamageJournalEntry>(d => d.SourceCard != null && d.SourceCard == cc && d.Amount > 0);
                    var targetDamages = journal.QueryJournalEntries<DealDamageJournalEntry>(d => d.TargetCard == cc && d.Amount > 0);
                    var hpGains = journal.QueryJournalEntries<GainHPJournalEntry>(d => d.TargetCard == cc);
                    var hp = cc.IsInPlay ? cc.HitPoints : 0;
                    if (!cc.IsInPlay || (ttc.IsHero && cc.IsFlipped))
                    {
                        hp = 0;
                    }

                    OutputToConsoleAndFileLine(cc.Title);
                    OutputToConsoleAndFileLine("\tDamage Dealt by Character: " + sourceDamages.Sum(d => d.Amount) + " HP");
                    OutputToConsoleAndFileLine("\tDamage Taken by Character: " + targetDamages.Sum(d => d.Amount) + " HP");
                    OutputToConsoleAndFileLine("\tHP Gained by Character:    " + hpGains.Sum(d => d.Amount) + " HP");
                    OutputToConsoleAndFileLine("\tFinal HP of Character:     " + hp + " HP");
                }
            }
            else
            {
                OutputToConsoleAndFileLine(ttc.Name);
                var damage = journal.QueryJournalEntries<DealDamageJournalEntry>(d => d.SourceTurnTaker != null && d.SourceTurnTaker == ttc.TurnTaker && d.Amount > 0).Select(d => d.Amount).Sum();
                if (damage > 0)
                {
                    OutputToConsoleAndFileLine("\tDamage Dealt: " + damage + " HP");
                }
            }

            var minionSourceDamages = journal.QueryJournalEntries<DealDamageJournalEntry>(d => d.SourceCard != null && d.SourceCard.Owner == ttc.TurnTaker && !ttc.CharacterCards.Contains(d.SourceCard) && d.Amount > 0 && d.SourceCard.IsTarget);
            var minionTargetDamages = journal.QueryJournalEntries<DealDamageJournalEntry>(d => d.SourceCard != null && d.TargetCard.Owner == ttc.TurnTaker && !ttc.CharacterCards.Contains(d.TargetCard) && d.Amount > 0 && d.SourceCard.IsTarget);
            var minionHpGains = journal.QueryJournalEntries<GainHPJournalEntry>(d => d.SourceCard != null && d.TargetCard.Owner == ttc.TurnTaker && !ttc.CharacterCards.Contains(d.TargetCard) && d.SourceCard.IsTarget);

            if (minionSourceDamages.Count() > 0)
            {
                OutputToConsoleAndFileLine("\tDamage Dealt by Targets:   " + minionSourceDamages.Sum(d => d.Amount) + " HP");
            }

            if (minionTargetDamages.Count() > 0)
            {
                OutputToConsoleAndFileLine("\tDamage Taken by Targets:   " + minionTargetDamages.Sum(d => d.Amount) + " HP");
            }

            if (minionHpGains.Count() > 0)
            {
                OutputToConsoleAndFileLine("\tHP Gained by Targets:      " + minionHpGains.Sum(d => d.Amount) + " HP");
            }

            var cardsPlayed = journal.QueryJournalEntries<PlayCardJournalEntry>(p => p.CardPlayed.Owner == ttc.TurnTaker);
            OutputToConsoleAndFileLine("\tPlayed:                    " + cardsPlayed.Count() + " cards");

            if (ttc.IsHero)
            {
                var powersUsed = journal.QueryJournalEntries<UsePowerJournalEntry>(u => u.CardWithPower.Owner == ttc.TurnTaker);
                var cardsDrawn = journal.QueryJournalEntries<DrawCardJournalEntry>(d => d.Hero == ttc.TurnTaker);

                OutputToConsoleAndFileLine("\tUsed:                      " + powersUsed.Count() + " powers");
                OutputToConsoleAndFileLine("\tDrew:                      " + cardsDrawn.Count() + " cards");
            }
        }

        private void PrintStats()
        {
            OutputToConsoleAndFileLine("VITAL STATISTICS:" + Environment.NewLine);

            OutputToConsoleAndFileLine("Number of Rounds Completed: " + (this.GameController.Game.Round - 1) + Environment.NewLine);

            foreach (var ttc in this.GameController.TurnTakerControllers)
            {
                PrintStats(ttc);
                OutputToConsoleAndFileLine("");
            }
        }

        private void PrintSavedCheckpoints()
        {
            OutputToConsoleAndFileLine("Available saved checkpoints:");
            var files = Directory.GetFiles(".", "save-*.dat");
            foreach (var file in files)
            {
                string userFilename = file.Substring(7).Substring(0, file.Length - 11);
                OutputToConsoleAndFileLine("- " + userFilename);
            }
        }

        private string ExtendCardTextToWindow(string text)
        {
            var width = Console.WindowWidth;
            text = "| " + text;
            if (width > text.Length + 2)
            {
                text += new string(' ', width - text.Length - 1) + "|";
            }

            return text;
        }

        private void PrintCard(Card card, bool showHP = true, bool overrideFlipped = false, bool showSpecialStrings = false)
        {
            if (card != _lastCardPrinted)
            {
                var width = Console.WindowWidth;
                string borderLine = "+" + new string('-', width - 2) + "+";
                OutputToConsoleAndFileLine(borderLine);
                string flippedText = (card.IsFlipped && !card.IsTrap ? " (Flipped)" : "");
                string hpText = (showHP && card.IsTarget ? " (" + (card.Definition.HitPointsText ?? card.MaximumHitPoints.ToString()) + " HP)" : "");

                var title = ExtendCardTextToWindow(ReplaceTemplates(card, card.Title) + flippedText + hpText);
                OutputToConsoleAndFileLine(title);

                bool flipped = overrideFlipped ? !card.IsFlipped : card.IsFlipped;

                IEnumerable<string> keywords = card.Definition.Keywords;
                if (flipped && card.Definition.FlippedKeywords != null)
                {
                    keywords = card.Definition.FlippedKeywords;
                }

                if (keywords != null && keywords.Count() > 0)
                {
                    var keywordstext = ExtendCardTextToWindow("[ " + keywords.ToRecursiveString().ToUpper() + " ]");
                    OutputToConsoleAndFileLine(keywordstext);
                }

                if (card.HasNemesisIcons)
                {
                    var nemesis = ExtendCardTextToWindow("Nemesis Icon: " + card.NemesisIdentifiers.Select(n => this.GameController.Game.NameForTurnTakerIdentifier(n) ?? n).ToRecursiveString());
                    OutputToConsoleAndFileLine(nemesis);
                }

                if (flipped && (card.IsCharacter || card.IsMissionCard || card.IsShield) && card.Identifier != "TheTalisman")
                {
                    foreach (string body in card.Definition.FlippedBody)
                    {
                        if (body.Length > 0)
                        {
                            string text = body;
                            if (card.IsVillain && card.IsCharacter)
                            {
                                // Ignore newlines in villain character card body
                                text = body.Replace("{BR}", " ");
                            }

                            OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                        }
                    }

                    foreach (var ability in card.Definition.IncapacitatedAbilities)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow("* " + ReplaceTemplates(card, ability.Text)));
                    }

                    if (card.Definition.FlippedGameplay.Count() > 0)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow("[ GAME PLAY ]"));
                        foreach (string text in card.Definition.FlippedGameplay)
                        {
                            OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                        }
                    }

                    var advanced = GetAdvancedString(this.GameController.Game, card, false, overrideFlipped);
                    if (advanced != null)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(advanced));
                    }

                    var challenge = GetChallengeString(card, false, overrideFlipped);
                    if (challenge != null)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(challenge));
                    }
                }
                else
                {
                    foreach (string body in card.Definition.Body)
                    {
                        if (body.Length > 0)
                        {
                            string text = body;
                            if (card.IsVillain && card.IsCharacter)
                            {
                                // Ignore newlines in villain character card body
                                text = body.Replace("{BR}", " ");
                            }

                            OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                        }
                    }

                    PrintPowers(card);

                    PrintActivatableAbilities(card);

                    if (card.Definition.Setup.Count() > 0)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow("[ SETUP ]"));
                        foreach (string text in card.Definition.Setup)
                        {
                            OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                        }
                    }

                    if (card.Definition.Gameplay.Count() > 0)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow("[ GAME PLAY ]"));
                        foreach (string text in card.Definition.Gameplay)
                        {
                            OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                        }
                    }

                    var advanced = GetAdvancedString(this.GameController.Game, card, false, overrideFlipped);
                    if (advanced != null)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(advanced));
                    }

                    var challenge = GetChallengeString(card, false, overrideFlipped);
                    if (challenge != null)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(challenge));
                    }

                    if (card.MagicNumber.HasValue)
                    {
                        OutputToConsoleAndFileLine("[ " + card.MagicNumber.Value + " ]");
                    }
                }

                if (card.Definition.FooterBody != null && card.Definition.FooterBody.Count() > 0)
                {
                    OutputToConsoleAndFileLine("\n");

                    if (card.Definition.FooterTitle != null)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow("[ " + ReplaceTemplates(card, card.Definition.FooterTitle) + " ]"));
                    }

                    foreach (string text in card.Definition.FooterBody)
                    {
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(ReplaceTemplates(card, text)));
                    }
                }

                OutputToConsoleAndFileLine(borderLine);

                if (showSpecialStrings)
                {
                    PrintSpecialStringsForCard(card);
                }
            }
        }

        private static string GetAdvancedString(Game game, Card card, bool bothSides = false, bool overrideFlipped = false)
        {
            string result = null;
            var isAdvanced = card.Owner.IsAdvanced;
            var flipped = overrideFlipped ? !card.IsFlipped : card.IsFlipped;
            if (card.Definition.Advanced != null && (!flipped || bothSides) && isAdvanced)
            {
                var showSide = "";
                if (bothSides)
                {
                    showSide = "FRONT ";
                }
                result += "[ ADVANCED " + showSide + "]\n| ";
                result += ReplaceTemplates(card, card.Definition.Advanced);
            }

            if (bothSides)
            {
                result += "\n\n";
            }

            if (card.Definition.FlippedAdvanced != null && (flipped || bothSides) && isAdvanced)
            {
                var showSide = "";
                if (bothSides)
                {
                    showSide = "BACK ";
                }
                result += "[ ADVANCED " + showSide + "]\n| ";
                result += ReplaceTemplates(card, card.Definition.FlippedAdvanced);
            }

            return result;
        }

        private static string GetChallengeString(Card card, bool bothSides = false, bool overrideFlipped = false)
        {
            string result = null;
            var isChallenge = card.Owner.IsChallenge;
            var flipped = overrideFlipped ? !card.IsFlipped : card.IsFlipped;
            bool hasFlippedChallengeText = (card.Definition.FlippedChallengeText != null && card.Definition.FlippedChallengeText.Count() > 0);

            if (card.Definition.ChallengeTitle != null && (!flipped || bothSides || !hasFlippedChallengeText) && isChallenge)
            {
                var showSide = "";
                if (bothSides)
                {
                    showSide = "FRONT ";
                }

                result += "[ CHALLENGE " + showSide + "- " + card.Definition.ChallengeTitle + " ]\n| ";
                var text = string.Join("\n| ", card.Definition.ChallengeText.ToArray());
                result += ReplaceTemplates(card, text);
            }

            if (bothSides)
            {
                result += Environment.NewLine;
            }

            if (hasFlippedChallengeText && (flipped || bothSides) && isChallenge)
            {
                var showSide = "";
                if (bothSides)
                {
                    showSide = "BACK ";
                }

                result += "[ CHALLENGE " + showSide + "- " + card.Definition.ChallengeTitle + " ]\n| ";
                var text = string.Join("\n| ", card.Definition.FlippedChallengeText.ToArray());
                result += ReplaceTemplates(card, text);
            }

            return result;
        }

        private void PrintPowers(Card card)
        {
            foreach (string power in card.Definition.Powers)
            {
                var text = ReplaceTemplates(card, power);
                var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var firstLine = "Power: " + lines[0];
                OutputToConsoleAndFileLine(ExtendCardTextToWindow(firstLine));
                if (lines.Length > 1)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = "       " + lines[i];
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(line));
                    }
                }
            }
        }

        private void PrintActivatableAbilities(Card card)
        {
            foreach (Handelabra.Sentinels.Engine.Model.CardDefinition.ActivatableAbilityDefinition ability in card.Definition.ActivatableAbilities)
            {
                var text = ReplaceTemplates(card, ability.Text);
                var lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var abilityName = ability.Name.Capitalize();
                var firstLine = abilityName + ": " + lines[0];
                OutputToConsoleAndFileLine(ExtendCardTextToWindow(firstLine));
                if (lines.Length > 1)
                {
                    for (int i = 1; i < lines.Length; i++)
                    {
                        var line = "       " + lines[i];
                        OutputToConsoleAndFileLine(ExtendCardTextToWindow(line));
                    }
                }
            }
        }

        private void PrintCard(string identifierOrTitle, int index = 0, bool overrideFlipped = false)
        {
            var lowerCase = identifierOrTitle.ToLower();
            var cards = this.GameController.FindCardsWhere(c => c.Identifier.ToLower().StartsWith(lowerCase) || c.Identifier.ToLower().StartsWith(lowerCase.Replace(" ", "")) || c.Title.ToLower().StartsWith(lowerCase), false).GroupBy(c => c.Identifier);
            if (cards.Count() > 1)
            {
                if (cards.First().Key.StartsWith("Proletariat"))
                {
                    foreach (var card in cards)
                    {
                        PrintCard(card.First(), overrideFlipped: overrideFlipped, showSpecialStrings: true);
                        _lastCardPrinted = card.First();
                    }
                    _lastCardPrinted = null;
                }
                else
                {
                    OutputToConsoleAndFileLine(cards.Count() + " cards match " + identifierOrTitle + ", please be more specific.");
                }
            }
            else if (cards.Count() == 0)
            {
                OutputToConsoleAndFileLine("Could not find card named " + identifierOrTitle);
            }
            else if (cards.Count() == 1)
            {
                var group = cards.First();
                var cardToShow = group.First();
                var play = group.Where(c => c.IsInPlayAndNotUnderCard).FirstOrDefault();
                if (play != null)
                {
                    cardToShow = play;
                }
                else
                {
                    var hand = group.Where(c => c.IsInHand).FirstOrDefault();
                    if (hand != null)
                    {
                        cardToShow = hand;
                    }
                }

                PrintCard(cardToShow, overrideFlipped: overrideFlipped, showSpecialStrings: true);
            }
        }

        private void PrintSpecialStringsForCard(string identifierOrTitle, int? index = null)
        {
            var lowerCase = identifierOrTitle.ToLower();
            var cards = this.GameController.FindCardsWhere(c => c.Identifier.ToLower().StartsWith(lowerCase) || c.Identifier.ToLower().StartsWith(lowerCase.Replace(" ", "")) || c.Title.ToLower().StartsWith(lowerCase), false).GroupBy(c => c.Identifier);
            if (cards.Count() > 1)
            {
                OutputToConsoleAndFileLine(cards.Count() + " cards match " + identifierOrTitle + ", please be more specific.");
            }
            else if (cards.Count() == 0)
            {
                OutputToConsoleAndFileLine("Could not find card named " + identifierOrTitle);
            }
            else if (cards.Count() == 1)
            {
                var group = cards.First();
                var cardToShow = group.First();
                if (group.Any(c => c.IsInHand || c.IsInPlayAndNotUnderCard) && index.HasValue)
                {
                    // Show preference to a card in hand or in play.
                    var handOrPlay = group.Where(c => c.IsInHand || c.IsInPlayAndNotUnderCard);
                    index = Math.Min(index.Value, handOrPlay.Count() - 1);
                    cardToShow = handOrPlay.ElementAt(index.Value);
                }

                PrintSpecialStringsForCard(cardToShow, index, true);
            }
        }

        private void PrintSpecialStringsForCard(Card card, int? index = null, bool printTitle = false)
        {
            var ss = this.GameController.FindCardController(card).GetSpecialStrings(false, true);

            if (ss.Count() > 0)
            {
                if (printTitle)
                {
                    OutputToConsoleAndFileLine(card.Title);
                }

                foreach (var special in ss)
                {
                    OutputToConsoleAndFileLine("[ " + special.GeneratedString() + " ]");
                }
            }
        }

        private void PrintFriendlyGame()
        {
            for (int i = 0; i < this.GameController.Game.TurnTakers.Count(); i++)
            {
                this.PrintFriendlyTurnTaker(i);
                OutputToConsoleAndFileLine("");
            }

            // Print status effects & special strings
            OutputToConsoleAndFileLine("---- ONGOING EFFECTS ----");
            var specials = this.GameController.GetSpecialStringsForEffectsList();
            if (this.GameController.Game.StatusEffects.Count > 0 || specials.Count() > 0)
            {
                foreach (var effect in this.GameController.Game.StatusEffects)
                {
                    OutputToConsoleAndFileLine(effect.ToString());
                }

                foreach (var special in specials)
                {
                    OutputToConsoleAndFileLine(special.GeneratedString());
                }
            }
            else
            {
                OutputToConsoleAndFileLine("None");
            }
        }

        private string FriendlyCardString(Card card, bool showHP = true, bool showLocation = false)
        {
            string result = card.Title;

            result += card.Title == "Sky-Scraper" ? " (" + card.Definition.Keywords.First() + ")" : "";

            if (card.IsLink)
            {
                result += " (Link)";
            }

            if (showHP && card.IsTarget)
            {
                result += " (" + card.HitPoints.Value + " HP)";
            }

            if (card.IsFlipped)
            {
                result += " (Flipped)";
            }

            if (showLocation)
            {
                result += " (" + card.Location.GetFriendlyName() + ")";
            }

            if (card.Owner.Identifier == "TheArgentAdept")
            {
                result += card.IsMelody ? " (M)" : "";
                result += card.IsRhythm ? " (R)" : "";
                result += card.IsHarmony ? " (H)" : "";
            }

            result += card.MagicNumber.HasValue ? " [" + card.MagicNumber.Value + "]" : "";

            if (card.TokenPools != null && card.TokenPools.Count() > 0)
            {
                foreach (var pool in card.TokenPools)
                {
                    var tokens = pool.CurrentValue == 1 ? "token" : "tokens";
                    result += "\n\t\t" + ReplaceTemplates(card, pool.Name) + ": " + pool.CurrentValue + " " + tokens;
                }
            }

            return result;
        }

        private void PrintFriendlyTurnTaker(string name)
        {
            var turnTakers = this.GameController.Game.TurnTakers.Where(t => t.Name.ToLower().StartsWith(name.ToLower()) || t.Identifier.ToLower().StartsWith(name.ToLower()));
            if (turnTakers.Count() > 1)
            {
                OutputToConsoleAndFileLine(turnTakers.Count() + " players match " + name + ", please be more specific.");
            }
            else if (turnTakers.Count() == 1)
            {
                PrintFriendlyTurnTaker(turnTakers.First());
            }
            else if (turnTakers.Count() == 0)
            {
                OutputToConsoleAndFileLine("Could not find a player named " + name);
            }
        }

        private void PrintFriendlyTurnTaker(int index)
        {
            if (index >= 0 && index < this.GameController.Game.TurnTakers.Count())
            {
                TurnTaker turnTaker = this.GameController.Game.TurnTakers.ElementAt(index);
                PrintFriendlyTurnTaker(turnTaker);
            }
            else
            {
                OutputToConsoleAndFileLine("There's nobody at that place.");
            }
        }

        private void PrintFriendlyCardList(IEnumerable<Card> cards, string name, string prefix = "", bool showHP = true, bool showLocation = false)
        {
            if (cards.Count() > 0)
            {
                OutputToConsoleAndFileLine(prefix + name + ": (" + cards.Count() + ")");
                foreach (var card in cards)
                {
                    // Don't print the card if it is out of game and belongs to an incapacitated hero.
                    if (!(card.Location.IsOutOfGame && card.Owner.IsHero && card.Owner.ToHero().IsIncapacitatedOrOutOfGame))
                    {
                        OutputToConsoleAndFileLine(prefix + "\t" + FriendlyCardString(card, showHP, showLocation));

                        // Print attached and under cards
                        if (card.NextToLocation != null && card.NextToLocation.Cards.Count() > 0)
                        {
                            PrintFriendlyCardList(card.NextToLocation.Cards, " Next To", prefix + "\t", showHP);
                        }

                        if (card.UnderLocation != null && card.UnderLocation.Cards.Count() > 0)
                        {
                            PrintFriendlyCardList(card.UnderLocation.Cards, " Under", prefix + "\t", showHP);
                        }

                        if (card.BelowLocation != null && card.BelowLocation.Cards.Count() > 0)
                        {
                            PrintFriendlyCardList(card.BelowLocation.Cards, " Title Cards", prefix + "\t", showHP);
                        }
                    }
                }
            }
            else
            {
                OutputToConsoleAndFileLine(prefix + name + ": None");
            }
        }

        private void PrintFriendlyTurnTaker(TurnTaker turnTaker)
        {
            string firstLine = "---- " + turnTaker.Name.ToUpper() + " ----";
            OutputToConsoleAndFileLine(firstLine);

            if (turnTaker.BattleZone != null)
            {
                OutputToConsoleAndFileLine("--" + turnTaker.BattleZone.Name + "--");
            }
            PrintFriendlyCardList(turnTaker.PlayArea.Cards, "Cards in Play");
            OutputToConsoleAndFileLine("Cards in Deck: (" + turnTaker.Deck.Cards.Count() + ")");
            var cardBack = "None";
            if (turnTaker.Deck.HasCards)
            {
                cardBack = turnTaker.Deck.TopCard.Owner.Name;
            }
            OutputToConsoleAndFileLine("Top Card Back: " + cardBack);
            PrintFriendlyCardList(turnTaker.Trash.Cards, "Cards in Trash", showHP: false);

            if (turnTaker.Identifier == "Tachyon")
            {
                PrintFriendlyCardList(turnTaker.Trash.Cards.Where(c => c.IsBurst), "Burst Cards in Trash", showHP: false);
            }

            var others = turnTaker.GetAllCards().Where(c => c.Location != turnTaker.PlayArea && c.Location != turnTaker.Deck && c.Location != turnTaker.Trash).ToList();

            if (turnTaker.IsHero)
            {
                HeroTurnTaker hero = turnTaker.ToHero();
                PrintFriendlyCardList(hero.Hand.Cards, "Cards in Hand", showHP: false);
                others = others.Where(c => c.Location != hero.Hand).ToList();
            }

            if (turnTaker.SubDecks != null)
            {
                foreach (var subdeck in turnTaker.SubDecks)
                {
                    if (subdeck.Identifier == "MissionDeck")
                    {
                        OutputToConsoleAndFileLine("Top mission card: " + subdeck.TopCard.Title);
                    }
                    OutputToConsoleAndFileLine("Cards in " + subdeck.GetFriendlyName() + ": (" + subdeck.Cards.Count() + ")");
                    others = others.Where(c => c.Location != subdeck).ToList();
                }
            }

            if (turnTaker.SubPlayAreas != null)
            {
                foreach (var subPlayArea in turnTaker.SubPlayAreas)
                {
                    PrintFriendlyCardList(subPlayArea.Cards, "Cards in " + subPlayArea.GetFriendlyName());
                    others = others.Where(c => c.Location != subPlayArea).ToList();
                }
            }

            if (others.Count() > 0)
            {
                // If OblivAeon mode, do not show out of game cards or cards under the Scion Reserve
                if (this.GameController.IsOblivAeonMode)
                {
                    others.RemoveAll(c => c.Location.IsOutOfGame || (c.Location.OwnerCard != null && c.Location.OwnerCard.Identifier == "TheScionReserve"));
                }

                PrintFriendlyCardList(others, "Other cards", showHP: false, showLocation: true);
            }
        }

        private void PrintTargets()
        {
            var targets = this.GameController.FindCardsWhere(c => c.IsInPlayAndNotUnderCard && c.IsTarget).OrderByDescending(c => c.HitPoints.Value);
            OutputToConsoleAndFileLine("");
            for (int i = 0; i < targets.Count(); i++)
            {
                var target = targets.ElementAt(i);
                OutputToConsoleAndFileLine((i + 1) + ". " + GetCardListString(target));
            }
        }

        private void PrintFriendlyMenu()
        {
            OutputToConsoleAndFileLine("");
            OutputToConsoleAndFileLine(
                "---- MEGA COMPUTER HELP MENU ----" + Environment.NewLine +
                "view\t\tView Game State" + Environment.NewLine +
                "view <#>\tView Player State (e.g. view 1 to view Villain)" + Environment.NewLine +
                "view me\t\tView Current Player State" + Environment.NewLine +
                "view v\t\tView Current Villain State" + Environment.NewLine +
                "view e\t\tView Current Environment State" + Environment.NewLine +
                "view <name>\tView Player State (e.g. view Baron)" + Environment.NewLine +
                "view hp\t\tView a short list of targets in play, from highest to lowest" + Environment.NewLine +
                "read\t\tRead all cards in options list" + Environment.NewLine +
                "read <#>\tRead card in options list (e.g. read 1)" + Environment.NewLine +
                "read <card>\tRead any card (e.g. read Mega)" + Environment.NewLine +
                "rf <card>\tRead the flip side of any card (e.g. rf Baron)" + Environment.NewLine +
                "journal\t\tShow the journal for the current round" + Environment.NewLine +
                "journal <#>\tShow the journal for more rounds (e.g. journal 2)" + Environment.NewLine +
                "save <name>\tSave checkpoint (e.g. save turn1). Checkpoints are made only at turn phase changes." + Environment.NewLine +
                "load\t\tList saved checkpoints" + Environment.NewLine +
                "load <name>\tLoad checkpoint (e.g. load turn1)" + Environment.NewLine +
                "delete <name>\tDelete checkpoint (e.g. delete turn1)" + Environment.NewLine +
                "cleardata\tReset stored user data" + Environment.NewLine +
                "cheat grab\tGrab a card from a hero's deck/trash and put it in their hand" + Environment.NewLine +
                "cheat play\tImmediately play a chosen card from the character's hand, deck or trash" + Environment.NewLine +
                "cheat stack (bottom)\tChoose a deck and decide which card will be on top (or bottom) of a deck" + Environment.NewLine +
                "cheat hp\tChoose a target and set their HP" + Environment.NewLine +
                "cheat destroy\tDestroy a card in play" + Environment.NewLine +
                "cheat win\tWin the game" + Environment.NewLine +
                "cheat lose\tLose the game" + Environment.NewLine +
                "cheat switch\tSwitch Sky-Scraper's character card (if in game)" + Environment.NewLine +
                "cheat tokens\tSet the number of tokens for Setback or Guise (if in game)" + Environment.NewLine +
                "cheat stops\tToggles cheat stops, whereby the game stops at the start of every turn for cheat commands" + Environment.NewLine +
                "quit\t\tQuit the game" + Environment.NewLine +
                "help\t\tShow this menu" + Environment.NewLine +
                Environment.NewLine +
                "Shortcuts: v = view, r = read, j = journal, s = save, l = load, q = quit" + Environment.NewLine);
        }

        public static string Evaluate(Game game, string expression)
        {
            // Replace H with its value
            expression = expression.Replace("H", game.H.ToString());

            var xsltExpression =
                string.Format("number({0})",
                    new Regex(@"([\+\-\*])").Replace(expression, " ${1} ")
                    .Replace("/", " div ")
                    .Replace("%", " mod "));
            var result = new XPathDocument(new StringReader("<r/>"))
                .CreateNavigator()
                .Evaluate(xsltExpression);

            if (result != null)
            {
                return result.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Replaces {templates} in text. We support the following so far:
        /// - {TurnTakerIdentifier} - e.g. {AbsoluteZero} resolves to "Absolute Zero"
        /// - {H} - resolves to number of heroes, and supports simple formulas like {H + 1}
        /// - {BR} - resolves to a newline
        /// - [i],[/i],[b],[/b] - changes to *
        /// </summary>
        /// <returns>The text with templates replaced by BBcode text.</returns>
        public static string ReplaceTemplates(Card card, string text)
        {
            if (text == null)
            {
                return "";
            }

            Game game = card.Owner.Game;

            MatchEvaluator evaluator = delegate (Match match)
            {
                string middle = match.Value.Trim(new char[] { '{', '}' });

                // Match on fixed strings we support
                if (middle.ToLower() == "br")
                {
                    return Environment.NewLine;
                }
                else if (middle.ToLower() == "magic")
                {
                    return "[*]";
                }
                else if (middle.ToLower() == "gazelle" || middle.ToLower() == "crocodile" || middle.ToLower() == "rhinoceros")
                {
                    return middle.ToUpper() + ":";
                }
                else if (middle.ToLower() == "iconh")
                {
                    return "(H)";
                }
                else if (middle.ToLower() == "arcana" || middle.ToLower() == "avian")
                {
                    return middle.ToUpper();
                }
                else
                {
                    // Try finding a turn taker with the name
                    string name = game.NameForTurnTakerIdentifier(middle);

                    // Failing that, check if there's a card with that identifier.
                    if (name == null)
                    {
                        name = game.TitleForCardIdentifier(middle);
                    }

                    if (name != null)
                    {
                        return name;
                    }
                    else if (middle.StartsWith("H"))
                    {
                        // Try resolving it as an H formula
                        string evaluated = Evaluate(game, middle);
                        if (evaluated != null)
                        {
                            // Highlight the evaluated value
                            return "*" + evaluated + "*";// + " (" + middle + ")";
                        }
                    }
                }

                // Just return it as is
                return middle;
            };

            string pattern = "{.*?}"; // non-greedy
            string result = Regex.Replace(text, pattern, evaluator);

            result = result.Replace("[i]", "*").Replace("[/i]", "*").Replace("[b]", "*").Replace("[/b]", "*").Replace("[u]", "").Replace("[/u]", "").Replace("\\", "");

            return result;
        }

        public void RunCoroutine(IEnumerator e)
        {
            while (e.MoveNext() && !QuitRequested)
            {

            }
        }

        public IEnumerator StartCoroutine(IEnumerator e)
        {
            while (e.MoveNext() && !QuitRequested)
            {

            }

            yield return null;
        }

        private void WriteFileOutput(string message)
        {
            if (this.FileOutput == null)
            {
                FileOutput = new System.IO.StreamWriter(this.OutputFileName, true);
                FileOutput.AutoFlush = true;
            }

            this.FileOutput.WriteLine(message);
        }

        private void CloseFileOutput()
        {
            if (this.FileOutput != null)
            {
                FileOutput.Close();
                FileOutput = null;
            }
        }

        private static string GetInput(string prompt = null)
        {
            string result = null;
            try
            {
                if (prompt != null)
                {
                    Console.WriteLine(prompt);
                    Console.Write("\t> ");
                }

                result = Console.ReadLine();
            }
            catch (System.ArgumentOutOfRangeException)
            {
                // Eat System.ArgumentOutOfRangeException exceptions from ReadLine
            }
            catch (System.OutOfMemoryException)
            {
                // Eat System.OutOfMemoryException exceptions from ReadLine
            }

            return result;
        }

        // always returns a string, never null, trimmed.
        private string GetCleanInput()
        {
            string line = GetInput();

            if (line == null)
            {
                line = "";
            }
            line = line.Trim();
            WriteFileOutput(line);
            return line;
        }

        private void PressEnterToContinue()
        {
            Console.WriteLine("(Press Enter to continue.)");
            GetInput();
        }

        private void PressEnterOrCheat(TurnTaker toTurnTaker)
        {
            Console.WriteLine("(Press Enter to continue, or go ahead and cheat.)");
            SelectOption(null, null, true, toTurnTaker: toTurnTaker);
        }

        private void OutputToConsoleAndFile(string message)
        {
            Console.Write(message);
            WriteFileOutput(message);
        }

        private void OutputToConsoleAndFileLine(string message)
        {
            message = message.Replace("[i]", "").Replace("[/i]", "");
            Console.WriteLine(message);
            WriteFileOutput(message);
        }

        private static string GetPhaseActionRemainder(TurnTaker turnTaker, Phase decisionType, TurnPhase activePhase)
        {
            if (turnTaker == activePhase.TurnTaker && activePhase.Phase == decisionType && activePhase.GetPhaseActionCount().HasValue)
            {
                return "(" + activePhase.GetPhaseActionCount().Value + ")";
            }
            return "";
        }

        private bool IsSkipInput(string input)
        {
            return input.StartsWith("w") || input.StartsWith(".");
        }

        private bool CanUsePowers(HeroTurnTakerController hero)
        {
            return this.GameController.CanUsePowers(hero, null);
        }

        private bool CanDrawCards(HeroTurnTakerController hero)
        {
            return this.GameController.CanDrawCards(hero, null);
        }

        private void ReadCommand(string input, IEnumerable<string> options)
        {
            bool overrideFlipped = false;
            var commandLength = 1;
            if (input.ToLower().StartsWith("rf"))
            {
                commandLength = 2;
                overrideFlipped = true;
            }
            else if (input.ToLower().StartsWith("read"))
            {
                commandLength = 4;
            }

            if (input.Length > commandLength)
            {
                // Read a card
                string param = input.Substring(commandLength + 1);
                int readIndex = param.ToInt(-1);
                _lastCardPrinted = null;
                if (readIndex >= 1 && readIndex <= options.Count())
                {
                    PrintCard(RemoveCardExtras(options.ElementAt(readIndex - 1)), overrideFlipped: overrideFlipped);
                }
                else
                {
                    PrintCard(param, overrideFlipped: overrideFlipped);
                }
            }
            else
            {
                // Read all cards in the options
                // Keep track of which instance of the card we want to look at in case there are two different ones in hand. (eg Magic Numbers)
                int index = 1;
                var instances = new List<string>();
                _lastCardPrinted = null;
                foreach (string card in options)
                {
                    var cardName = RemoveCardExtras(card);
                    OutputToConsoleAndFileLine(index + ":");
                    PrintCard(cardName, instances.Where(s => s == cardName).Count(), overrideFlipped: overrideFlipped);
                    index += 1;
                    instances.Add(cardName);
                }
            }
        }

        private static string RemoveCardExtras(string cardTitle)
        {
            return cardTitle.Replace(" (H)", "").Replace(" (R)", "").Replace(" (M)", "");
        }

        private static EndingResult? CheatEnding;
        private static bool QuitRequested;
        private bool _ignoreException;
        private IEnumerable<DecisionAnswerJournalEntry> _replayDecisionAnswers;
        private bool _replayingGame;
    }


}
