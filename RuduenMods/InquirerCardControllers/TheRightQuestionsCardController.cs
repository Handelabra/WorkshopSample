using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.Inquirer
{
    // TODO: TEST!
    public class TheRightQuestionsCardController : CardController
    {
        public TheRightQuestionsCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;
            List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();

            // Destroy Ongoing/Environment.
            coroutine = this.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria((Card c) => c.IsOngoing || c.IsEnvironment, "ongoing or environment", true, false, null, null, false), false, storedResultsAction, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (this.GetNumberOfCardsDestroyed(storedResultsAction) > 0)
            {
                // Play a distortion.
                coroutine = this.SelectAndPlayCardsFromHand(this.DecisionMaker, 1, false, 0, new LinqCardCriteria((Card c) => c.IsDistortion, "distortion", true));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }

            // Bounce one of your distortions.
            coroutine = this.GameController.SelectAndMoveCard(this.DecisionMaker, (Card c) => c.IsDistortion && c.Owner == this.HeroTurnTaker && c.IsInPlay, this.HeroTurnTaker.Hand);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}