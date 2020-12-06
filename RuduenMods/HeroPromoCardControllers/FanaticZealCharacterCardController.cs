using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Fanatic
{
    public class FanaticZealCharacterCardController : PromoDefaultCharacterCardController
    {
        public FanaticZealCharacterCardController(Card card, TurnTakerController turnTakerController)
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

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Deal target damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), powerNumerals[1], DamageType.Radiant, powerNumerals[0], false, 0, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, powerNumerals[1], DamageType.Radiant, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw or play.
            List<Function> list = new List<Function>() {
                new Function(this.HeroTurnTakerController, "Play a card", SelectionType.PlayCard, 
                    () => this.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false), 
                    this.CanPlayCardsFromHand(this.DecisionMaker), 
                    this.HeroTurnTaker.Name + " cannot use any powers, so they must play a card."),
                new Function(this.HeroTurnTakerController, "Use a Power", SelectionType.UsePower, 
                    () => this.GameController.SelectAndUsePower(this.DecisionMaker, false, cardSource: this.GetCardSource()), 
                    this.GameController.CanUsePowers(this.DecisionMaker, cardSource: this.GetCardSource()) && this.GameController.GetUsablePowersThisTurn(this.DecisionMaker, cardSource: this.GetCardSource()).Count() > 0, 
                    this.HeroTurnTaker.Name + " cannot play any cards, so they must use a power.")
            };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(base.GameController, this.HeroTurnTakerController, list, false, null, this.HeroTurnTaker.Name + " cannot play any cards or use any powers.", cardSource: this.GetCardSource());
            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}