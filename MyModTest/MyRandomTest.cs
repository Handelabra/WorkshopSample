using NUnit.Framework;
using System;
using Workshopping;
using Workshopping.MigrantCoder;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Workshopping.TheBaddies;
using System.Collections.Generic;

namespace MyModTest
{
    [TestFixture()]
    public class MyRandomTest : RandomGameTest
    {
        static string[] ModHeroes = { "Workshopping.MigrantCoder" };
        static string[] ModVillains = { "Workshopping.TheBaddies" };
        static string[] ModEnvironments = { "Workshopping.DevStream" };

        [Test]
        public void TestRandomGameWithModsToCompletion()
        {
            GameController gameController = SetupRandomGameController(false,
                DeckDefinition.AvailableHeroes.Concat(ModHeroes),
                DeckDefinition.AvailableVillains.Concat(ModVillains),
                DeckDefinition.AvailableEnvironments.Concat(ModEnvironments));
            RunGame(gameController);
        }

        [Test]
        public void TestSomewhatReasonableGameWithModsToCompletion()
        {
            GameController gameController = SetupRandomGameController(true,
                DeckDefinition.AvailableHeroes.Concat(ModHeroes),
                DeckDefinition.AvailableVillains.Concat(ModVillains),
                DeckDefinition.AvailableEnvironments.Concat(ModEnvironments));
            RunGame(gameController);
        }

        [Test]
        public void TestMyStuff()
        {
            GameController gameController = SetupRandomGameController(false,
                availableVillains:ModVillains,
                availableEnvironments:ModEnvironments,
                useHeroes: new List<string> { "Workshopping.MigrantCoder" });
            RunGame(gameController);
        }

        [Test]
        public void TestRandomGameAgainstBaronJeremy()
        {
            GameController gameController = SetupRandomGameController(false,
                DeckDefinition.AvailableHeroes.Concat(ModHeroes),
                DeckDefinition.AvailableVillains.Concat(ModVillains),
                DeckDefinition.AvailableEnvironments.Concat(ModEnvironments),
                overrideVillain: "BaronBlade",
                overrideVariants: new Dictionary<string, string> { { "BaronBlade", "BaronJeremyCharacter" } });
            RunGame(gameController);
        }

        [Test]
        public void TestRandomGameAgainstCustomVariantHeroes()
        {
            GameController gameController = SetupRandomGameController(false,
                DeckDefinition.AvailableHeroes.Concat(ModHeroes),
                DeckDefinition.AvailableVillains.Concat(ModVillains),
                DeckDefinition.AvailableEnvironments.Concat(ModEnvironments),
                useHeroes: new List<string> { "SkyScraper", "TheSentinels" },
                overrideVariants: new Dictionary<string, string>
                    {
                        { "SkyScraper", "CentristSkyScraperNormalCharacter" },
                        { "TheSentinels", "TheSerpentinelsInstructions" }
                    });
            RunGame(gameController);
        }
    }
}
