using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace RuduenPromosWorkshop.HeroPromos
{
    public class TheHarpyArcaneFlockCharacterCardController : HeroCharacterCardController
    {
        public TheHarpyArcaneFlockCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }



        //public override IEnumerator UsePower(int index = 0)
        //{
        //    TokenPool avianPool = base.Card.FindTokenPool(TokenPool.AvianControlPool);
        //    TokenPool arcanaPool = base.Card.FindTokenPool(TokenPool.ArcanaControlPool);

        //        if (avianPool == null || !base.TurnTaker.IsHero)
        //        {
        //            TurnTaker turnTaker = this.FindTurnTakersWhere((TurnTaker tt) => tt.Identifier == "TheHarpy", false).FirstOrDefault<TurnTaker>();
        //            if (turnTaker != null)
        //            {
        //                avianPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.AvianControlPool);
        //                arcanaPool = turnTaker.CharacterCard.FindTokenPool(TokenPool.ArcanaControlPool);
        //            }
        //        }
        //    IEnumerator coroutine;

        //    // Sanity check - need birds for more birds, and we can avoid shenanigans related to if the token pool already exists. Dang it, Guise!
        //    if (arcanaPool != null && avianPool != null && avianPool.CurrentValue >= 1)
        //    {
        //        List<Function> list = new List<Function>();
        //        list.Add(new Function(this.DecisionMaker, "Add one {avian}", SelectionType.AddTokens, () => this.AddAvianToken(avianPool, arcanaPool)));
        //        list.Add(new Function(this.DecisionMaker, "Remove one {avian}", SelectionType.RemoveTokens, () => this.RemoveAvianToken(avianPool, arcanaPool)));
        //    }
        //    else
        //    {
        //        coroutine = this.GameController.SendMessageAction("There are no {avian}, so none will be added or removed.", Priority.Medium, this.GetCardSource(null));
        //        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //    }

        //    // Draw a card.
        //    coroutine = base.DrawCard(null, false, null, true);
        //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

        //}

        //public IEnumerator AddAvianToken(TokenPool avianPool, TokenPool arcanePool)
        //{
        //    yield break;
        //}

        //public IEnumerator RemoveAvianToken(TokenPool avianPool, TokenPool arcanePool)
        //{
        //    yield break;
        //}
    }
}