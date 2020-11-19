using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.HeroPromos
{
    public class PlaceholderOngoingCardController : CardController
    {
        public PlaceholderOngoingCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
    }
}