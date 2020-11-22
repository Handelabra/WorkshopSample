using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.HeroPromos
{
    public class HeroPromosCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public HeroPromosCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine = DrawCards(this.DecisionMaker, 3);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        // Keep cards out when incapacitation.
        protected override IEnumerator RemoveCardsFromGame(IEnumerable<Card> cards)
        {
            Log.Debug("None of " + this.TurnTakerControllerWithoutReplacements.Name + "'s cards are removed from the game.");
            IEnumerator coroutine = this.GameController.SendMessageAction(this.Card.Title + "does not remove any cards from the game.", Priority.Medium, this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        //// No way to interact with stolen cards, so don't worry about it.
        //public override bool KeepUnderCardOnIncapacitation(Card card)
        //{
        //    bool flag = card.Location.OwnerCard == base.Card;
        //    if (!flag)
        //    {
        //        flag = base.KeepUnderCardOnIncapacitation(card);
        //    }
        //    return flag;
        //}

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            // TODO: Implement Incapacitated Abilities.
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        // Increase damage dealt by hero targets until the start of the next round. 
                        IncreaseDamageStatusEffect increaseDamageStatusEffect = new IncreaseDamageStatusEffect(1);
                        increaseDamageStatusEffect.SourceCriteria.IsHero = new bool?(true);
                        increaseDamageStatusEffect.SourceCriteria.IsTarget = new bool?(true);
                        increaseDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
                        coroutine = this.AddStatusEffect(increaseDamageStatusEffect, true);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        // Decrease damage dealt to hero targets until the start of the next round. 
                        ReduceDamageStatusEffect reduceDamageStatusEffect = new ReduceDamageStatusEffect(1);
                        reduceDamageStatusEffect.TargetCriteria.IsHero = new bool?(true);
                        reduceDamageStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
                        coroutine = this.AddStatusEffect(reduceDamageStatusEffect, true);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}