using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.Inquirer
{
    public class InquirerHardFactsCharacterCardController : HeroCharacterCardController
    {
        private List<Card> actedDistortions;

        public InquirerHardFactsCharacterCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            this.actedDistortions = new List<Card>();
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<int> numerals = new List<int>(){
                            this.GetPowerNumeral(0, 3),  // Max HP
                            this.GetPowerNumeral(1, 1),  // Number of Targets
                            this.GetPowerNumeral(2, 1)   // Damage.
            };

            // You may play an ongoing.
            IEnumerator coroutine;
            coroutine = this.SelectAndPlayCardsFromHand(this.HeroTurnTakerController, 1, false, 0, new LinqCardCriteria((Card c) => c.IsOngoing, "ongoing", true));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Make distortions HP targets. (Note that this does not apply to pre-existing targets - this is a quirk of how the engine currently applies those effects.)
            MakeTargetStatusEffect makeTargetStatusEffect = new MakeTargetStatusEffect(numerals[0], false);
            makeTargetStatusEffect.CardsToMakeTargets.HasAnyOfTheseKeywords = new List<string>() { "distortion" };
            makeTargetStatusEffect.UntilStartOfNextTurn(this.TurnTaker);
            coroutine = this.AddStatusEffect(makeTargetStatusEffect, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            int powerNumeral2 = base.GetPowerNumeral(1, 4);
            int damageAmount = base.GetPowerNumeral(2, 2);
            SelectCardsDecision selectCardsDecision = new SelectCardsDecision(base.GameController, this.DecisionMaker, (Card c) => c.IsInPlay && c.IsDistortion, SelectionType.CardToDealDamage, null, false, null, true, true, false, new Func<int>(this.NumDistortionsToDamage), null, null, null, base.GetCardSource(null));
            IEnumerator coroutine2 = base.GameController.SelectCardsAndDoAction(selectCardsDecision, (SelectCardDecision sc) => this.DistortionDamageResponse(sc, numerals[1], numerals[2]), null, null, base.GetCardSource(null), null, false, null);
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine2);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine2);
            }
            this.actedDistortions.Clear();
        }

        private int NumDistortionsToDamage()
        {
            if (!base.Card.IsIncapacitatedOrOutOfGame)
            {
                int num = base.FindCardsWhere((Card c) => c.IsDistortion && c.IsInPlay, false, null, false).Except(this.actedDistortions).Count<Card>();
                return this.actedDistortions.Count<Card>() + num;
            }
            return 0;
        }

        private IEnumerator DistortionDamageResponse(SelectCardDecision sc, int numberOfTargets, int damageAmount)
        {
            Card selectedCard = sc.SelectedCard;
            this.actedDistortions.Add(selectedCard);
            IEnumerator coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, selectedCard), (Card c) => new int?(damageAmount), DamageType.Psychic, () => numberOfTargets, false, new int?(numberOfTargets));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        yield break;
                    }
                case 1:
                    {
                        yield break;
                    }
                case 2:
                    {
                        yield break;
                    }
            }
            yield break;
        }
    }
}