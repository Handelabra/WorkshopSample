using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenWorkshop.CaptainCosmic
{
    public class CaptainCosmicCosmicShieldingCharacterCardController : PromoDefaultCharacterCardController
    {
        public CaptainCosmicCosmicShieldingCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);
            List<SelectCardDecision> storedResults = new List<SelectCardDecision>();

            IEnumerator coroutine;

            // Select a construct target. 
            coroutine = this.GameController.SelectCardAndStoreResults(this.DecisionMaker, SelectionType.SelectTargetFriendly,
                new LinqCardCriteria((Card c) => c.IsInPlayAndHasGameText && c.IsConstruct && c.IsTarget, "construct target"),
                storedResults, false, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            Card selectedCard = this.GetSelectedCard(storedResults);
            if (selectedCard != null)
            {
                // Reduce all damage to the construct by 1. 
                ReduceDamageStatusEffect redu = new ReduceDamageStatusEffect(powerNumeral);
                redu.UntilStartOfNextTurn(this.HeroTurnTaker);
                redu.TargetCriteria.IsSpecificCard = selectedCard;

                coroutine = this.GameController.AddStatusEffect(redu, true, this.GetCardSource());
                if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // Redirect all hero damage to the construct.
                RedirectDamageStatusEffect redir = new RedirectDamageStatusEffect();
                redir.TargetCriteria.IsHero = true;
                redir.RedirectTarget = selectedCard;
                redir.CardDestroyedExpiryCriteria.Card = selectedCard;
                redir.UntilCardLeavesPlay(selectedCard);
                redir.UntilTargetLeavesPlay(selectedCard);
                redir.TargetRemovedExpiryCriteria.Card = selectedCard;
                redir.UntilStartOfNextTurn(this.HeroTurnTaker);

                coroutine = this.GameController.AddStatusEffect(redir, true, this.GetCardSource());
                if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
            else
            {
                // Draw a card.
                coroutine = this.DrawCards(this.DecisionMaker, 1);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }
    }
}