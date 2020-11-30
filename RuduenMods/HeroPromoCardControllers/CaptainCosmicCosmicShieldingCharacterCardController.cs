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
                // Reduce damage to fixed amount.
                OnDealDamageStatusEffect onDealDamageStatusEffect = new OnDealDamageStatusEffect(this.CardWithoutReplacements, "ReduceToDamageResponse", "Whenever " + selectedCard.AlternateTitleOrTitle + " would be dealt damage, reduce that damage to " + powerNumeral + ".", new TriggerType[] { TriggerType.DealDamage }, this.HeroTurnTaker, this.Card, new int[] { powerNumeral });
                onDealDamageStatusEffect.TargetCriteria.IsSpecificCard = selectedCard;
                onDealDamageStatusEffect.CanEffectStack = false;
                onDealDamageStatusEffect.BeforeOrAfter = BeforeOrAfter.Before;
                onDealDamageStatusEffect.CardDestroyedExpiryCriteria.Card = selectedCard;
                onDealDamageStatusEffect.UntilCardLeavesPlay(selectedCard);
                onDealDamageStatusEffect.UntilTargetLeavesPlay(selectedCard);
                onDealDamageStatusEffect.TargetRemovedExpiryCriteria.Card = selectedCard;
                onDealDamageStatusEffect.UntilStartOfNextTurn(this.HeroTurnTaker);
                onDealDamageStatusEffect.UntilStartOfNextTurn(this.HeroTurnTaker);

                coroutine = this.AddStatusEffect(onDealDamageStatusEffect);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                // Redirect all damage deal to what's next to the construct to the construct.
                // Figure out the card it's next to. The GetCardThisCardIsNextTo function is protected, so we have to manually check. 
                // TODO: Rework into a different onDealDamageStatusEffect if necessary, since it can be done to future-proof for a theoretical effect which could move constructs to a different target.
                if (selectedCard.Location.IsNextToCard)
                {
                    RedirectDamageStatusEffect redir = new RedirectDamageStatusEffect();
                    redir.TargetCriteria.IsSpecificCard = selectedCard.Location.OwnerCard;
                    redir.RedirectTarget = selectedCard;
                    redir.CardDestroyedExpiryCriteria.Card = selectedCard;
                    redir.UntilCardLeavesPlay(selectedCard);
                    redir.UntilTargetLeavesPlay(selectedCard);
                    redir.TargetRemovedExpiryCriteria.Card = selectedCard;
                    redir.UntilStartOfNextTurn(this.HeroTurnTaker);

                    coroutine = this.GameController.AddStatusEffect(redir, true, this.GetCardSource());
                    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
            else
            {
                // Draw a card.
                coroutine = this.DrawCards(this.DecisionMaker, 1);
                if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }


#pragma warning disable IDE0060 // Remove unused parameter
        public IEnumerator ReduceToDamageResponse(DealDamageAction dd, TurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            IEnumerator coroutine;
            int? powerNumeral = null;
            if (powerNumerals != null)
            {
                powerNumeral = powerNumerals.ElementAtOrDefault(0);
            }
            if (powerNumeral == null)
            {
                powerNumeral = 1;
            }

            coroutine = this.GameController.ReduceDamage(dd, dd.Amount - (int)powerNumeral, null, this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}