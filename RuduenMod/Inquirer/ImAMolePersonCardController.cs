using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.Inquirer
{
    public class ImAMolePersonCardController : InquirerFormSharedCardController
    {
        public ImAMolePersonCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            base.AddTriggers();
            // Add trigger for increasing healing.
            base.AddTrigger<GainHPAction>((GainHPAction g) => g.HpGainer == base.CharacterCard,
                (GainHPAction g) => base.GameController.IncreaseHPGain(g, 1, base.GetCardSource(null)),
                new TriggerType[] { TriggerType.IncreaseHPGain, TriggerType.ModifyHPGain },
                TriggerTiming.Before, null, false, true, null, false, null, null, false, false);

            // Add trigger for end of turn damage.
            base.AddDealDamageAtEndOfTurnTrigger(base.TurnTaker, base.CharacterCard, (Card c) => true, TargetType.SelectTarget, 1, DamageType.Melee);
        }
    }
}