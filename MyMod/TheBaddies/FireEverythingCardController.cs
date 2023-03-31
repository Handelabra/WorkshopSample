using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.TheBaddies
{
    public class FireEverythingCardController : CardController
    {
        public FireEverythingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            return DealDamage(this.CharacterCard, c => c.IsTarget && !IsVillainTarget(c), 5, DamageType.Psychic);
        }
    }
}
