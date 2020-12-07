using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.HeroPromos;
using System.Collections.Generic;
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
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(PromoDefaultCharacterCardController))); // replace with your own namespace
        }

        [Test()]
        public void TestAbsoluteZeroPlay()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/RuduenWorkshop.AbsoluteZeroOverchillCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(az.CharacterCard.IsPromoCard);
            Card card = PutInHand("CryoChamber");
            PutIntoPlay("IsothermicTransducer");

            DecisionSelectCard = card;
            DecisionSelectFunction = 0;

            QuickHPStorage(az);
            QuickHandStorage(az);
            UsePower(az);
            QuickHPCheck(-1); // Damage dealt through DR.
            QuickHandCheck(1); // 1 Played, 2 Drawn.
            AssertInPlayArea(az, card);
        }

        [Test()]
        public void TestAbsoluteZeroDestroy()
        {
            SetupGameController("BaronBlade", "AbsoluteZero/RuduenWorkshop.AbsoluteZeroOverchillCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(az.CharacterCard.IsPromoCard);
            List<Card> transducers = new List<Card>(this.GameController.FindCardsWhere((Card c) => c.Identifier == "IsothermicTransducer"));
            PlayCard(transducers[0]);
            DiscardAllCards(az);
            PutInHand(transducers[1]);

            DecisionSelectFunction = 1;

            // Only available card is a copy of a limited card. Play will fail, cause destroy.

            QuickHPStorage(az);
            QuickHandStorage(az);
            UsePower(az);
            QuickHPCheck(-1); // Damage dealt through DR.
            QuickHandCheck(2); // No play, draw 2.
            AssertInTrash(az, transducers[0]);
        }

        [Test()]
        public void TestAkashThriyaNoTrash()
        {
            SetupGameController("BaronBlade", "AkashThriya/RuduenWorkshop.AkashThriyaSeedRotationCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(thriya.CharacterCard.IsPromoCard);

            // Set seed so it doesn't randomly use the self-destructing one.
            Card seed = PutOnDeck("NoxiousPod");

            UsePower(thriya);
            AssertIsInPlay(seed);
        }

        [Test()]
        public void TestAkashThriyaTrash()
        {
            SetupGameController("BaronBlade", "AkashThriya/RuduenWorkshop.AkashThriyaSeedRotationCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(thriya.CharacterCard.IsPromoCard);

            // Set seed so it doesn't randomly use the self-destructing one.
            Card seed = PutOnDeck("NoxiousPod");
            Card cycled = PutInTrash("VitalizedThorns");

            UsePower(thriya);
            AssertIsInPlay(seed);
            AssertInDeck(cycled);
        }

        [Test()]
        public void TestArgentAdeptPlaySafe()
        {
            SetupGameController("BaronBlade", "TheArgentAdept/RuduenWorkshop.TheArgentAdeptAriaCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(adept.CharacterCard.IsPromoCard);

            Card card = PutInHand("DrakesPipes");

            DecisionSelectCard = card;

            UsePower(adept);
            AssertInPlayArea(adept, card);
        }

        [Test()]
        public void TestArgentAdeptPlayPerform()
        {
            SetupGameController("BaronBlade", "TheArgentAdept/RuduenWorkshop.TheArgentAdeptAriaCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(adept.CharacterCard.IsPromoCard);

            Card card = PutInHand("ScherzoOfFrostAndFlame");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectCard = card;

            QuickHPStorage(mdp);
            UsePower(adept);
            AssertInPlayArea(adept, card);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestArgentAdeptPlayPerformAccompany()
        {
            SetupGameController("BaronBlade", "TheArgentAdept/RuduenWorkshop.TheArgentAdeptAriaCharacter", "Legacy", "TheBlock");

            StartGame();

            Assert.IsTrue(adept.CharacterCard.IsPromoCard);

            Card card = PutInHand("SyncopatedOnslaught");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCards = new Card[] { card, adept.CharacterCard, mdp, mdp };

            QuickHPStorage(mdp);
            UsePower(adept);
            AssertInPlayArea(adept, card);
            QuickHPCheck(-2);
        }

        [Test()]
        public void TestBenchmarkNoSoftware()
        {
            SetupGameController("BaronBlade", "Benchmark/RuduenWorkshop.BenchmarkDownloadManagerCharacter", "Legacy", "TheBlock");

            StartGame();

            DiscardAllCards(bench);

            Assert.IsTrue(bench.CharacterCard.IsPromoCard);
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(legacy);
            UsePower(bench);
            QuickHPCheck(-1);
        }

        [Test()]
        public void TestBenchmarkSoftwareAndIncap()
        {
            SetupGameController("BaronBlade", "Benchmark/RuduenWorkshop.BenchmarkDownloadManagerCharacter", "Legacy", "TheBlock");

            StartGame();

            Assert.IsTrue(bench.CharacterCard.IsPromoCard);
            Card software = PutInHand("AutoTargetingProtocol");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectCardToPlay = software;

            QuickHPStorage(mdp);
            UsePower(bench);
            GoToStartOfTurn(bench);
            QuickHPCheck(-1);
            AssertInPlayArea(bench, software); // Card is in play.

            DestroyCard(bench.CharacterCard);
            AssertNotInPlay(software); // Removed after during incap.
        }

        // TODO: Fix if Handlabra fix!
        [Test(Description = "Failing Handlabra Case", ExpectedResult = false)]
        public bool TestBrokenBenchmarkSoftwareIndestructibleBounce()
        {
            SetupGameController("BaronBlade", "Benchmark/RuduenWorkshop.BenchmarkDownloadManagerCharacter", "Legacy", "TheBlock");

            StartGame();

            Assert.IsTrue(bench.CharacterCard.IsPromoCard);
            Card software = PutInHand("AutoTargetingProtocol");
            PutInHand("AllyMatrix"); // Add second one so the decision selection always has a choice.
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            // Play, damage, bounce, play.
            DecisionSelectCards = new Card[] { software, mdp, software, null };

            QuickHPStorage(mdp);
            UsePower(bench);
            PutIntoPlay("OverhaulLoadout");
            return (software.Location == bench.TurnTaker.PlayArea); // The card should be in the play area! Expect a fail right now.
        }

        [Test()]
        public void TestBunkerFullSalvoNoOtherPower()
        {
            SetupGameController("BaronBlade", "Bunker/RuduenWorkshop.BunkerFullSalvoCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);

            GoToUsePowerPhase(bunker);

            QuickHandStorage(bunker);
            UsePower(bunker);
            QuickHandCheck(2); //  2 Drawn.

            AssertPhaseActionCount(0); // Powers used.
        }

        [Test()]
        public void TestBunkerFullSalvoOneOtherPower()
        {
            SetupGameController("BaronBlade", "Bunker/RuduenWorkshop.BunkerFullSalvoCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            PutIntoPlay("FlakCannon");

            DecisionSelectTarget = mdp;

            GoToUsePowerPhase(bunker);

            QuickHandStorage(bunker);
            UsePower(bunker);
            QuickHandCheck(1); //  2 Drawn, 1 Discarded
            AssertNumberOfCardsInTrash(bunker, 1); // 1 Discarded.
            AssertPhaseActionCount(1); // 1 Power Remaining
        }

        [Test()]
        public void TestBunkerModeShiftRecharge()
        {
            SetupGameController("BaronBlade", "Bunker/RuduenWorkshop.BunkerModeShiftCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);

            Card recharge = PutOnDeck("RechargeMode");
            DecisionSelectCard = recharge;

            GoToUsePowerPhase(bunker);

            UsePower(bunker);
            AssertPhaseActionCount(0); // Powers used.

            GoToDrawCardPhase(bunker);
            AssertPhaseActionCount(2); // 2 Draws
        }

        [Test()]
        public void TestBunkerModeShiftTurret()
        {
            SetupGameController("BaronBlade", "Bunker/RuduenWorkshop.BunkerModeShiftCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);

            Card turret = PutOnDeck("TurretMode");
            DecisionSelectCard = turret;

            GoToUsePowerPhase(bunker);

            UsePower(bunker);
            AssertPhaseActionCount(1); // 1 Use Remaining from Turret Mode.
        }

        [Test()]
        public void TestBunkerModeShiftUpgrade()
        {
            SetupGameController("BaronBlade", "Bunker/RuduenWorkshop.BunkerModeShiftCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(bunker.CharacterCard.IsPromoCard);

            Card upgrade = PutOnDeck("UpgradeMode");
            Card equip = PutInHand("FlakCannon");
            DecisionSelectCards = new Card[] { upgrade, equip };

            GoToUsePowerPhase(bunker);

            UsePower(bunker);
            AssertPhaseActionCount(0); // Powers used.
            AssertIsInPlay(equip);
        }

        [Test()]
        public void TestCaptainCosmicNoConstruct()
        {
            SetupGameController("BaronBlade", "CaptainCosmic/RuduenWorkshop.CaptainCosmicCosmicShieldingCharacter", "Legacy", "TheBlock");

            StartGame();

            Assert.IsTrue(cosmic.CharacterCard.IsPromoCard);

            QuickHandStorage(cosmic);
            UsePower(cosmic);
            QuickHandCheck(1);
        }

        [Test()]
        public void TestCaptainCosmicConstruct()
        {
            SetupGameController("BaronBlade", "CaptainCosmic/RuduenWorkshop.CaptainCosmicCosmicShieldingCharacter", "Legacy", "TheBlock");

            StartGame();

            Assert.IsTrue(cosmic.CharacterCard.IsPromoCard);

            Card construct = PutIntoPlay("CosmicWeapon");

            QuickHandStorage(cosmic);
            QuickHPStorage(construct, cosmic.CharacterCard);
            UsePower(cosmic);
            QuickHandCheck(1); // Draw.
            DealDamage(legacy, construct, 2, DamageType.Melee);
            AssertNextToCard(construct, cosmic.CharacterCard);
            GoToStartOfTurn(cosmic);
            AssertInTrash(construct);
        }

        [Test()]
        public void TestChronoRanger()
        {
            SetupGameController("BaronBlade", "ChronoRanger/RuduenWorkshop.ChronoRangerHighNoonCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(chrono.CharacterCard.IsPromoCard);

            PlayCard("DefensiveDisplacement");

            Card card = PutInHand("TerribleTechStrike");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectFunction = 1;
            DecisionSelectTarget = mdp;

            DecisionSelectCard = card;

            QuickHPStorage(chrono.CharacterCard, mdp);
            QuickHandStorage(chrono);
            UsePower(chrono);
            DealDamage(chrono.CharacterCard, mdp, 1, DamageType.Melee);
            DealDamage(mdp, chrono.CharacterCard, 1, DamageType.Melee);
            QuickHPCheck(-1, -1); // Damage dealt through DR.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestExpatriettePowerDeck()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "Expatriette/RuduenWorkshop.ExpatrietteQuickShotCharacter", "Megalopolis");

            Assert.IsTrue(expatriette.CharacterCard.IsPromoCard);

            StartGame();

            Card equipment = PutOnDeck("Pride");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectPower = equipment;
            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(expatriette);
            AssertInPlayArea(expatriette, equipment); // Equipment played.
            QuickHPCheck(-1); // Damage dealt.
        }

        [Test()]
        public void TestExpatriettePowerNoDeck()
        {
            // No cards in deck test.
            SetupGameController("BaronBlade", "Expatriette/RuduenWorkshop.ExpatrietteQuickShotCharacter", "Megalopolis");
            Assert.IsTrue(expatriette.CharacterCard.IsPromoCard);

            StartGame();

            PutInTrash(expatriette.HeroTurnTaker.Deck.Cards); // Move all cards in deck to trash.
            Card ongoing = PutInHand("HairtriggerReflexes");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCard = ongoing;
            DecisionSelectTarget = mdp;

            AssertNumberOfCardsInDeck(expatriette, 0); // Deck remains empty.
            QuickHandStorage(expatriette);
            UsePower(expatriette);
            AssertNumberOfCardsInDeck(expatriette, 0); // Deck remains empty.
        }

        [Test()]
        public void TestFanaticPlay()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "Fanatic/RuduenWorkshop.FanaticZealCharacter", "Megalopolis");

            Assert.IsTrue(fanatic.CharacterCard.IsPromoCard);

            StartGame();

            Card play = PutInHand("AegisOfResurrection");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            QuickHPStorage(fanatic.CharacterCard, mdp);
            UsePower(fanatic);
            AssertIsInPlay(play);
            QuickHPCheck(-1, -1); // Damage dealt to BB (canceled), mdp, and self.
        }

        [Test()]
        public void TestFanaticPower()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "Fanatic/RuduenWorkshop.FanaticZealCharacter", "Megalopolis");

            Assert.IsTrue(fanatic.CharacterCard.IsPromoCard);

            StartGame();

            PutIntoPlay("Absolution");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectFunction = 1;
            DecisionSelectTargets = new Card[] { mdp, baron.CharacterCard, mdp }; // Attempt to attack MDP twice.

            QuickHPStorage(fanatic.CharacterCard, mdp);
            UsePower(fanatic);
            QuickHPCheck(-1, -4); // Damage dealt to BB (canceled), mdp, and self.
        }

        [Test()]
        public void TestGuiseOngoing()
        {
            SetupGameController("BaronBlade", "Guise/RuduenWorkshop.GuiseShenanigansCharacter", "Megalopolis");
            Assert.IsTrue(guise.CharacterCard.IsPromoCard);

            StartGame();
            GoToUsePowerPhase(guise);
            Card ongoing = PutIntoPlay("GrittyReboot");

            QuickHandStorage(guise);
            UsePower(guise);
            QuickHandCheck(1); // 1 Card Drawn.
            DestroyCard(ongoing);
            AssertIsInPlay(ongoing);
            GoToStartOfTurn(guise);
            AssertIsInPlay(ongoing); // First new turn, should survive self-destruct.
            GoToStartOfTurn(guise);
            AssertInTrash(ongoing); // Second new turn. Without a power, it's gone! 
        }

        [Test()]
        public void TestGuiseNoOngoing()
        {
            SetupGameController("BaronBlade", "Guise/RuduenWorkshop.GuiseShenanigansCharacter", "Megalopolis");
            Assert.IsTrue(guise.CharacterCard.IsPromoCard);

            StartGame();
            GoToUsePowerPhase(guise);

            QuickHandStorage(guise);
            AssertNextMessage("Guise does not have any Ongoings in play, so he cannot make any indestructible. Whoops!");
            UsePower(guise);
            QuickHandCheck(1); // 1 Card Drawn.
        }


        [Test()]
        public void TestHaka()
        {
            SetupGameController("BaronBlade", "Haka/RuduenWorkshop.HakaVigorCharacter", "Megalopolis");
            Assert.IsTrue(haka.CharacterCard.IsPromoCard);

            StartGame();

            DiscardAllCards(haka);
            PutInHand("VitalitySurge");
            GoToUsePowerPhase(haka);

            UsePower(haka);
            AssertNumberOfCardsInHand(haka, 2); // Make sure the net effect is 2 cards in hand, even if the played card results in a draw. 
        }

        [Test()]
        public void TestHarpy()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "TheHarpy/RuduenWorkshop.TheHarpyExtremeCallingCharacter", "Megalopolis");

            Assert.IsTrue(harpy.CharacterCard.IsPromoCard);

            DecisionSelectWord = "Flip 2 {arcana}";

            StartGame();
            QuickHPStorage(harpy);
            UsePower(harpy);
            QuickHPCheck(-2); // Damage dealt.
            ;
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool), 1);
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.AvianControlPool), 4);
        }

        [Test()]
        public void TestHarpyFancierTrigger()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "TheHarpy/RuduenWorkshop.TheHarpyExtremeCallingCharacter", "Megalopolis");

            Assert.IsTrue(harpy.CharacterCard.IsPromoCard);

            StartGame();

            PutIntoPlay("HarpyHex");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectWord = "Flip 1 {avian}";
            DecisionSelectTarget = mdp;

            QuickHPStorage(harpy.CharacterCard, mdp);
            UsePower(harpy);
            QuickHPCheck(-1, -1); // Damage dealt.
            ;
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool), 4);
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.AvianControlPool), 1);
        }

        [Test()]
        public void TestHarpyNumerology()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "TheHarpy/RuduenWorkshop.TheHarpyExtremeCallingCharacter", "Megalopolis");

            Assert.IsTrue(harpy.CharacterCard.IsPromoCard);

            PutIntoPlay("AppliedNumerology");

            DecisionSelectWord = "Flip 1 {arcana}";

            StartGame();
            QuickHPStorage(harpy);
            UsePower(harpy);
            QuickHPCheck(-1); // Damage dealt.
            ;
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool), 2);
            AssertTokenPoolCount(harpy.CharacterCard.FindTokenPool(TokenPool.AvianControlPool), 3);
        }

        [Test()]
        public void TestKnyfePower()
        {
            // No cards in deck test.
            SetupGameController("BaronBlade", "Knyfe/RuduenWorkshop.KnyfeKineticLoopCharacter", "Megalopolis");
            Assert.IsTrue(knyfe.CharacterCard.IsPromoCard);

            StartGame();

            QuickHandStorage(knyfe);
            QuickHPStorage(knyfe);
            UsePower(knyfe);
            DealDamage(knyfe, knyfe, 1, DamageType.Energy);
            QuickHPCheck(0); // 1 damage, healed 1.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestKnyfePowerNoDamageDealt()
        {
            SetupGameController("BaronBlade", "Knyfe/RuduenWorkshop.KnyfeKineticLoopCharacter", "TheBlock");
            Assert.IsTrue(knyfe.CharacterCard.IsPromoCard);

            StartGame();

            DealDamage(knyfe, knyfe, 5, DamageType.Energy);

            PutIntoPlay("DefensiveDisplacement");

            QuickHandStorage(knyfe);
            QuickHPStorage(knyfe);
            UsePower(knyfe);
            DealDamage(knyfe, knyfe, 1, DamageType.Energy);
            QuickHPCheck(0); // No damage or healing.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestLegacyPlayOneShot()
        {
            SetupGameController("BaronBlade", "Legacy/RuduenWorkshop.LegacyInTheFrayCharacter", "Megalopolis");
            Assert.IsTrue(legacy.CharacterCard.IsPromoCard);

            StartGame();

            DiscardAllCards(legacy);
            Card card=PutInHand("FlyingSmash");
            GoToUsePowerPhase(legacy);

            DecisionSelectCardToPlay = card;

            QuickHandStorage(legacy);
            UsePower(legacy);
            AssertInTrash(card);
            QuickHandCheck(-1); // 1 played, 0 drawn.
        }

        [Test()]
        public void TestLegacyNotOneShot()
        {
            SetupGameController("BaronBlade", "Legacy/RuduenWorkshop.LegacyInTheFrayCharacter", "Megalopolis");
            Assert.IsTrue(legacy.CharacterCard.IsPromoCard);

            StartGame();

            DiscardAllCards(legacy);
            Card card = PutInHand("TheLegacyRing");
            GoToUsePowerPhase(legacy);

            QuickHandStorage(legacy);
            UsePower(legacy);
            AssertInHand(card);
            QuickHandCheck(1); // 0 played, 1 drawn.
        }

        [Test()]
        public void TestLifelineNormalHit()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Lifeline/RuduenWorkshop.LifelineEnergyTapCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(lifeline.CharacterCard.IsPromoCard);

            StartGame();
            UsePower(legacy);

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            DealDamage(lifeline, lifeline.CharacterCard, 5, DamageType.Melee);

            QuickHPStorage(lifeline.CharacterCard, mdp);
            UsePower(lifeline);
            QuickHPCheck(1, -2); // One successful hit, one HP regained. 
        }

        [Test()]
        public void TestLifelineRedirectedHit()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Lifeline/RuduenWorkshop.LifelineEnergyTapCharacter", "Legacy", "Tachyon", "Megalopolis");
            Assert.IsTrue(lifeline.CharacterCard.IsPromoCard);

            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");
            PutIntoPlay("SynapticInterruption");

            DecisionSelectCards = new Card[] { mdp, tachyon.CharacterCard, mdp };

            UsePower(legacy);
            UsePower(legacy); // Boost damage to 3 for redirect.

            DealDamage(lifeline, lifeline.CharacterCard, 5, DamageType.Melee);

            QuickHPStorage(lifeline.CharacterCard, tachyon.CharacterCard, mdp);
            UsePower(lifeline);
            QuickHPCheck(1, 0, -6); // Two hits but only one target damaged, so 1 HP.
        }

        [Test()]
        public void TestLuminaryNoDevice()
        {
            SetupGameController("BaronBlade", "Luminary/RuduenWorkshop.LuminaryReprogramCharacter", "Megalopolis");
            Assert.IsTrue(luminary.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(luminary);

            UsePower(luminary);
            AssertNumberOfCardsInTrash(luminary, 1);
        }

        [Test()]
        public void TestLuminaryDeviceDestroyed()
        {
            SetupGameController("BaronBlade", "Luminary/RuduenWorkshop.LuminaryReprogramCharacter", "Megalopolis");
            Assert.IsTrue(luminary.CharacterCard.IsPromoCard);

            StartGame();

            DiscardAllCards(luminary);
            Card card = PutInHand("RegressionTurret");
            Card destroyed = PutIntoPlay("BacklashGenerator");
            GoToUsePowerPhase(luminary);

            DecisionSelectCards = new Card[] { destroyed, card };

            UsePower(luminary);
            AssertInTrash(destroyed);
            AssertIsInPlay(card);
        }

        [Test()]
        public void TestLuminaryDevicenotDestroyed()
        {
            SetupGameController("BaronBlade", "Luminary/RuduenWorkshop.LuminaryReprogramCharacter", "Megalopolis");
            Assert.IsTrue(luminary.CharacterCard.IsPromoCard);

            StartGame();

            DiscardAllCards(luminary);
            Card card = PutInHand("RegressionTurret");
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            GoToUsePowerPhase(luminary);

            DecisionSelectCards = new Card[] { mdp, card };

            UsePower(luminary);
            AssertIsInPlay(mdp);
            AssertInHand(card);
        }

        [Test()]
        public void TestNaturalistAllIconsPower()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);
            Card power = PutInHand("NaturalFormsPower");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = power;

            UsePower(naturalist);
            AssertIsInPlay(power);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);
            // To check triggers, confirm that 2 cards were drawn, Naturalist regained 2 HP, and targets (including MDP) were dealt 1 damage.
            UsePower(power);
            QuickHPCheck(2, -1);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestNaturalistAllIconsPlay()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);
            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("PrimalCharge");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertInTrash(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);
            // To check triggers, confirm that 2 cards were drawn, Naturalist regained 2 HP, and targets (including MDP) were dealt 1 damage.
            UsePower(power);
            QuickHPCheck(2, -1);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestNaturalistNoIcons()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);

            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("NaturalBornVigor");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertIsInPlay(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);

            // To check triggers, confirm that no cards were drawn and no damage was healed or dealt.
            UsePower(power);

            QuickHPCheck(0, 0);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestNaturalistGazelle()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);

            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("CraftyAssault");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertInTrash(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);

            // To check triggers, confirm that 2 cards were drawn and no damage was healed or dealt.
            UsePower(power);

            QuickHPCheck(0, 0);
            QuickHandCheck(2);
        }

        [Test()]
        public void TestNaturalistRhino()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);

            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("IndomitableForce");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertIsInPlay(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);

            // To check triggers, confirm that no cards were drawn and 2 damage was healed, but none was dealt.
            UsePower(power);

            QuickHPCheck(2, 0);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestNaturalistCrocodile()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);

            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("ThePredatorsEye");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertInTrash(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);

            // To check triggers, confirm that no cards were drawn and 0 damage was healed, but 1 was dealt.
            UsePower(power);

            QuickHPCheck(0, -1);
            QuickHandCheck(0);
        }

        [Test()]
        public void TestNaturalistCombo()
        {
            SetupGameController("BaronBlade", "TheNaturalist/RuduenWorkshop.TheNaturalistVolatileFormCharacter", "Megalopolis");
            Assert.IsTrue(naturalist.CharacterCard.IsPromoCard);

            StartGame();

            GoToUsePowerPhase(naturalist);

            Card power = PutIntoPlay("NaturalFormsPower");
            Card play = PutInHand("ThePredatorsEye");
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            PutIntoPlay("TheNimbleGazelle");

            DecisionSelectCardToPlay = play;

            UsePower(naturalist);
            AssertInTrash(play);

            DealDamage(naturalist, naturalist.CharacterCard, 5, DamageType.Toxic);

            QuickHPStorage(naturalist.CharacterCard, mdp);
            QuickHandStorage(naturalist);

            // To check triggers, confirm that 2 cards were drawn and 0 damage was healed, but 1 was dealt.
            UsePower(power);

            QuickHPCheck(0, -1);
            QuickHandCheck(2);
        }


        [Test()]
        public void TestMrFixerPowerA()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "MrFixer/RuduenWorkshop.MrFixerFlowingStrikeCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(fixer.CharacterCard.IsPromoCard);

            StartGame();
            UsePower(legacy);
            Card tool = PutInHand("DualCrowbars");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectCardToPlay = tool;
            DecisionSelectTarget = mdp;

            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer);
            QuickHPCheck(-1, -2);
            AssertInPlayArea(fixer, tool); // Card put into play.
        }

        [Test()]
        public void TestMrFixerPowerB()
        {
            // Tool not in hand.
            SetupGameController("BaronBlade", "MrFixer/RuduenWorkshop.MrFixerFlowingStrikeCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(fixer.CharacterCard.IsPromoCard);

            StartGame();
            UsePower(legacy);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            MoveAllCards(fixer, fixer.HeroTurnTaker.Hand, fixer.HeroTurnTaker.Deck);

            Card tool = PutOnDeck("DualCrowbars");

            DecisionSelectTarget = mdp;

            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer);
            QuickHPCheck(-1, -2);
            AssertInPlayArea(fixer, tool); // Card put into play.
        }

        [Test()]
        public void TestMrFixerPowerC()
        {
            // Tool not in hand and empty deck.
            SetupGameController("BaronBlade", "MrFixer/RuduenWorkshop.MrFixerFlowingStrikeCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(fixer.CharacterCard.IsPromoCard);

            StartGame();
            UsePower(legacy);
            Card mdp = GetCardInPlay("MobileDefensePlatform");
            MoveAllCards(fixer, fixer.HeroTurnTaker.Hand, fixer.HeroTurnTaker.Trash);
            MoveAllCards(fixer, fixer.HeroTurnTaker.Deck, fixer.HeroTurnTaker.Trash);

            DecisionSelectTarget = mdp;

            QuickHPStorage(fixer.CharacterCard, mdp);
            UsePower(fixer);
            QuickHPCheck(-1, -2);
            AssertNotInPlay((Card c) => c.IsTool);
        }

        [Test()]
        public void TestLaComodoraPower()
        {
            SetupGameController("BaronBlade", "LaComodora/RuduenWorkshop.LaComodoraTemporalScavengeCharacter", "Megalopolis");
            Assert.IsTrue(comodora.CharacterCard.IsPromoCard);

            StartGame();

            GoToPlayCardPhase(comodora);

            Card equip = PlayCard("ConcordantHelm");

            GoToUsePowerPhase(comodora);
            UsePower(comodora);

            DecisionYesNo = true;
            DecisionSelectCard = equip;

            GoToStartOfTurn(baron);
            PutIntoPlay("DeviousDisruption");

            AssertFlipped(equip);
            AssertInPlayArea(comodora, equip);

            GoToUsePowerPhase(comodora);
            UsePower(comodora);
            AssertNotFlipped(equip);
            GoToStartOfTurn(baron);
            PutIntoPlay("DeviousDisruption");

            // Make sure effect has been re-applied and therefore still works here.
            AssertFlipped(equip);
            AssertInPlayArea(comodora, equip);
        }

        [Test()]
        public void TestLaComodoraPowerGuiseDangIt()
        {
            SetupGameController("BaronBlade", "LaComodora/RuduenWorkshop.LaComodoraTemporalScavengeCharacter", "Guise/SantaGuiseCharacter", "Megalopolis");
            Assert.IsTrue(comodora.CharacterCard.IsPromoCard);

            StartGame();

            GoToPlayCardPhase(comodora);

            Card equip = PlayCard("ConcordantHelm");

            GoToUsePowerPhase(comodora);
            UsePower(comodora);

            DecisionYesNo = true;
            DecisionSelectCard = equip;

            GoToStartOfTurn(baron);
            PutIntoPlay("DeviousDisruption");

            AssertFlipped(equip);
            AssertInPlayArea(comodora, equip);

            UsePower(guise, 1);
            AssertNotFlipped(equip);
        }

        [Test()]
        public void TestNightMistPowerDraw()
        {
            SetupGameController("BaronBlade", "NightMist/RuduenWorkshop.NightMistLimitedNumerologyCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(mist.CharacterCard.IsPromoCard);

            StartGame();

            QuickHandStorage(mist);
            QuickHPStorage(mist, legacy);
            UsePower(mist);
            DealDamage(mist, mist, 2, DamageType.Energy);
            DealDamage(mist, legacy, 2, DamageType.Energy);
            QuickHPCheck(-1, -1); // 1 Net Damage.
            QuickHandCheck(1); // Card drawn.
        }

        [Test()]
        public void TestNightMistPowerPlay()
        {
            SetupGameController("BaronBlade", "NightMist/RuduenWorkshop.NightMistLimitedNumerologyCharacter", "Legacy", "Megalopolis");
            Assert.IsTrue(mist.CharacterCard.IsPromoCard);

            StartGame();
            Card play = PutInHand("TomeOfElderMagic");

            DecisionSelectFunction = 1;
            DecisionSelectCardToPlay = play;

            QuickHandStorage(mist);
            QuickHPStorage(mist, legacy);
            UsePower(mist);
            DealDamage(mist, mist, 2, DamageType.Energy);
            DealDamage(mist, legacy, 2, DamageType.Energy);
            QuickHPCheck(-1, -1); // 1 Net Damage.
            AssertInPlayArea(mist, play);
        }

        [Test()]
        public void TestOmnitronXPower()
        {
            SetupGameController("BaronBlade", "OmnitronX/RuduenWorkshop.OmnitronXElectroShieldedSystemsCharacter", "Megalopolis");
            Assert.IsTrue(omnix.CharacterCard.IsPromoCard);

            StartGame();
            Card component = PutIntoPlay("ElectroDeploymentUnit");
            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectFunction = 1;

            QuickHPStorage(omnix.CharacterCard, mdp);
            UsePower(omnix);
            QuickHPCheck(-1, -1);
            DestroyCard(component);
            AssertInPlayArea(omnix, component); // Card not destroyed.
            GoToStartOfTurn(omnix);
            DestroyCard(component);
            AssertInTrash(component); // New turn, effect wears off, destroyed.
        }

        [Test()]
        public void TestParsePowerNoDeck()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Parse/RuduenWorkshop.ParseLaplaceShotCharacter", "Megalopolis");
            Assert.IsTrue(parse.CharacterCard.IsPromoCard);

            StartGame();

            MoveAllCards(parse, env.TurnTaker.Deck, env.TurnTaker.Trash);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            UsePower(parse);
            AssertInTrash(mdp); // Destroyed.
        }

        [Test()]
        public void TestParsePowerNoTrash()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Parse/RuduenWorkshop.ParseLaplaceShotCharacter", "Megalopolis");
            Assert.IsTrue(parse.CharacterCard.IsPromoCard);

            StartGame();

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            UsePower(parse);
            QuickHPCheck(0);
        }

        [Test()]
        public void TestParsePowerTrashSkipAttack()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Parse/RuduenWorkshop.ParseLaplaceShotCharacter", "Megalopolis");
            Assert.IsTrue(parse.CharacterCard.IsPromoCard);

            StartGame();

            DiscardTopCards(baron, 2);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = null;

            QuickHPStorage(mdp);
            UsePower(parse);
            QuickHPCheck(0);
            AssertNumberOfCardsInTrash(baron, 2);
        }

        [Test()]
        public void TestParsePowerTrashAttack()
        {
            // Tool in hand.
            SetupGameController("BaronBlade", "Parse/RuduenWorkshop.ParseLaplaceShotCharacter", "Megalopolis");
            Assert.IsTrue(parse.CharacterCard.IsPromoCard);

            StartGame();

            DiscardTopCards(env, 2);

            Card mdp = GetCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionYesNo = true;

            QuickHPStorage(mdp);
            UsePower(parse);
            QuickHPCheck(-2);
            AssertNumberOfCardsInTrash(baron, 0);
        }

        [Test()]
        public void TestTachyonControlledPacePowerNoTrash()
        {
            SetupGameController("BaronBlade", "Tachyon/RuduenWorkshop.TachyonControlledPaceCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(tachyon.CharacterCard.IsPromoCard);

            GoToUsePowerPhase(tachyon);

            QuickHandStorage(tachyon);
            UsePower(tachyon);

            AssertPhaseActionCount(0); // Powers used.
        }

        [Test()]
        public void TestTachyonControlledPacePowerOngoingTrash()
        {
            SetupGameController("BaronBlade", "Tachyon/RuduenWorkshop.TachyonControlledPaceCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(tachyon.CharacterCard.IsPromoCard);

            Card lingering = PutInTrash(GetCardWithLittleEffect(tachyon));

            GoToUsePowerPhase(tachyon);

            QuickHandStorage(tachyon);
            UsePower(tachyon);

            AssertPhaseActionCount(0); // Powers used.

            AssertInPlayArea(tachyon, lingering);
        }

        [Test()]
        public void TestTachyonControlledPacePowerOneshotTrash()
        {
            SetupGameController("BaronBlade", "Tachyon/RuduenWorkshop.TachyonControlledPaceCharacter", "TheBlock");

            StartGame();

            Assert.IsTrue(tachyon.CharacterCard.IsPromoCard);

            Card oneshot = PutInTrash("SuckerPunch");

            GoToUsePowerPhase(tachyon);

            QuickHandStorage(tachyon);
            UsePower(tachyon);

            AssertPhaseActionCount(0); // Powers used.

            AssertOnBottomOfDeck(tachyon, oneshot);
        }

        [Test()]
        public void TestVoidGuardMainstay()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "VoidGuardMainstay/RuduenWorkshop.VoidGuardMainstayShrugItOffCharacter", "Megalopolis");

            Assert.IsTrue(voidMainstay.CharacterCard.IsPromoCard);

            StartGame();

            QuickHPStorage(voidMainstay);
            QuickHandStorage(voidMainstay);
            UsePower(voidMainstay);
            DealDamage(voidMainstay, voidMainstay, 1, DamageType.Melee);
            DealDamage(voidMainstay, voidMainstay, 2, DamageType.Melee);
            QuickHandCheck(1); // Card drawn.
            QuickHPCheck(-2); // Only the 2 went through.
        }

        [Test()]
        public void TestVoidGuardMainstayIncreasePierces()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "VoidGuardMainstay/RuduenWorkshop.VoidGuardMainstayShrugItOffCharacter", "Legacy", "TheBlock");

            Assert.IsTrue(voidMainstay.CharacterCard.IsPromoCard);

            StartGame();

            UsePower(legacy);

            QuickHPStorage(voidMainstay);
            QuickHandStorage(voidMainstay);
            UsePower(voidMainstay);
            DealDamage(voidMainstay, voidMainstay, 0, DamageType.Melee);
            QuickHandCheck(1); // Card drawn.
            QuickHPCheck(0); // Increased damage should not work.
        }

        [Test()]
        public void TestVoidGuardMainstayGuiseDangIt()
        {
            // Equipment Test
            SetupGameController("BaronBlade", "VoidGuardMainstay/RuduenWorkshop.VoidGuardMainstayShrugItOffCharacter", "Guise", "TheHarpy", "TheBlock");

            Assert.IsTrue(voidMainstay.CharacterCard.IsPromoCard);

            StartGame();

            PutIntoPlay("AppliedNumerology");

            Card play = PutInHand("ICanDoThatToo");

            DecisionSelectTurnTaker = harpy.TurnTaker;
            DecisionSelectCard = voidMainstay.CharacterCard;
            DecisionYesNo = true;

            PutIntoPlay("UhYeahImThatGuy");

            QuickHPStorage(guise);
            QuickHandStorage(guise);
            PlayCard(play);
            DealDamage(guise, guise, 2, DamageType.Melee); // -1 from Numerology, then prevent. Numerology only works on cards in Guise's play area!
            QuickHandCheck(0); // Played and drawn.
            QuickHPCheck(0); // Prevented.
        }
    }
}