using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using RuduenWorkshop.Cascade;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RuduenModsTest
{
    [TestFixture]
    public class CascadeMageTest : BaseTest
    {
        [OneTimeSetUp]
        public void DoSetup()
        {
            // Tell the engine about our mod assembly so it can load up our code.
            // It doesn't matter which type as long as it comes from the mod's assembly.
            //var a = Assembly.GetAssembly(typeof(InquirerCharacterCardController)); // replace with your own type
            ModHelper.AddAssembly("RuduenWorkshop", Assembly.GetAssembly(typeof(CascadeCharacterCardController))); // replace with your own namespace
        }

        protected HeroTurnTakerController Cascade { get { return FindHero("Cascade"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Cascade);
            Assert.IsInstanceOf(typeof(CascadeTurnTakerController), Cascade);
            Assert.IsInstanceOf(typeof(CascadeCharacterCardController), Cascade.CharacterCardController);

            Assert.AreEqual(27, Cascade.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestSetupWorks()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            AssertNumberOfCardsInHand(Cascade, 4); // And four cards in hand.
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And 4 cards in the Riverbank.
            AssertNumberOfCardsAtLocation(Cascade.TurnTaker.FindSubDeck("RiverDeck"), 30); // And 30 cards in the River Deck.
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "RushingWaters", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to purchase. (Cost 3.)

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.
            Assert.IsTrue(cardToBuy.Location == Cascade.TurnTaker.Trash || cardToBuy.Location == Cascade.TurnTaker.Deck || cardToBuy.Location == Cascade.HeroTurnTaker.Hand); // Bought.
            AssertNumberOfCardsInHand(Cascade, 4);
        }

        [Test()]
        public void TestInnatePowerNoAffordable()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "StormSwell", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to purchase. (Cost 3.)

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.

            AssertAtLocation(cardToBuy, GetCard("Riverbank").UnderLocation);
            AssertNumberOfCardsInHand(Cascade, 4);
        }

        [Test()]
        public void TestInnatePowerGuiseDangIt()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "RushingWaters", GetCard("Riverbank").UnderLocation); // Move Rushing Water under so we definitely have something to purchase. (Cost 3.)

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;
            DecisionSelectPower = Cascade.CharacterCard;

            HeroTurnTakerController guise = FindHero("Guise");

            PlayCard("ICanDoThatToo"); // Guise uses the innate power.

            // Even if Guise discards everything, he should fail to get the card due to all discarded cards having a total magic value of 0.
            Assert.IsTrue(cardToBuy.Location == GetCard("Riverbank").UnderLocation); // Not bought.
            // Guise redraws to 4.
            AssertNumberOfCardsInHand(guise, 4);
        }

        [Test()]
        public void TestInnatePowerGuiseDangItAgain()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = GetCard("Retcon"); // Get guise's card...
            MoveCard(Cascade, cardToBuy, GetCard("Riverbank").UnderLocation); // Move Retcon into the riverback. Yes, it doesn't have a cost. That's the point.

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.
            Assert.IsTrue(cardToBuy.Location == GetCard("Riverbank").UnderLocation); // Not bought. Even if the card's available, the lack of cost means the interaction fails.
            AssertNumberOfCardsInHand(Cascade, 4);
        }

        [Test()]
        public void TestDropletWithMove()
        {
            // Most basic purchase equivalent!
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            DealDamage(Cascade, Cascade, 3, DamageType.Melee);

            List<Card> wasUnderCards = new List<Card>(GetCard("Riverbank").UnderLocation.Cards);

            DecisionYesNo = true;

            QuickHPStorage(Cascade);
            PlayCard("Droplet"); // Play the card.
            QuickHPCheck(1); // Damage dealt.
            AssertAtLocation(wasUnderCards, Cascade.TurnTaker.FindSubDeck("RiverDeck"));
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // 4 cards in the Riverbank.
        }

        [Test()]
        public void TestDropletNoMove()
        {
            // Most basic purchase equivalent!
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            DealDamage(Cascade, Cascade, 3, DamageType.Melee);

            List<Card> wasUnderCards = new List<Card>(GetCard("Riverbank").UnderLocation.Cards);

            DecisionYesNo = false;

            QuickHPStorage(Cascade);
            PlayCard("Droplet"); // Play the card.
            QuickHPCheck(1); // Damage dealt.
            AssertAtLocation(wasUnderCards, GetCard("Riverbank").UnderLocation);
        }

        [Test()]
        public void TestWaterSurge()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Legacy", "Megalopolis");

            StartGame();

            DealDamage(Cascade, Cascade, 3, DamageType.Melee);
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectTargetFriendly = Cascade.CharacterCard;

            PlayCard("InspiringPresence"); // Use to boost damage by 1 to make sure character card is source.

            QuickHPStorage(mdp, Cascade.CharacterCard);
            PlayCard("WaterSurge"); // Play the card.
            QuickHPCheck(-3, 1);
        }

        [Test()]
        public void TestRushingWaters()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            DealDamage(Cascade, Cascade, 3, DamageType.Melee);
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectTargetFriendly = Cascade.CharacterCard;

            PlayCard("RushingWaters"); // Play the card.
            AssertNumberOfCardsInTrash(Cascade, 3); // Constant flow and two other played cards in trash.
        }

        [Test()]
        public void TestWaterlog()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            Card ongoing = PlayCard("LivingForceField");
            DecisionDestroyCard = ongoing;

            PlayCard("Waterlog"); // Play the card.
            AssertInTrash(ongoing); // Ongoing destroyed.
        }

        //[Test()]
        //public void TestStreamSurge()
        //{
        //    SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

        //    StartGame();

        //    List<Card> targets = new List<Card>
        //    {
        //        FindCardInPlay("MobileDefensePlatform"),
        //        FindCardInPlay("BaronBladeCharacter"),
        //        Cascade.CharacterCard,
        //        FindCardInPlay("BaronBladeCharacter")
        //    };
        //    Card followUp = PutInHand("WaterSurge");
        //    DealDamage(Cascade, Cascade, 4, DamageType.Melee);
        //    DealDamage(Cascade, targets[0], 9, DamageType.Melee);

        //    DecisionSelectTargets = targets.ToArray();
        //    DecisionYesNo = true;
        //    DecisionSelectCardToPlay = followUp;

        //    QuickHPStorage(targets[1], Cascade.CharacterCard);
        //    PlayCard("StreamSurge"); // Play the card.
        //    AssertNumberOfCardsInTrash(Cascade, 2); // Constant flow and other played cards in trash.
        //    AssertInTrash(targets[0]); // MDP Destroyed.
        //    QuickHPCheck(-3, 1); // Blade took 3 hits total. Cascade damaged 1, healed 2.

        //}

        [Test()]
        public void TestRiverWornStone()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Legacy", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);
            MoveCard(Cascade, GetCard("RiverWornStone", 1), Cascade.HeroTurnTaker.Trash); // Move spare copy to the trash so draw 2 has two cards.

            Card power = PlayCard("RiverWornStone"); // Play the card.
            Card cost = PutInHand("WaterSurge");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectCard = cost;

            PlayCard("InspiringPresence"); // Use to boost damage by 1 to make sure character card is source.

            QuickHPStorage(mdp);

            UsePower(power, 0);
            QuickHPCheck(-2); // 1 damage for cost, 1 for boost.
            AssertAtLocation(cost, Cascade.TurnTaker.FindSubDeck("RiverDeck")); // Card was moved into the river deck.
            // Discard all cards to clear things for additional tests.
            DiscardAllCards(Cascade);

            QuickHandStorage(Cascade);
            GoToDrawCardPhase(Cascade);
            RunActiveTurnPhase();
            QuickHandCheck(2); // Confirm drew 2 cards.
        }

        [Test()]
        public void TestRiverWornStoneGuiseDangIt()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);
            MoveCard(Cascade, GetCard("RiverWornStone", 1), Cascade.HeroTurnTaker.Trash); // Move spare copy to the trash so draw 2 has two cards.

            Card power = PlayCard("RiverWornStone"); // Play the card.
            Card cost = PutInHand("BlatantReference");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectCard = cost;
            DecisionSelectPower = power;

            PlayCard("GuiseTheBarbarian"); // Damage boost to make sure no 1 damage instance occurs.

            QuickHPStorage(mdp);

            PlayCard("LemmeSeeThat"); // Guise borrows the card.
            UsePower(power); // Guise uses that power. Does this work?

            QuickHPCheck(0); // No damage - card was moved into the river, but no spell value exists, so we try to deal null value, opposed to dealing 0 damage.
            AssertAtLocation(cost, Cascade.TurnTaker.FindSubDeck("RiverDeck")); // Card was moved into the river deck, necessitating the core 'Dang it Guise' case.
        }

        //[Test()]
        //public void TestDivergingWaters()
        //{
        //    SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

        //    StartGame();

        //    DiscardAllCards(Cascade); // Discard all cards so draw cards can pull an appropriate amount.
        //    Card mdp = FindCardInPlay("MobileDefensePlatform");
        //    Card followUp = PutInHand("WaterSurge");
        //    Card waters = PutInHand("DivergingWaters");

        //    DealDamage(Cascade, Cascade, 4, DamageType.Melee);

        //    DecisionSelectTarget = mdp;
        //    DecisionYesNo = true;
        //    DecisionSelectCardToPlay = followUp;

        //    QuickHPStorage(mdp, Cascade.CharacterCard);
        //    QuickHandStorage(Cascade);

        //    PlayCard(waters); // Play the card.

        //    QuickHPCheck(-2, 2); // MDP took one hit, Cascade took one hit.
        //    QuickHandCheck(1); // Two cards played/returned, 3 cards drawn. Net +1.
        //    AssertInTrash(followUp); // Used card in discard.
        //    AssertAtLocation(waters, Cascade.TurnTaker.FindSubDeck("RiverDeck"), true); // Card returned to river.

        //}

        [Test()]
        public void TestRippledVisions()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Legacy", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);
            Card revealed = MoveCard(Cascade, "Droplet", Cascade.TurnTaker.FindSubDeck("RiverDeck")); // Move droplet to top of deck for reference.

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            PlayCard("InspiringPresence"); // Use to boost damage by 1 to make sure character card is source.

            DecisionSelectTarget = mdp;

            QuickHPStorage(mdp);
            PlayCard("RippledVisions");
            QuickHPCheck(-2); // 1 damage for cost, 1 for boost.
            AssertAtLocation(revealed, Cascade.TurnTaker.FindSubDeck("RiverDeck"));
        }

        [Test()]
        public void TestRippledVisionsGuiseDangIt()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Legacy", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);
            Card play = PutInHand(Cascade, "RippledVisions"); // Move into hand to prevent triggering from beneath Riverbank.
            Card revealed = MoveCard(Cascade, "Retcon", Cascade.TurnTaker.FindSubDeck("RiverDeck")); // Move Retcon to top of deck for reference.

            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            PlayCard("InspiringPresence"); // Use Legacy to give +1 boost as necessary.

            QuickHPStorage(mdp);
            PlayCard(play);
            QuickHPCheck(0); // 0 damage, since no magic number exists.
            AssertAtLocation(revealed, Cascade.TurnTaker.FindSubDeck("RiverDeck"));
        }

        [Test()]
        public void TestCondensedOrb()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);

            Card mdp = FindCardInPlay("MobileDefensePlatform");
            List<Card> targets = new List<Card>() { Cascade.CharacterCard, mdp };

            DealDamage(Cascade.CharacterCard, targets.AsEnumerable(), 5, DamageType.Melee);

            DecisionSelectCards = targets.ToArray();

            QuickHPStorage(Cascade.CharacterCard, mdp);
            PlayCard("CondensedOrb");
            QuickHPCheck(3, 3);
        }

        [Test()]
        public void TestStormSwell()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);

            List<Card> targets = new List<Card>() { FindCardInPlay("MobileDefensePlatform"), FindCardInPlay("BaronBladeCharacter") };

            DealDamage(Cascade.CharacterCard, targets[0], 7, DamageType.Melee);

            DecisionSelectCards = targets.ToArray();

            QuickHPStorage(targets[1]);
            PlayCard("StormSwell");
            AssertInTrash(targets[0]); // MDP destroyed.
            QuickHPCheck(-4); // BB 4 damage.
        }

        //[Test()]
        //public void TestShapeTheStream()
        //{
        //    // Most basic purchase equivalent!
        //    SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

        //    StartGame();

        //    AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.

        //    PlayCard("ShapeTheStream"); // Play the card.

        //    AssertNumberOfCardsInTrash(Cascade, 2); // Shape the stream and gained card should now be in trash.
        //    AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And 4 cards in the Riverbank.
        //}

        //[Test()]
        //public void TestShapeTheStreamGuiseDangIt()
        //{
        //    // Most basic purchase equivalent!
        //    SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Megalopolis");

        //    StartGame();

        //    AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
        //    Card retcon = MoveCard(Cascade, "Retcon", GetCard("Riverbank").UnderLocation); // Move Retcon into riverbank.

        //    DecisionMoveCard = retcon;

        //    PlayCard("ShapeTheStream"); // Play the card.

        //    AssertInTrash(Cascade, retcon); // Someone else's card is now in your trash. You monster.
        //    AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And Riverbank has been reset.
        //}

        // TODO: More in-depth tests for what happens when you play pre-existing cards. Apparently we have Guise + Toolbox to go off of and Akash's seeds - probably just
        // goes to the original owner, which is fine.

        [Test()]
        public void TestRisingWaters()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);

            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card played = PlayCard("RisingWaters");

            QuickHPStorage(mdp);
            DealDamage(Cascade.CharacterCard, mdp, 1, DamageType.Melee);
            QuickHPCheck(-2); // Boosted damage.

            GoToStartOfTurn(Cascade);
            AssertInTrash(played); // Self-destructed due to no other cards.
        }

        [Test()]
        public void TestPerpetualFlow()
        {
            // Most basic purchase equivalent!
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "StormSwell", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to play.
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            QuickHPStorage(mdp);
            Card played = PlayCard("PerpetualFlow"); // Play the card.

            AssertInTrash(played, cardToBuy);
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And Riverbank has been reset.
            QuickHPCheck(-4); // MDP should've taken damage from Storm Swell.
        }

        [Test()]
        public void TestPerpetualFlowGuiseDangIt()
        {
            // Most basic purchase equivalent!
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Megalopolis");

            StartGame();
            Card destroy = PlayCard("LivingForceField");

            AssertNumberOfCardsInDeck(Cascade, 2); // Should start with 2 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = GetCard("Retcon");
            MoveCard(Cascade, cardToBuy, GetCard("Riverbank").UnderLocation); // Move Retcon under.
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            QuickHPStorage(mdp);
            Card played = PlayCard("PerpetualFlow"); // Play the card.

            AssertInTrash(played, cardToBuy, destroy);
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And Riverbank has been reset.
        }

        // TODO: More in-depth tests for what happens when you play pre-existing cards. Apparently we have Guise + Toolbox to go off of and Akash's seeds - probably just
        // goes to the original owner, which is fine.

        [Test()]
        public void TestMeetingTheOcean()
        {
            SetupGameController("BaronBlade", "RuduenWorkshop.Cascade", "Guise", "Legacy", "Megalopolis");

            StartGame();

            DiscardAllCards(Cascade);

            PutInHand(Cascade, new List<string>() { "Waterlog", "RisingWaters", "Retcon" }.ToArray());

            // Pull 2, 3, and NA cards for testing.
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionYesNo = true;

            PlayCard("InspiringPresence"); // Increase damage by 1 to check for null rather than 0.

            QuickHPStorage(mdp);
            QuickHandStorage(Cascade);

            PlayCard("MeetingTheOcean");
            UsePower("MeetingTheOcean");
            QuickHPCheck(-7); // Instance of 2 and 3, increased to 3 and 4.
            QuickHandCheck(-3); // Confirm all cards used.
        }

        // TODO: Add riverbank tests when the River deck has been emptied! Yes, it will stop drawing cards - but you have a full deck to play with already, so at that stage that's your own fault!
    }
}