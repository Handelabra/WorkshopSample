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
            PowerDescription = "Until the start of your next turn, reduce damage dealt by this hero by 1. Discard the top card of your deck. If it is an Equipment card, play it. Use a power.";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            IEnumerator coroutine;
            Card card;
            List<MoveCardAction> storedResults = new List<MoveCardAction>();

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
                coroutine = this.GameController.DiscardTopCards(cardController.DecisionMaker, cardController.HeroTurnTaker.Deck, 1, storedResults);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (storedResults != null)
                {
                    card = storedResults.FirstOrDefault().CardToMove;

                    if (card != null)
                    {
                        coroutine = this.GameController.SendMessageAction("Discarded " + card.Title + ".", Priority.Medium, this.GetCardSource(), new List<Card>() { card });
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                        // If card is equipment, play it.
                        if (card != null && IsEquipment(card))
                        {
                            coroutine = this.GameController.PlayCard(cardController.DecisionMaker, card);
                            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        }
                    }
                }

                coroutine = this.GameController.SelectAndUsePower(cardController.DecisionMaker, false, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }


        }

    }
}