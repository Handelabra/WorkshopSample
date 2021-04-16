using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
namespace Workshopping.TheBaddies
{
    public class SmashBackFieldCardController : CardController
    {
        public SmashBackFieldCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override void AddTriggers()
        {
            // The first time {TheBaddies} is dealt damage by a target each turn, {TheBaddies} deals that target 2 melee damage.
            AddCounterDamageTrigger(dd => dd.Target == this.CharacterCard && dd.DidDealDamage, () => this.CharacterCard, () => this.CharacterCard, true, 2, DamageType.Melee);
        }
    }
}
