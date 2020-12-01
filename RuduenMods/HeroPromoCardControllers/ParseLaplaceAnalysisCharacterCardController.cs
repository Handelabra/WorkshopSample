using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
            int powerNumeral = this.GetPowerNumeral(0, 1);

            List<SelectLocationDecision> storedResultsDeck = new List<SelectLocationDecision>();
            List<DealDamageAction> storedResultsDamage = new List<DealDamageAction>();

            // Select a non-hero deck.
            coroutine = this.GameController.SelectADeck(this.DecisionMaker, SelectionType.RevealTopCardOfDeck, (Location l) => !l.IsHero, storedResultsDeck, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            Location deck = this.GetSelectedLocation(storedResultsDeck);
            if (deck != null)
            {
                // Reveal a card.
                List<Card> storedCardResults = new List<Card>();
                coroutine = this.GameController.RevealCards(base.TurnTakerController, deck, 1, storedCardResults, false, RevealedCardDisplay.ShowRevealedCards, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                coroutine = this.CleanupCardsAtLocations(new List<Location> { deck.OwnerTurnTaker.Revealed }, deck, cardsInList: storedCardResults);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            int trashCount = deck.OwnerTurnTaker.Trash.NumberOfCards;

            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.Card), trashCount, DamageType.Projectile, powerNumeral, false, 0, storedResultsDamage: storedResultsDamage, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResultsDamage.Count > 0 && storedResultsDamage.Any((DealDamageAction dd) => dd.DidDealDamage))
            {
                coroutine = this.GameController.ShuffleTrashIntoDeck(this.FindTurnTakerController(deck.OwnerTurnTaker), cardSource: this.GetCardSource());
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}