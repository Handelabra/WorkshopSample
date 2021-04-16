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
            var damage = DealDamage(this.CharacterCard, c => c.IsNonVillainTarget, 5, DamageType.Psychic);
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(damage);
            }
            else
            {
                this.GameController.ExhaustCoroutine(damage);
            }
        }
    }
}
