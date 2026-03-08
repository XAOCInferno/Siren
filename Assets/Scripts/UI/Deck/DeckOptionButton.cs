using System;
using Debug;
using UnityEngine;

public enum DeckActionType
{
    Draw = 0,
    EndTurn = 1
}

namespace UI.Deck
{
    public class DeckOptionButton : SimpleButton
    {
        public override void OnButtonPressed()
        {
        }
    }
}