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

        public override IEnumerator UsePower(int index = 0)
        {
            // Break down into two powers.
            IEnumerator coroutine;
            if (index == 1)
            {
                List<int> powerNumerals = new List<int>
                {
                    this.GetPowerNumeral(0, 2),
                    this.GetPowerNumeral(1, 4)
                };

                List<DestroyCardAction> storedResultsAction = new List<DestroyCardAction>();
                // Charge ability attempt.
                // Destroy two of your charges.
                coroutine = this.GameController.SelectAndDestroyCards(this.DecisionMaker,
                    new LinqCardCriteria((Card c) => c.IsInPlay && c.Owner == this.HeroTurnTaker && c.DoKeywordsContain("charge"), "charge", true, false, null, null, false),
                    powerNumerals[0], false, null, null, storedResultsAction, null, false, null, null, null, this.GetCardSource(null));
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                if (this.GetNumberOfCardsDestroyed(storedResultsAction) == powerNumerals[0])
                {
                    // If two were destroyed, someone draws 5.
                    coroutine = this.GameController.SelectHeroToDrawCards(this.DecisionMaker, powerNumerals[1], false, false, null, false, null, new LinqTurnTakerCriteria((TurnTaker tt) => tt.IsHero && !tt.ToHero().IsIncapacitatedOrOutOfGame, "active heroes"), null, null, this.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                // Selection: Draw or use a cast and destroy.

                List<Function> list = new List<Function>
                {
                    new Function(this.DecisionMaker, "Draw a card", SelectionType.DrawCard, () => this.DrawCard(this.HeroTurnTaker), this.CanDrawCards(this.DecisionMaker), this.TurnTaker.Name + " cannot activate any Cast effects, so they must draw a card.", null),
                    new Function(this.DecisionMaker, "Activate a card's Cast effect and destroy that card", SelectionType.ActivateAbility, () => this.CastAndDestroySpell(this.DecisionMaker), this.GameController.GetActivatableAbilitiesInPlay(this.DecisionMaker, "cast", false).Count() > 0, this.TurnTaker.Name + " cannot draw any cards, so they must activate a card's Cast effect and destroy that card.", null)
                };
                SelectFunctionDecision selectFunction = new SelectFunctionDecision(this.GameController, this.DecisionMaker, list, false, null, this.TurnTaker.Name + " cannot draw any cards nor activate any Cast effects, so" + this.Card.AlternateTitleOrTitle + " has no effect.", null, this.GetCardSource(null));
                coroutine = this.GameController.SelectAndPerformFunction(selectFunction, null, null);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        // TODO: Replace with something more unique!
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        coroutine = this.SelectHeroToPlayCard(this.DecisionMaker);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        coroutine = base.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 2:
                    {
                        coroutine = base.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
            }
            yield break;
        }

        public IEnumerator CastAndDestroySpell(HeroTurnTakerController heroTurnTakerController)
        {
            IEnumerator coroutine;
            // Stanard power.
            List<ActivateAbilityDecision> storedResults = new List<ActivateAbilityDecision>();

            // Use a Cast.
            //coroutine = this.GameController.SelectAndActivateAbility(this.DecisionMaker, "cast", null, storedResults);
            coroutine = this.GameController.SelectAndActivateAbility(heroTurnTakerController, "cast", null, storedResults, false, this.GetCardSource(null));
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0)
            {
                // Destroy the cast card.
                coroutine = this.GameController.DestroyCard(this.DecisionMaker, storedResults.FirstOrDefault().SelectedCard);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}