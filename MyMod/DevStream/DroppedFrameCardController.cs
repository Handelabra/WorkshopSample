using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.DevStream
{
    public class DroppedFrameCardController: CardController
    {
        public DroppedFrameCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            // When this card enters play, deal each non-chat target 1 Lightning damage.
            var damage = DealDamage(this.Card, c => !c.DoKeywordsContain("chat"), 1, DamageType.Lightning);
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(damage);
            }
            else
            {
                this.GameController.ExhaustCoroutine(damage);
            }
        }

        public override void AddTriggers()
        {
            // At the start of the Environment turn...
            AddStartOfTurnTrigger(tt => tt == this.TurnTaker, StartOfTurnResponse, TriggerType.GainHP);
        }

        private IEnumerator StartOfTurnResponse(PhaseChangeAction phaseChangeAction)
        {
            // ... each target regains 1 HP.
            var e = this.GameController.GainHP(this.DecisionMaker, c => true, 1, cardSource:GetCardSource());
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
