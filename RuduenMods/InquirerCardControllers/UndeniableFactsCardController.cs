using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace RuduenWorkshop.Inquirer
{
    public class UndeniableFactsCardController : CardController
    {
        // TODO: TEST!
        public UndeniableFactsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // Add trigger for destroyed Distortion.
            this.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.DrawCardResponse), TriggerType.DrawCard, TriggerTiming.After);
            //this.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.MoveInsteadResponse), TriggerType.MoveCard, TriggerTiming.Before);
        }

        //private IEnumerator MoveInsteadResponse(DestroyCardAction d)
        //{
        //    // Cancel the destroy card action.
        //    IEnumerator coroutine;
        //    coroutine = this.CancelAction(d, true, true, null, false);
        //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        //    // Move the card to the bottom of your deck instead.
        //    coroutine = this.GameController.MoveCard(this.TurnTakerController, d.CardToDestroy.Card, d.CardToDestroy.Card.Owner.Deck, true);
        //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}

        private IEnumerator DrawCardResponse(GameAction d)
        {
            // Draw a card.
            IEnumerator drawCoroutine = this.DrawCard(null, false, null, true);
            if (this.UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                this.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 2);

            // Play 2 Distortion cards.
            IEnumerator coroutine = this.SelectAndPlayCardsFromHand(this.HeroTurnTakerController, powerNumeral, false, new int?(0), new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}