using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Controller.Luminary;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Workshopping.Cascade;

namespace RuduenModTest
{
    [TestFixture]
    public class CascadeMageTest : BaseTest
    {
        protected HeroTurnTakerController Cascade { get { return FindHero("Cascade"); } }

        [Test()]
        public void TestModWorks()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            Assert.AreEqual(3, this.GameController.TurnTakerControllers.Count());

            Assert.IsNotNull(Cascade);
            Assert.IsInstanceOf(typeof(CascadeTurnTakerController), Cascade);
            Assert.IsInstanceOf(typeof(CascadeCharacterCardController), Cascade.CharacterCardController);

            Assert.AreEqual(27, Cascade.CharacterCard.HitPoints);
        }

        [Test()]
        public void TestSetupWorks()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.
            AssertNumberOfCardsInHand(Cascade, 4); // And four cards in hand.
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And 4 cards in the Riverbank.
            AssertNumberOfCardsAtLocation(Cascade.TurnTaker.FindSubDeck("RiverDeck"), 31); // And 31 cards in the River Deck.
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "RushingWaters", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to purchase. (Cost 3.) 

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.
            Assert.IsTrue(cardToBuy.Location == Cascade.TurnTaker.Trash || cardToBuy.Location == Cascade.TurnTaker.Deck || cardToBuy.Location == Cascade.HeroTurnTaker.Hand); // Bought.
            AssertNumberOfCardsInHand(Cascade, 3);
        }

        [Test()]
        public void TestInnatePowerNoAffordable()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "StormSwell", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to purchase. (Cost 3.) 

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.

            AssertAtLocation(cardToBuy, GetCard("Riverbank").UnderLocation);
            AssertNumberOfCardsInHand(Cascade, 3);
        }

        [Test()]
        public void TestInnatePowerDangItGuise()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Guise", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = MoveCard(Cascade, "RushingWaters", GetCard("Riverbank").UnderLocation); // Move Rushing Water under so we definitely have something to purchase. (Cost 3.) 

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;
            DecisionSelectPower = Cascade.CharacterCard;

            HeroTurnTakerController guise = FindHero("Guise");

            PlayCard("ICanDoThatToo"); // Guise uses the innate power. 

            // Even if Guise discards everything, he should fail to get the card due to all discarded cards having a total magic value of 0. 
            Assert.IsTrue(cardToBuy.Location == GetCard("Riverbank").UnderLocation); // Not bought.
            // Guise redraws to 3. 
            AssertNumberOfCardsInHand(guise, 3);
        }

        [Test()]
        public void TestInnatePowerDangItGuiseAgain()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Guise", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.
            MoveCards(Cascade, (Card c) => c.Location == GetCard("Riverbank").UnderLocation, Cascade.TurnTaker.FindSubDeck("RiverDeck"), numberOfCards: 4, overrideIndestructible: true); // Move all cards back to the river deck just in case.
            Card cardToBuy = GetCard("Retcon"); // Get guise's card...
            MoveCard(Cascade, cardToBuy, GetCard("Riverbank").UnderLocation); // Move Retcon into the riverback. Yes, it doesn't have a cost. That's the point. 

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.
            Assert.IsTrue(cardToBuy.Location == GetCard("Riverbank").UnderLocation); // Not bought. Even if the card's available, the lack of cost means the interaction fails. 
            AssertNumberOfCardsInHand(Cascade, 3);
        }

        [Test()]
        public void TestDropletWithMove()
        {
            // Most basic purchase equivalent! 
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

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
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

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
        public void TestFloodbank()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            DealDamage(Cascade, Cascade, 3, DamageType.Melee);
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectTargetFriendly = Cascade.CharacterCard;

            QuickHPStorage(mdp, Cascade.CharacterCard);
            PlayCard("Floodbank"); // Play the card. 
            QuickHPCheck(-2, 2);

        }

        [Test()]
        public void TestRushingWaters()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

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
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            Card ongoing = PlayCard("LivingForceField");
            DecisionDestroyCard = ongoing;

            PlayCard("Waterlog"); // Play the card. 
            AssertInTrash(ongoing); // Ongoing destroyed.
        }

        [Test()]
        public void TestStreamSurge()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            List<Card> targets = new List<Card>
            {
                FindCardInPlay("MobileDefensePlatform"),
                FindCardInPlay("BaronBladeCharacter"),
                Cascade.CharacterCard,
                FindCardInPlay("BaronBladeCharacter")
            };
            Card followUp = PutInHand("Floodbank");
            DealDamage(Cascade, Cascade, 4, DamageType.Melee);
            DealDamage(Cascade, targets[0], 9, DamageType.Melee);

            DecisionSelectTargets = targets.ToArray();
            DecisionYesNo = true;
            DecisionSelectCardToPlay = followUp;

            QuickHPStorage(targets[1], Cascade.CharacterCard);
            PlayCard("StreamSurge"); // Play the card. 
            AssertNumberOfCardsInTrash(Cascade, 2); // Constant flow and other played cards in trash.
            AssertInTrash(targets[0]); // MDP Destroyed. 
            QuickHPCheck(-3, 1); // Blade took 3 hits total. Cascade damaged 1, healed 2. 

        }

        [Test()]
        public void TestRiverWornStone()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            GoToUsePowerPhase(Cascade);
            MoveCard(Cascade, GetCard("RiverWornStone",1), Cascade.HeroTurnTaker.Trash); // Move spare copy to the trash so draw 2 has two cards.

            Card power = PlayCard("RiverWornStone"); // Play the card. 
            Card cost = PutInHand("Floodbank");
            Card mdp = FindCardInPlay("MobileDefensePlatform");

            DecisionSelectTarget = mdp;
            DecisionSelectCard = cost;

            QuickHPStorage(mdp);
           
            UsePower(power, 0);
            QuickHPCheck(-1); // 1 damage for cost. 
            AssertAtLocation(cost, Cascade.TurnTaker.FindSubDeck("RiverDeck")); // Card was moved into the river deck.
            // Discard all cards to clear things for additional tests.
            DiscardAllCards(Cascade);

            QuickHandStorage(Cascade);
            GoToDrawCardPhase(Cascade);
            RunActiveTurnPhase();
            QuickHandCheck(2); // Confirm drew 2 cards.
        }

        [Test()]
        public void TestRiverWornStoneDangItGuise()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Guise", "Megalopolis");

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

        [Test()]
        public void TestDivergingWaters()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            DiscardAllCards(Cascade); // Discard all cards so draw cards can pull an appropriate amount. 
            Card mdp = FindCardInPlay("MobileDefensePlatform");
            Card followUp = PutInHand("Floodbank");
            Card waters = PutInHand("DivergingWaters");


            DealDamage(Cascade, Cascade, 4, DamageType.Melee);

            DecisionSelectTarget = mdp;
            DecisionYesNo = true;
            DecisionSelectCardToPlay = followUp;

            QuickHPStorage(mdp, Cascade.CharacterCard);
            QuickHandStorage(Cascade); 

            PlayCard(waters); // Play the card. 

            QuickHPCheck(-2, 2); // MDP took one hit, Cascade took one hit.
            QuickHandCheck(1); // Two cards played/returned, 3 cards drawn. Net +1. 
            AssertInTrash(followUp); // Used card in discard.
            AssertAtLocation(waters, Cascade.TurnTaker.FindSubDeck("RiverDeck"), true); // Card returned to river. 

        }

        [Test()]
        public void TestShapeTheStream()
        {
            // Most basic purchase equivalent! 
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.

            PlayCard("ShapeTheStream"); // Play the card. 

            AssertNumberOfCardsInTrash(Cascade, 2); // Shape the stream and gained card should now be in trash. 
            AssertNumberOfCardsAtLocation(GetCard("Riverbank").UnderLocation, 4); // And 4 cards in the Riverbank.

        }
    }
}