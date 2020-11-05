using NUnit.Framework;
using System;
using Workshopping;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using System.Collections.Generic;
using Workshopping.BreachMage;

namespace RuduenModTest
{
    [TestFixture]
    public class BreachMageTest : BaseTest
    {
        protected HeroTurnTakerController BreachMage { get { return FindHero("BreachMage"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageTurnTakerController), BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageCharacterCardController), BreachMage.CharacterCardController);

            Assert.AreEqual(27, BreachMage.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast. 
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed. 
        }

        [Test()]
        public void TestInnatePowerB()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            List<Card> charges = new List<Card>();
            charges.Add(PlayCard("MoltenHammer", 0));
            charges.Add(PlayCard("MoltenHammer", 1));

            QuickHandStorage(BreachMage);
            UsePower(BreachMage.CharacterCard, 1); // Default Innate. Cast. 
            QuickHandCheck(5); // 5 Cards Drawn.
            AssertInTrash(charges); // All used charges in trash. 
        }

        [Test()]
        public void TestOpenBreach()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            PlayCard("OpenBreach");
            // Discard hand to make sure there's only one valid card. It's messy, but tracking for decision isn't working well. 
            DiscardAllCards(BreachMage);
            Card spell = PutInHand("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            GoToEndOfTurn(BreachMage); // End of turn: Spell played. 
            GoToStartOfTurn(BreachMage); // Start of turn: Spell cast. 
            QuickHPCheck(-4); // Damage Dealt. Base 4.
            AssertInTrash(spell); // Spell destroyed. 
        }

        [Test()]
        public void TestPotentBreach()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            PlayCard("PotentBreach");
            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            GoToStartOfTurn(BreachMage);
            QuickHPCheck(-5); // Damage Dealt. Base 4, plus 1 additional. 
            AssertInTrash(spell); // Spell destroyed. 
        }

        [Test()]
        public void TestScryingBolt()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast. 
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed. 

            // TODO: Add scrying test at some point! (Right now, more complex than it's worth.) 
        }

        [Test()]
        public void TestRadiance()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("Radiance");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            QuickHandStorage(BreachMage);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast. 
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed. 
            QuickHandCheck(1); // Card drawn.

            // TODO: Add scrying test at some point! (Right now, more complex than it's worth.) 
        }

        [Test()]
        public void TestSpectralEcho()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("SpectralEcho");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card ongoing = PlayCard("LivingForceField");
            DecisionSelectTarget = mdp;
            DecisionDestroyCard = ongoing;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast. 
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(ongoing, spell); // Ongoing & Spell destroyed. 

            // TODO: Add scrying test at some point! (Right now, more complex than it's worth.) 
        }
    }
}
