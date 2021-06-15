using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller.TheSentinels;

namespace Workshopping.TheSentinels
{
    public class TheIdealizardCharacterCardController : SentinelHeroCharacterCardController
    {
        public TheIdealizardCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

		public override IEnumerator UsePower(int index = 0)
		{
			// The Idealist deals 1 target 2 psychic damage.
			// Until the start of your next turn, reduce damage dealt by a target dealt damage this way by 1.
			var numberOfTargets = GetPowerNumeral(0, 1);
			var damageAmount = GetPowerNumeral(1, 2);
			var damageReduceAmount = GetPowerNumeral(2, 1);
			return this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker,
																   new DamageSource(this.GameController, this.Card),
																   damageAmount,
																   DamageType.Psychic,
																   numberOfTargets,
																   false,
																   numberOfTargets,
																   addStatusEffect: dd => ReduceDamageResponse(dd, damageReduceAmount),
																   cardSource: GetCardSource());
		}

		private IEnumerator ReduceDamageResponse(DealDamageAction dd, int damageReduceAmount)
		{
			if (dd.DidDealDamage && dd.Target.IsInPlayAndHasGameText)
			{
				ReduceDamageStatusEffect effect = new ReduceDamageStatusEffect(damageReduceAmount);
				effect.SourceCriteria.IsSpecificCard = dd.Target;
				effect.UntilStartOfNextTurn(this.TurnTaker);
				effect.UntilCardLeavesPlay(dd.Target);

				var e = AddStatusEffect(effect);
				if (UseUnityCoroutines)
				{
					yield return this.GameController.StartCoroutine(e);
				}
				else
				{
					this.GameController.ExhaustCoroutine(e);

				}
			}
		}
	}
}
