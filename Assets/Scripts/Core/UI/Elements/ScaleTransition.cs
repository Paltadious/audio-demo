using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Core.UI.Elements
{
    public class ScaleTransition : TransitionHandler
    {
        public RectTransform target;
        public Vector2 pressedScale = new Vector2(0.95f, 0.95f);
        public float transitionDuration = 0.1f;

        public override void OnPressed()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                target.localScale = pressedScale;
                return;
            }
#endif
            target.DOScale(new Vector3(pressedScale.x, pressedScale.y, 1f), transitionDuration);
        }

        public override void OnNormal()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                target.localScale = Vector3.one;
                return;
            }
#endif
            target.DOScale(Vector3.one, transitionDuration);
        }

        public override void OnDisabled()
        {
        }
    }
}