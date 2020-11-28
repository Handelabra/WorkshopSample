using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.TheArgentAdept
{
    public class TheArgentAdeptAriaCharacterCardController : PromoDefaultCharacterCardController
    {
        public TheArgentAdeptAriaCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {


            List<PlayCardAction> storedResults = new List<PlayCardAction>();

            IEnumerator coroutine;

            // Play card.
            coroutine = this.GameController.SelectAndPlayCardFromHand(this.HeroTurnTakerController, false, storedResults: storedResults, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            if (storedResults.Count > 0 && storedResults.FirstOrDefault().IsSuccessful)
            {
                Card card = storedResults.FirstOrDefault().CardToPlay;

                if (card.HasActivatableAbility("perform"))
                {
                    if (card.GetNumberOfActivatableAbilities("perform") == 1)
                    {
                        IEnumerable<ActivatableAbility> aa = this.FindCardController(card).GetActivatableAbilities("perform");
                        coroutine = this.GameController.ActivateAbility(aa.FirstOrDefault(), this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                    else
                    {
                        // Use very specific check on card to future-proof for multiple options. 
                        coroutine = this.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, "perform", new LinqCardCriteria((Card c) => c == card), null, false, this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                }
                else
                {
                    coroutine = this.GameController.SendMessageAction("The played card does not have perform text to activate.", Priority.Medium, this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }

                if (card.HasActivatableAbility("accompany"))
                {
                    if (card.GetNumberOfActivatableAbilities("accompany") == 1)
                    {
                        IEnumerable<ActivatableAbility> aa = this.FindCardController(card).GetActivatableAbilities("accompany");
                        coroutine = this.GameController.ActivateAbility(aa.FirstOrDefault(), this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                    else
                    {
                        // Use very specific check on card to future-proof for multiple options. 
                        coroutine = this.GameController.SelectAndActivateAbility(base.HeroTurnTakerController, "accompany", new LinqCardCriteria((Card c) => c == card), null, false, this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                }
                else
                {
                    coroutine = this.GameController.SendMessageAction("The played card does not have accompany text to activate.", Priority.Medium, this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
        }
    }
}