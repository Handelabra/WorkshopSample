using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Cascade
{
    public class MeetingTheOceanCardController : CascadeRiverSharedCardController
    {
        public MeetingTheOceanCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            this.GameController.AddCardControllerToList(CardControllerListType.IncreasePhaseActionCount, this);
        }

        public override IEnumerator UsePower(int index = 0)
        {
            List<int> powerNumerals = new List<int>() {
                this.GetPowerNumeral(0, 3),
                this.GetPowerNumeral(1, 1),
            };

            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();
            IEnumerator coroutine;
            coroutine = this.SelectAndDiscardCards(this.DecisionMaker, powerNumerals[0], false, 0, storedResults, false, null, null, null, SelectionType.DiscardCard, this.TurnTaker);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            foreach (DiscardCardAction discardCardAction in storedResults)
            {
                if (discardCardAction.IsSuccessful && discardCardAction.CardToDiscard.MagicNumber != null)
                {
                    coroutine = this.GameController.SelectTargetsAndDealDamage(this.DecisionMaker, new DamageSource(this.GameController, this.CharacterCard), (int)discardCardAction.CardToDiscard.MagicNumber, DamageType.Cold, powerNumerals[1], false, powerNumerals[1], false, false, false, null, null, null, null, null, false, null, null, false, null, base.GetCardSource(null));
                    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                }
            }
        }
    }
}