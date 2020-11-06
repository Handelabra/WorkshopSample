using Handelabra.Sentinels.Engine.Controller;
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
        }

        [Test()]
        public void TestInnatePower()
        {
            SetupGameController("BaronBlade", "Workshopping.Cascade", "Megalopolis");

            StartGame();

            AssertNumberOfCardsInDeck(Cascade, 1); // Should start with 1 card in deck.

            UsePower(Cascade.CharacterCard, 0); // Default Innate. Cast.

            AssertNumberOfCardsInTrash(Cascade, 1);
            DrawCard(Cascade, 40); // Attempt to draw all cards.
            DiscardAllCards(Cascade); // Discard all cards.
            AssertNumberOfCardsInTrash(Cascade, 6); // Confirm new card is also in trash.
        }
    }
}