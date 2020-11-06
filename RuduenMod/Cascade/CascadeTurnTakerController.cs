using System;
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
            // At the start of game, move all River cards into River deck. (This best preserves the 'identity' of the cards.)
            // It's messy, but search through all river cards from Cascade's deck and always move them to the river deck. 
            IEnumerable<Card> riverCards = base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
            coroutine = base.GameController.MoveCards(this, riverCards, this.TurnTaker.FindSubDeck("RiverDeck"));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            // Then shuffle. 
            coroutine = base.GameController.ShuffleLocation(this.TurnTaker.FindSubDeck("RiverDeck"));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            //// It's messy, but search through all river cards from Cascade's deck and always move them to the river deck. 
            //coroutine = base.GameController.MoveCards(this,
            //    new LinqCardCriteria((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker, "river", true, false, null, null, false),
            //    (Card c) => this.TurnTaker.FindSubDeck("RiverDeck"),
            //    false, SelectionType.MoveCard, null, false, null, true, false, false, null, null, false, MoveCardDisplay.None, null);
            //if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // ShuffleCardsIntoLocation doesn't seem to work gravefully. 
            //IEnumerable<Card> riverCards = base.GameController.FindCardsWhere((Card c) => c.DoKeywordsContain("river") && c.Owner == this.TurnTaker);
            //// Find all river cards and shuffle them into the river deck.
            //coroutine = base.GameController.ShuffleCardsIntoLocation(this, riverCards, this.TurnTaker.FindSubDeck("RiverDeck"));

            // Then move 4 cards to hand. Don't draw to avoid animation. This is the start of game, so it just loops. It's messy, but it works for now!
            // TODO: Seriously, is there a better option? 
            while (this.HeroTurnTaker.NumberOfCardsInHand < 4)
            {
                coroutine = this.GameController.MoveCard(this, this.HeroTurnTaker.Deck.TopCard, this.HeroTurnTaker.Hand);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}
