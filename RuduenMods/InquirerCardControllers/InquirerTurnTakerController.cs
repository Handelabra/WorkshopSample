using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;

namespace RuduenWorkshop.Inquirer
{
    public class InquirerTurnTakerController : HeroTurnTakerController
    {
        public InquirerTurnTakerController(TurnTaker turnTaker, GameController gameController)
            : base(turnTaker, gameController)
        {
        }
    }
}