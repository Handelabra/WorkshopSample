using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Spellforge
{
    // TODO: TEST!
    public class ControlledCardController : SpellforgeSharedModifierCardController
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
            int amountToSet = 1;
            bool damageCriteria(DealDamageAction dd) => dd.CardSource.Card == cardSource.Card && dd.CanDealDamage && dd.Amount > amountToSet && dd.Target.IsHero;

            trigger = this.AddTrigger<DealDamageAction>((DealDamageAction dd) => damageCriteria(dd),
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
            int amountToSet = (int)otherObjects[0];
            ITrigger trigger = (ITrigger)otherObjects[1];
            coroutine = this.GameController.ReduceDamage(dd, dd.Amount - amountToSet, trigger, cardSource);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}