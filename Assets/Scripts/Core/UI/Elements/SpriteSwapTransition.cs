using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.Elements
{
    public class SpriteSwapTransition : TransitionHandler
    {
        public Image target;
        public Sprite normal;
        public Sprite pressed;
        public Sprite disabled;

        public override void OnPressed()
        {
            if (pressed != null)
            {
                target.sprite = pressed;
            }
        }

        public override void OnNormal()
        {
            if (normal != null)
            {
                target.sprite = normal;
            }
        }

        public override void OnDisabled()
        {
            if (disabled != null)
            {
                target.sprite = disabled;
            }
        }
    }
}