using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.BreachMage
{
    public abstract class BreachMageSpellSharedCardController : CardController
    {
        public BreachMageSpellSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator ActivateAbility(string abilityKey)
        {
            IEnumerator coroutine;
            if (abilityKey == "cast")
            {
                coroutine = this.ActivateCast();
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        protected virtual IEnumerator ActivateCast()
        {
            IEnumerator coroutine = this.GameController.SendMessageAction("This spell isn't implemented properly yet. Blame Ruduen.", Priority.Medium, GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}