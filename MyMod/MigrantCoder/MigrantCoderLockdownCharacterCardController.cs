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

            return AddStatusEffect(effect);
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator e = null;

            switch (index)
            {
                case 0:
                    // One player may play a card now.
                    e = SelectHeroToPlayCard(this.DecisionMaker);
                    break;
                case 1:
                    // One hero may use a power now.
                    e = this.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: GetCardSource());
                    break;
                case 2:
                    // One player may draw a card now
                    e = this.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: GetCardSource());
                    break;
            }

            return e;
        }
    }
}
