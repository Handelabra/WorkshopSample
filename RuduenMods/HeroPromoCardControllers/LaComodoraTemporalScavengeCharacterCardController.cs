using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using RuduenWorkshop.HeroPromos;
using System.Collections;
using System.Collections.Generic;

namespace RuduenWorkshop.LaComodora
{
    public class LaComodoraTemporalScavengeCharacterCardController : PromoDefaultCharacterCardController
    {
        public LaComodoraTemporalScavengeCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator UsePower(int index = 0)
        {
            string turnTakerName;
            IEnumerator coroutine;

            if (this.TurnTaker.IsHero) { turnTakerName = this.TurnTaker.Name; }
            else { turnTakerName = this.Card.Title; }

            // Flip a card face-up.
            coroutine = this.GameController.SelectAndFlipCards(this.DecisionMaker, new LinqCardCriteria((Card c) => c.Location == this.HeroTurnTaker.PlayArea && c.IsFaceDownNonCharacter && !c.IsMissionCard, "face-down cards in " + turnTakerName + "'s play area"), 1, false, false, 1, null, true, cardSource: this.GetCardSource());
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Set up an effect to respond when your equipment is destroyed
            WhenCardIsDestroyedStatusEffect whenCardIsDestroyedStatusEffect = new WhenCardIsDestroyedStatusEffect(this.CardWithoutReplacements, "WhenEquipIsDestroyedFlip", "Whenever a construct is destroyed, " + turnTakerName + " may shuffle it into their deck and either draw or play a card.", new TriggerType[]
            { TriggerType.FlipCard }, this.HeroTurnTaker, this.Card, null);
            whenCardIsDestroyedStatusEffect.CardDestroyedCriteria.HasAnyOfTheseKeywords = new List<string> { "equipment" };
            whenCardIsDestroyedStatusEffect.CardDestroyedCriteria.OwnedBy = this.HeroTurnTaker;
            whenCardIsDestroyedStatusEffect.UntilEndOfNextTurn(this.HeroTurnTaker);
            whenCardIsDestroyedStatusEffect.Priority = new StatusEffectPriority?(StatusEffectPriority.High);
            whenCardIsDestroyedStatusEffect.CanEffectStack = false;
            whenCardIsDestroyedStatusEffect.PostDestroyDestinationMustBeChangeable = true;
            coroutine = this.AddStatusEffect(whenCardIsDestroyedStatusEffect, true);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

#pragma warning disable IDE0060 // Remove unused parameter

        public IEnumerator WhenEquipIsDestroyedFlip(DestroyCardAction destroy, HeroTurnTaker hero, StatusEffect effect, int[] powerNumerals = null)
        {
            IEnumerator coroutine;
            if (hero != null && destroy.CanBeCancelled)
            {
                if (destroy.CardToDestroy.Card.IsMissionCard)
                {
                    coroutine = this.GameController.SendMessageAction("Mission cards cannot be flipped face-down, so they will still be destroyed.", Priority.Low, this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
                else
                {
                    List<YesNoCardDecision> storedResults = new List<YesNoCardDecision>();
                    coroutine = this.GameController.MakeYesNoCardDecision(this.DecisionMaker, SelectionType.FlipCardFaceDown, destroy.CardToDestroy.Card, storedResults: storedResults, cardSource: this.GetCardSource());
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                    if (this.DidPlayerAnswerYes(storedResults))
                    {
                        destroy.PreventMoveToTrash = true;

                        // Cancel the destruction.
                        coroutine = this.GameController.CancelAction(destroy, false, true, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

                        // Flip the card face-down.
                        coroutine = this.GameController.FlipCard(destroy.CardToDestroy, cardSource: this.GetCardSource());
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                    }
                }
            }
        }

#pragma warning restore IDE0060 // Remove unused parameter
    }
}