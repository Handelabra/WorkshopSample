using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.Wordsmith
{
    public class WaveCardController : WordsmithSharedEssenceCardController
    {
        public WaveCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator PerformModifiedAction()
        {
            // Deal Non-Heroes 2. 
            IEnumerator coroutine = this.DealDamage(this.CharacterCard, (Card card) => !card.IsHero, 2, DamageType.Sonic);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}