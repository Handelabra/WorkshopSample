using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.DevStream
{
    public class SpamBotCardController: CardController
    {
        public SpamBotCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            SpecialStringMaker.ShowNonEnvironmentTargetWithHighestHP(2);
        }

        public override void AddTriggers()
        {
            //At the end of the environment turn, this card deals the non-environment target with the second highest HP {H-1} psychic damage.
            AddEndOfTurnTrigger(tt => tt == this.TurnTaker, p => DealDamageToHighestHP(this.Card, 2, c => c.IsNonEnvironmentTarget, c => H-1, DamageType.Psychic), TriggerType.DealDamage);

        }
    }
}
