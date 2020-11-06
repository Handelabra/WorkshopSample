using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Workshopping.BreachMage
{
    public class CascadeRiverSharedCardController : CardController
    {
        public CascadeRiverSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }

        public Location RiverDeck()
        {
            return base.TurnTaker.FindSubDeck("RiverDeck");
        }

        public Location RiverDeckPlayArea()
        {

            return base.TurnTaker.FindSubPlayArea("RiverDeck");

        }

    }
}