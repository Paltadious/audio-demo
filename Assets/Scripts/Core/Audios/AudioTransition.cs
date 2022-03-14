using FMODUnity;
using Project.Ui.Elements;
using UnityEngine;

namespace Project.Audios
{
    public class AudioTransition : TransitionHandler
    {
        [SerializeField] EventReference[] onPressedSounds;
        [SerializeField] EventReference[] onNormalSounds;
        [SerializeField] EventReference[] onDisabledSounds;

        public override void OnPressed()
        {
            foreach (EventReference sound in onPressedSounds)
            {
                RuntimeManager.PlayOneShot(sound);
            }
        }

        public override void OnNormal()
        {
            foreach (EventReference sound in onNormalSounds)
            {
                RuntimeManager.PlayOneShot(sound);
            }
        }

        public override void OnDisabled()
        {
            foreach (EventReference sound in onDisabledSounds)
            {
                RuntimeManager.PlayOneShot(sound);
            }
        }
    }
}