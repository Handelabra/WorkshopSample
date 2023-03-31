using System;
using System.Collections;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.TheBaddies
{
    public class TheBaddiesCharacterCardController : VillainCharacterCardController
    {
        public TheBaddiesCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override bool CanBeDestroyed
        {
            get
            {
                // Can only be destroyed if we are flipped.
                return this.CharacterCard.IsFlipped;
            }
        }

        public override void AddSideTriggers()
        {
            if (!this.Card.IsFlipped)
            {
                // At the end of the villain turn...
                this.SideTriggers.Add(AddEndOfTurnTrigger(tt => tt == this.TurnTaker, EndVillainTurnResponse, TriggerType.DestroyCard));

                if (IsGameAdvanced)
                {
                    // At the end of the villain turn...
                    this.SideTriggers.Add(AddEndOfTurnTrigger(tt => tt == this.TurnTaker, AdvancedEndVillainTurnResponse, TriggerType.DestroyCard));
                }

                AddDefeatedIfMovedOutOfGameTriggers();
            }
            else
            {

                // If HP is <= 0, we are defeated.
                AddDefeatedIfDestroyedTriggers();
            }
        }

        public override IEnumerator DestroyAttempted(DestroyCardAction destroyCard)
        {
            if (!this.Card.IsFlipped)
            {
                // Run a FlipAction when we would be destroyed.
                var e = this.GameController.FlipCard(this, actionSource: destroyCard.ActionSource, cardSource: GetCardSource());
                if (UseUnityCoroutines)
                {
                    yield return this.GameController.StartCoroutine(e);
                }
                else
                {
                    this.GameController.ExhaustCoroutine(e);

                }
            }
        }

        private IEnumerator EndVillainTurnResponse(PhaseChangeAction phaseChange)
        {
            // Destroy 1 hero ongoing card.
            return this.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria(c => c.IsInPlay && c.IsHero && IsOngoing(c), "hero ongoing"), false, cardSource: GetCardSource());
        }

        private IEnumerator AdvancedEndVillainTurnResponse(PhaseChangeAction phaseChange)
        {
            // Destroy 1 equipment card.
            return this.GameController.SelectAndDestroyCard(this.DecisionMaker, new LinqCardCriteria(c => c.IsInPlay && this.GameController.IsEquipment(c), "equipment"), false, cardSource: GetCardSource());
        }

        public override IEnumerator AfterFlipCardImmediateResponse()
        {
            var e = base.AfterFlipCardImmediateResponse();
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(e);
            }
            else
            {
                this.GameController.ExhaustCoroutine(e);

            }

            // When flipped to this side, restore to 30 HP.
            var restoreHP = this.GameController.ChangeMaximumHP(this.Card, 30, true, cardSource: GetCardSource());
            if (UseUnityCoroutines)
            {
                yield return this.GameController.StartCoroutine(restoreHP);
            }
            else
            {
                this.GameController.ExhaustCoroutine(restoreHP);

            }
        }

        protected override EndingResult GetEndingResultType()
        {
            // If on his back side, it's a normal victory.
            // If on his front side, a premature victory.

            if (this.Card.IsFlipped)
            {
                return EndingResult.VillainDestroyedVictory;
            }
            else
            {
                return EndingResult.PrematureVictory;
            }
        }
    }
}
