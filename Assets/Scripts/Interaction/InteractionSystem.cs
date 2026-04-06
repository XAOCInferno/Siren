namespace Interaction
{
    public enum EInteractionState
    {
        UnInteractable,
        Idle,
        Hovered,
        Selected,
    }

    public static class InteractionSystem
    {
        private static IInteractable _selected;

        public static void SetIdle(IInteractable interactable)
        {
            interactable.SetIdle();
        }

        public static void SetHovered(IInteractable interactable)
        {
            interactable.SetHovered();
        }

        public static void SetSelected(IInteractable interactable)
        {
            _selected?.SetIdle();
            _selected = interactable;
            interactable.SetSelected();
        }

        public static void ClearSelected()
        {
            _selected?.SetIdle();
            _selected = null;
        }

        public static void SetInteractable(IInteractable interactable, bool isInteractable)
        {
            interactable.SetInteractable(isInteractable);
            if (!isInteractable)
            {
                if (interactable == _selected)
                {
                    _selected = null;
                }
            }
        }
    }
}