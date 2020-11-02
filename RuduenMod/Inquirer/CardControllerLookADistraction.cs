using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerLookADistraction : CardControllerInquirerDistortionShared
    {
        public CardControllerLookADistraction(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.NextToCriteria = new LinqCardCriteria((Card c) => c.IsTarget && !card.IsHero, "non-hero targets", false, false, null, null, false);
        }

        protected override IEnumerator ActivateNextToEffect(Card nextTo)
        {
            // Damage other targets.
            IEnumerator coroutine = base.DealDamage(nextTo, (Card c) => !c.IsHero && c != nextTo, 2, DamageType.Melee);
            yield return base.RunCoroutine(coroutine);
        }

        protected override IEnumerator OnDestroyResponse(DestroyCardAction dc)
        {
            // Damage Inquirer.
            Card nextTo = base.GetCardThisCardIsNextTo(true);
            if (nextTo != null && nextTo.IsInPlayAndHasGameText)
            {
                IEnumerator coroutine = base.DealDamage(nextTo, base.CharacterCard, 1, DamageType.Melee);
                yield return base.RunCoroutine(coroutine);
            }
        }
    }
}
