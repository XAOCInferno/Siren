using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public enum EUnityButtonStates
    {
        Normal = 0,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    public abstract class SimpleButton : MonoBehaviour
    {
        protected UnityEngine.UI.Button button;

        private ColorBlock _activeColors;
        private readonly Dictionary<EUnityButtonStates, Color> _baseButtonColors = new();

        protected virtual void Awake()
        {
            button = GetComponent<UnityEngine.UI.Button>();
            Assert.IsNotNull(button);

            // Get active colors
            _activeColors = button.colors;

            // Get base colors
            _baseButtonColors.Add(EUnityButtonStates.Normal, button.colors.normalColor);
            _baseButtonColors.Add(EUnityButtonStates.Highlighted, button.colors.highlightedColor);
            _baseButtonColors.Add(EUnityButtonStates.Pressed, button.colors.pressedColor);
            _baseButtonColors.Add(EUnityButtonStates.Selected, button.colors.selectedColor);
            _baseButtonColors.Add(EUnityButtonStates.Disabled, button.colors.disabledColor);
        }

        public abstract void OnButtonPressed();

        public void ChangeActiveColor(EUnityButtonStates state, Color color)
        {
            switch (state)
            {
                case EUnityButtonStates.Normal:
                    _activeColors.normalColor = color;
                    break;
                case EUnityButtonStates.Highlighted:
                    _activeColors.highlightedColor = color;
                    break;
                case EUnityButtonStates.Pressed:
                    _activeColors.pressedColor = color;
                    break;
                case EUnityButtonStates.Selected:
                    _activeColors.selectedColor = color;
                    break;
                case EUnityButtonStates.Disabled:
                    _activeColors.disabledColor = color;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            button.colors = _activeColors;
        }

        public void ResetActiveColorToDefault(EUnityButtonStates state)
        {
            switch (state)
            {
                case EUnityButtonStates.Normal:
                    _activeColors.normalColor = _baseButtonColors[EUnityButtonStates.Normal];
                    break;
                case EUnityButtonStates.Highlighted:
                    _activeColors.highlightedColor = _baseButtonColors[EUnityButtonStates.Highlighted];
                    break;
                case EUnityButtonStates.Pressed:
                    _activeColors.pressedColor = _baseButtonColors[EUnityButtonStates.Pressed];
                    break;
                case EUnityButtonStates.Selected:
                    _activeColors.selectedColor = _baseButtonColors[EUnityButtonStates.Selected];
                    break;
                case EUnityButtonStates.Disabled:
                    _activeColors.disabledColor = _baseButtonColors[EUnityButtonStates.Disabled];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            button.colors = _activeColors;
        }
    }
}