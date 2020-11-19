using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    public class BreachMageTwincasterCharacterCardController : HeroCharacterCardController
    {
        public string str;

        public BreachMageTwincasterCharacterCardController(Card card, TurnTakerController turnTakerController)
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
                    this.GetPowerNumeral(0, 2) // Number of charges.
                };

                List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();

                // Destroy two of your charges.
                coroutine = this.GameController.SelectAndDestroyCards(this.HeroTurnTakerController,
                    new LinqCardCriteria((Card c) => c.IsInPlay && c.Owner == this.HeroTurnTaker && c.DoKeywordsContain("charge"), "charge", true, false, null, null, false),
                    powerNumerals[0], false, null, null, storedResultsAction, null, false, null, null, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.GetNumberOfCardsDestroyed(storedResultsAction) == powerNumerals[0])
                {
                    // If two were destroyed, select use a spell twice.
                    List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

                    // Use a Cast.
                    //coroutine = this.GameController.SelectAndActivateAbility(this.HeroTurnTakerController, "cast", null, storedResults);
                    coroutine = this.GameController.SelectAndActivateAbility(this.HeroTurnTakerController, "cast", null, storedResults, false, this.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    if (storedResults.Count > 0)
                    {
                        // Select an ability on that card. Just in case a card has multiple Cast effects, or is no longer valid due to being destroyed.
                        coroutine = this.GameController.SelectAndActivateAbility(this.HeroTurnTakerController, "cast", new LinqCardCriteria(storedResults.FirstOrDefault().SelectedCard), null, false, this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                        // Destroy the cast card.
                        coroutine = this.GameController.DestroyCard(this.HeroTurnTakerController, storedResults.FirstOrDefault().SelectedCard);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                }
            }
            else
            {
                List<int> powerNumerals = new List<int>
                {
                    this.GetPowerNumeral(0, 1), // Number of targets,
                    this.GetPowerNumeral(1, 2) // Amount of damage.
                };
                // Damage.
                coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), powerNumerals[1], DamageType.Fire, powerNumerals[0], false, powerNumerals[0], false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
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