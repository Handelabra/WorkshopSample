using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    public class CycleOfMagicCardController : CardController
    {
        public CycleOfMagicCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Draw 2 cards.
            coroutine = this.DrawCards(this.DecisionMaker, 2);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Use a Cast.
            coroutine = this.GameController.SelectAndActivateAbility(this.DecisionMaker, "cast", null, storedResults, false, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Move the top card back to top of deck.
                coroutine = this.GameController.MoveCard(this.DecisionMaker, storedResults.FirstOrDefault().SelectedCard, this.HeroTurnTaker.Deck);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}