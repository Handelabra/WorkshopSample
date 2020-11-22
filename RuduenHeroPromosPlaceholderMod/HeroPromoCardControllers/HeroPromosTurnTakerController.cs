using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;

namespace RuduenWorkshop.HeroPromos
{
    public class HeroPromosTurnTakerController : HeroTurnTakerController
    {
        public HeroPromosTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {


        }

        //public override IEnumerator StartGame()
        //{
        //    IEnumerator coroutine;

        //    // Remove turn taker from the game. It looks like doing so will fix the "H" count. 
        //    coroutine = this.GameController.ReplaceTurnTaker(this.TurnTaker, null, false, true);
        //    if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
        //}
    }
}