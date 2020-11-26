using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    public class VisionShockCardController : BreachMageSpellSharedCardController
    {
        public VisionShockCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator ActivateCast()
        {
            IEnumerator coroutine;
            // Damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 4, DamageType.Lightning, new int?(1), false, new int?(1), false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Reveal the top 2 cards of 1 deck and rearranged. Based on Hyperactive Senses.
            List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
            coroutine = this.SelectDecks(this.DecisionMaker, 1, SelectionType.RevealCardsFromDeck, (Location l) => true, storedResults, false);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Fetch selected deck.
            Location deck = (from l in storedResults
                             where l.Completed && l.SelectedLocation.Location != null
                             select l.SelectedLocation.Location).FirstOrDefault();
            if (deck != null)
            {
                // Reveal top 2 cards of deck.
                List<Card> storedCards = new List<Card>();
                coroutine = this.GameController.RevealCards(this.TurnTakerController, deck, 2, storedCards, false, RevealedCardDisplay.Message, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                while (storedCards.Count<Card>() > 0 && !this.GameController.IsGameOver)
                {
                    // If there are cards remaining, select the top card to return.
                    List<SelectCardDecision> storedTop = new List<SelectCardDecision>();
                    coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.MoveCardOnDeck, storedCards, storedTop, false, false, null, null, null, null, null, false, true, null);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    Card topCard = this.GetSelectedCard(storedTop);
                    if (topCard != null)
                    {
                        // Move other cards on top first, so the selected one is the 'top' card.
                        Card otherCard = (from c in storedCards
                                          where c.NativeDeck == topCard.NativeDeck && c != topCard
                                          select c).FirstOrDefault<Card>();
                        if (otherCard != null)
                        {
                            coroutine = this.GameController.MoveCard(this.TurnTakerController, otherCard, otherCard.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, this.GetCardSource(null));
                            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                            storedCards.Remove(otherCard);
                        }

                        // Move actual top card.
                        coroutine = this.GameController.MoveCard(this.TurnTakerController, topCard, topCard.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        storedCards.Remove(topCard);
                        otherCard = null;
                    }
                    topCard = null;
                    storedTop = null;
                }
                // Cleanup all revealed cards if there was some catastrophic failure.
                coroutine = this.CleanupCardsAtLocations(new List<Location> { deck.OwnerTurnTaker.Revealed }, deck, false, true, false, false, false, true, storedCards);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}