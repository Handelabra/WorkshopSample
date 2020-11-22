using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.HeroPromos
{
    public class MrFixerFlowingStrikeCardController : HeroPromosSharedToHeroCardController
    {
        public MrFixerFlowingStrikeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "MrFixerCharacter";
            PowerDescription = "This Hero deals 1 Target and themselves 1 Melee Damage. Reveal cards from the top of your deck until you reveal a Tool or Style card, play it, and discard the other cards revealed this way.";
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

            // Grouped logic for putting the first tool or style into play and discarding the rest.
            coroutine = this.RevealCards_PutSomeIntoPlay_DiscardRemaining(cardController.DecisionMaker, cardController.HeroTurnTaker.Deck, null, new LinqCardCriteria((Card c) => c.IsTool || c.IsStyle, "tool or style"), isPutIntoPlay: false, revealUntilNumberOfMatchingCards: 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}