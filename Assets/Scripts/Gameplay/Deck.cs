using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class DeckData
    {
        protected List<Card> cards;
    }

    public class Deck : MonoBehaviour
    {
        protected DeckData deckData = new DeckData();
    }
}