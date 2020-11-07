using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Workshopping.Cascade;

namespace Workshopping.Cascade
{
    // Token: 0x0200054D RID: 1357
    public class RiverbankCardController : CascadeRiverSharedCardController
    {
        public RiverbankCardController(Card card, TurnTakerController turnTakerController) : base(card, turnTakerController)
        {
            base.SpecialStringMaker.ShowNumberOfCardsUnderCard(base.Card, () => true);
            base.AddThisCardControllerToList(CardControllerListType.MakesIndestructible);
        }
        public override void AddTriggers()
        {
            base.AddTrigger<MoveCardAction>((MoveCardAction m) => m.CardToMove.DoKeywordsContain("river") && m.Origin == base.Riverbank().UnderLocation && m.Destination != base.RiverDeck(), new Func<MoveCardAction, IEnumerator>(this.RefillRiverbankResponse), TriggerType.MoveCard, TriggerTiming.After, ActionDescription.Unspecified, false, true, null, false, null, null, false, false);
        }

        public override bool AskIfCardIsIndestructible(Card card)
        {
            return card == base.Card || card.Location == base.Card.UnderLocation;
        }
        private IEnumerator RefillRiverbankResponse(MoveCardAction m)
        {
            IEnumerator coroutine;
            Card remainingCard = Riverbank().UnderLocation.Cards.FirstOrDefault();

            // Move remaining riverbank cards. 
            while (remainingCard != null)
            {
                coroutine = base.GameController.MoveCard(this.HeroTurnTakerController, remainingCard, RiverDeck(), evenIfIndestructible: true);
                if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
                remainingCard = Riverbank().UnderLocation.Cards.FirstOrDefault();
            }

            // Then, move the top four to the riverbank. Normal empty deck logic should work if they aren't available.
            coroutine = base.GameController.MoveCards(this.HeroTurnTakerController, RiverDeck().GetTopCards(4), Riverbank().UnderLocation);
            if (base.UseUnityCoroutines) { yield return base.GameController.StartCoroutine(coroutine); } else { base.GameController.ExhaustCoroutine(coroutine); }
        }
    }
}