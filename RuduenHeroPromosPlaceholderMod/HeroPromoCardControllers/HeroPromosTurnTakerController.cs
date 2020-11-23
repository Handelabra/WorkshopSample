using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RuduenPromosWorkshop.HeroPromos
{
    public class HeroPromosTurnTakerController : HeroTurnTakerController
    {
        public HeroPromosTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {


        }

        public override IEnumerator StartGame()
        {
            IEnumerator coroutine;

            List<HeroTurnTakerController> httcs = new List<HeroTurnTakerController>(this.GameController.FindHeroTurnTakerControllers());

            foreach(HeroTurnTakerController httc in httcs)
            {
                switch (httc.TurnTaker.Identifier)
                {
                    case "Expatriette":
                        coroutine = this.GameController.PlayCard(this, this.FindCardsWhere((Card c) => c.Identifier == "ExpatrietteQuickshot").FirstOrDefault(), true);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    case "MrFixer":
                        coroutine = this.GameController.PlayCard(this, this.FindCardsWhere((Card c) => c.Identifier == "MrFixerFlowingStrike").FirstOrDefault(), true);
                        if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
                        break;
                    default:
                        break;
                }
            }

            coroutine = this.GameController.DestroyCard(this, this.CharacterCard);
            if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }

            //// Remove turn taker from the game. It looks like doing so will fix the "H" count. 
            //coroutine = this.GameController.ReplaceTurnTaker(this.TurnTaker, null, false, true);
            //if (this.UseUnityCoroutines) { yield return this.GameController.StartCoroutine(coroutine); } else { this.GameController.ExhaustCoroutine(coroutine); }
            yield break;
        }
    }
}