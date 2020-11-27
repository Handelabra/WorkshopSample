using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Spellforge
{
    // TODO: TEST!
    public class ArticulateTheLethologicalCardController : CardController
    {
        public ArticulateTheLethologicalCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Select type. 
            List<Function> list = new List<Function>
                {
                    new Function(this.DecisionMaker, "Prefix", SelectionType.RevealTopCardOfDeck,
                    () => this.RevealCards_MoveMatching_ReturnNonMatchingCards(this.DecisionMaker, this.HeroTurnTaker.Deck, false, false, true, new LinqCardCriteria((Card c) => c.DoKeywordsContain("prefix"), "prefix"), 2, shuffleReturnedCards: true),
                    this.HeroTurnTaker.Deck.Cards.Count() > 0),
                     new Function(this.DecisionMaker, "Essence", SelectionType.RevealTopCardOfDeck,
                    () => this.RevealCards_MoveMatching_ReturnNonMatchingCards(this.DecisionMaker, this.HeroTurnTaker.Deck, false, false, true, new LinqCardCriteria((Card c) => c.DoKeywordsContain("essence"), "essence"), 2, shuffleReturnedCards: true),
                    this.HeroTurnTaker.Deck.Cards.Count() > 0),
                      new Function(this.DecisionMaker, "Suffix", SelectionType.RevealTopCardOfDeck,
                    () => this.RevealCards_MoveMatching_ReturnNonMatchingCards(this.DecisionMaker, this.HeroTurnTaker.Deck, false, false, true, new LinqCardCriteria((Card c) => c.DoKeywordsContain("suffix"), "suffix"), 2, shuffleReturnedCards: true),
                    this.HeroTurnTaker.Deck.Cards.Count() > 0),
                };

            SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.DecisionMaker, list, false, null, this.TurnTaker.Name + " does not have any cards in their deck, so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));
            coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // You may play card.
            coroutine = this.SelectAndPlayCardFromHand(this.DecisionMaker, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}