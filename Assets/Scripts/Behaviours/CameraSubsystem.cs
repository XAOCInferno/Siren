using JetBrains.Annotations;
using UnityEngine;

namespace Behaviours
{
    public delegate void MainCameraChangedEventHandler([CanBeNull] Camera oldCamera, Camera newCamera);

    public static class CameraSubsystem
    {
        public static event MainCameraChangedEventHandler MainCameraChangedEvent;
        private static Camera _currentCamera;

        public static void ChangeMainCamera(Camera camera)
        {
            Camera oldCamera = _currentCamera;
            if (_currentCamera != null)
            {
                //Disable old
                _currentCamera.enabled = false;
            }

            //Enable new
            _currentCamera = camera;
            _currentCamera.enabled = true;
            //Broadcast
            MainCameraChangedEvent?.Invoke(oldCamera, _currentCamera);
        }

        public static Camera GetMainCamera()
        {
            return _currentCamera;
        }
    }
}