using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.MigrantCoder
{
    public class CodersKeyboardCardController : CardController
    {
        public CodersKeyboardCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
        }

        public override void AddTriggers()
        {
            // You may use an additional power during your power phase.
            AddAdditionalPhaseActionTrigger(tt => ShouldIncreasePhaseActionCount(tt), Phase.UsePower, 1);
        }

        public override IEnumerator Play()
        {
            // You may use an additional power during your power phase.
            return IncreasePhaseActionCountIfInPhase(tt => tt == this.TurnTaker, Phase.UsePower, 1);
        }

        public override bool DoesHaveActivePlayMethod
        {
            get
            {
                return false;
            }
        }

        private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
        {
            return tt == this.TurnTaker;
        }

        public override bool AskIfIncreasingCurrentPhaseActionCount()
        {
            return this.GameController.ActiveTurnPhase.IsUsePower
                   && ShouldIncreasePhaseActionCount(this.GameController.ActiveTurnTaker);
        }
    }
}
