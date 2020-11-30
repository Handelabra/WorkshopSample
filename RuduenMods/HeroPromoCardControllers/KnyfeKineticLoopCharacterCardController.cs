using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Linq;

namespace RuduenWorkshop.Knyfe
{
    public class KnyfeKineticLoopCharacterCardController : PromoDefaultCharacterCardController
    {
        public KnyfeKineticLoopCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            string turnTakerName;
            IEnumerator coroutine;
            int powerNumeral = this.GetPowerNumeral(0, 1);

            // Draw a card.
            coroutine = this.GameController.DrawCard(this.HeroTurnTaker);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Set up heal response.
            if (base.TurnTaker.IsHero)
            {
                turnTakerName = this.TurnTaker.Name;
            }
            else
            {
                turnTakerName = this.Card.Title;
            }
            OnDealDamageStatusEffect onDealDamageStatusEffect = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "HealResponse", "Whenever " + turnTakerName + " is dealt damage, they regain " + powerNumeral + " HP.", new TriggerType[] { TriggerType.DealDamage }, this.HeroTurnTaker, this.Card, new int[] { powerNumeral });
            onDealDamageStatusEffect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            onDealDamageStatusEffect.SourceCriteria.IsSpecificCard = this.CharacterCard;
            onDealDamageStatusEffect.CanEffectStack = true;
            onDealDamageStatusEffect.BeforeOrAfter = BeforeOrAfter.After;
            onDealDamageStatusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);

            coroutine = this.AddStatusEffect(onDealDamageStatusEffect);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        }

        #pragma warning disable IDE0060 // Remove unused parameter

        public IEnumerator HealResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerator coroutine;
            int? powerNumeral = null;
            if (powerNumerals != null)
            {
                powerNumeral = powerNumerals.ElementAtOrDefault(0);
            }
            if (powerNumeral == null)
            {
                powerNumeral = 1;
            }

            // Heal.
            if (dd.DidDealDamage)
            {
                coroutine = this.GameController.GainHP(hero.CharacterCard, powerNumeral);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}