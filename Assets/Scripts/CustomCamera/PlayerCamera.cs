using System;
using Behaviours;
using Debug;
using Global;
using Input;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomCamera
{
    public class PlayerCamera : DynamicObject
    {
        [SerializeField] private float moveDuration = 0.35f;

        protected ECameraViewMode currentViewMode;

        public Camera playerCamera { get; private set; }

        private void Awake()
        {
            playerCamera = GetComponent<Camera>();
            if (!playerCamera)
            {
                DebugSystem.Error("Camera not present on main camera!");
                return;
            }

            CameraSubsystem.ChangeMainCamera(this);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            //Move to the hand marker
            Transform handMkr = CameraSubsystem.GetCameraViewTransform(ECameraViewMode.Hand);
            if (!handMkr)
            {
                DebugSystem.Error("No hand marker available, cannot change camera position");
                return;
            }

            //Start position
            currentViewMode = ECameraViewMode.Hand;
            transform.position = handMkr.position;
            transform.rotation = handMkr.rotation;

            //Bind events
            InputEvents.OnInputMoveCamera += OnInputMoveCamera;
        }

        public void ChangeCameraViewMode(ECameraViewMode newViewMode)
        {
            //Check if move is valid
            if (newViewMode == currentViewMode) return;

            //Set our new view mode and save old
            ECameraViewMode oldMode = currentViewMode;
            currentViewMode = newViewMode;

            //Get the mkr
            Transform moveMkr = CameraSubsystem.GetCameraViewTransform(currentViewMode);
            if (!moveMkr) return;

            //Now move
            MoveTo(moveMkr.position, moveDuration);
            RotateTo(moveMkr.rotation, moveDuration);

            //Now broadcast
            CameraEvents.InvokeOnCameraMoved(this, new CameraEvents.CameraMovedEventPayload(oldMode, newViewMode));
        }

        protected void OnInputMoveCamera([CanBeNull] object sender, InputEvents.InputMoveCameraEventPayload payload)
        {
            //Change the camera view
            ChangeCameraViewMode(GetNextViewMode(payload.direction));
        }

        //TODO: This will likely need to hook into the game state once that's made to see if we're in the board phase / card phase
        protected ECameraViewMode GetNextViewMode(EMoveCameraDirection moveDirection)
        {
            switch (currentViewMode)
            {
                case ECameraViewMode.Hand:
                    //hand->board
                    if (moveDirection == EMoveCameraDirection.Up)
                    {
                        return ECameraViewMode.Board;
                    }

                    //hand->board right
                    if (moveDirection == EMoveCameraDirection.Right)
                    {
                        return ECameraViewMode.BoardRight;
                    }

                    //hand->board left
                    if (moveDirection == EMoveCameraDirection.Left)
                    {
                        return ECameraViewMode.BoardLeft;
                    }

                    //invalid move
                    return ECameraViewMode.Hand;
                case ECameraViewMode.Board:
                    return moveDirection == EMoveCameraDirection.Down
                        ? ECameraViewMode.Hand
                        : ECameraViewMode.Board;
                case ECameraViewMode.BoardLeft:
                    return moveDirection is EMoveCameraDirection.Right or EMoveCameraDirection.Down
                        ? ECameraViewMode.Hand
                        : ECameraViewMode.Board;
                case ECameraViewMode.BoardRight:
                    return moveDirection is EMoveCameraDirection.Left or EMoveCameraDirection.Down
                        ? ECameraViewMode.Hand
                        : ECameraViewMode.Board;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}