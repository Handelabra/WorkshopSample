using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Parse
{
    public class ParseLaplaceAnalysisCharacterCardController : PromoDefaultCharacterCardController
    {
        public ParseLaplaceAnalysisCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;

            // Select a non-hero deck.
            List<SelectLocationDecision> storedDeckResults = new List<SelectLocationDecision>();
            coroutine = this.GameController.SelectADeck(this.DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => !l.IsHero, storedDeckResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            Location deck = this.GetSelectedLocation(storedDeckResults);
            if (deck != null)
            {
                // Reveal a card.
                List<Card> storedCardResults = new List<Card>();
                coroutine = this.GameController.RevealCards(base.TurnTakerController, deck, 1, storedCardResults, false, RevealedCardDisplay.None, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                coroutine = this.CleanupCardsAtLocations(new List<Location>
                {
                    deck.OwnerTurnTaker.Revealed
                }, deck, cardsInList: storedCardResults);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // Yes/No for shuffling cards back.
            int cardsToShuffle = deck.OwnerTurnTaker.Trash.NumberOfCards;
            if (cardsToShuffle > 0)
            {
                // Ask if we should shuffle the trash to the deck.
                YesNoAmountDecision yesNoDecision = new YesNoAmountDecision(this.GameController, this.DecisionMaker, SelectionType.ShuffleCardFromTrashIntoDeck, cardsToShuffle);
                coroutine = this.GameController.MakeDecisionAction(yesNoDecision);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.DidPlayerAnswerYes(yesNoDecision))
                {
                    int powerNumeral = this.GetPowerNumeral(0, 1);
                    coroutine = this.GameController.ShuffleTrashIntoDeck(this.FindTurnTakerController(deck.OwnerTurnTaker), cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.Card), cardsToShuffle, DamageType.Projectile, powerNumeral, false, powerNumeral, cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else // No cards in trash. 
            {
                coroutine = this.GameController.SendMessageAction("There are no cards in the corresponding trash.", Priority.Medium, this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}