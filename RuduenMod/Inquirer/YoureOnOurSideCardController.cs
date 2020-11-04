using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Inquirer
{
    // TODO: TEST!
    public class YoureOnOurSideCardController : InquirerDistortionSharedCardController
    {
        public YoureOnOurSideCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && !card.IsHero, "non-hero targets", false, false, null, null, false);
        }

        public override IEnumerator Play()
        {
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null)
            {
                // Damage another target. 
                IEnumerator coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, nextTo), 3, DamageType.Psychic, new int?(1), false, new int?(1), true, false, false, (Card c) => !c.IsHero && c != nextTo);
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
