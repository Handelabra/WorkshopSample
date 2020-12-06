using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.VoidGuardMainstay
{
    public class VoidGuardMainstayShrugItOffCharacterCardController : HeroCharacterCardController
    {
        public VoidGuardMainstayShrugItOffCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            int powerNumeral = GetPowerNumeral(0, 1);
            string turnTakerName;

            // Draw a card.
            coroutine = this.GameController.DrawCard(this.HeroTurnTaker);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Set up response.
            if (base.TurnTaker.IsHero)
            {
                turnTakerName = this.TurnTaker.Name;
            }
            else
            {
                turnTakerName = this.Card.Title;
            }

            // ReduceDamage isn't technically an accurate trigger, but CancelAction alone causes the numbers to not work well when boosting or reducing, so it has to be kept. 
            OnDealDamageStatusEffect onDealDamageStatusEffect = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "PreventResponse", "Whenever " + turnTakerName + " is dealt exactly " + powerNumeral + " damage, prevent that damage.", new TriggerType[] { TriggerType.ReduceDamage, TriggerType.CancelAction }, this.HeroTurnTaker, this.Card, new int[] { powerNumeral });
            onDealDamageStatusEffect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            onDealDamageStatusEffect.CanEffectStack = false;
            onDealDamageStatusEffect.BeforeOrAfter = BeforeOrAfter.Before;
            onDealDamageStatusEffect.DamageAmountCriteria.EqualTo = powerNumeral;
            onDealDamageStatusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);

            coroutine = this.AddStatusEffect(onDealDamageStatusEffect);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

#pragma warning disable IDE0060 // Remove unused parameter

        public IEnumerator PreventResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // Exactly 1 damage is checked in the Criteria, so just prevent the damage.
            IEnumerator coroutine = this.CancelAction(dd, true, true, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override bool CanOrderAffectOutcome(GameAction action)
        {
            return action is DealDamageAction && (action as DealDamageAction).Target == this.CharacterCard;
        }
    }
}