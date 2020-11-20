using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.BreachMage
{
    // Manually tested!
    public class FlareCascadeCardController : BreachMageSpellSharedCardController
    {
        public FlareCascadeCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        protected override IEnumerator ActivateCast()
        {
            IEnumerator coroutine;
            List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();
            // Damage.
            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 3, DamageType.Fire, 1, false, 1, false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // You may destroy 1-3 charges.
            coroutine = this.GameController.SelectAndDestroyCards(this.DecisionMaker,
                new LinqCardCriteria((Card c) => c.IsInPlay && c.Owner == this.HeroTurnTaker && c.DoKeywordsContain("charge"), "charge", true, false, null, null, false),
                3, false, 0, null, storedResultsAction, null, false, null, false, null, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            for (int i = 0; i < this.GetNumberOfCardsDestroyed(storedResultsAction); i++)
            {
                // Repeat damage for every charge destroyed.
                coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), 3, DamageType.Fire, new int?(1), false, new int?(1), false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}