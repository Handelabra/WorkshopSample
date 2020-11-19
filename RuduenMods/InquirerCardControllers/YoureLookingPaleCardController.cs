using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Inquirer
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
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage.
                IEnumerator coroutine = this.DealDamage(nextTo, nextTo, 4, DamageType.Psychic, true, false, false, null, null, null, false, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Heal.
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = this.GameController.GainHP(nextTo, 2);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}