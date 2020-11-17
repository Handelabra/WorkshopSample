using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Workshopping.Inquirer;

namespace RuduenModsTest
{
    [TestFixture]
    public class InquirerTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("Workshopping", Assembly.GetAssembly(typeof(InquirerCharacterCardController))); // replace with your own namespace
        }

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
        public void TestLiesOnLiesInnatePower()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "Workshopping.Inquirer", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "InquirerCharacter", "InquirerLiesOnLiesCharacter" }
            };
            SetupGameController(setupItems, false, promos);

            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = mdp;

            Card distortion = PlayCard("YoureLookingPale"); // 5 Damage.

            UsePower(Inquirer);

            // Only one card to return, and should destroy the thing, since movement is not destruction.
            AssertInTrash(mdp);
            AssertInPlayArea(baron, distortion); // Distortion handling logic should leave it in play near BB.
        }

        [Test()]
        public void TestHardFactsInnatePower()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "Workshopping.Inquirer", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "InquirerCharacter", "InquirerHardFactsCharacter" }
            };
            SetupGameController(setupItems, false, promos);

            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card distortion = PutInHand("YoureLookingPale");

            DecisionNextToCard = mdp;
            DecisionSelectCardToPlay = distortion;
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);

            UsePower(Inquirer); // 5 damage from play, 1 more from distortion attack.

            QuickHPCheck(-6);
        }

        [Test()]
        public void TestHardFactsInnatePowerImbuedVitalityFirst()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "Workshopping.Inquirer", "RealmOfDiscord"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "InquirerCharacter", "InquirerHardFactsCharacter" }
            };
            SetupGameController(setupItems, false, promos);

            StartGame();

            GoToPlayCardPhase(Inquirer);

            PlayCard("ImbuedVitality");

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card distortion = PutInHand("YoureLookingPale");

            DecisionNextToCard = mdp;
            DecisionSelectCards = new List<Card>() { distortion, mdp, distortion, mdp, mdp };

            QuickHPStorage(mdp);

            UsePower(Inquirer); // 5 damage from play, 2 more from 2 distortion attacks.

            QuickHPCheck(-7);
            AssertMaximumHitPoints(distortion, 6); // Ongoing affect re-applies over one-time effect.

            GoToStartOfTurn(Inquirer);
            AssertMaximumHitPoints(distortion, 6); // Return to Imbued Vitality.
        }

        [Test()]
        public void TestHardFactsInnatePowerImbuedVitalitySecond()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "Workshopping.Inquirer", "RealmOfDiscord"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "InquirerCharacter", "InquirerHardFactsCharacter" }
            };
            SetupGameController(setupItems, false, promos);

            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card distortion = PutInHand("YoureLookingPale");

            UsePower(Inquirer); // Card played, power used. HP at 3.
            PlayCard("ImbuedVitality"); // Card destroyed, but HP updated to 6.
            PlayCard(distortion); // Replay card.

            AssertMaximumHitPoints(distortion, 6); // This effect came later and should be more relevant.

            DestroyCard("ImbuedVitality"); // Destroy, HP should return to 3.

            AssertNotTarget(distortion); // No longer a target. This isn't great - I'd expect it to go back to 3, but there's little to do outside of debugging Handlabra code.
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

            QuickHandStorage(Inquirer.ToHero());
            UsePower(power);
            QuickHandCheck(2);
            AssertInTrash(distortion); // Distortion was destroyed.
        }

        [Test()]
        public void TestUndeniableFacts()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();
            GoToUsePowerPhase(Inquirer);

            // Put out example cards.
            PlayCard("YoureLookingPale");
            PlayCard("UndeniableFacts");

            QuickHandStorage(Inquirer);
            GoToStartOfTurn(Inquirer);
            QuickHandCheck(1); // Draw 1 card.
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
        public void TestYoureLookingPaleInitial()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = mdp;
            QuickHPStorage(mdp);

            PlayCard("YoureLookingPale");
            QuickHPCheck(-5); // 5 damage
        }

        [Test()]
        public void TestYoureLookingPaleAfter()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = mdp;
            QuickHPStorage(mdp);

            PlayCard("YoureLookingPale");

            GoToStartOfTurn(Inquirer);
            QuickHPCheck(-3); // 5 damage, healed 2.
        }

        [Test()]
        public void TestYoureOnOurSideInitial()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card bb = GetCardInPlay("BaronBladeCharacter");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = bb;
            QuickHPStorage(mdp, Inquirer.CharacterCard);

            PlayCard("YoureOnOurSide");
            QuickHPCheck(-2, 0); // 2 damage to others.
        }

        [Test()]
        public void TestYoureOnOurSideAfter()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card bb = GetCardInPlay("BaronBladeCharacter");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = bb;
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp, Inquirer.CharacterCard);

            PlayCard("YoureOnOurSide");

            GoToStartOfTurn(Inquirer);
            QuickHPCheck(-2, -1); // 2 damage to others, -1 to Inquirer
        }

        [Test()]
        public void TestIveFixedTheWoundInitial()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            DealDamage(Inquirer, Inquirer, 10, DamageType.Melee);

            GoToPlayCardPhase(Inquirer);

            QuickHPStorage(Inquirer);
            PlayCard("IveFixedTheWound");
            QuickHPCheck(5); // 5 Healing
        }

        [Test()]
        public void TestIveFixedTheWoundAfter()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            DealDamage(Inquirer, Inquirer, 10, DamageType.Melee);

            GoToPlayCardPhase(Inquirer);

            QuickHPStorage(Inquirer);
            PlayCard("IveFixedTheWound");
            GoToStartOfTurn(Inquirer);
            QuickHPCheck(3); // 5 Healing, 2 Damage.
        }

        [Test()]
        public void TestLookADistractionInitial()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card bb = GetCardInPlay("BaronBladeCharacter");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = bb;
            QuickHPStorage(mdp, Inquirer.CharacterCard);

            PlayCard("LookADistraction");
            QuickHPCheck(-4, 0); // 4 damage to others.
        }

        [Test()]
        public void TestLookADistractionAfter()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card bb = GetCardInPlay("BaronBladeCharacter");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionNextToCard = bb;
            DecisionSelectTarget = mdp;
            QuickHPStorage(mdp, Inquirer.CharacterCard);

            PlayCard("LookADistraction");

            GoToStartOfTurn(Inquirer);
            QuickHPCheck(-4, -1); // 4 damage to others, -1 to Inquirer
        }

        [Test()]
        public void TestUntilYouMakeIt()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            GoToPlayCardPhase(Inquirer);

            Card form = GetCard("ImAMolePerson");
            Card safeCard = GetCardWithLittleEffect(Inquirer);

            MoveCard(Inquirer, form, Inquirer.TurnTaker.Deck, true);
            PutInHand(safeCard);

            QuickHandStorage(Inquirer);

            List<Card> cards = new List<Card>
            {
                form, // First search for Form.
                safeCard // Then play safe card.
            };
            DecisionSelectCards = ArrangeDecisionCards(cards);

            PlayCard("UntilYouMakeIt");
            QuickHandCheck(0); // Draw 1, Play 1, Net 0.
            AssertNumberOfCardsInPlay(Inquirer, 3); // Should now have character card, new form, and card in play, since safe cards are preferred.
        }

        private IEnumerable<Card> ArrangeDecisionCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                yield return card;
            }
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

        [Test()]
        public void TestTheRightQuestions()
        {
            SetupGameController("BaronBlade", "Workshopping.Inquirer", "Megalopolis");
            StartGame();

            Card ongoing = PlayCard("LivingForceField");
            Card distortion = GetCard("YoureLookingPale");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Inquirer.PutInHand(distortion);

            GoToPlayCardPhase(Inquirer);

            DecisionDestroyCard = ongoing;
            DecisionSelectCardToPlay = distortion;
            DecisionSelectCardsIndex = 4; // Most other decisions are set, but the fourth for the return must be the played distortion.
            DecisionSelectCard = distortion;
            DecisionNextToCard = mdp;

            QuickHPStorage(mdp);

            PlayCard("TheRightQuestions");

            AssertInTrash(ongoing); // Destroyed Ongoing.
            AssertInHand(distortion);  // Played and returned distortion.
            QuickHPCheck(-5); // All damage dealt, no destroy trigger hit.
        }
    }
}