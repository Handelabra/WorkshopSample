using Handelabra;
using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace RuduenPromosWorkshop.HeroPromos
{
    public abstract class HeroPromosSharedToHeroCardController : CardController
    {

        public abstract IEnumerator PowerCoroutine(CardController cardController); // Override on individual coroutines! 
        public string ToHeroIdentifier;
        public string PowerDescription;
        public HeroPromosSharedToHeroCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            ToHeroIdentifier = "";
            PowerDescription = "";
            this.AddAsPowerContributor();
        }

        public LinqCardCriteria NextToCardCriteria()
        {
            return new LinqCardCriteria((Card c) => c.Identifier == this.ToHeroIdentifier, "matching hero");
        }

        public override IEnumerator DeterminePlayLocation(List<MoveCardDestination> storedResults, bool isPutIntoPlay, List<IDecision> decisionSources, Location overridePlayArea = null, LinqTurnTakerCriteria additionalTurnTakerCriteria = null)
        {
            IEnumerator coroutine = base.SelectCardThisCardWillMoveNextTo(this.NextToCardCriteria(), storedResults, isPutIntoPlay, decisionSources);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        }


        private Power AddedPower(CardController cardController)
        {
            return new Power(cardController.DecisionMaker, cardController, PowerDescription, this.PowerCoroutine(cardController), 0, null, this.GetCardSource());
        }

        public override IEnumerable<Power> AskIfContributesPowersToCardController(CardController cardController)
        {
            if (this.GetCardThisCardIsNextTo(true) != null && cardController.Card == this.GetCardThisCardIsNextTo(true))
            {
                List<Power> list = new List<Power>()
                {
                    AddedPower(cardController)
                };
                return list;
            }
            return null;
        }

    }
}