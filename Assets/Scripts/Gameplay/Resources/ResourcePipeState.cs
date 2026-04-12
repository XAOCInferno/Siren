using UnityEngine;
using Utils.StateMachine;

namespace Gameplay.Resources
{
    public enum EResourcePipeState
    {
        Offline = 0,
        Powered,
        PreviewingCost,
        PreviewingGain,
    }

    public class ResourcePipeState : MonoBehaviour
    {
        /// <summary>
        /// The value of resource this represents. EG if this is 1 it is active when player has >=1 energy
        /// </summary>
        [SerializeField] protected int representedResourceValue = 1;

        /// <summary>
        /// The resource this represents.
        /// </summary>
        [SerializeField] protected EResourceType representedResourceType = EResourceType.Energy;

        [SerializeField] protected bool representsLocalPlayer = true;

        protected EnumStateMachine<EResourcePipeState> stateMachine = new();

        public EnumStateMachine<EResourcePipeState> GetStateMachine() => stateMachine;
        public int GetRepresentedResourceValue() => representedResourceValue;
        public EResourceType GetRepresentedResourceType() => representedResourceType;
        public bool GetRepresentsLocalPlayer() => representsLocalPlayer;
    }
}