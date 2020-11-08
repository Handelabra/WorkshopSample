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
    public abstract class CascadeRiverSharedCardController : CardController
    {
        // TO DO: If this doesn't work cleanly, remove the entire static variable! 

        protected static Location _riverDeck;
        protected static Card _riverbank;
        protected static TurnTakerController _turnTakerController; 
        public CascadeRiverSharedCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
            // Set these up on initialization, so Guise doesn't do bad things. 
            _turnTakerController = TurnTakerController;
            _riverbank = null;
            _riverDeck = null; 
        }
        public Location RiverDeck()
        {

            if (CascadeRiverSharedCardController._riverDeck == null)
            {
                // TODO: These must always find Cascade's river deck. Even if Guise is using things! 
                CascadeRiverSharedCardController._riverDeck = _turnTakerController.TurnTaker.FindSubDeck("RiverDeck");
            }
            return CascadeRiverSharedCardController._riverDeck;

        }
        public Card Riverbank()
        {
            if (CascadeRiverSharedCardController._riverbank == null)
            {
                CascadeRiverSharedCardController._riverbank = _turnTakerController.TurnTaker.FindCard("Riverbank", false);
            }
            return CascadeRiverSharedCardController._riverbank;
        }
    }
}