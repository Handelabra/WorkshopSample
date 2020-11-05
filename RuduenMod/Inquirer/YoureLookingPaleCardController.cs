using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class YoureLookingPaleCardController : InquirerDistortionSharedCardController
    {
        public YoureLookingPaleCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero, "non-hero targets");
        }

        public override IEnumerator Play()
        {
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage.
                IEnumerator coroutine = base.DealDamage(nextTo, nextTo, 5, DamageType.Psychic, true, false, false, null, null, null, false, null);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Heal. 
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = base.GameController.GainHP(nextTo, 2);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}
