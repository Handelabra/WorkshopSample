using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Luminary
{
    public class LuminaryReprogramCharacterCardController : PromoDefaultCharacterCardController
    {
        public LuminaryReprogramCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int[] powerNumerals = new int[] {
                this.GetPowerNumeral(0, 5),
                this.GetPowerNumeral(1, 2)
            };
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

            IEnumerator coroutine;

            // Discard the top card of the deck.
            coroutine = DiscardCardsFromTopOfDeck(this.DecisionMaker, 1, responsibleTurnTaker: this.HeroTurnTaker);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Select a device target.
            coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTarget,
                new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsDevice && c.IsTarget, "device target"),
                storedResults, false, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            Card selectedCard = this.GetSelectedCard(storedResults);
            if (selectedCard != null)
            {
                // Device Damages Self.
                coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, selectedCard), selectedCard, powerNumerals[0], DamageType.Lightning, cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // If device was destroyed...
                DestroyCardJournalEntry destroyCardJournalEntry =
                    (from dcje in this.Journal.DestroyCardEntriesThisTurn()
                     where selectedCard == dcje.Card
                     select dcje).LastOrDefault<DestroyCardJournalEntry>();
                if (destroyCardJournalEntry != null && destroyCardJournalEntry.Card != null)
                {
                    // Draw 2 cards.
                    coroutine = this.DrawCards(this.DecisionMaker, powerNumerals[1]);
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    // You may play a device.
                    coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true,
                        cardCriteria: new LinqCardCriteria((Card c) => c.IsDevice, "device"),
                        cardSource: this.GetCardSource());
                    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
        }
    }
}