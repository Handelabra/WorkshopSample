using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.BreachMage
{
    // Manually tested!
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
                // Selection: Draw or use a cast and destroy. 

                List<Function> list = new List<Function>
                {
                    new Function(this.HeroTurnTakerController, "Draw a card", SelectionType.DrawCard, () => this.DrawCard(this.HeroTurnTaker), this.CanDrawCards(this.HeroTurnTakerController), this.TurnTaker.Name + " cannot activate any Cast effects, so they must draw a card.", null),
                    new Function(this.HeroTurnTakerController, "Activate a card's Cast effect and destroy that card", SelectionType.ActivateAbility, () => this.CastAndDestroySpell(this.HeroTurnTakerController), this.GameController.GetActivatableAbilitiesInPlay(this.HeroTurnTakerController, "cast", false).Count() > 0, this.TurnTaker.Name + " cannot draw any cards, so they must activate a card's Cast effect and destroy that card.", null)
                };
                SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.HeroTurnTakerController, list, false, null, this.TurnTaker.Name + " cannot draw any cards nor activate any Cast effects, so" + this.Card.Title + " has no effect.", null, this.GetCardSource(null));
                coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        public IEnumerator CastAndDestroySpell(HeroTurnTakerController heroTurnTakerController)
        {
            IEnumerator coroutine;
            // Stanard power.
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Use a Cast.
            //coroutine = this.GameController.SelectAndActivateAbility(this.HeroTurnTakerController, "cast", null, storedResults);
            coroutine = this.GameController.SelectAndActivateAbility(heroTurnTakerController, "cast", null, storedResults, false, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Destroy the cast card.
                coroutine = this.GameController.DestroyCard(this.HeroTurnTakerController, storedResults.FirstOrDefault().SelectedCard);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}