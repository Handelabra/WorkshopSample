using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    // TODO: TEST!
    public class OfDisruptionCardController : SpellforgeSharedModifierCardController
    {
        public OfDisruptionCardController(Card card, TurnTakerController turnTakerController)
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
            coroutine = this.DealDamage(dd.Target, dd.Target, 1, DamageType.Sonic, cardSource: cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}