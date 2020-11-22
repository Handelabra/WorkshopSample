using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.HeroPromos
{
    public class ExpatrietteQuickshotCardController : CardController
    {
        public ExpatrietteQuickshotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {

            // Break down into selection.
            IEnumerator coroutine;
            List<SelectFunctionDecision> selectFunctionDecisions = new List<SelectFunctionDecision>();

            List<Function> list = new List<Function>
                {
                    new Function(this.DecisionMaker, "Discard a card", SelectionType.DiscardCard, () => this.DiscardAndCheckHandCard(), this.DecisionMaker.HasCardsInHand, this.TurnTaker.Name + " cannot discard the top card of their deck, so they must discard a card."),
                    new Function(this.DecisionMaker, "Discard the top card of your deck", SelectionType.DiscardFromDeck, () => this.DiscardAndCheckTopCard(), !this.HeroTurnTaker.Deck.IsEmpty, this.TurnTaker.Name + " cannot discard a card, so they must discard the top card of their deck.", null)
                };
            SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.DecisionMaker, list, false, null, this.TurnTaker.Name + " cannot discard a card nor discard the top card of their deck, so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));
            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, selectFunctionDecisions, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }


        }

        public IEnumerator DiscardAndCheckTopCard()
        {
            IEnumerator coroutine;
            List<MoveCardAction> storedResults = new List<MoveCardAction>();

            coroutine = this.GameController.DiscardTopCards(this.DecisionMaker, this.HeroTurnTaker.Deck, 1, storedResults);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If card is equipment, play it and use a power. 
            if (storedResults.Count > 0 && IsEquipment(storedResults.FirstOrDefault().CardToMove))
            {
                coroutine = this.IfEquipmentPlayCardAndUsePower(storedResults.FirstOrDefault().CardToMove);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public IEnumerator DiscardAndCheckHandCard()
        {
            IEnumerator coroutine;
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();

            coroutine = this.GameController.SelectAndDiscardCard(this.DecisionMaker, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // If card is equipment, play it and use a power. 
            if (storedResults.Count > 0 && IsEquipment(storedResults.FirstOrDefault().CardToDiscard))
            {
                coroutine = this.IfEquipmentPlayCardAndUsePower(storedResults.FirstOrDefault().CardToDiscard);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public IEnumerator IfEquipmentPlayCardAndUsePower(Card card)
        {
            IEnumerator coroutine;

            coroutine = this.GameController.PlayCard(this.DecisionMaker, card);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            coroutine = this.GameController.SelectAndUsePower(this.DecisionMaker, false);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}