using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Diagnostics;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    // TODO: TEST!
    public class ShapeTheStreamCardController : CascadeRiverSharedCardController
    {
        public ShapeTheStreamCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public override IEnumerator Play()
        {
            IEnumerator coroutine;

            // Select and move one of the Riverbank cards.
            // Yes, this is messy, but it's still the cleanest way of mimicing the official SelectCardAndDoAction without access to the evenIfIndestructable flag. Battle Zones shouldn't be an issue. 

            coroutine = this.GameController.SelectCardAndDoAction(new SelectCardDecision(this.GameController, this.HeroTurnTakerController, SelectionType.MoveCard, this.GameController.FindCardsWhere((Card c) => c.Location == this.Riverbank().UnderLocation)),
                (SelectCardDecision d) => this.GameController.MoveCard(this.HeroTurnTakerController, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, this.GetCardSource()),
                false);

            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        // Code for debugging: For whatever reason, tests don't handle this gracefully when running all. 
        //public bool FindCardsFunc(Card c)
        //{
        //    Log.Debug("Card: " + c.Identifier + " Location A: " + c.Location.Identifier + " Location B: " + Riverbank().UnderLocation.Identifier);
        //    if (c.Location == Riverbank().UnderLocation)
        //    {
        //        return true;
        //    };
        //    return false;
        //}

        //public override IEnumerator Play()
        //{
        //    IEnumerator coroutine;

        //    // Select and move one of the Riverbank cards.
        //    // Yes, this is messy, but it's still the cleanest way of mimicing the official SelectCardAndDoAction without access to the evenIfIndestructable flag. Battle Zones shouldn't be an issue. 
        //    SelectCardDecision selectCardDecision = new SelectCardDecision(this.GameController, this.HeroTurnTakerController, SelectionType.MoveCard, this.GameController.FindCardsWhere((Card c) => FindCardsFunc(c), true, null, null), false, false, null, null, null, null, null, false, true, base.GetCardSource(), null);

        //    coroutine = this.GameController.SelectCardAndDoAction(selectCardDecision, (SelectCardDecision d) => this.GameController.MoveCard(this.HeroTurnTakerController, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, selectCardDecision.CardSource), true);

        //    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}

        public override int WaterCost()
        {
            return 5;
        }
    }
}