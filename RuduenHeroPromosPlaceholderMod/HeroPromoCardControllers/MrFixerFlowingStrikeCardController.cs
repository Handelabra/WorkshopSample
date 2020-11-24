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
            PowerDescription = "This Hero deals 1 Target 1 Melee Damage and themselves 0 Melee Damage. Play a Tool card. If you do not, reveal cards from the top of your deck until you reveal a Tool card, play it, and shuffle the other cards revealed this way into your deck.";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            List<int> powerNumerals = new List<int>()
            {
                cardController.GetPowerNumeral(0, 1), // Number of targets. 
                cardController.GetPowerNumeral(1, 1), // Amount of damage.
                cardController.GetPowerNumeral(2, 0) // Amount of damage.
            };

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Deal target damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(cardController.DecisionMaker, new DamageSource(this.GameController, cardController.CharacterCard), powerNumerals[1], DamageType.Melee, powerNumerals[0], false, powerNumerals[0], cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, cardController.CharacterCard), cardController.CharacterCard, powerNumerals[2], DamageType.Melee, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Play a tool card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(cardController.HeroTurnTakerController, false, cardCriteria: new LinqCardCriteria((Card c) => c.IsTool, "tool"), storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults != null && (storedResults.Count == 0 || !storedResults.FirstOrDefault().IsSuccessful))
            {
                coroutine = this.RevealCards_MoveMatching_ReturnNonMatchingCards(cardController.HeroTurnTakerController, cardController.HeroTurnTaker.Deck, true, false, false, new LinqCardCriteria((Card c) => c.IsTool, "tool"), 1, showMessage: true);

                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            }
        }
    }
}