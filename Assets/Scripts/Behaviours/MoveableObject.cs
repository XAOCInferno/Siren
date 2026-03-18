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

    public abstract class MoveableObject : MonoBehaviour
    {
        private IEnumerator _currentSmoothLerpCoroutine;

        protected Func<EMoveCompleteCallbackType, int> moveCompleteCallback;

        protected void MoveToLocation(Vector3 location, float duration)
        {
            //End last move, if any
            CancelMove();

            //Kickoff coroutine
            _currentSmoothLerpCoroutine = SmoothLerp(location, duration);
            StartCoroutine(_currentSmoothLerpCoroutine);
        }

        protected void MoveToLocation(Vector3 location, float duration,
            [CanBeNull] Func<EMoveCompleteCallbackType, int> callback)
        {
            //End last move, if any
            CancelMove();

            //Callback
            moveCompleteCallback = callback;

            //Kickoff coroutine
            _currentSmoothLerpCoroutine = SmoothLerp(location, duration);
            StartCoroutine(_currentSmoothLerpCoroutine);
        }

        private IEnumerator SmoothLerp(Vector3 toLocation, float time)
        {
            Vector3 startingPos = transform.position;

            float elapsedTime = 0;

            while (elapsedTime < time)
            {
                transform.position = Vector3.Lerp(startingPos, toLocation, (elapsedTime / time));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _currentSmoothLerpCoroutine = null;
            OnMoveComplete();
        }

        protected void OnMoveComplete()
        {
            moveCompleteCallback?.Invoke(EMoveCompleteCallbackType.Completed);
            moveCompleteCallback = null;
        }

        protected void CancelMove()
        {
            //End coroutine
            if (_currentSmoothLerpCoroutine != null)
            {
                StopCoroutine(_currentSmoothLerpCoroutine);
            }

            //Call callback
            moveCompleteCallback?.Invoke(EMoveCompleteCallbackType.EndedEarly);
        }
    }
}