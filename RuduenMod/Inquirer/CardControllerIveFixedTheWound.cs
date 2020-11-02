using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerIveFixedTheWound : CardControllerInquirerDistortionShared
    {
        public CardControllerIveFixedTheWound(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && card.IsHero, "hero targets", false, false, null, null, false);
        }

        protected override IEnumerator ActivateNextToEffect(Card nextTo)
        {
            // Heal. 
            IEnumerator coroutine = base.GameController.GainHP(nextTo, 5);
            yield return base.RunCoroutine(coroutine);
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Damage target.
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = base.DealDamage(nextTo, nextTo, 2, DamageType.Psychic, true, false, false, null, null, null, false, null);
                yield return base.RunCoroutine(coroutine);
            }
        }
    }
}
