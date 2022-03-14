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
    }
}