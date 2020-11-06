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
            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.Card), powerNumerals[1], DamageType.Projectile, powerNumerals[0], false, powerNumerals[0], false, false, false, null, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            //coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), powerNumerals[1], DamageType.Melee, powerNumerals[0], false, powerNumerals[1]);

            // Heal.
            coroutine = base.GameController.GainHP(this.CharacterCard, powerNumerals[2]);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            // Discard card.
            coroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, true, null, storedResults, SelectionType.DiscardCard);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            if (base.DidDiscardCards(storedResults))
            {
                // If you do, draw a card.
                coroutine = base.DrawCard(this.HeroTurnTaker, false, null, true);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}