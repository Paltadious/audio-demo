using Project.Ui.Elements;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI.Elements
{
    public class OnOffButton : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] bool selected;

        [ListDrawerSettings(Expanded = true, AlwaysAddDefaultValue = false)]
        [SerializeField] TransitionHandler[] transitionHandlers;

        public ReadOnlyReactiveProperty<bool> Selected => new ReadOnlyReactiveProperty<bool>(button
                .OnClickAsObservable().Select(_ =>
                {
                    SetSelected(!selected);
                    return selected;
                })
            , selected, false);

        public void SetSelected(bool selected)
        {
            this.selected = selected;

            if (selected)
            {
                for (int i = 0; i < transitionHandlers.Length; i++)
                {
                    transitionHandlers[i].OnNormal();
                }
            }
            else
            {
                for (int i = 0; i < transitionHandlers.Length; i++)
                {
                    transitionHandlers[i].OnDisabled();
                }
            }
        }
    }
}