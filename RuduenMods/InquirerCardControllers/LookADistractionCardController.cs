using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Inquirer
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
            Card nextTo = this.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage another target.
                IEnumerator coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, nextTo), 3, DamageType.Melee, 1, false, 1, false, false, false, (Card c) => !c.IsHero && c != nextTo, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
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