using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    public class BreachMageChargeCasterCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public BreachMageChargeCasterCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            // Break down into two powers.
            IEnumerator coroutine;
            if (index == 1)
            {
                List<int> powerNumerals = new List<int>
                {
                    this.GetPowerNumeral(0, 2),
                    this.GetPowerNumeral(1, 5)
                };

                List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();
                // Charge ability attempt.
                // Destroy two of your charges.
                coroutine = this.GameController.SelectAndDestroyCards(this.HeroTurnTakerController,
                    new LinqCardCriteria((Card c) => c.IsInPlay && c.Owner == this.HeroTurnTaker && c.DoKeywordsContain("charge"), "charge", true, false, null, null, false),
                    powerNumerals[0], false, null, null, storedResultsAction, null, false, null, null, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.GetNumberOfCardsDestroyed(storedResultsAction) == powerNumerals[0])
                {
                    // If two were destroyed, someone draws 5.
                    coroutine = this.GameController.SelectHeroToDrawCards(this.HeroTurnTakerController, powerNumerals[1], false, false, null, false, null, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "active heroes"), null, null, this.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                // Stanard power.
                List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

                // Bounce an equipment.
                coroutine = this.GameController.SelectAndMoveCard(this.HeroTurnTakerController, (Card c) => c.IsInPlay && this.IsEquipment(c) && c.Owner == this.TurnTaker, this.HeroTurnTaker.Hand);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // Play an equipment.
                coroutine = this.SelectAndPlayCardsFromHand(this.HeroTurnTakerController, 1, false, 0, new LinqCardCriteria((Card c) => this.IsEquipment(c), "equipment", true));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            switch (index)
            {
                case 0:
                    {
                        var message = this.GameController.SendMessageAction("This is the first thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 1:
                    {
                        var message = this.GameController.SendMessageAction("This is the second thing that does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
                case 2:
                    {
                        var message = this.GameController.SendMessageAction("Tricked you! Also does nothing.", Priority.Medium, GetCardSource());
                        if (UseUnityCoroutines)
                        {
                            yield return this.GameController.StartCoroutine(message);
                        }
                        else
                        {
                            this.GameController.ExhaustCoroutine(message);
                        }
                        break;
                    }
            }
        }
    }
}