using System;
using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Behaviours
{
    public enum EMoveCompleteCallbackType
    {
        EndedEarly = 0,
        Completed,
    }

    /// <summary>
    /// Job to set position or local position of an object. Used with dynamic objects
    /// </summary>
    [BurstCompile]
    public struct MoveDynamicObjectJob : IJobParallelForTransform
    {
        [ReadOnly] public bool isWorldPosition;
        [ReadOnly] public Vector3 startPosition;
        [ReadOnly] public Vector3 endPosition;
        [ReadOnly] public float elapsedTime;
        [ReadOnly] public float duration;
        [ReadOnly] public bool processing;

        public void Execute(int index, TransformAccess transform)
        {
            processing = true;
            float movePercent = duration == 0 ? 1 : elapsedTime / duration;
            Vector3 location = Vector3.Lerp(startPosition, endPosition, movePercent);
            if (isWorldPosition)
            {
                transform.position = location;
            }
            else
            {
                transform.localPosition = location;
            }

            processing = false;
        }
    }

    [BurstCompile]
    public struct RotateDynamicObjectJob : IJobParallelForTransform
    {
        [ReadOnly] public bool isWorldRotation;
        [ReadOnly] public Quaternion startRotation;
        [ReadOnly] public Quaternion endRotation;
        [ReadOnly] public float elapsedTime;
        [ReadOnly] public float duration;
        [ReadOnly] public bool processing;

        public void Execute(int index, TransformAccess transform)
        {
            processing = true;
            Quaternion rotation = Quaternion.Lerp(startRotation, endRotation, elapsedTime / duration);
            if (isWorldRotation)
            {
                transform.rotation = rotation;
            }
            else
            {
                transform.localRotation = rotation;
            }

            processing = false;
        }
    }

    [BurstCompile]
    public class DynamicObject : MonoBehaviour
    {
        // Callback for whenever object moves or rotates
        protected Func<int> onMovementCallback = () => 0;

        // Callback for when object completes a move
        protected Func<EMoveCompleteCallbackType, int> moveCompleteCallback;

        // Callback for when object completes a rotation
        protected Func<EMoveCompleteCallbackType, int> rotateCompleteCallback;

        // Transform array containing just this.transform
        private TransformAccessArray _transformAccessArray;

        ///~State related
        private bool _doMove = false;

        private float _moveToDuration = 0;
        private float _elapsedMoveToTime = 0;
        private Vector3 _moveFrom = Vector3.zero;
        private Vector3 _moveTo = Vector3.zero;
        private bool _moveIsWorldPosition = false;

        private bool _doRotate = false;
        private float _rotateToDuration = 0;
        private float _elapsedRotateToTime = 0;
        private Quaternion _rotateFrom = Quaternion.identity;
        private Quaternion _rotateTo = Quaternion.identity;

        ///~State related End

        // Our jobs
        private MoveDynamicObjectJob _lastMoveJob;

        private RotateDynamicObjectJob _lastRotateJob;

        // Bound callback functions. Key: bound to object, Value: List of bound functions
        private readonly Dictionary<GameObject, List<Func<int>>> _mapOfBoundCallbackFunctions = new();

        private void Awake()
        {
            _transformAccessArray = new TransformAccessArray(new[] { transform });
        }

        private void Update()
        {
            // Return early if we're still processing an action. We aren't using JobHandle "Complete" here as we don't want to hold up the main thread.
            if (_lastMoveJob.processing || _lastRotateJob.processing) return;

            // Handle movements via jobs
            if (_doMove)
            {
                HandleMove();
            }

            if (_doRotate)
            {
                HandleRotation();
            }
        }

        private void OnDestroy()
        {
            _transformAccessArray.Dispose();
        }

        /// <summary>
        /// Handles any movement via a job.
        /// </summary>
        private void HandleMove()
        {
            // Ensure positions are not equal
            if (_moveTo == _moveFrom) return;

            // Begin the job
            _lastMoveJob = new MoveDynamicObjectJob()
            {
                isWorldPosition = _moveIsWorldPosition,
                startPosition = _moveFrom,
                endPosition = _moveTo,
                duration = _moveToDuration,
                elapsedTime = _elapsedMoveToTime,
            };
            _lastMoveJob.Schedule(_transformAccessArray);

            // Increment our progress
            _elapsedMoveToTime += Time.deltaTime;

            // Call movement callback
            onMovementCallback();

            // If we've finished move, then call move complete
            if (_elapsedMoveToTime >= _moveToDuration)
            {
                OnMoveComplete(EMoveCompleteCallbackType.Completed);
            }
        }

        /// <summary>
        /// Handles any rotation via a job.
        /// </summary>
        private void HandleRotation()
        {
            // Begin the job
            _lastRotateJob = new RotateDynamicObjectJob()
            {
                startRotation = _rotateFrom,
                endRotation = _rotateTo,
                duration = _rotateToDuration,
                elapsedTime = _elapsedRotateToTime,
            };
            _lastRotateJob.Schedule(_transformAccessArray).Complete();

            // Increment our progress
            _elapsedRotateToTime += Time.deltaTime;

            // Call rotation callback
            onMovementCallback();

            // If we've finished rotation, then call rotate complete
            if (_elapsedRotateToTime >= _rotateToDuration)
            {
                OnRotateComplete(EMoveCompleteCallbackType.Completed);
            }
        }

        // Shared
        /// <summary>
        /// Bind a function to the movement callback. Movement callback is called whenever an object moves or rotates via this controller
        /// </summary>
        /// <param name="receiver">The object that this is bound to.</param>
        /// <param name="funcToCall">The function you wish to call.</param>
        public void BindToOnMovementCallback(GameObject receiver, Func<int> funcToCall)
        {
            // Bind
            onMovementCallback += funcToCall;

            // Add to our map so wee can remove it later if calling Unbind
            if (!_mapOfBoundCallbackFunctions.ContainsKey(receiver))
            {
                _mapOfBoundCallbackFunctions.Add(receiver, new List<Func<int>>());
            }

            _mapOfBoundCallbackFunctions[receiver].Add(funcToCall);
        }

        /// <summary>
        /// UnBinds all functions of a game object bound to the movement callback.
        /// </summary>
        /// <param name="receiver">The object that we wish to unbind.</param>
        public void UnBindToOnMovementCallback(GameObject receiver)
        {
            // Try to get out bound function list
            if (!_mapOfBoundCallbackFunctions.TryGetValue(receiver, out List<Func<int>> funcList)) return;

            // Iterate over all bindings and remove them
            foreach (Func<int> func in funcList)
            {
                onMovementCallback -= func;
            }
        }

        // Move
        /// <summary>
        /// Moves this object to the designated position over time.
        /// </summary>
        /// <param name="location">The world position the object should move to.</param>
        /// <param name="duration">How long the move should take in seconds.</param>
        public void MoveTo(Vector3 location, float duration, bool isWorldPosition)
        {
            // End last move, if any
            CancelMove();

            // Begin new move
            StartMove(location, duration, isWorldPosition);
        }

        /// <summary>
        /// Moves this object to the designated position over time.
        /// </summary>
        /// <param name="location">The world position the object should move to.</param>
        /// <param name="duration">How long the move should take in seconds.</param>
        /// <param name="callback">The function we should call on completing the move.</param>
        public void MoveTo(Vector3 location, float duration, bool isWorldPosition,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            MoveTo(location, duration, isWorldPosition);

            //Callback
            moveCompleteCallback = callback;
        }

        /// <summary>
        /// Sets the movement details for this move. Called when a move first begins.
        /// </summary>
        /// <param name="location">The world position the object should move to.</param>
        /// <param name="duration">How long the move should take in seconds.</param>
        private void StartMove(Vector3 location, float duration, bool isWorldPosition)
        {
            _moveIsWorldPosition = isWorldPosition;
            _moveFrom = isWorldPosition ? transform.position : transform.localPosition;
            _moveTo = location;
            _moveToDuration = duration;
            _elapsedMoveToTime = 0;
            _doMove = true;
        }

        /// <summary>
        /// Sets the rotation details for this move. Called when a move first begins.
        /// </summary>
        /// <param name="rotation">The world rotation the object should rotate to.</param>
        /// <param name="duration">How long the rotation should take in seconds.</param>
        private void StartRotate(Quaternion rotation, float duration)
        {
            _rotateFrom = transform.rotation;
            _rotateTo = rotation;
            _rotateToDuration = duration;
            _elapsedRotateToTime = 0;
            _doRotate = true;
        }

        /// <summary>
        /// Ends the current move immediately, regardless on progress. It is always safe to call this.
        /// </summary>
        public void CancelMove()
        {
            OnMoveComplete(EMoveCompleteCallbackType.EndedEarly);
        }

        /// <summary>
        /// Called when a movement has completed.
        /// </summary>
        /// <param name="reason">The reason why the move has completed.</param>
        private void OnMoveComplete(EMoveCompleteCallbackType reason)
        {
            _doMove = false;
            moveCompleteCallback?.Invoke(reason);
            moveCompleteCallback = null;
        }

        // Rotate
        /// <summary>
        /// Rotates this object to the designated rotation over time.
        /// </summary>
        /// <param name="rotation">The world rotation the object should rotate to.</param>
        /// <param name="duration">How long the rotation should take in seconds.</param>
        public void RotateTo(Quaternion rotation, float duration)
        {
            //End last rotate, if any
            CancelRotate();

            // Begin new rotate
            StartRotate(rotation, duration);
        }

        /// <summary>
        /// Rotates this object to the designated rotation over time.
        /// </summary>
        /// <param name="rotation">The world rotation the object should rotate to.</param>
        /// <param name="duration">How long the rotation should take in seconds.</param>
        /// <param name="callback">The function we should call on completing the rotation.</param>
        public void RotateTo(Quaternion rotation, float duration,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            //End last rotate, if any
            CancelMove();

            //Callback
            rotateCompleteCallback = callback;

            RotateTo(rotation, duration);
        }

        /// <summary>
        /// Ends the current rotation immediately, regardless on progress. It is always safe to call this.
        /// </summary>
        public void CancelRotate()
        {
            OnRotateComplete(EMoveCompleteCallbackType.EndedEarly);
        }

        /// <summary>
        /// Called when a rotation has completed.
        /// </summary>
        /// <param name="reason">The reason why the rotation has completed.</param>
        protected void OnRotateComplete(EMoveCompleteCallbackType reason)
        {
            _doRotate = false;
            rotateCompleteCallback?.Invoke(reason);
            rotateCompleteCallback = null;
        }
    }
}