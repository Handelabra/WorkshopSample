using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller.TheSentinels;

namespace Workshopping.TheSentinels
{
    public class MainsnakeCharacterCardController : SentinelHeroCharacterCardController
    {
        public MainsnakeCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

		public override IEnumerator UsePower(int index = 0)
		{
			// Mainstay deals 1 target 3 melee damage.
			// The next damage dealt to that target is irreducible.
			var numberOfTargets = GetPowerNumeral(0, 1);
			var damageAmount = GetPowerNumeral(1, 3);
			return this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker,
																   new DamageSource(this.GameController, this.Card),
																   damageAmount,
																   DamageType.Melee,
																   numberOfTargets,
																   false,
																   numberOfTargets,
																   addStatusEffect: NextDamageIsIrreducibleResponse,
																   selectTargetsEvenIfCannotDealDamage: true,
																   cardSource: GetCardSource());
		}

		private IEnumerator NextDamageIsIrreducibleResponse(DealDamageAction dd)
		{
			// The next damage dealt to that target is irreducible.
			MakeDamageIrreducibleStatusEffect effect = new MakeDamageIrreducibleStatusEffect();
			effect.TargetCriteria.IsSpecificCard = dd.Target;
			effect.NumberOfUses = 1;
			effect.UntilCardLeavesPlay(dd.Target);

			return AddStatusEffect(effect);
		}
	}
}
