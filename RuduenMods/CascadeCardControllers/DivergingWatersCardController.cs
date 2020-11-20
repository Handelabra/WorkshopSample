using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Cascade
{
    public class DivergingWatersCardController : CascadeRiverSharedCardController
    {
        public DivergingWatersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            // Draw 3 cards.
            coroutine = this.DrawCards(this.DecisionMaker, 3);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Play a card.
            coroutine = this.SelectAndPlayCardsFromHand(this.DecisionMaker, 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Move to the bottom of the draw deck.
            coroutine = this.GameController.MoveCard(this.DecisionMaker, this.Card, RiverDeck(), toBottom: true, evenIfIndestructible: true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override bool DoNotMoveOneShotToTrash
        {
            // TODO: Based on Benchmark, but potentially change to always true if appropriate.
            get { return this.Card.IsInLocation(this.RiverDeck()); }
        }
    }
}