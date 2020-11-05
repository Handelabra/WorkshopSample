using System;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.RuduenFanMods.BreachMage
{
    public class BreachMageCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public BreachMageCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
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

        public override IEnumerator UsePower(int index = 0)
        {
            // Break down into two powers. 
            IEnumerator coroutine;
            if (index == 1)
            {
                List<int> powerNumerals = new List<int>();
                powerNumerals.Add(base.GetPowerNumeral(0, 2));
                powerNumerals.Add(base.GetPowerNumeral(1, 5));

                List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();
                // Charge ability attempt. 
                // Destroy two of your charges. 
                coroutine = base.GameController.SelectAndDestroyCards(base.HeroTurnTakerController, new LinqCardCriteria((Card c) => c.IsInPlay && c.DoKeywordsContain("charge"), "charge"), powerNumerals[0], false, null, null, storedResultsAction);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

                if (base.GetNumberOfCardsDestroyed(storedResultsAction) == powerNumerals[0])
                {
                    // If two were destroyed, someone draws 5. 
                    coroutine = base.GameController.SelectHeroToDrawCards(base.HeroTurnTakerController, powerNumerals[1], false, false, null, false, null, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "active heroes"), null, null, base.GetCardSource(null));
                    if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                // Stanard power. 
                List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

                // Use a Cast. 
                coroutine = base.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, "cast", null, storedResults, false);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }

                if (storedResults.Count > 0)
                {
                    // Destroy the Cast card.
                    base.GameController.DestroyCard(base.HeroTurnTakerController, storedResults[0].SelectedCard);
                }

            }
        }
    }
}