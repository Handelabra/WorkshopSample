using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
{
    public class BackupPlanCardController : CardController
    {
        public BackupPlanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
		
		public override IEnumerator UsePower(int index = 0)
		{
            List<int> powerNumerals = new List<int>();
            powerNumerals.Add(base.GetPowerNumeral(0, 1));
            powerNumerals.Add(base.GetPowerNumeral(1, 1));
            powerNumerals.Add(base.GetPowerNumeral(2, 1));


            IEnumerator coroutine;
            // Deal Damage.
            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), powerNumerals[2], DamageType.Melee, powerNumerals[1], false, 1);
                        if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            // Heal.
            coroutine = base.GameController.GainHP(this.CharacterCard, powerNumerals[3]);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

            // Discard card.
            coroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, true, null, null, SelectionType.DiscardCard);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }

        }
	}
}
