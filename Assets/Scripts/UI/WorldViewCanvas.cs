using CustomCamera;
using Debug;
using JetBrains.Annotations;
using UnityEngine;

namespace UI
{
    public class WorldViewCanvas : MonoBehaviour
    {
        private Canvas _canvas;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            _canvas = GetComponent(typeof(Canvas)) as Canvas;
            CameraSubsystem.MainCameraChangedEvent += OnMainCameraChanged;
        }

        private void Start()
        {
            SetCamera(CameraSubsystem.GetMainCamera().playerCamera);
        }

        private void SetCamera(Camera newCamera)
        {
            if (_canvas == null) return;
            _canvas.worldCamera = newCamera;
            DebugSystem.Log("Main camera changed");
        }

        private void OnMainCameraChanged([CanBeNull] PlayerCamera oldCamera, PlayerCamera newCamera)
        {
            SetCamera(newCamera.playerCamera);
        }
    }
}