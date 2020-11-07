using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Workshopping.Cascade
{
    public interface ICascadeRiverSharedCardController
    {
        public Location RiverDeck();
        public Card Riverbank();
    }
    public class CascadeRiverSharedCardController : CardController
    {
        // TO DO: If this doesn't work cleanly, remove the entire static variable! 

        protected static Location _riverDeck;
        protected static Card _riverbank;
        public CascadeRiverSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            _riverbank = null;
            _riverDeck = null;
        }
        public Location RiverDeck()
        {

            if (CascadeRiverSharedCardController._riverDeck == null)
            {
                CascadeRiverSharedCardController._riverDeck = base.TurnTaker.FindSubDeck("RiverDeck");
            }
            return CascadeRiverSharedCardController._riverDeck;

        }
        public Card Riverbank()
        {
            if (CascadeRiverSharedCardController._riverbank == null)
            {
                CascadeRiverSharedCardController._riverbank = base.FindCard("Riverbank", false);
            }
            return CascadeRiverSharedCardController._riverbank;
        }

        public virtual int WaterCost()
        {
            return 1; // Default. 
        }
    }
}