using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
namespace Workshopping.MigrantCoder
{
    public class BugFixCardController: CardController
    {
        public BugFixCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Migrant Coder deals 1 target 4 lightning damage.
            var numberOfTargets = GetPowerNumeral(0, 1);
            var damageAmount = GetPowerNumeral(1, 4);
            return this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker,
                                                                                     new DamageSource(this.GameController, this.CharacterCard),
                                                                                     damageAmount,
                                                                                     DamageType.Lightning,
                                                                                     numberOfTargets,
                                                                                     false,
                                                                                     numberOfTargets,
                                                                                     cardSource: GetCardSource());
        }
    }
}
