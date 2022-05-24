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
    public class Test : BaseTest
    {
        protected TurnTakerController baddies { get { return FindVillain("TheBaddies"); } }
        protected TurnTakerController hug { get { return FindVillain("TheHugMonsterTeam"); } }
        protected HeroTurnTakerController migrant { get { return FindHero("MigrantCoder"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("Workshopping.TheBaddies", "Workshopping.MigrantCoder", "Workshopping.DevStream");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesTurnTakerController), baddies);
            Assert.IsInstanceOf(typeof(TheBaddiesCharacterCardController), baddies.CharacterCardController);

            Assert.IsNotNull(migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderTurnTakerController), migrant);
            Assert.IsInstanceOf(typeof(MigrantCoderCharacterCardController), migrant.CharacterCardController);

            Assert.IsNotNull(env);

            Assert.AreEqual(40, baddies.CharacterCard.HitPoints);
            Assert.AreEqual(30, migrant.CharacterCard.HitPoints);
            QuickHPStorage(baddies, migrant);

            StartGame();

            AssertIsInPlay("SmashBackField");

            // Always deals 5 psychic to non villains!
            PlayCard("FireEverything");

            QuickHPCheck(0, -6); // Nemesis!

            StackDeck("DroppedFrame");
            PlayTopCard(env);

            // Deals 1 damage
            QuickHPCheck(-1, -1);

            // Heals 1 at the start of the environment turn
            GoToStartOfTurn(env);
            QuickHPCheck(1, 1);
        }

        [Test()]
        public void TestHugMonster()
        {
            SetupGameController("Workshopping.TheHugMonsterTeam", "Workshopping.MigrantCoder", "BugbearTeam", "Legacy", "GreazerTeam", "TheWraith", "Megalopolis");

            StartGame();

            var warm = PlayTopCard(hug);

            QuickHPStorage(hug, migrant, bugbearTeam, legacy, greazerTeam, wraith);
            GoToEndOfTurn(hug);
            QuickHPCheck(0, -2, 0, -2, 0, -4);

            GoToPlayCardPhase(migrant);
            DestroyCard(warm);
            GoToEndOfTurn(migrant);
            QuickHPCheckZero();

        }

        [Test()]
        public void TestTheRealBaddies()
        {
            SetupGameController("Workshopping.TheBaddies/Workshopping.TheRealBaddiesCharacter", "Workshopping.MigrantCoder", "Workshopping.DevStream");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(baddies);
            Assert.IsInstanceOf(typeof(TheRealBaddiesTurnTakerController), baddies);
            Assert.IsInstanceOf(typeof(TheRealBaddiesCharacterCardController), baddies.CharacterCardController);

            Assert.AreEqual(50, baddies.CharacterCard.HitPoints);
            QuickHPStorage(baddies, migrant);

            StartGame();

            AssertNotInPlay("SmashBackField");
            AssertIsInPlay("WrightWeigh");

            GoToEndOfTurn();
        }

        [Test()]
        public void TestDevStream()
        {
            SetupGameController("BaronBlade", "Workshopping.MigrantCoder", "Unity", "TheArgentAdept", "Workshopping.DevStream");

            StartGame();

            // Spam bot does H - 1 = 2 damage, plus 1 for nemesis bonus
            GoToStartOfTurn(env);
            PlayCard("SpamBot");
            PlayCard("Modder");
            QuickHPStorage(baron, migrant, unity, adept);
            GoToEndOfTurn(env);
            QuickHPCheck(0, -3, 0, 0);

            // Modder plays cards at the start of turn
            GoToEndOfTurn(adept);
            var bb = StackDeck("BladeBattalion");
            var keyboard = StackDeck("CodersKeyboard");
            var raptor = StackDeck("RaptorBot");
            var bell = StackDeck("XusBell");
            GoToStartOfTurn(env);
            AssertIsInPlay(bb, keyboard, raptor, bell);
        }
    }
}
