using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Inquirer
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
            this.AddTrigger<DestroyCardAction>((DestroyCardAction d) => d.CardToDestroy.Card.IsDistortion && d.WasCardDestroyed, new Func<DestroyCardAction, IEnumerator>(this.DealDamageResponse), TriggerType.DealDamage, TriggerTiming.After);
        }

        private IEnumerator DealDamageResponse(GameAction d)
        {
            // Pick any target
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();
            IEnumerator coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTarget, new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsTarget && !c.IsHero, "targets", false, false, null, null, false), storedResults, false, false, null, true, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            Card selectedCard = this.GetSelectedCard(storedResults);

            // That target damages themselves.
            coroutine = this.DealDamage(selectedCard, selectedCard, 1, DamageType.Psychic, false, false, false, null, null, null, false, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Draw 2 cards.
            int powerNumeral = this.GetPowerNumeral(0, 2);
            IEnumerator coroutine = this.DrawCards(this.DecisionMaker, powerNumeral);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Destroy a distortion.
            coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker,
                new LinqCardCriteria((Card c) => c.IsInPlay && c.IsDistortion, "distortion"),
                true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}