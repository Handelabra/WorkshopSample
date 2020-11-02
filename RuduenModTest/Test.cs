using NUnit.Framework;
using System;
using Workshopping;
using Workshopping.RuduenFanMods.BreachMage;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;

namespace RuduenModTest
{
    [TestFixture()]
    public class Test : BaseTest
    {
        protected HeroTurnTakerController migrant { get { return FindHero("BreachMage"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(migrant);
            Assert.IsInstanceOf(typeof(BreachMageTurnTakerController), migrant);
            Assert.IsInstanceOf(typeof(BreachMageCharacterCardController), migrant.CharacterCardController);

            Assert.AreEqual(39, migrant.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestPunchingBag()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(migrant);

            // Punching Bag does 1 damage!
            QuickHPStorage(migrant);
            PlayCard("PunchingBag");
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

            StartGame();

            var mdp = GetCardInPlay("MobileDefensePlatform");

            // Base power draws 3 cards! Deals 1 target 2 damage!
            QuickHandStorage(migrant.ToHero());
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);

            UsePower(migrant.CharacterCard);

            QuickHandCheck(3);
            QuickHPCheck(-2);

        }
    }
}
