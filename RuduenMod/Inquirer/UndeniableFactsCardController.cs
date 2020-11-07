using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace Workshopping.Inquirer
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
            base.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.DrawCardResponse), TriggerType.DrawCard, TriggerTiming.After);
            //base.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.MoveInsteadResponse), TriggerType.MoveCard, TriggerTiming.Before);
        }

        //private IEnumerator MoveInsteadResponse(DestroyCardAction d)
        //{
        //    // Cancel the destroy card action.
        //    IEnumerator coroutine;
        //    coroutine = base.CancelAction(d, true, true, null, false);
        //    if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

        //    // Move the card to the bottom of your deck instead.
        //    coroutine = base.GameController.MoveCard(base.TurnTakerController, d.CardToDestroy.Card, d.CardToDestroy.Card.Owner.Deck, true);
        //    if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        //}

        private IEnumerator DrawCardResponse(GameAction d)
        {
            // Draw a card.
            IEnumerator drawCoroutine = base.DrawCard(null, false, null, true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(drawCoroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(drawCoroutine);
            }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = base.GetPowerNumeral(0, 2);

            // Play 2 Distortion cards.
            IEnumerator coroutine = base.SelectAndPlayCardsFromHand(base.HeroTurnTakerController, powerNumeral, false, new int?(0), new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}