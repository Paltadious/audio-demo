using UnityEngine.UI;

namespace Project.Ui.Elements
{
    public class TransitionButton : Button
    {
        public TransitionHandler[] transitionHandlers;

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            switch (state)
            {
                case SelectionState.Normal:
                    for (int i = 0; i < transitionHandlers.Length; i++)
                    {
                        transitionHandlers[i].OnNormal();
                    }

                    break;
                case SelectionState.Highlighted:
                    break;
                case SelectionState.Pressed:
                    for (int i = 0; i < transitionHandlers.Length; i++)
                    {
                        transitionHandlers[i].OnPressed();
                    }

                    break;
                case SelectionState.Selected:
                    for (int i = 0; i < transitionHandlers.Length; i++)
                    {
                        transitionHandlers[i].OnNormal();
                    }

                    break;
                case SelectionState.Disabled:
                    for (int i = 0; i < transitionHandlers.Length; i++)
                    {
                        transitionHandlers[i].OnDisabled();
                    }

                    break;
                default:
                    break;
            }
        }
    }
}