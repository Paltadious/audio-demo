using Core.Audios;
using FMOD.Studio;
using Project.UI.Elements;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    public class SettingsScreen : MonoBehaviour
    {
        public OnOffButton musicButton;
        public OnOffButton sfxButton;
        public Button soundTestButton1;
        public Button soundTestButton2;

        void Start()
        {
            musicButton.SetSelected(Audio.Instance.IsMusicEnabled);
            sfxButton.SetSelected(Audio.Instance.IsSFXEnabled);

            musicButton.Selected.SkipLatestValueOnSubscribe()
                .Subscribe(val => Audio.Instance.EnableMusic(val));
            sfxButton.Selected.SkipLatestValueOnSubscribe()
                .Subscribe(val => Audio.Instance.EnableSFX(val));
            soundTestButton1.onClick.AddListener(() => Audio.Instance.PlaySound(SoundType.TestSound, true));
            soundTestButton2.onClick.AddListener(() => Audio.Instance.PlaySound(SoundType.TestSound2, true));
        }
    }
}