using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Wordsmith
{
    // TODO: TEST!
    public class OfResonanceCardController : WordsmithSharedModifierCardController
    {
        public OfResonanceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }


        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Deal 1 target 2 sonic. 
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 2, DamageType.Projectile, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw. 
            coroutine = this.DrawCard(this.HeroTurnTaker, false, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        protected override ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger. 
            ITrigger trigger = null; // Use null base to initialize. 

            // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same. 
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.ActionSources == cardSource.ActionSources && !dd.Target.IsHero && dd.DidDealDamage;

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd), 
                (DealDamageAction dd) => this.TrackOriginalTargetsAndRunResponse(dd, cardSource), 
                new TriggerType[]
                {
                    TriggerType.DealDamage
                }, 
                TriggerTiming.After);

            return trigger;
        }

        protected override IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            IEnumerator coroutine;

            // Self Damage Response
            coroutine = this.DealDamage(this.CharacterCard, dd.Target, 1, DamageType.Sonic, cardSource: cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}