using System;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Audios
{
    [Serializable]
    public class FmodEvent
    {
        [SerializeField]
        EventReference eventRef;

        [SerializeField]
        SoundType soundType;

        [Searchable]
        public SoundType SoundType => soundType;

        [NonSerialized]
        EventInstance? instance;

        public EventInstance? Instance => instance ?? (instance = RuntimeManager.CreateInstance(eventRef));

        public EventInstance? PlaySound()
        {
            Instance.Value.start();
            return Instance;
        }

        public EventInstance? StopSound(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.IMMEDIATE)
        {
            Instance.Value.stop(stopMode);
            return Instance;
        }

        public EventInstance? PauseSound(bool pause = true)
        {
            Instance.Value.setPaused(pause);
            return Instance;
        }

        public void Mute(bool isMute)
        {
            Instance.Value.setVolume(isMute ? 0 : 1);
        }
    }
}