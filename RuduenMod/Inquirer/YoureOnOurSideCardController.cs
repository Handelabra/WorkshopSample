using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class YoureOnOurSideCardController : InquirerDistortionSharedCardController
    {
        public YoureOnOurSideCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero, "non-hero targets", false, false, null, null, false);
        }

        public override IEnumerator Play()
        {
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage other targets.
                IEnumerator coroutine = this.DealDamage(nextTo, (Card c) => !c.IsHero && c != nextTo, 2, DamageType.Melee);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Damage Inquirer.
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = this.DealDamage(nextTo, this.CharacterCard, 1, DamageType.Melee);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}