using Core.UI.Elements;
using FMODUnity;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Audios
{
    public class AudioClickHandler : MonoBehaviour
    {
        [InfoBox("Sound will be played when clicking on this object.")]
        [SerializeField]
        GameObject clickableSource;

        [SerializeField]
        EventReference sound;

        void Start()
        {
            IClickable clickable = clickableSource.GetComponent<IClickable>();
            if (clickable != null)
            {
                clickable.OnClick().Subscribe(_ => OnClick());
                return;
            }

            Button button = clickableSource.GetComponent<Button>();
            if (button == null)
                return;

            button.onClick.AddListener(OnClick);
        }

        void OnClick() => RuntimeManager.PlayOneShot(sound);
    }
}