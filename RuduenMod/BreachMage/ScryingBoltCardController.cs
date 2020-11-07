using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class ScryingBoltCardController : BreachMageSpellSharedCardController
    {
        public ScryingBoltCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator ActivateCast()
        {
            IEnumerator coroutine;
            // Damage.
            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 4, DamageType.Lightning, new int?(1), false, new int?(1), false, false, false, null, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Reveal the top 2 cards of 1 deck and rearranged. Based on Hyperactive Senses.
            List<SelectLocationDecision> storedResults = new List<SelectLocationDecision>();
            coroutine = base.SelectDecks(this.DecisionMaker, 1, SelectionType.RevealCardsFromDeck, (Location l) => true, storedResults, false);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Fetch selected deck.
            Location deck = (from l in storedResults
                             where l.Completed && l.SelectedLocation.Location != null
                             select l.SelectedLocation.Location).FirstOrDefault();
            if (deck != null)
            {
                // Reveal top 2 cards of deck.
                List<Card> storedCards = new List<Card>();
                coroutine = base.GameController.RevealCards(base.TurnTakerController, deck, 2, storedCards, false, RevealedCardDisplay.Message, null, base.GetCardSource(null));
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
                while (storedCards.Count<Card>() > 0 && !base.GameController.IsGameOver)
                {
                    // If there are cards remaining, select the top card to return.
                    List<SelectCardDecision> storedTop = new List<SelectCardDecision>();
                    coroutine = base.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.MoveCardOnDeck, storedCards, storedTop, false, false, null, null, null, null, null, false, true, null);
                    if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

                    Card topCard = base.GetSelectedCard(storedTop);
                    if (topCard != null)
                    {
                        // Move other cards on top first, so the selected one is the 'top' card.
                        Card otherCard = (from c in storedCards
                                          where c.NativeDeck == topCard.NativeDeck && c != topCard
                                          select c).FirstOrDefault<Card>();
                        if (otherCard != null)
                        {
                            coroutine = base.GameController.MoveCard(base.TurnTakerController, otherCard, otherCard.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, base.GetCardSource(null));
                            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
                            storedCards.Remove(otherCard);
                        }

                        // Move actual top card.
                        coroutine = base.GameController.MoveCard(base.TurnTakerController, topCard, topCard.NativeDeck, false, false, true, null, false, null, null, null, false, false, null, false, false, false, false, base.GetCardSource(null));
                        if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
                        storedCards.Remove(topCard);
                        otherCard = null;
                    }
                    topCard = null;
                    storedTop = null;
                }
                // Cleanup all revealed cards if there was some catastrophic failure.
                coroutine = base.CleanupCardsAtLocations(new List<Location> { deck.OwnerTurnTaker.Revealed }, deck, false, true, false, false, false, true, storedCards);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}