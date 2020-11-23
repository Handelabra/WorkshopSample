using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenPromosWorkshop.HeroPromos
{
    public class MrFixerFlowingStrikeCardController : HeroPromosSharedToHeroCardController
    {
        public MrFixerFlowingStrikeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "MrFixerCharacter";
            PowerDescription = "This Hero deals 1 Target 1 Melee Damage and themselves 1 Melee Damage. Play a Tool card. If you do not, reveal cards from the top of your deck until you reveal a Tool card, play it, and discard the other cards revealed this way.";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            List<int> powerNumerals = new List<int>()
            {
                cardController.GetPowerNumeral(0, 1), // Number of targets. 
                cardController.GetPowerNumeral(1, 1) // Amount of damage.
            };

            IEnumerator coroutine;

            // Deal target damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(cardController.DecisionMaker, new DamageSource(this.GameController, cardController.CharacterCard), powerNumerals[1], DamageType.Melee, powerNumerals[0], false, powerNumerals[0], cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, cardController.CharacterCard), cardController.CharacterCard, powerNumerals[1], DamageType.Melee, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Selection: Draw or use a cast and destroy. 

            List<Function> list = new List<Function>
                {
                    new Function(cardController.DecisionMaker, "Play a Tool card", SelectionType.PlayCard,
                    () => cardController.GameController.SelectAndPlayCardFromHand(cardController.HeroTurnTakerController, false, cardCriteria: new LinqCardCriteria((Card c) => c.IsTool, "tool"), cardSource: cardController.GetCardSource()),
                    cardController.HeroTurnTaker.Hand.Cards.Any(c => c.IsTool), cardController.TurnTaker.Name + " cannot reveal cards from their deck, so they must Play a Tool card.", null),
                    new Function(cardController.DecisionMaker, "Activate a card's Cast effect and destroy that card", SelectionType.ActivateAbility, 
                    () => this.RevealCards_PutSomeIntoPlay_DiscardRemaining(cardController.DecisionMaker, cardController.HeroTurnTaker.Deck, null, cardCriteria: new LinqCardCriteria((Card c) => c.IsTool, "tool"), 
                    isPutIntoPlay: false, revealUntilNumberOfMatchingCards: 1), 
                    cardController.HeroTurnTaker.Deck.Cards.Count() > 0 , cardController.TurnTaker.Name + " cannot play any Tool cards, so they must reveal cards until they reveal a Tool card and play it.", null)
                };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.DecisionMaker, list, false, null, this.TurnTaker.Name + " cannot play any tool cards nor reveal cards from their deck so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));
            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}