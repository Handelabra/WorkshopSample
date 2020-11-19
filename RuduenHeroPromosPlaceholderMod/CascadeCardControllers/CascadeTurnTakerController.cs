using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Cascade
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

            // Shuffle river deck.
            coroutine = this.GameController.ShuffleLocation(riverDeck);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Then, move the top four to the riverbank. That should already exist due to being a non-real card.
            coroutine = this.GameController.MoveCards(this, riverDeck.GetTopCards(4), this.TurnTaker.FindCard("Riverbank", false).UnderLocation);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        // Old logic for River.
        //// It's messy, but search through all river cards from Cascade's deck and always move them to the river deck.
        //coroutine = this.GameController.MoveCards(this,
        //    new LinqCardCriteria((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker, "river", true, false, null, null, false),
        //    (Card c) => this.TurnTaker.FindSubDeck("RiverDeck"),
        //    false, SelectionType.MoveCard, null, false, null, true, false, false, null, null, false, MoveCardDisplay.None, null);
        //if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        // ShuffleCardsIntoLocation doesn't seem to work gracefully.
        //IEnumerable<Card> riverCards = this.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
        //// Find all river cards and shuffle them into the river deck.
        //coroutine = this.GameController.ShuffleCardsIntoLocation(this, riverCards, this.TurnTaker.FindSubDeck("RiverDeck"));

        // Old logic for river V2
        //// At the start of game, move all River cards into River deck. (This best preserves the 'identity' of the cards.)
        //// It's messy, but search through all river cards from Cascade's deck and always move them to the river deck.
        //int handSize = this.NumberOfCardsInHand;
        //IEnumerable<Card> riverCards = this.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
        //coroutine = this.GameController.MoveCards(this, riverCards, riverDeck);
        //if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        //// Then shuffle.
        //coroutine = this.GameController.ShuffleLocation(riverDeck);
        //if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        //// Then, move the top four to the riverbank. That should already exist due to being a non-real card.
        //coroutine = this.GameController.MoveCards(this, riverDeck.GetTopCards(4), this.TurnTaker.FindCard("Riverbank", false).UnderLocation );
        //if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        //// Then move enough replacements to get starting cards to hand. Don't draw to avoid animation. This is the start of game, so it just loops. It's messy, but it works for now!
        //// TODO: Seriously, is there a better option?
        //while (this.HeroTurnTaker.NumberOfCardsInHand < handSize)
        //{
        //    coroutine = this.GameController.MoveCard(this, this.HeroTurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
        //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}
    }
}