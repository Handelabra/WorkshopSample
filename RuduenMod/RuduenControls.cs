using Handelabra.Sentinels.Engine.Controller;
using Handelabra.Sentinels.Engine.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Workshopping
{
    public interface RuduenCardHandler
    {
        public IEnumerable RunCoroutine(IEnumerator coroutine);
    }
    public class RuduenHeroCharacterCardController : HeroCharacterCardController, RuduenCardHandler
    {
        public RuduenHeroCharacterCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
        public IEnumerable RunCoroutine(IEnumerator coroutine)
        {
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
    }

    public class RuduenCardController : CardController, RuduenCardHandler
    {
        public RuduenCardController(Card card, TurnTakerController turnTakerController)
            : base(card, turnTakerController)
        {
        }
        public IEnumerable RunCoroutine(IEnumerator coroutine)
        {
            if (base.UseUnityCoroutines)
            {
                yield return base.GameController.StartCoroutine(coroutine);
            }
            else
            {
                base.GameController.ExhaustCoroutine(coroutine);
            }
        }
    }
}
