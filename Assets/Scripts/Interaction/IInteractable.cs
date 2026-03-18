namespace Interaction
{
    public interface IInteractable
    {
        public void SetIdle();
        public void SetHovered();
        public void SetSelected();
        public void SetInteractable(bool interactable);
    }
}