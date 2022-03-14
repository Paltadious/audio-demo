using UnityEngine;

namespace Core.UI.Elements
{
    public class TransitionHandler : MonoBehaviour
    {
        public virtual void OnNormal(){}
        public virtual void OnPressed(){}
        public virtual void OnDisabled(){}
    }
}