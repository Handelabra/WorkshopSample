using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class ShineCardController : BreachMageSpellSharedCardController
    {
        public ShineCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator ActivateCast()
        {
            IEnumerator coroutine;
            // Damage.
            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 4, DamageType.Radiant, new int?(1), false, new int?(1), false, false, false, null, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

            // Players each draw a card.
            coroutine = base.EachPlayerDrawsACard((HeroTurnTaker tt) => true, false, true, null, true, false);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}
