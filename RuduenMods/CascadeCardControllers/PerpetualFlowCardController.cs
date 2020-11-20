using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.Cascade
{
    // TODO: TEST!
    public class PerpetualFlowCardController : CascadeRiverSharedCardController
    {
        public PerpetualFlowCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Select and play one of the Riverbank cards.
            // Yes, this is messy, but it's still the cleanest way of mimicing the official SelectCardAndDoAction without access to the evenIfIndestructable flag. Battle Zones shouldn't be an issue.

            coroutine = this.GameController.SelectCardAndDoAction(new SelectCardDecision(this.GameController, this.DecisionMaker, SelectionType.PlayCard, this.GameController.FindCardsWhere((Card c) => c.Location == this.Riverbank().UnderLocation)),
                (SelectCardDecision d) => this.GameController.PlayCard(this.TurnTakerController, d.SelectedCard),
                false);

            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}