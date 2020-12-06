using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Lifeline
{
    public class LifelineEnergyTapCharacterCardController : PromoDefaultCharacterCardController
    {
        public LifelineEnergyTapCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<int> powerNumerals = new List<int>()
            {
                this.GetPowerNumeral(0, 2), // Number of targets.
                this.GetPowerNumeral(1, 1), // Amount of damage.
            };

            List<DealDamageAction> storedResults = new List<DealDamageAction>();

            IEnumerator coroutine;

            // Deal up to 2 targets damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), powerNumerals[1], DamageType.Infernal, powerNumerals[0], false, 0, storedResultsDamage: storedResults, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Count unique targets damaged this way. 
            List<Card> damagedTargets = new List<Card>();
            foreach (DealDamageAction dd in storedResults.Where((DealDamageAction dd) => dd.DidDealDamage))
            {
                if (!damagedTargets.Contains(dd.Target))
                {
                    damagedTargets.Add(dd.Target);
                }
            }

            if (damagedTargets.Count > 0)
            {
                coroutine = this.GameController.GainHP(this.CharacterCard, damagedTargets.Count);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}