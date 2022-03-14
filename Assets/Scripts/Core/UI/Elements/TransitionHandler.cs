using UnityEngine;

namespace Project.Ui.Elements
{
    public abstract class TransitionHandler : MonoBehaviour
    {
        public abstract void OnNormal();
        public abstract void OnPressed();
        public abstract void OnDisabled();
    }
}