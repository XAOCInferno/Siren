using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomCamera
{
    public enum ECameraViewMode
    {
        Hand = 0,
        Board,
        BoardRight,
        BoardLeft,
    }

    public delegate void MainCameraChangedEventHandler([CanBeNull] PlayerCamera oldCamera, PlayerCamera newCamera);

    public static class CameraSubsystem
    {
        public static event MainCameraChangedEventHandler MainCameraChangedEvent;
        private static PlayerCamera _currentCamera;

        private static readonly Dictionary<ECameraViewMode, Transform> CameraViewTransforms = new();

        public static void ChangeMainCamera(PlayerCamera camera)
        {
            PlayerCamera oldCamera = _currentCamera;
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

        public static PlayerCamera GetMainCamera()
        {
            return _currentCamera;
        }

        public static void SetCameraViewTransform(ECameraViewMode viewMode, Transform transform)
        {
            CameraViewTransforms.Add(viewMode, transform);
        }

        [CanBeNull]
        public static Transform GetCameraViewTransform(ECameraViewMode viewMode)
        {
            return CameraViewTransforms[viewMode];
        }
    }
}