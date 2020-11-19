using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Inquirer
{
    // TODO: TEST!
    public class UntilYouMakeItCardController : CardController
    {
        public UntilYouMakeItCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Search for form.
            coroutine = this.GameController.SelectCardFromLocationAndMoveIt(this.HeroTurnTakerController, this.TurnTaker.Deck, new LinqCardCriteria((Card c) => c.IsForm, () => "form", true, false, null, null, false), new MoveCardDestination[]
            {
                new MoveCardDestination(this.TurnTaker.PlayArea, false, false, false)
            }, true, true, true, false, null, false, false, null, false, false, null, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw card.
            coroutine = this.DrawCard(null, true, null, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Play card.
            coroutine = this.SelectAndPlayCardFromHand(this.HeroTurnTakerController, true, null, null, false, false, true, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}