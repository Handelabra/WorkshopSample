using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.NightMist
{
    public class NightMistLimitedNumerologyCharacterCardController : PromoDefaultCharacterCardController
    {
        public NightMistLimitedNumerologyCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);
            IEnumerator coroutine;

            // Reduce Nightmist's damage to hero targets.
            ReduceDamageStatusEffect statusEffect = new ReduceDamageStatusEffect(powerNumeral);
            statusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);
            statusEffect.TargetCriteria.IsHero = true;
            statusEffect.SourceCriteria.IsSpecificCard = this.CharacterCard;

            coroutine = this.AddStatusEffect(statusEffect, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw or play.
            List<Function> list = new List<Function>() {
                new Function(this.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => base.DrawCard(base.HeroTurnTaker)),
                new Function(this.HeroTurnTakerController, "Play a card", SelectionType.PlayCard, () => base.SelectAndPlayCardFromHand(base.HeroTurnTakerController, false))
            };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, base.HeroTurnTakerController, list, false, cardSource: this.GetCardSource());
            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}