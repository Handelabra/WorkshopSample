using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    // TODO: TEST!
    public class InspiredCardController : SpellforgeSharedModifierCardController
    {
        public InspiredCardController(Card card, TurnTakerController turnTakerController)
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

        protected override ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger.
            ITrigger trigger = null;
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.ActionSources == cardSource.ActionSources; // Only if the action sources of this play and the damage are an exact match, AKA the triggering step is the same.

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd),
                (DealDamageAction dd) => this.TrackOriginalTargetsAndRunResponse(dd, cardSource),
                new TriggerType[]
                {
                    TriggerType.IncreaseDamage
                },
                TriggerTiming.Before);

            return trigger;
        }

        protected override IEnumerator RunResponse(DealDamageAction dd, CardSource cardSource, params object[] otherObjects)
        {
            IEnumerator coroutine;

            // Deal damage response.
            coroutine = this.GameController.IncreaseDamage(dd, 1, cardSource: cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}