using UnityEngine;

namespace CustomCamera
{
    public abstract class CameraPoint : MonoBehaviour
    {
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //Draw our axis
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.up);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.right);
            
            //Draw a central sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
#endif
    }
}