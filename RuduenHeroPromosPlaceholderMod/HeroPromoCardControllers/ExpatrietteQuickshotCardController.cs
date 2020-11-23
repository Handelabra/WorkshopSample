using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenPromosWorkshop.HeroPromos
{
    public class ExpatrietteQuickshotCardController : HeroPromosSharedToHeroCardController
    {
        public ExpatrietteQuickshotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "ExpatrietteCharacter";
            PowerDescription = "Until the start of your next turn, reduce damage dealt by this hero by 1. Play the top card of your deck. Use a power.";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            IEnumerator coroutine;

            ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(this.GetPowerNumeral(0, 1));
            reduceDamageStatusEffect.SourceCriteria.IsSpecificCard = cardController.CharacterCard;
            reduceDamageStatusEffect.UntilStartOfNextTurn(cardController.TurnTaker);
            coroutine = base.AddStatusEffect(reduceDamageStatusEffect, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (cardController.HeroTurnTaker.Deck.IsEmpty)
            {
                coroutine = this.GameController.SendMessageAction(cardController.TurnTaker.Name + " cannot discard a card nor discard the top card of their deck, so" + this.Card.Title + " has no effect.", Priority.Medium, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                coroutine = this.GameController.PlayTopCard(cardController.DecisionMaker, cardController.TurnTakerController, numberOfCards: 1, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            coroutine = this.GameController.SelectAndUsePower(cardController.DecisionMaker, false, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

    }
}