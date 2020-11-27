using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace RuduenWorkshop.Spellforge
{
    public class SpellforgeTurnTakerController : HeroTurnTakerController
    {
        public SpellforgeTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}