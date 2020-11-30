using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.OmnitronX
{
    public class OmnitronXElectroShieldedSystemsCharacterCardController : PromoDefaultCharacterCardController
    {
        public OmnitronXElectroShieldedSystemsCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);

            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

            IEnumerator coroutine;


            MakeIndestructibleStatusEffect makeIndestructibleStatusEffect = new MakeIndestructibleStatusEffect();
            makeIndestructibleStatusEffect.CardsToMakeIndestructible.HasAnyOfTheseKeywords = new List<string>() { "component" };
            makeIndestructibleStatusEffect.CardsToMakeIndestructible.OwnedBy = this.HeroTurnTaker;
            makeIndestructibleStatusEffect.UntilStartOfNextTurn(this.HeroTurnTaker);
            coroutine = this.AddStatusEffect(makeIndestructibleStatusEffect, true);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal damage to a target equal to component count.
            int amount = this.FindCardsWhere((Card c) => c.IsInPlay && c.IsComponent && c.Owner == this.HeroTurnTaker).Count();

            coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), amount, DamageType.Lightning, powerNumeral, false, powerNumeral, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Deal self damage.
            coroutine = this.GameController.DealDamageToTarget(new DamageSource(this.GameController, this.CharacterCard), this.CharacterCard, amount, DamageType.Lightning, cardSource: this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}