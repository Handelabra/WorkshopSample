using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace Workshopping.BreachMage
{
    public class BreachMageSpellSharedCardController : CardController
    {
        public BreachMageSpellSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator ActivateAbility(string abilityKey)
        {
            IEnumerator coroutine = null;
            if (abilityKey == "cast")
            {
                coroutine = this.ActivateCast();
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected virtual IEnumerator ActivateCast()
        {
            IEnumerator coroutine = this.GameController.SendMessageAction("This spell isn't implemented properly yet. Blame Ruduen.", Priority.Medium, GetCardSource());
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}