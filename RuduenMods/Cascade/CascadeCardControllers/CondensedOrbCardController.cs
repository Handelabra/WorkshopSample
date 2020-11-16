using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Cascade
{
    // TODO: TEST!
    public class CondensedOrbCardController : CascadeRiverSharedCardController
    {
        public CondensedOrbCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine = this.GameController.SelectAndGainHP(this.HeroTurnTakerController, 3, false, null, 2);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}