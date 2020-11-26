using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Wordsmith
{
    // TODO: TEST!
    public class OfInspirationCardController : WordsmithSharedModifierCardController
    {
        public OfInspirationCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }


        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Draw 3. 
            coroutine = this.DrawCards(this.DecisionMaker, 3);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override ITrigger AddModifierTrigger(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger. 
            ITrigger trigger = base.AddModifierTrigger(cardSource); // Use null base to initialize. 

            // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same. 
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.ActionSources == cardSource.ActionSources && dd.Target.IsHeroCharacterCard && dd.DidDealDamage;

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd),
                (DealDamageAction dd) => this.TrackOriginalTargetsAndRunResponse(dd, cardSource),
                new TriggerType[]
                {
                    TriggerType.DrawCard
                },
                TriggerTiming.After);

            return trigger;
        }

        protected override IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            IEnumerator coroutine;
            // Draw Response - Should target only characters, but generic sanity check is included.
            coroutine = this.DrawCard(dd.Target.Owner.ToHero());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}