using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.Wordsmith;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class WordsmithTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(WordsmithCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController Wordsmith { get { return FindHero("Wordsmith"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Wordsmith);
            Assert.IsInstanceOf(typeof(WordsmithTurnTakerController), Wordsmith);
            Assert.IsInstanceOf(typeof(WordsmithCharacterCardController), Wordsmith.CharacterCardController);

            Assert.AreEqual(26, Wordsmith.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestDefineInnatePower()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "WordsmithCharacter", "WordsmithDefineCharacter" }
            };
            SetupGameController(setupItems, promoIdentifiers: promos);

            StartGame();

            DiscardAllCards(Wordsmith);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(Wordsmith);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDefineInnatePowerDiscardPrefix()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "WordsmithCharacter", "WordsmithDefineCharacter" }
            };
            SetupGameController(setupItems, promoIdentifiers: promos);

            StartGame();

            Card prefix = PutInHand("Controlled");
            PutInHand("OfHealing");
            UsePower(legacy);
            UsePower(legacy);
            UsePower(legacy);

            DecisionSelectCards = new List<Card>() { prefix, null, Wordsmith.CharacterCard };

            QuickHPStorage(Wordsmith.CharacterCard);
            UsePower(Wordsmith);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestDefineInnatePowerDiscardSuffix()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "WordsmithCharacter", "WordsmithDefineCharacter" }
            };
            SetupGameController(setupItems, promoIdentifiers: promos);

            PutInHand("Controlled");
            Card suffix = PutInHand("OfHealing");
            UsePower(legacy);
            UsePower(legacy);
            UsePower(legacy);

            DecisionSelectCards = new List<Card>() { null, suffix, Wordsmith.CharacterCard };

            QuickHPStorage(Wordsmith.CharacterCard);
            UsePower(Wordsmith);
            QuickHPCheck(-1); // Hit for 4, healed 3. 
        }

        [Test()]
        public void TestDefineInnatePowerDiscardPrefixSuffix()
        {
            IEnumerable<string> setupItems = new List<string>()
            {
                "BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis"
            };
            Dictionary<string, string> promos = new Dictionary<string, string>
            {
                { "WordsmithCharacter", "WordsmithDefineCharacter" }
            };
            SetupGameController(setupItems, promoIdentifiers: promos);

            StartGame();
            Card prefix = PutInHand("Controlled");
            Card suffix = PutInHand("OfHealing");
            UsePower(legacy);
            UsePower(legacy);
            UsePower(legacy);

            DecisionSelectCards = new List<Card>() { prefix, suffix, Wordsmith.CharacterCard };

            DealDamage(Wordsmith, Wordsmith.CharacterCard, 4, DamageType.Melee);

            QuickHPStorage(Wordsmith.CharacterCard);
            UsePower(Wordsmith);
            QuickHPCheck(2); // Hit for 1, healed 3. 
        }

        [Test()]
        public void TestEssenceNoDiscard()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-4);
        }

        [Test()]
        public void TestEssenceDiscardPrefix()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-5);
        }

        [Test()]
        public void TestEssenceDiscardSuffix()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("OfDisruption");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-5); // 4 base damage, 1 self damage.
        }

        [Test()]
        public void TestEssenceDiscardPrefixSuffix()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfDisruption");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-7); // 5 base damage, 2 self damage.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardControlled()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "TheBlock");

            StartGame();

            UsePower(legacy);
            UsePower(legacy);
            UsePower(legacy);

            DiscardAllCards(Wordsmith);
            PutInHand("Controlled");
            PutInHand("OfResonance");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card play = PutInHand("Impact");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-1, -1, -8); // 1 controlled. 4 boosted to MDP, doubled by Resonance.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardInspired()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "TheBlock");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfResonance");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card play = PutInHand("Impact");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-2, -2, -4); // 1 controlled. 2 boosted to MDP, doubled by Resonance.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardPiercing()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "TheBlock");

            StartGame();

            PutIntoPlay("DefensiveDisplacement");

            DiscardAllCards(Wordsmith);
            PutInHand("Piercing");
            PutInHand("OfResonance");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card play = PutInHand("Impact");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-1, -1, -2); // 1 base and irreducible. Self-damage is also irreducible.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardOfDisruption()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfDisruption");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card play = PutInHand("Impact");


            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-2, -2, -4); // 2 base damage, 2 targetted damage for mdp only.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardOfDisruptionRedirect()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "MrFixer", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfDisruption");
            PutIntoPlay("DrivingMantis");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTargets = new List<Card>() { fixer.CharacterCard, mdp, Wordsmith.CharacterCard, baron.CharacterCard }.ToArray();

            DecisionRedirectTarget = mdp;

            Card play = PutInHand("Impact");


            QuickHPStorage(Wordsmith.CharacterCard, fixer.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-2, 0, -8); // 2 base damage, 2 + 2 redirected to MDP + trigger, 2+2 direct to MDP without trigger.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardOfHealing()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfHealing");
            DealDamage(Wordsmith, Wordsmith.CharacterCard, 5, DamageType.Melee);

            Card mdp = GetCardInPlay("MobileDefensePlatform");


            QuickHPStorage(Wordsmith.CharacterCard, mdp);
            PlayCard("Impact");
            QuickHPCheck(1, -2); // 2 damage, 3 healing to self; 2 damage to enemy.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardOfAura()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfAura");
            Card play = PutInHand("Impact");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            QuickHandStorage(Wordsmith, legacy);

            PlayCard(play);

            QuickHPCheck(-2, -2, -2); // 3 Damage each due to Inspired. 
            QuickHandCheck(-2, 1); // 3 used, 1 drawn for Wordsmith, 1 drawn for others.
        }

        [Test()]
        [Category("DiscardModifier")]
        public void TestDiscardOfResonance()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Inspired");
            PutInHand("OfResonance");

            Card play = PutInHand("Impact");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-2, -2, -4); // 2 base damage, 2 targetted damage for mdp only.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayControlled()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "TheBlock");

            StartGame();
            Card[] cards = FindCardsWhere((Card c) => c.Identifier == "Inspired").ToArray();
            PutInHand(cards);
            PutInHand(FindCardsWhere((Card c) => c.Identifier == "Controlled").ToArray());

            DecisionSelectCards = cards;

            QuickHandStorage(Wordsmith);
            PlayCard("Controlled");
            QuickHandCheck(3); // 3 cards played, 6 cards drawn.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayInspired()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "TheBlock");

            StartGame();
            Card[] cards = FindCardsWhere((Card c) => c.Identifier == "Inspired").ToArray();
            PutInHand(cards);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = cards;

            QuickHandStorage(Wordsmith);
            PlayCard("Inspired");
            QuickHandCheck(2); // 1 cards played, 3 cards drawn.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayPiercing()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Ra", "TheBlock");

            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            // TODO: Find a better option!
            // For whatever reason, the power selection logic is always defaulting to Wordsmith's, so just consume it so it can default otherwise.
            UsePower(Wordsmith);

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Piercing");
            QuickHPCheck(-2); // Damage was boosted.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayOfAura()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "TheBlock");

            StartGame();


            Card card = PutInHand("OfAura");
            Card environment = PutIntoPlay("DefensiveDisplacement");

            QuickHandStorage(Wordsmith);
            PlayCard(card);
            QuickHandCheckZero(); // One played, one drawn.
            AssertInTrash(environment); // Destroyed.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayOfDisruption()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "TheBlock");

            StartGame();

            Card card = PutInHand("OfDisruption");
            Card ongoing = PutIntoPlay("LivingForceField");

            QuickHandStorage(Wordsmith);
            PlayCard(card);
            QuickHandCheckZero(); // One played, one drawn.
            AssertInTrash(ongoing); // Destroyed.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayOfHealing()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "TheBlock");

            StartGame();

            Card card = PutInHand("OfHealing");

            DealDamage(Wordsmith, Wordsmith.CharacterCard, 5, DamageType.Melee);

            QuickHPStorage(Wordsmith);
            QuickHandStorage(Wordsmith);
            PlayCard(card);
            QuickHPCheck(2); // Heal 2. 
            QuickHandCheckZero(); // One played, one drawn.
        }

        [Test]
        [Category("DiscardModifier")]
        public void TestPlayOfResonance()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "TheBlock");

            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            // TODO: Find a better option!
            // For whatever reason, the power selection logic is always defaulting to Wordsmith's, so just consume it so it can default otherwise.

            DecisionSelectTarget = mdp;

            Card card = PutInHand("OfResonance");

            DealDamage(Wordsmith, Wordsmith.CharacterCard, 5, DamageType.Melee);

            QuickHPStorage(mdp);
            QuickHandStorage(Wordsmith);
            PlayCard(card);
            QuickHPCheck(-2); // Deal 2. 
            QuickHandCheckZero(); // One played, one drawn.
        }
    }
}