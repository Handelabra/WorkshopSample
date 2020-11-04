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

            GoToPlayCardPhase(Inquirer);

            PlayCard("ImAMolePerson");
            PlayCard("ImAVictorian");
            PlayCard("ImANinja");

            DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            QuickHPStorage(mdp, Inquirer.CharacterCard);

            DecisionSelectTarget = mdp;
            DecisionYesNo = true;
            DecisionDoNotSelectCard = SelectionType.DestroyCard;

            AssertNumberOfCardsInTrash(Inquirer, 0);
            GoToEndOfTurn(Inquirer);
            AssertNumberOfCardsInTrash(Inquirer, 2); // Discards a card, but do not play as a result.

            GoToStartOfTurn(Inquirer);
            AssertNumberOfCardsInTrash(Inquirer, 2); // One successful shuffle, two failed shuffles.

            QuickHPCheck(-2, 2); // Two total damage - 1 base, 1 buff. 2 Healing - 1 base, 1 buff.
        }

        [Test()]
        public void TestBackupPlan()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);
            Card power = PlayCard("BackupPlan");

            DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            QuickHPStorage(mdp, Inquirer.CharacterCard);
            QuickHandStorage(Inquirer);

            DecisionSelectTarget = mdp;
            UsePower(power);

            QuickHPCheck(-1, 1);
            AssertNumberOfCardsInTrash(Inquirer, 1); // Discarded.
            QuickHandCheck(0); // Discard and draw for net 0 change.

        }

        [Test()]
        public void TestYoureLookingPale()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp);

            // TODO: DetermineLocation works, but this seems to suppress the Play step in regression tests, so skip the distortions for now!
            // Adding this.NextToCriteria in the constructor seems to cause the issue! 
            //PlayCard("YoureLookingPale");
            //PlayCard("LookADistraction");

            //GoToStartOfTurn(Inquirer);
            //QuickHPCheck(-2); // 4 damage, healed 2.

        }

        [Test()]
        public void TestUntilYouMakeIt()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card safeCard = GetCardWithLittleEffect(Inquirer);
            PutInHand(safeCard); // Add safe card for play. 
            QuickHandStorage(Inquirer);
            DecisionSelectCardToPlay = safeCard;

            PlayCard("UntilYouMakeIt");
            QuickHandCheck(0); // Draw 1, Play 1, Net 0. 
            AssertNumberOfCardsInPlay(Inquirer, 3); // Should now have character card, new form, and safe card in play.
        }

        [Test()]
        public void TestFisticuffs()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            QuickHPStorage(mdp, Inquirer.CharacterCard);
            QuickHandStorage(Inquirer);

            DecisionSelectTarget = mdp;
            PlayCard("Fisticuffs");

            QuickHPCheck(-3, 2);
            AssertNumberOfCardsInTrash(Inquirer, 2); // Discarded and played card.
            QuickHandCheck(-1); // Fisticuffs source is ambiguous, so don't check hand - just a net of -1. 

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
