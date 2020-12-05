using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;

namespace RuduenWorkshop.AkashThriya
{
    public class AkashThriyaSeedRotationCharacterCardController : PromoDefaultCharacterCardController
    {
        public AkashThriyaSeedRotationCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;

            // Select and move one of your seeds to the bottom of your deck.
            coroutine = this.GameController.SelectAndMoveCard(this.DecisionMaker, (Card c) => c.IsPrimordialSeed && c.Location == this.HeroTurnTaker.Trash, this.HeroTurnTaker.Deck, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Reveal seeds until one is played.
            coroutine = this.RevealCards_MoveMatching_ReturnNonMatchingCards(this.HeroTurnTakerController, this.HeroTurnTaker.Deck, true, false, false, new LinqCardCriteria((Card c) => c.IsPrimordialSeed, "primordial seed"), 1, showMessage: true);

            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}