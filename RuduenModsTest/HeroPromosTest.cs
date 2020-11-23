using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenPromosWorkshop.HeroPromos;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class HeroPromosTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenPromosWorkshop", Assembly.GetAssembly(typeof(HeroPromosCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController HeroPromos { get { return FindHero("HeroPromos"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(HeroPromos);
            Assert.IsInstanceOf(typeof(HeroPromosTurnTakerController), HeroPromos);
            Assert.IsInstanceOf(typeof(HeroPromosCharacterCardController), HeroPromos.CharacterCardController);

            Assert.AreEqual(0, HeroPromos.CharacterCard.HitPoints);
        }

        //[Test()]
        //public void TestModWorksPostGameStart()
        //{
        //    SetupGameController("BaronBlade", "Legacy", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

        //    StartGame();

        //    Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());
        //    // TurnTakerController count still exists, though one is null.

        //    Assert.IsNull(HeroPromos);
        //    Assert.IsInstanceOf(typeof(HeroPromosTurnTakerController), HeroPromos);
        //    Assert.IsInstanceOf(typeof(HeroPromosCharacterCardController), HeroPromos.CharacterCardController);

        //    Assert.AreEqual(100, HeroPromos.CharacterCard.HitPoints);
        //}


        //[Test()]
        //public void TestInnatePower()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

        //    StartGame();

        //    // Sample Power - Draw 3 Cards.

        //    QuickHandStorage(HeroPromos.ToHero());
        //    UsePower(HeroPromos.CharacterCard);
        //    QuickHandCheck(0);
        //}

        [Test()]
        public void TestExpatriettePowerDeckA()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "Expatriette", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();

            Card equipment = PutOnDeck("Pride");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectPower = equipment;
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(expatriette, 1);
            AssertInPlayArea(expatriette, equipment); // Equipment played. 
            QuickHPCheck(-1); // Damage dealt. 
        }

        [Test()]
        public void TestExpatriettePowerDeckB()
        {
            // Not Equipment Test
            SetupGameController("BaronBlade", "Expatriette", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();

            Card ongoing = PutOnDeck("HairtriggerReflexes");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(expatriette, 1);
            AssertInTrash(expatriette, ongoing); // Card not played.
            QuickHPCheck(0); // No damage dealt - no power.
        }

        [Test()]
        public void TestExpatriettePowerDeckChain()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "Expatriette", "TheArgentAdept", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();

            Card safeCard = PutInHand("AssaultRifle");
            Card equipment = PutOnDeck("Pride");
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            PlayCard("InspiringSupertonic");

            DecisionSelectCard = safeCard;

            UsePower(expatriette); // Use power so it's 'consumed' for testing purposes. Play a safe card.

            DecisionSelectPowers = new List<Card>() { expatriette.CharacterCard, equipment }.ToArray();
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(adept);
            AssertInPlayArea(expatriette, equipment); // Equipment played. 
            QuickHPCheck(-1); // Damage dealt. 
        }

        [Test()]
        public void TestExpatriettePowerDeckChainB()
        {
            // TODO: Set up a scenario where the power use can trigger another power use and link in. Probably safe since card source seems to depend on chains.
        }

        [Test()]
        public void TestExpatriettePowerNoDeck()
        {
            // No cards in deck test.
            SetupGameController("BaronBlade", "Expatriette", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();

            PutInTrash(expatriette.HeroTurnTaker.Deck.Cards); // Move all cards in deck to trash.
            Card ongoing = PutInHand("HairtriggerReflexes");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCard = ongoing;
            DecisionSelectTarget = mdp;

            AssertNumberOfCardsInDeck(expatriette, 0); // Deck remains empty.
            QuickHandStorage(expatriette);
            UsePower(expatriette, 1);
            AssertNumberOfCardsInDeck(expatriette, 0); // Deck remains empty.
        }

        [Test()]
        public void TestMrFixerPowerA()
        {
            // Tool Test
            SetupGameController("BaronBlade", "MrFixer", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card tool = PutOnDeck("DualCrowbars");

            DecisionSelectTarget = mdp;

            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer, 1);
            QuickHPCheck(-1, -1);
            AssertInPlayArea(fixer, tool); // Card played. 
        }

        [Test()]
        public void TestMrFixerPowerB()
        {
            // Style Test
            SetupGameController("BaronBlade", "MrFixer", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            Card style = PutOnDeck("GreaseMonkeyFist");

            DecisionSelectTarget = mdp;

            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer, 1);
            QuickHPCheck(-1, -1);
            AssertInPlayArea(fixer, style); // Card played. 
        }

        [Test()]
        public void TestMrFixerPowerC()
        {
            // Style Test
            SetupGameController("BaronBlade", "MrFixer", "RuduenPromosWorkshop.HeroPromos", "Megalopolis");

            StartGame();
            PutInTrash(fixer.HeroTurnTaker.Deck.Cards.Where((Card c) => c.IsTool || c.IsStyle)); // Move all tools and styles to trash.
            Card mdp = GetCardInPlay("MobileDefensePlatform");


            DecisionSelectTarget = mdp;


            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer, 1);
            QuickHPCheck(-1, -1);
            AssertNotInPlay((Card c) => c.IsTool || c.IsStyle); // No tools or styles were played. 
            AssertNumberOfCardsInDeck(fixer, 0); // Deck was emptied as part of reveal/discard. 
        }


        //[Test()]
        //public void TestInnatePower()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");

        //    StartGame();

        //    QuickHandStorage(Inquirer.ToHero());
        //    UsePower(Inquirer.CharacterCard);
        //    QuickHandCheck(1);
        //}

        //[Test()]
        //public void TestLiesOnLiesInnatePower()
        //{
        //    IEnumerable<string> setupItems = new List<string>()
        //    {
        //        "BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis"
        //    };
        //    Dictionary<string, string> promos = new Dictionary<string, string>
        //    {
        //        { "InquirerCharacter", "InquirerLiesOnLiesCharacter" }
        //    };
        //    SetupGameController(setupItems, false, promos);

        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = mdp;

        //    Card distortion = PlayCard("YoureLookingPale"); // 5 Damage.

        //    UsePower(Inquirer);

        //    // Only one card to return, and should destroy the thing, since movement is not destruction.
        //    AssertInTrash(mdp);
        //    AssertInPlayArea(baron, distortion); // Distortion handling logic should leave it in play near BB.
        //}

        //[Test()]
        //public void TestHardFactsInnatePower()
        //{
        //    IEnumerable<string> setupItems = new List<string>()
        //    {
        //        "BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis"
        //    };
        //    Dictionary<string, string> promos = new Dictionary<string, string>
        //    {
        //        { "InquirerCharacter", "InquirerHardFactsCharacter" }
        //    };
        //    SetupGameController(setupItems, false, promos);

        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    Card distortion = PutInHand("YoureLookingPale");

        //    DecisionNextToCard = mdp;
        //    DecisionSelectCardToPlay = distortion;
        //    DecisionSelectTarget = mdp;

        //    QuickHPStorage(mdp);

        //    UsePower(Inquirer); // 5 damage from play, 1 more from distortion attack.

        //    QuickHPCheck(-6);
        //}

        //[Test()]
        //public void TestHardFactsInnatePowerImbuedVitalityFirst()
        //{
        //    IEnumerable<string> setupItems = new List<string>()
        //    {
        //        "BaronBlade", "RuduenPromosWorkshop.Inquirer", "RealmOfDiscord"
        //    };
        //    Dictionary<string, string> promos = new Dictionary<string, string>
        //    {
        //        { "InquirerCharacter", "InquirerHardFactsCharacter" }
        //    };
        //    SetupGameController(setupItems, false, promos);

        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    PlayCard("ImbuedVitality");

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    Card distortion = PutInHand("YoureLookingPale");

        //    DecisionNextToCard = mdp;
        //    DecisionSelectCards = new List<Card>() { distortion, mdp, distortion, mdp, mdp };

        //    QuickHPStorage(mdp);

        //    UsePower(Inquirer); // 5 damage from play, 2 more from 2 distortion attacks.

        //    QuickHPCheck(-7);
        //    AssertMaximumHitPoints(distortion, 6); // Ongoing affect re-applies over one-time effect.

        //    GoToStartOfTurn(Inquirer);
        //    AssertMaximumHitPoints(distortion, 6); // Return to Imbued Vitality.
        //}

        //[Test()]
        //public void TestHardFactsInnatePowerImbuedVitalitySecond()
        //{
        //    IEnumerable<string> setupItems = new List<string>()
        //    {
        //        "BaronBlade", "RuduenPromosWorkshop.Inquirer", "RealmOfDiscord"
        //    };
        //    Dictionary<string, string> promos = new Dictionary<string, string>
        //    {
        //        { "InquirerCharacter", "InquirerHardFactsCharacter" }
        //    };
        //    SetupGameController(setupItems, false, promos);

        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card distortion = PutInHand("YoureLookingPale");

        //    UsePower(Inquirer); // Card played, power used. HP at 3.
        //    PlayCard("ImbuedVitality"); // Card destroyed, but HP updated to 6.
        //    PlayCard(distortion); // Replay card.

        //    AssertMaximumHitPoints(distortion, 6); // This effect came later and should be more relevant.

        //    DestroyCard("ImbuedVitality"); // Destroy, HP should return to 3.

        //    AssertNotTarget(distortion); // No longer a target. This isn't great - I'd expect it to go back to 3, but there's little to do outside of debugging Handlabra code.
        //}

        //[Test()]
        //public void TestTheLieTheyTellThemselves()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();
        //    GoToUsePowerPhase(Inquirer);

        //    // Put out example cards.
        //    Card distortion = PlayCard("YoureLookingPale");
        //    Card power = PlayCard("TheLieTheyTellThemselves");

        //    QuickHandStorage(Inquirer.ToHero());
        //    UsePower(power);
        //    QuickHandCheck(2);
        //    AssertInTrash(distortion); // Distortion was destroyed.
        //}

        //[Test()]
        //public void TestUndeniableFacts()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();
        //    GoToUsePowerPhase(Inquirer);

        //    // Put out example cards.
        //    PlayCard("YoureLookingPale");
        //    PlayCard("UndeniableFacts");

        //    QuickHandStorage(Inquirer);
        //    GoToStartOfTurn(Inquirer);
        //    QuickHandCheck(1); // Draw 1 card.
        //}

        //[Test()]
        //public void TestForms()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    PlayCard("ImAMolePerson");
        //    PlayCard("ImAVictorian");
        //    PlayCard("ImANinja");

        //    DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);

        //    DecisionSelectTarget = mdp;
        //    DecisionYesNo = true;
        //    DecisionDoNotSelectCard = SelectionType.DestroyCard;

        //    AssertNumberOfCardsInTrash(Inquirer, 0);
        //    GoToEndOfTurn(Inquirer);
        //    AssertNumberOfCardsInTrash(Inquirer, 2); // Discards a card, but do not play as a result.

        //    GoToStartOfTurn(Inquirer);
        //    AssertNumberOfCardsInTrash(Inquirer, 2); // One successful shuffle, two failed shuffles.

        //    QuickHPCheck(-2, 2); // Two total damage - 1 base, 1 buff. 2 Healing - 1 base, 1 buff.
        //}

        //[Test()]
        //public void TestBackupPlan()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);
        //    Card power = PlayCard("BackupPlan");

        //    DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);
        //    QuickHandStorage(Inquirer);

        //    DecisionSelectTarget = mdp;
        //    UsePower(power);

        //    QuickHPCheck(-1, 1);
        //    AssertNumberOfCardsInTrash(Inquirer, 1); // Discarded.
        //    QuickHandCheck(0); // Discard and draw for net 0 change.
        //}

        //[Test()]
        //public void TestYoureLookingPaleInitial()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = mdp;
        //    QuickHPStorage(mdp);

        //    PlayCard("YoureLookingPale");
        //    QuickHPCheck(-5); // 5 damage
        //}

        //[Test()]
        //public void TestYoureLookingPaleAfter()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = mdp;
        //    QuickHPStorage(mdp);

        //    PlayCard("YoureLookingPale");

        //    GoToStartOfTurn(Inquirer);
        //    QuickHPCheck(-3); // 5 damage, healed 2.
        //}

        //[Test()]
        //public void TestYoureOnOurSideInitial()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card bb = GetCardInPlay("BaronBladeCharacter");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = bb;
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);

        //    PlayCard("YoureOnOurSide");
        //    QuickHPCheck(-2, 0); // 2 damage to others.
        //}

        //[Test()]
        //public void TestYoureOnOurSideAfter()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card bb = GetCardInPlay("BaronBladeCharacter");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = bb;
        //    DecisionSelectTarget = mdp;
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);

        //    PlayCard("YoureOnOurSide");

        //    GoToStartOfTurn(Inquirer);
        //    QuickHPCheck(-2, -1); // 2 damage to others, -1 to Inquirer
        //}

        //[Test()]
        //public void TestIveFixedTheWoundInitial()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    DealDamage(Inquirer, Inquirer, 10, DamageType.Melee);

        //    GoToPlayCardPhase(Inquirer);

        //    QuickHPStorage(Inquirer);
        //    PlayCard("IveFixedTheWound");
        //    QuickHPCheck(5); // 5 Healing
        //}

        //[Test()]
        //public void TestIveFixedTheWoundAfter()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    DealDamage(Inquirer, Inquirer, 10, DamageType.Melee);

        //    GoToPlayCardPhase(Inquirer);

        //    QuickHPStorage(Inquirer);
        //    PlayCard("IveFixedTheWound");
        //    GoToStartOfTurn(Inquirer);
        //    QuickHPCheck(3); // 5 Healing, 2 Damage.
        //}

        //[Test()]
        //public void TestLookADistractionInitial()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card bb = GetCardInPlay("BaronBladeCharacter");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = bb;
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);

        //    PlayCard("LookADistraction");
        //    QuickHPCheck(-4, 0); // 4 damage to others.
        //}

        //[Test()]
        //public void TestLookADistractionAfter()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card bb = GetCardInPlay("BaronBladeCharacter");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    DecisionNextToCard = bb;
        //    DecisionSelectTarget = mdp;
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);

        //    PlayCard("LookADistraction");

        //    GoToStartOfTurn(Inquirer);
        //    QuickHPCheck(-4, -1); // 4 damage to others, -1 to Inquirer
        //}

        //[Test()]
        //public void TestUntilYouMakeIt()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    Card persona = GetCard("ImAMolePerson");
        //    Card safeCard = GetCardWithLittleEffect(Inquirer);

        //    MoveCard(Inquirer, persona, Inquirer.TurnTaker.Deck, true);
        //    PutInHand(safeCard);

        //    QuickHandStorage(Inquirer);

        //    List<Card> cards = new List<Card>
        //    {
        //        persona, // First search for persona.
        //        safeCard // Then play safe card.
        //    };
        //    DecisionSelectCards = ArrangeDecisionCards(cards);

        //    PlayCard("UntilYouMakeIt");
        //    QuickHandCheck(0); // Draw 1, Play 1, Net 0.
        //    AssertNumberOfCardsInPlay(Inquirer, 3); // Should now have character card, new persona, and card in play, since safe cards are preferred.
        //}

        //private IEnumerable<Card> ArrangeDecisionCards(List<Card> cards)
        //{
        //    foreach (Card card in cards)
        //    {
        //        yield return card;
        //    }
        //}

        //[Test()]
        //public void TestFisticuffs()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    GoToPlayCardPhase(Inquirer);

        //    DealDamage(Inquirer, Inquirer, 3, DamageType.Melee);
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");
        //    QuickHPStorage(mdp, Inquirer.CharacterCard);
        //    QuickHandStorage(Inquirer);

        //    DecisionSelectTarget = mdp;
        //    PlayCard("Fisticuffs");

        //    QuickHPCheck(-3, 2);
        //    AssertNumberOfCardsInTrash(Inquirer, 2); // Discarded and played card.
        //    QuickHandCheck(-1); // Fisticuffs source is ambiguous, so don't check hand - just a net of -1.
        //}

        //[Test()]
        //public void TestTheRightQuestions()
        //{
        //    SetupGameController("BaronBlade", "RuduenPromosWorkshop.Inquirer", "Megalopolis");
        //    StartGame();

        //    Card ongoing = PlayCard("LivingForceField");
        //    Card distortion = GetCard("YoureLookingPale");
        //    Card mdp = GetCardInPlay("MobileDefensePlatform");

        //    Inquirer.PutInHand(distortion);

        //    GoToPlayCardPhase(Inquirer);

        //    DecisionDestroyCard = ongoing;
        //    DecisionSelectCardToPlay = distortion;
        //    DecisionSelectCardsIndex = 4; // Most other decisions are set, but the fourth for the return must be the played distortion.
        //    DecisionSelectCard = distortion;
        //    DecisionNextToCard = mdp;

        //    QuickHPStorage(mdp);

        //    PlayCard("TheRightQuestions");

        //    AssertInTrash(ongoing); // Destroyed Ongoing.
        //    AssertInHand(distortion);  // Played and returned distortion.
        //    QuickHPCheck(-5); // All damage dealt, no destroy trigger hit.
        //}
    }
}