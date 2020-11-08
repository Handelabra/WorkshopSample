using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class IveFixedTheWoundCardController : InquirerDistortionSharedCardController
    {
        public IveFixedTheWoundCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && c.IsHero, "hero targets", false, false, null, null, false);
        }

        public override IEnumerator Play()
        {
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Heal.
                IEnumerator coroutine = this.GameController.GainHP(nextTo, 5);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Damage target.
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = this.DealDamage(nextTo, nextTo, 2, DamageType.Psychic, false, false, false, null, null, null, false, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}