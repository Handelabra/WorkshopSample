using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    // TODO: TEST!
    public class OfAuraCardController : SpellforgeSharedModifierCardController
    {
        public OfAuraCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Destroy.
            coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria((Card c) => c.IsEnvironment, "environment", true, false, null, null, false), false, null, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw 1.
            coroutine = this.DrawCard(this.HeroTurnTaker);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        protected override ITrigger AddModifierTriggerOverride(CardSource cardSource)
        {
            // Mostly copied from AddReduceDamageToSetAmountTrigger since that doesn't return an ITrigger.
            ITrigger trigger = null; // Use null base to initialize.

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