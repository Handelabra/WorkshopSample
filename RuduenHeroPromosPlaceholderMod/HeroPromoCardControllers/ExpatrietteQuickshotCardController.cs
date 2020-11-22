using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.HeroPromos
{
    public class ExpatrietteQuickshotCardController : HeroPromosSharedToHeroCardController
    {
        public ExpatrietteQuickshotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "ExpatrietteCharacter";
            PowerDescription = "";
        }

        public override IEnumerator PowerCoroutine(CardController cardController)
        {
            // Break down into selection.
            IEnumerator coroutine;
            List<SelectFunctionDecision> selectFunctionDecisions = new List<SelectFunctionDecision>();

            List<Function> list = new List<Function>
                {
                    new Function(cardController.DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => this.DiscardAndCheckHandCard(cardController), cardController.DecisionMaker.HasCardsInHand, this.TurnTaker.Name + " cannot discard the top card of their deck, so they must discard a card."),
                    new Function(cardController.DecisionMaker, "Discard the top card of your deck", SelectionType.DiscardFromDeck, () => this.DiscardAndCheckTopCard(cardController), !cardController.HeroTurnTaker.Deck.IsEmpty, this.TurnTaker.Name + " cannot discard a card, so they must discard the top card of their deck.", null)
                };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, cardController.DecisionMaker, list, false, null, cardController.TurnTaker.Name + " cannot discard a card nor discard the top card of their deck, so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));
            coroutine = cardController.GameController.SelectAndPerformFunction(selectFunction, selectFunctionDecisions, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public IEnumerator DiscardAndCheckTopCard(CardController cardController)
        {
            IEnumerator coroutine;
            List<MoveCardAction> storedResults = new List<MoveCardAction>();

            coroutine = this.GameController.DiscardTopCards(cardController.DecisionMaker, cardController.HeroTurnTaker.Deck, 1, storedResults);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If card is equipment, play it and use a power. 
            if (storedResults.Count > 0 && IsEquipment(storedResults.FirstOrDefault().CardToMove))
            {
                coroutine = this.IfEquipmentPlayCardAndUsePower(cardController, storedResults.FirstOrDefault().CardToMove);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public IEnumerator DiscardAndCheckHandCard(CardController cardController)
        {
            IEnumerator coroutine;
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();

            coroutine = this.GameController.SelectAndDiscardCard(cardController.DecisionMaker, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If card is equipment, play it and use a power. 
            if (storedResults.Count > 0 && IsEquipment(storedResults.FirstOrDefault().CardToDiscard))
            {
                coroutine = this.IfEquipmentPlayCardAndUsePower(cardController, storedResults.FirstOrDefault().CardToDiscard);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public IEnumerator IfEquipmentPlayCardAndUsePower(CardController cardController, Card card)
        {
            IEnumerator coroutine;

            coroutine = this.GameController.PlayCard(cardController.DecisionMaker, card);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            coroutine = this.GameController.SelectAndUsePower(cardController.DecisionMaker, false);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }


    }
}