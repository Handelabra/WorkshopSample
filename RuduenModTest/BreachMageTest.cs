using NUnit.Framework;
using System;
using Workshopping;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Workshopping.Inquirer;
using System.Collections.Generic;

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
            Assert.IsInstanceOf(typeof(InquirerTurnTakerController), BreachMage);
            Assert.IsInstanceOf(typeof(InquirerCharacterCardController), BreachMage.CharacterCardController);

            Assert.AreEqual(26, BreachMage.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");

            StartGame();

            QuickHandStorage(BreachMage.ToHero());
            UsePower(BreachMage.CharacterCard,0); // Default Innate. 
            QuickHandCheck(1);
        }

        [Test()]
        public void TestTheLieTheyTellThemselves()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();
            GoToUsePowerPhase(BreachMage);

            // Put out example cards.
            Card distortion = PlayCard("YoureLookingPale");
            Card power = PlayCard("TheLieTheyTellThemselves");

            QuickHandStorage(BreachMage.ToHero());
            UsePower(power);
            QuickHandCheck(2);
            AssertInTrash(distortion); // Distortion was destroyed. 

        }
    }
}
