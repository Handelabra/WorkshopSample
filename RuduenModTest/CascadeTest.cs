using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using Handelabra.Sentinels.UnitTest;
using NUnit.Framework;
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
            Card cardToBuy = MoveCard(Cascade, "ConstantFlow", GetCard("Riverbank").UnderLocation); // Move Storm Swell under so we definitely have something to purchase. (Cost 3.) 

            DecisionMoveCard = cardToBuy;
            DecisionYesNo = true;

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.

            AssertInTrash(cardToBuy); // Bought.
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