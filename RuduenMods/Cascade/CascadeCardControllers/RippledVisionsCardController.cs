using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    public class RippledVisionsCardController : CascadeRiverSharedCardController
    {
        public RippledVisionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
			IEnumerator coroutine;
			Location riverDeck = this.RiverDeck();
			// Deck should be River Deck. Reveal top card of deck.
			List<Card> storedResults = new List<Card>();
			coroutine = base.GameController.RevealCards(base.TurnTakerController, riverDeck, 1, storedResults, false, RevealedCardDisplay.None, null, base.GetCardSource(null));
			if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

			// If card was found, ask where to move the card.
			Card card = storedResults.FirstOrDefault<Card>();
			if (card != null)
			{
				coroutine = base.GameController.SelectLocationAndMoveCard(base.HeroTurnTakerController, card, new MoveCardDestination[]
				{
						new MoveCardDestination(riverDeck, false, true, false),
						new MoveCardDestination(riverDeck, true, true, false)
				}, false, true, null, null, null, false, false, null, false, base.GetCardSource(null));
				if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

				// Damage based on revealed card, if it has a number.
				if (card.MagicNumber != null)
                {
					// Damage based on moved card's magic number.
					coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), (int) card.MagicNumber, DamageType.Cold, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
					if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
				}
			}

			// Cleanup. Shouldn't be necessary, but just in case.
			coroutine = base.CleanupCardsAtLocations(new List<Location>
				{
					riverDeck.OwnerTurnTaker.Revealed
				}, 
				riverDeck, false, true, false, false, false, true, storedResults);
			if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }



		}
    }
}