using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Bunker
{
    public class BunkerFullSalvoCharacterCardController : PromoDefaultCharacterCardController
    {
        public BunkerFullSalvoCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 2);

            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Draw 2 cards.
            coroutine = this.DrawCards(this.HeroTurnTakerController, powerNumeral);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Discard 1 for each of your other powers.
            coroutine = SelectAndDiscardCards(this.HeroTurnTakerController, GetNumberOfOtherCardsWithPowersInPlay());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (this.GetNumberOfOtherCardsWithPowersInPlay() == 0)
            {
                coroutine = base.GameController.SendMessageAction("There are no other cards with powers in play, so " + this.TurnTaker.Name + " may not be able to use any more powers this turn.", Priority.High, base.GetCardSource(null), null, false);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            CardCriteria cardCriteria = new CardCriteria(null)
            {
                IsOneOfTheseCards = GetCardsWithPowersInPlay()
            };
            coroutine = base.SetPhaseActionCountThisTurn(this.TurnTaker, Phase.UsePower, cardCriteria);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        private List<Card> GetCardsWithPowersInPlay()
        {
            return this.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.HasPowers && c.Owner == this.TurnTaker).ToList();
        }

        private int GetNumberOfOtherCardsWithPowersInPlay()
        {
            return this.FindCardsWhere((Card c) => c.IsInPlayAndHasGameText && c.HasPowers && c.Owner == this.TurnTaker & c != this.Card).Count<Card>();
        }
    }
}