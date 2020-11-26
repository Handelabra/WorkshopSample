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
        public void TestInnatePowerDiscardPrefix()
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
        public void TestInnatePowerDiscardSuffix()
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
        public void TestInnatePowerDiscardPrefixSuffix()
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
            PutInHand("Wild");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-6);
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
            PutInHand("Wild");
            PutInHand("OfDisruption");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("Ray");
            QuickHPCheck(-7); // 6 base damage, 1 self damage.
        }

        [Test()]
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
            QuickHPCheck(-1, -1, -1); // 1 base and irreducible. Self-damage is not irreducible.
        }

        [Test()]
        public void TestDiscardOfDisruption()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Wild");
            PutInHand("OfDisruption");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            Card play = PutInHand("Impact");


            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-3, -3, -4); // 3 base damage, 1 targetted damage for mdp only.
        }

        [Test()]
        public void TestDiscardOfHealing()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Wild");
            PutInHand("OfHealing");
            DealDamage(Wordsmith, Wordsmith.CharacterCard, 5, DamageType.Melee);

            Card mdp = GetCardInPlay("MobileDefensePlatform");


            QuickHPStorage(Wordsmith.CharacterCard, mdp);
            PlayCard("Impact");
            QuickHPCheck(0, -3); // 3 damage, 3 healing to self; 3 damage to enemy.
        }

        [Test()]
        public void TestDiscardOfInspiration()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Wild");
            PutInHand("OfInspiration");
            Card play = PutInHand("Impact");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            QuickHandStorage(Wordsmith, legacy);

            PlayCard(play);

            QuickHPCheck(-3,-3,-3); // 3 Damage each due to wild. 
            QuickHandCheck(-2, 1); // 3 used, 1 drawn for Wordsmith, 1 drawn for others.
        }

        [Test()]
        public void TestDiscardOfResonance()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Wordsmith", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Wordsmith);
            PutInHand("Wild");
            PutInHand("OfResonance");

            Card play = PutInHand("Impact");

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            QuickHPStorage(Wordsmith.CharacterCard, legacy.CharacterCard, mdp);
            PlayCard(play);
            QuickHPCheck(-3, -3, -4); // 3 base damage, 1 targetted damage for mdp only.
        }
    }
}