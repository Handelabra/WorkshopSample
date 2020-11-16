using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Cascade
{
    public class RushingWatersCardController : CascadeRiverSharedCardController
    {
        public RushingWatersCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            // Play 2 Cards.
            IEnumerator coroutine = this.SelectAndPlayCardsFromHand(this.HeroTurnTakerController, 2, false, new int?(0), null, false, null, null);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}