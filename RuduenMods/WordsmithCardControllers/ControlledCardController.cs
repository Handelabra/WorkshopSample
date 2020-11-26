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
            // Play 2. 
            coroutine = this.SelectAndPlayCardsFromHand(this.DecisionMaker, 2, cardCriteria: new LinqCardCriteria((Card c) => c.DoKeywordsContain("prefix") || c.DoKeywordsContain("suffix"), "prefix or suffix"));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        protected override ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger. 
            ITrigger trigger = null; // Use null base to initialize. 
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.ActionSources == cardSource.ActionSources; // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same.
            int amountToSet = 1;

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd) && dd.CanDealDamage && dd.Amount > amountToSet && dd.Target.IsHero, 
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