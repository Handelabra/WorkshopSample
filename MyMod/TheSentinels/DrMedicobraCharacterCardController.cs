using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller.TheSentinels;

namespace Workshopping.TheSentinels
{
    public class DrMedicobraCharacterCardController : SentinelHeroCharacterCardController
    {
        public DrMedicobraCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

		public override IEnumerator UsePower(int index = 0)
		{
			var hpGainAmount = GetPowerNumeral(0, 2);
			// Each of The Sentinels regains 2 HP.
			var e = this.GameController.GainHP(this.DecisionMaker,
											   c => c.IsHeroCharacterCard
													&& c.Owner == this.TurnTaker,
											   hpGainAmount,
											   cardSource: GetCardSource());
			if (UseUnityCoroutines)
			{
				yield return this.GameController.StartCoroutine(e);
			}
			else
			{
				this.GameController.ExhaustCoroutine(e);

			}

			// You may draw a card.
			e = DrawCard(optional: true);
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
