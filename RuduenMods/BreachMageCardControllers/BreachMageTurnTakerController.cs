using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace RuduenWorkshop.BreachMage
{
    public class BreachMageTurnTakerController : HeroTurnTakerController
    {
        public BreachMageTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}