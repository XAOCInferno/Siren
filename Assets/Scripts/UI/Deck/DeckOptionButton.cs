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
        [SerializeField] protected DeckActionType actionType;

        public override void OnButtonPressed()
        {
            DebugSubsystem.Log($"Pressed {Enum.GetName(typeof(DeckActionType), actionType)}");
        }
    }
}