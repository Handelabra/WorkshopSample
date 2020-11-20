using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.BreachMage;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class BreachMageTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(BreachMageCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController BreachMage { get { return FindHero("BreachMage"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageTurnTakerController), BreachMage);
            Assert.IsInstanceOf(typeof(BreachMageCharacterCardController), BreachMage.CharacterCardController);

            Assert.AreEqual(27, BreachMage.CharacterCard.HitPoints);
            AssertNumberOfCardsInHand(BreachMage, 4); // Starting hand.
            AssertNumberOfCardsInDeck(BreachMage, 36); // Starting deck.
        }

        //[Test()]
        //public void TestInnatePower()
        //{
        //    SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

        //    StartGame();

        //    Card spell = PlayCard("ScryingBolt");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionSelectTarget = mdp;

        //    QuickHPStorage(mdp);
        //    UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
        //    QuickHPCheck(-4); // Damage Dealt.
        //    AssertInTrash(spell); // Spell destroyed.
        //}

        [Test()]
        public void TestInnatePowerDraw()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 0;

            QuickHandStorage(BreachMage);
            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate.
            QuickHandCheck(1); // Drawn card.
            QuickHPCheck(0); // No Damage Dealt.
            AssertIsInPlay(spell); // Spell unused.
        }

        [Test()]
        public void TestInnatePowerCast()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHandStorage(BreachMage);
            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate.
            QuickHandCheck(0); // No drawn card.
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell  used.
        }


        [Test()]
        public void TestInnatePowerB()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            List<Card> charges = new List<Card>
            {
                PlayCard("HammerCharm", 0),
                PlayCard("HammerCharm", 1)
            };

            QuickHandStorage(BreachMage);
            UsePower(BreachMage.CharacterCard, 1); // Charge innate.
            QuickHandCheck(5); // 5 Cards Drawn.
            AssertInTrash(charges); // All used charges in trash.
        }

        [Test()]
        public void TestTwincasterInnatePower()
        {
            List<string> identifiers = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "BreachMageCharacter", "BreachMageTwincasterCharacter" }
            };

            SetupGameController(identifiers, false, promos);

            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-2); // Damage Dealt.
        }

        [Test()]
        public void TestTwincasterInnatePowerB()
        {
            List<string> identifiers = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "BreachMageCharacter", "BreachMageTwincasterCharacter" }
            };

            SetupGameController(identifiers, false, promos);

            StartGame();

            List<Card> usedCards = new List<Card>()
            {
                PlayCard("HammerCharm", 0),
                PlayCard("HammerCharm", 1),
                PlayCard("ScryingBolt")
            };
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 1);
            QuickHPCheck(-8); // Damage Dealt twice.
            AssertInTrash(usedCards); // All used charges in trash.
        }

        [Test()]
        public void TestCycleOfMagic()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            QuickHandStorage(BreachMage);
            PlayCard("CycleOfMagic");
            QuickHPCheck(-4); // Damage Dealt.
            QuickHandCheck(2); // 2 Cards Drawn.
            AssertInDeck(spell);
        }

        [Test()]
        public void TestFocusBreach()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            List<Card> cards = new List<Card>
            {
                PutInDeck("OpenBreach"),
                PutInHand("ScryingBolt")
            };

            DecisionSelectCards = cards;
            // Put the breach into play, then play the spell as a safe play.

            QuickHandStorage(BreachMage);
            PlayCard("FocusBreach");
            AssertInPlayArea(BreachMage, cards[0]);
            AssertInPlayArea(BreachMage, cards[1]);
            QuickHandCheck(0); // One drawn, one played.
        }

        [Test()]
        public void TestOpenBreach()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

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
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

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
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.

            // TODO: Add scrying test at some point! (Right now, more complex than it's worth.)
        }

        [Test()]
        public void TestShine()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("Shine");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            QuickHandStorage(BreachMage);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-4); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestHauntingEcho()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("HauntingEcho");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card ongoing = PlayCard("LivingForceField");
            DecisionSelectTarget = mdp;
            DecisionDestroyCard = ongoing;
            DecisionSelectFunction = 1;


            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(ongoing, spell); // Ongoing & Spell destroyed.
        }

        [Test()]
        public void TestFlareCascade()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("FlareCascade");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
        }

        [Test()]
        public void TestFlareCascadeCharged()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            List<Card> charges = new List<Card>
            {
                PlayCard("HammerCharm", 0),
                PlayCard("HammerCharm", 1)
            };

            Card spell = PlayCard("FlareCascade");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;
            DecisionDestroyCards = charges.ToArray();
            DecisionYesNo = true;
            DecisionSelectFunction = 1;

            QuickHPStorage(mdp);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-9); // Damage Dealt.
            AssertInTrash(spell); // Spell destroyed.
            AssertInTrash(charges); // Charges used.
        }

        [Test()]
        public void TestMoltenWave()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("MoltenWave");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card bb = GetCardInPlay("BaronBladeCharacter");
            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            DealDamage(mdp, mdp, 7, DamageType.Fire); // Set up MDP to be destroyed so AoE also hits BB.

            QuickHPStorage(bb);
            UsePower(BreachMage.CharacterCard, 0); // Default Innate. Cast.
            QuickHPCheck(-3); // Damage Dealt.
            AssertInTrash(spell, mdp); // Spell destroyed, MDP destroyed via damage.
        }

        [Test()]
        public void TestHammerCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();
            GetCardInPlay("MobileDefensePlatform");
            Card ongoing = PlayCard("LivingForceField");
            DecisionDestroyCard = ongoing;

            PlayCard("HammerCharm");
            AssertInTrash(ongoing); // Ongoing & Spell destroyed.
        }

        [Test()]
        public void TestStaffCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("StaffCharm");
            QuickHPCheck(-6); // Damage Dealt. Base 4, plus 2 additional.
            AssertInTrash(spell); // Spell destroyed.
        }

        [Test()]
        public void TestSpiralCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card spell = PlayCard("ScryingBolt");
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("SpiralCharm");
            QuickHPCheck(-4); // Damage Dealt. Base 4.
            AssertInPlayArea(BreachMage, spell); // Spell not destroyed.
        }

        [Test()]
        public void TestVigorCharm()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.BreachMage", "Megalopolis");

            StartGame();

            Card card = PutInHand("VigorCharm");

            QuickHPStorage(BreachMage);
            QuickHandStorage(BreachMage);
            PlayCard(card);
            QuickHPCheck(-2); // Damage Dealt. Base 2.
            QuickHandCheck(2); // Draw 2.
            AssertInPlayArea(BreachMage, card); // Charm still in play.
        }
    }
}