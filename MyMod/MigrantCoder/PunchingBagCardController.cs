using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.MigrantCoder
{
    public class PunchingBagCardController : CardController
    {
        public PunchingBagCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            var damage = DealDamage(this.CharacterCard, this.CharacterCard, 1, DamageType.Melee);
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
