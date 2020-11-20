using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Linq;

namespace RuduenWorkshop.Cascade
{
    // Token: 0x0200054D RID: 1357
    public class RiverbankCardController : CascadeRiverSharedCardController
    {
        public RiverbankCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            this.SpecialStringMaker.ShowNumberOfCardsUnderCard(this.Card, () => true);
            this.AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }

        public override void AddTriggers()
        {
            this.AddTrigger<MoveCardAction>((MoveCardAction m) => m.Origin == this.Riverbank().UnderLocation && m.Destination != this.RiverDeck(), new Func<MoveCardAction, IEnumerator>(this.RefillRiverbankResponse), TriggerType.MoveCard, TriggerTiming.After);
            this.AddTrigger<PlayCardAction>((PlayCardAction p) => p.Origin == this.Riverbank().UnderLocation, new Func<PlayCardAction, IEnumerator>(this.RefillRiverbankResponse), TriggerType.PlayCard, TriggerTiming.After);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            return card == this.Card || card.Location == this.Card.UnderLocation;
        }

        private IEnumerator RefillRiverbankResponse(PlayCardAction p)
        {
            IEnumerator coroutine = RefillRiverbankResponseHelper();
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        private IEnumerator RefillRiverbankResponse(MoveCardAction m)
        {
            IEnumerator coroutine = RefillRiverbankResponseHelper();
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        private IEnumerator RefillRiverbankResponseHelper()
        {
            IEnumerator coroutine;
            Card remainingCard = Riverbank().UnderLocation.Cards.FirstOrDefault();
            //// Move remaining riverbank cards.
            //// Removed during physical revamp to make the process simpler.
            //while (remainingCard != null)
            //{
            //    coroutine = this.GameController.MoveCard(this.DecisionMaker, remainingCard, RiverDeck(), toBottom: true, evenIfIndestructible: true);
            //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            //    remainingCard = Riverbank().UnderLocation.Cards.FirstOrDefault();
            //}

            // Confirm number of cards to move.
            int cardsToMove = 4 - Riverbank().UnderLocation.Cards.Count();

            // Then, move the top card to the riverbank. Normal empty deck logic should work if they aren't available.
            coroutine = this.GameController.MoveCards(this.DecisionMaker, RiverDeck().GetTopCards(cardsToMove), Riverbank().UnderLocation);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}