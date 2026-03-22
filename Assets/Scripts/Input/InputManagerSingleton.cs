using Debug;
using Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public enum EMoveCameraDirection
    {
        Up,
        Down,
        Left,
        Right,
    }

    public class InputManagerSingleton : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;

        public static InputManagerSingleton instance { get; private set; }


        private void Awake()
        {
            //Ensure singleton is unique
            if (instance != null && instance != this)
            {
                DebugSystem.Warn("Multiple InputManagers in scene, this is invalid!");
                Destroy(gameObject);
                return;
            }

            //Set instance to this
            instance = this;

            //Check for input action asset, or error out
            if (!inputActionAsset)
            {
                DebugSystem.Error("No input system action asset defined!");
                return;
            }

            //Bind events
            //Up
            inputActionAsset.FindAction("MoveCameraUp", true).started += (ctx) =>
            {
                DoCameraMove(EMoveCameraDirection.Up);
            };
            //Down
            inputActionAsset.FindAction("MoveCameraDown", true).started += (ctx) =>
            {
                DoCameraMove(EMoveCameraDirection.Down);
            };
            //Left
            inputActionAsset.FindAction("MoveCameraLeft", true).started += (ctx) =>
            {
                DoCameraMove(EMoveCameraDirection.Left);
            };
            //Right
            inputActionAsset.FindAction("MoveCameraRight", true).started += (ctx) =>
            {
                DoCameraMove(EMoveCameraDirection.Right);
            };
        }

        protected void DoCameraMove(EMoveCameraDirection moveDirection)
        {
            InputEvents.InvokeOnInputMoveCamera(this,
                new InputEvents.InputMoveCameraEventPayload(moveDirection, true));
        }
    }
}