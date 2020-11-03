using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
{
    public class TheLieTheyTellThemselvesCardController : CardController
    {
        public TheLieTheyTellThemselvesCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // Add trigger for destroyed Distortion.
            base.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.DealDamageResponse), TriggerType.DealDamage, TriggerTiming.After);
        }

        private IEnumerator DealDamageResponse(GameAction d)
        {
            // Pick any target
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
            IEnumerator coroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTarget, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget, "targets", false, false, null, null, false), storedResults, false, false, null, true, base.GetCardSource(null));
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            Card selectedCard = base.GetSelectedCard(storedResults);

            // That target damages themselves.
            coroutine = base.DealDamage(selectedCard, selectedCard, 1, DamageType.Psychic, false, false, false, null, null, null, false, null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
            yield break;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Draw 2 cards.
            int powerNumeral = base.GetPowerNumeral(0, 2);
            IEnumerator coroutine = base.DrawCards(this.HeroTurnTakerController, powerNumeral);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            // Destroy a distortion.
            coroutine = base.GameController.SelectAndDestroyCard(this.DecisionMaker, 
                new LinqCardCriteria((Card c) => c.IsInPlay && c.IsDistortion, "distortion"), 
                true);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}