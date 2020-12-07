using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.TheNaturalist
{
    public class TheNaturalistVolatileFormCharacterCardController : PromoDefaultCharacterCardController
    {
        public TheNaturalistVolatileFormCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
			// Based on the core display logic. 
			Func<string> specialString = delegate ()
			{
				List<string> list = new List<string>();
				if (base.TurnTaker.IsHero)
				{
					if (base.CanActivateEffect(base.TurnTakerController, "{gazelle}"))
					{
						list.Add("{gazelle}");
					}
					if (base.CanActivateEffect(base.TurnTakerController, "{rhinoceros}"))
					{
						list.Add("{rhinoceros}");
					}
					if (base.CanActivateEffect(base.TurnTakerController, "{crocodile}"))
					{
						list.Add("{crocodile}");
					}
				}
				else
				{
					if (base.CanActivateEffect(base.Card, "{gazelle}"))
					{
						list.Add("{gazelle}");
					}
					if (base.CanActivateEffect(base.Card, "{rhinoceros}"))
					{
						list.Add("{rhinoceros}");
					}
					if (base.CanActivateEffect(base.Card, "{crocodile}"))
					{
						list.Add("{crocodile}");
					}
				}
				if (list.Count<string>() > 0)
				{
					return string.Concat(new string[]
					{
						base.TurnTaker.Name,
						"'s current ",
						list.Count<string>().ToString_SingularOrPlural("form", "forms"),
						": ",
						list.ToCommaList(false, false, null, null)
					});
				}
				return base.TurnTaker.Name + " does not have any form cards in play.";
			};
			this.SpecialStringMaker.ShowSpecialString(specialString, null, null);
		}

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            // Play a card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Successful play means performing check logic for each of the three symbols on text. 
            if (storedResults.Count > 0 && storedResults.FirstOrDefault().IsSuccessful)
            {
                Card card = storedResults.FirstOrDefault().CardToPlay;
                if (card != null)
                {
                    foreach (string icon in new string[] { "{gazelle}", "{rhinoceros}", "{crocodile}" }.Where((string i) => this.CardHasIconText(card, i)))
                    {
						// If we reach this, we've searched and confirm we have the icon, so run the add icon status! 
						ActivateEffectStatusEffect activateEffectStatusEffect = new ActivateEffectStatusEffect(this.HeroTurnTaker, card, icon);
						activateEffectStatusEffect.UntilEndOfNextTurn(base.TurnTaker);
						coroutine = this.AddStatusEffect(activateEffectStatusEffect, true);
						if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                }
            }

            // Draw a card.
            coroutine = this.DrawCards(this.DecisionMaker, 1);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        private bool CardHasIconText(Card card, string icon)
        {
			if ((card.Definition.Body.Where((string bodyText) => bodyText.Contains(icon)).Count() > 0) ||
				(card.Definition.Powers.Where((string powerText) => powerText.Contains(icon)).Count() > 0))
			{
				return true;
			}
            return false;
        }
    }
}