using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.MigrantCoder
{
    public class MigrantCoderLockdownCharacterCardController : HeroCharacterCardController
    {
        public MigrantCoderLockdownCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Reduce damage dealt to {MigrantCoder} by 1 until the start of your next turn.
            var damageReduceAmount = GetPowerNumeral(0, 1);
            var effect = new ReduceDamageStatusEffect(damageReduceAmount);
            effect.TargetCriteria.IsSpecificCard = this.CharacterCard;
            effect.TargetCriteria.OutputString = this.TurnTaker.Name;
            effect.UntilStartOfNextTurn(this.TurnTaker);

            var e = AddStatusEffect(effect);
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);
            }
        }
    }
}
