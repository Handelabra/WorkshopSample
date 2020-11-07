using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Cascade
{
    public class CascadeTurnTakerController : HeroTurnTakerController
    {
        public CascadeTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }

        public override IEnumerator StartGame()
        {
            IEnumerator coroutine;
            Location riverDeck = this.TurnTaker.FindSubDeck("RiverDeck");
            // At the start of game, move all River cards into River deck. (This best preserves the 'identity' of the cards.)

            // It's messy, but search through all river cards from Cascade's deck and always move them to the river deck.
            int handSize = this.NumberOfCardsInHand;
            IEnumerable<Card> riverCards = base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
            coroutine = base.GameController.MoveCards(this, riverCards, riverDeck);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Then shuffle.
            coroutine = base.GameController.ShuffleLocation(riverDeck);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Then, move the top four to the riverbank. That should already exist due to being a non-real card.
            IEnumerable<Card> riverbankCards = riverDeck.GetTopCards(4);
            coroutine = base.GameController.MoveCards(this, riverbankCards, this.TurnTaker.FindCard("Riverbank", false).UnderLocation );
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Then move enough replacements to get starting cards to hand. Don't draw to avoid animation. This is the start of game, so it just loops. It's messy, but it works for now!
            // TODO: Seriously, is there a better option?
            while (this.HeroTurnTaker.NumberOfCardsInHand < handSize)
            {
                coroutine = this.GameController.MoveCard(this, this.HeroTurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        // Old logic for River.
        //// It's messy, but search through all river cards from Cascade's deck and always move them to the river deck.
        //coroutine = base.GameController.MoveCards(this,
        //    new LinqCardCriteria((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker, "river", true, false, null, null, false),
        //    (Card c) => this.TurnTaker.FindSubDeck("RiverDeck"),
        //    false, SelectionType.MoveCard, null, false, null, true, false, false, null, null, false, MoveCardDisplay.None, null);
        //if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

        // ShuffleCardsIntoLocation doesn't seem to work gracefully.
        //IEnumerable<Card> riverCards = base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
        //// Find all river cards and shuffle them into the river deck.
        //coroutine = base.GameController.ShuffleCardsIntoLocation(this, riverCards, this.TurnTaker.FindSubDeck("RiverDeck"));
    }
}