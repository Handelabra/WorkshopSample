using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class LookADistractionCardController : InquirerDistortionSharedCardController
    {
        public LookADistractionCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && !c.IsHero, "non-hero target", false, false, null, null, false);
        }

        public override IEnumerator Play()
        {
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage another target.
                IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, nextTo), 4, DamageType.Melee, 1, false, 1, false, false, false, (Card c) => !c.IsHero && c != nextTo, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Damage Inquirer.
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = base.DealDamage(nextTo, base.CharacterCard, 1, DamageType.Melee);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}