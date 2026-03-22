namespace CustomCamera
{
    public class CameraPointGameplay : CameraPoint
    {
        public ECameraViewMode viewMode;

        private void Awake()
        {
            CameraSubsystem.SetCameraViewTransform(viewMode, transform);
        }
    }
}