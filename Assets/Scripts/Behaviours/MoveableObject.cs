using System.Collections;
using UnityEngine;

namespace Behaviours
{
    public class MoveableObject : MonoBehaviour
    {
        private IEnumerator _currentSmoothLerpCoroutine;

        protected void MoveToLocation(Vector3 location, float duration)
        {
            if (_currentSmoothLerpCoroutine != null)
            {
                StopCoroutine(_currentSmoothLerpCoroutine);
            }

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
        }
    }
}