using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

// TODO: TEST!

namespace RuduenWorkshop.Cascade
{
    public class CascadeCharacterCardController : HeroCharacterCardController, ICascadeRiverSharedCardController
    {
        public string str;
        protected static Location _riverDeck;
        protected static Card _riverbank;

        public CascadeCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _riverbank = null;
            _riverDeck = null;
        }

        public override IEnumerator UsePower(int index = 0)
        {
            IEnumerator coroutine;
            List<DiscardCardAction> storedResults = new List<DiscardCardAction>();

            // Initial test: Discard up to three cards.
            coroutine = this.GameController.SelectAndDiscardCards(this.DecisionMaker, 3, false, 0, storedResults, allowAutoDecide: true, cardSource: this.GetCardSource());
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            int? spellValue = 0;
            foreach (DiscardCardAction discard in storedResults)
            {
                spellValue += discard.CardToDiscard.MagicNumber;
            }

            // Select a card under the riverbank whose cost is less than the others.
            // Yes, this is messy, but it's still the cleanest way of mimicing the official SelectCardAndDoAction without access to the evenIfIndestructable flag. Battle Zones shouldn't be an issue.
            // Do null checks first for short circuiting purposes!
            coroutine = this.GameController.SelectCardAndDoAction(
                new SelectCardDecision(this.GameController, this.DecisionMaker, SelectionType.MoveCard, this.GameController.FindCardsWhere((Card c) => c.Location == this.Riverbank().UnderLocation && c.FindTokenPool("CascadeCostPool") != null && c.FindTokenPool("CascadeCostPool").MaximumValue != null && c.FindTokenPool("CascadeCostPool").MaximumValue <= spellValue)),
                (SelectCardDecision d) => this.GameController.MoveCard(this.DecisionMaker, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, this.GetCardSource()),
                false);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            // Draw until you have 4 cards.
            coroutine = this.DrawCardsUntilHandSizeReached(this.DecisionMaker, 4);
            if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }

        //public IEnumerator MoveCardTest(SelectCardDecision d)
        //{
        //    IEnumerator coroutine = this.GameController.MoveCard(this.DecisionMaker, d.SelectedCard, this.HeroTurnTaker.Trash, false, false, false, null, false, null, null, null, true, false, null, false, false, false, false, this.GetCardSource());
        //    if (UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}

        //public bool ConfirmCost(Card c, int? spellValue)
        //{
        //    if (c.Location == this.Riverbank().UnderLocation)
        //    {
        //        int? tokenPoolValue = c.FindTokenPool("CascadeCostPool").MaximumValue;
        //        if (tokenPoolValue <= spellValue)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        // TODO: Replace with something more unique!
        public override IEnumerator UseIncapacitatedAbility(int index)
        {
            IEnumerator coroutine;
            switch (index)
            {
                case 0:
                    {
                        coroutine = this.SelectHeroToPlayCard(this.DecisionMaker);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 1:
                    {
                        coroutine = base.GameController.SelectHeroToUsePower(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
                case 2:
                    {
                        coroutine = base.GameController.SelectHeroToDrawCard(this.DecisionMaker, cardSource: this.GetCardSource(null));
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    }
            }
            yield break;
        }

        public Location RiverDeck()
        {
            if (CascadeCharacterCardController._riverDeck == null)
            {
                CascadeCharacterCardController._riverDeck = this.TurnTaker.FindSubDeck("RiverDeck");
            }
            return CascadeCharacterCardController._riverDeck;
        }

        public Card Riverbank()
        {
            if (CascadeCharacterCardController._riverbank == null)
            {
                CascadeCharacterCardController._riverbank = this.FindCard("Riverbank", false);
            }
            return CascadeCharacterCardController._riverbank;
        }
    }
}