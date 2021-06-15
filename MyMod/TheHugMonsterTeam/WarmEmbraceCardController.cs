using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.TheHugMonsterTeam
{
    public class WarmEmbraceCardController : CardController
    {
        public WarmEmbraceCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        { }

        public override void AddTriggers()
        {
            // At the end of each turn...
            AddEndOfTurnTrigger(tt => true, EndOfTurnResponse, TriggerType.DealDamage);
        }

        private IEnumerator EndOfTurnResponse(PhaseChangeAction phaseChangeAction)
        {
            // ... {TheHugMonster} deals each hero target 1 Melee damage and 1 Fire damage.
            var info = new List<DealDamageAction>();
            info.Add(new DealDamageAction(GetCardSource(), new DamageSource(this.GameController, this.CharacterCard), null, 1, DamageType.Melee, false));
            info.Add(new DealDamageAction(GetCardSource(), new DamageSource(this.GameController, this.CharacterCard), null, 1, DamageType.Fire, false));

            return DealMultipleInstancesOfDamage(info, c => c.IsHero);
        }
    }
}
