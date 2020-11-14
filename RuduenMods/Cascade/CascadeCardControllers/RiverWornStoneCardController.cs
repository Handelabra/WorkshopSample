using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    public class RiverWornStoneCardController : CascadeRiverSharedCardController
    {
        public RiverWornStoneCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
        }

        public override void AddTriggers()
        {
            this.AddAdditionalPhaseActionTrigger((TurnTaker tt) => this.ShouldIncreasePhaseActionCount(tt), Phase.DrawCard, 1);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            int powerNumeral = this.GetPowerNumeral(0, 1);

            IEnumerator coroutine;
            List<MoveCardAction> storedResultsMove = new List<MoveCardAction>();
            // Select and move card to the bottom of the river deck. 
            coroutine = this.GameController.SelectCardsFromLocationAndMoveThem(this.HeroTurnTakerController, this.HeroTurnTaker.Hand, 1, 1,
                new LinqCardCriteria((Card card) => this.HeroTurnTaker.Hand.HasCard(card)),
                new MoveCardDestination[]
                {
                    new MoveCardDestination(this.RiverDeck(), true)
                },
                storedResultsMove: storedResultsMove);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Track moved card for damage.
            if (storedResultsMove.Count > 0 && storedResultsMove.FirstOrDefault().IsSuccessful && storedResultsMove.FirstOrDefault().CardToMove.MagicNumber != null)
            {
                // Damage based on moved card's magic number.
                coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), (int)storedResultsMove.FirstOrDefault().CardToMove.MagicNumber, DamageType.Infernal, new int?(powerNumeral), false, new int?(powerNumeral), false, false, false, null, null, null, null, null, false, null, null, false, null, this.GetCardSource(null));
                if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            }
        }

        private bool ShouldIncreasePhaseActionCount(TurnTaker tt)
        {
            return tt == this.TurnTaker;
        }

        public override bool AskIfIncreasingCurrentPhaseActionCount()
        {
            return this.GameController.ActiveTurnPhase.IsDrawCard && this.ShouldIncreasePhaseActionCount(this.GameController.ActiveTurnTaker);
        }
    }
}