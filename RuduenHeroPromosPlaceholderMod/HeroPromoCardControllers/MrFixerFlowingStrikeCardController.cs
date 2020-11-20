using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.HeroPromos
{
    public class MrFixerFlowingStrikeCardController : CardController
    {
        public MrFixerFlowingStrikeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<int> powerNumerals = new List<int>()
            {
                this.GetPowerNumeral(0, 1), // Number of targets. 
                this.GetPowerNumeral(1, 1) // Amount of damage.
            };

            IEnumerator coroutine;

            // Deal target damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), powerNumerals[1], DamageType.Melee, powerNumerals[0], false, powerNumerals[0], cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal self damage.
            coroutine = base.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, powerNumerals[1], DamageType.Melee, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Grouped logic for putting the first tool or style into play and discarding the rest.
            coroutine = this.RevealCards_PutSomeIntoPlay_DiscardRemaining(this.DecisionMaker, this.HeroTurnTaker.Deck, null, new LinqCardCriteria((Card c) => c.IsTool || c.IsStyle, "tool or style"), isPutIntoPlay: false, revealUntilNumberOfMatchingCards: 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}