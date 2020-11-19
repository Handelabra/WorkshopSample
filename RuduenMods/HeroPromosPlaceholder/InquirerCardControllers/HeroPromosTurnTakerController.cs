using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace Workshopping.HeroPromos
{
    public class HeroPromosTurnTakerController : HeroTurnTakerController
    {
        public HeroPromosTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}