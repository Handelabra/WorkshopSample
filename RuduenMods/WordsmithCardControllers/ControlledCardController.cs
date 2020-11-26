using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;

namespace RuduenWorkshop.Wordsmith
{
    // TODO: TEST!
    public class ControlledCardController : WordsmithSharedModifierCardController
    {
        public ControlledCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Destroy.
            coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing", true, false, null, null, false), false, null, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw. 
            coroutine = this.DrawCard(this.HeroTurnTaker, false, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override ITrigger AddModifierTrigger(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger. 
            ITrigger trigger = base.AddModifierTrigger(cardSource); // Use null base to initialize. 
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.ActionSources == cardSource.ActionSources; // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same.
            int amountToSet = 1;

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd) && dd.CanDealDamage && dd.Amount > amountToSet, 
                (DealDamageAction dd) => this.TrackOriginalTargetsAndRunResponse(dd, cardSource, amountToSet, trigger), 
                new TriggerType[]
                {
                    TriggerType.ReduceDamage
                }, 
                TriggerTiming.Before);

            return trigger;
        }

        protected override IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            IEnumerator coroutine;
            int amountToSet = (int) otherObjects[0];
            ITrigger trigger = (ITrigger) otherObjects[1];
            coroutine = this.GameController.ReduceDamage(dd, dd.Amount - amountToSet, trigger, cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

    }
}