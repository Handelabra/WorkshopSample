using NUnit.Framework;
using System;
using Workshopping;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.Engine.Controller;
using System.Linq;
using System.Collections;
using Handelabra.Sentinels.UnitTest;
using Workshopping.Inquirer;

namespace RuduenModTest
{
    [TestFixture()]
    public class Test : BaseTest
    {
        protected HeroTurnTakerController Inquirer { get { return FindHero("Inquirer"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Inquirer);
            Assert.IsInstanceOf(typeof(InquirerTurnTakerController), Inquirer);
            Assert.IsInstanceOf(typeof(InquirerCharacterCardController), Inquirer.CharacterCardController);

            Assert.AreEqual(26, Inquirer.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");

            StartGame();

            QuickHandStorage(Inquirer.ToHero());
            UsePower(Inquirer.CharacterCard);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestTheLieTheyTellThemselves()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();
            GoToUsePowerPhase(Inquirer);

            // Put out example cards.
            Card distortion = PlayCard("YoureLookingPale");
            Card power = PlayCard("TheLieTheyTellThemselves");

            //// TODO: CHECK HAND WHEN DISTORTION TEST WORKS!
            //QuickHandStorage(Inquirer.ToHero());
            //UsePower(power);
            //QuickHandCheck(2);

            //// TODO: Adjust logic to account for distortion destruction.

            // Check enemy and destroy distortion for damage.
            //var mdp = GetCardInPlay("MobileDefensePlatform");
            //DecisionSelectTarget = mdp;
            //QuickHPStorage(mdp);
            //DestroyCard(distortion);
            //QuickHPCheck(-1);

        }

        [Test()]
        public void TestUndeniableFacts()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();
            GoToUsePowerPhase(Inquirer);

            // Put out example cards.
            Card distortion = PlayCard("YoureLookingPale");
            Card power = PlayCard("UndeniableFacts");

            //// TODO: When distortions check functions, run it! 
        }

        [Test()]
        public void TestForms()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            PlayCard("ImAMolePerson");
            PlayCard("ImAVictorian");
            PlayCard("ImANinja");

            DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);
            DealDamage(Inquirer, mdp, 2, DamageType.Melee);
            QuickHPCheck(-3);

            AssertNumberOfCardsInTrash(Inquirer, 0);
            DiscardCard(Inquirer);
            AssertNumberOfCardsInTrash(Inquirer, 2);

            DecisionYesNo = true;
            DecisionDoNotSelectCard = SelectionType.DestroyCard;

            QuickHPStorage(Inquirer);
            GoToEndOfTurn(Inquirer);
            QuickHPCheck(2);
            AssertNumberOfCardsInTrash(Inquirer, 4); // Second discard x2, also triggers a play.

            GoToStartOfTurn(Inquirer);
            AssertNumberOfCardsInTrash(Inquirer, 1); // Two successful shuffles, one unsuccessful resulting in destruction. 
        }

        //[Test()]
        //public void TestPunchingBag()
        //{
        //    SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

        //    StartGame();

        //    GoToUsePowerPhase(inquirer);

        //    // Punching Bag does 1 damage!
        //    QuickHPStorage(inquirer);
        //    PlayCard("PunchingBag");
        //    QuickHPCheck(-1);
        //}

        //[Test()]
        //public void TestInnatePower()
        //{
        //    SetupGameController("BaronBlade", "Workshopping.BreachMage", "Megalopolis");

        //    StartGame();

        //    var mdp = GetCardInPlay("MobileDefensePlatform");

        //    // Base power draws 3 cards! Deals 1 target 2 damage!
        //    QuickHandStorage(inquirer.ToHero());
        //    DecisionSelectTarget = mdp;
        //    QuickHPStorage(mdp);

        //    UsePower(inquirer.CharacterCard);

        //    QuickHandCheck(3);
        //    QuickHPCheck(-2);

        //}
    }
}
