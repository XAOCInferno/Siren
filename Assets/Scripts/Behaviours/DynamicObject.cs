using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

namespace Behaviours
{
    public enum EMoveCompleteCallbackType
    {
        EndedEarly = 0,
        Completed,
    }

    public class DynamicObject : MonoBehaviour
    {
        private IEnumerator _currentSmoothRotateCoroutine;
        private IEnumerator _currentSmoothMoveCoroutine;

        protected Func<EMoveCompleteCallbackType, int> moveCompleteCallback;
        protected Func<EMoveCompleteCallbackType, int> rotateCompleteCallback;

        // Move
        public void MoveTo(Vector3 location, float duration)
        {
            //End last move, if any
            CancelMove();

            //Kickoff coroutine
            _currentSmoothMoveCoroutine = SmoothMove(location, duration);
            StartCoroutine(_currentSmoothMoveCoroutine);
        }

        public void MoveTo(Vector3 location, float duration,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            //End last move, if any
            CancelMove();

            //Callback
            moveCompleteCallback = callback;

            //Kickoff coroutine
            _currentSmoothMoveCoroutine = SmoothMove(location, duration);
            StartCoroutine(_currentSmoothMoveCoroutine);
        }

        private IEnumerator SmoothMove(Vector3 toLocation, float time)
        {
            Vector3 startingPos = transform.position;

            float elapsedTime = 0;

            while (elapsedTime < time)
            {
                transform.position = Vector3.Lerp(startingPos, toLocation, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _currentSmoothMoveCoroutine = null;
            OnMoveComplete(EMoveCompleteCallbackType.Completed);
        }

        public void CancelMove()
        {
            //End coroutine
            if (_currentSmoothMoveCoroutine != null)
            {
                StopCoroutine(_currentSmoothMoveCoroutine);
            }

            //Call callback
            OnMoveComplete(EMoveCompleteCallbackType.EndedEarly);
        }

        protected void OnMoveComplete(EMoveCompleteCallbackType reason)
        {
            moveCompleteCallback?.Invoke(reason);
            moveCompleteCallback = null;
        }

        // Rotate
        public void RotateTo(Quaternion rotation, float duration)
        {
            //End last rotate, if any
            CancelRotate();

            //Kickoff coroutine
            _currentSmoothRotateCoroutine = SmoothRotate(rotation, duration);
            StartCoroutine(_currentSmoothRotateCoroutine);
        }

        public void RotateTo(Quaternion rotation, float duration,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            //End last rotate, if any
            CancelMove();

            //Callback
            rotateCompleteCallback = callback;

            //Kickoff coroutine
            _currentSmoothRotateCoroutine = SmoothRotate(rotation, duration);
            StartCoroutine(_currentSmoothRotateCoroutine);
        }

        private IEnumerator SmoothRotate(Quaternion toRotation, float time)
        {
            Quaternion startingRotation = transform.rotation;

            float elapsedTime = 0;

            while (elapsedTime < time)
            {
                transform.rotation = Quaternion.Lerp(startingRotation, toRotation, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _currentSmoothRotateCoroutine = null;
            OnRotateComplete(EMoveCompleteCallbackType.Completed);
        }

        public void CancelRotate()
        {
            //End coroutine
            if (_currentSmoothRotateCoroutine != null)
            {
                StopCoroutine(_currentSmoothRotateCoroutine);
            }

            //Call callback
            OnRotateComplete(EMoveCompleteCallbackType.EndedEarly);
        }

        protected void OnRotateComplete(EMoveCompleteCallbackType reason)
        {
            rotateCompleteCallback?.Invoke(reason);
            rotateCompleteCallback = null;
        }
    }
}