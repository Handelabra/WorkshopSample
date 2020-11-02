using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.Inquirer
{
    // TODO: TEST!
    public class CardControllerFisticuffs : CardControllerInquirerDistortionShared
    {
        public CardControllerFisticuffs(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Damage.
            coroutine = base.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(base.GameController, base.CharacterCard), 3, DamageType.Melee, new int?(1), false, new int?(1));
            yield return base.RunCoroutine(coroutine);

            // Heal.
             coroutine = base.GameController.GainHP(this.CharacterCard, 3);
            yield return base.RunCoroutine(coroutine);

            // Discard card.
            coroutine = base.GameController.SelectAndDiscardCard(base.HeroTurnTakerController, true, null, null, SelectionType.DiscardCard);
            yield return base.RunCoroutine(coroutine);
        }
    }
}
