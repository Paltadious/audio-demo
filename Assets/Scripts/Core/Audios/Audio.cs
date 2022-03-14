using System;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Core.Other;
using Core.Other.CSharpExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Core.Audios
{
    public class Audio : SingletonBehaviour<Audio>
    {
        [SerializeField]
        [ListDrawerSettings(Expanded = true, AlwaysAddDefaultValue = true, ShowPaging = false)]
        [TableList]
        FmodEvent[] events;

        const string musicVcaPath = "vca:/Music";
        const string sfxVcaPath = "vca:/SFX";

        const string isMusicEnabled = "isMusicEnabled";
        const string isSFXEnabled = "isSFXEnabled";

        const string AudioLog = "[Audio]";

        VCA musicVca;
        VCA sfxVca;

        DictionaryDisposable<EventInstance, IDisposable> stopSoundSubs =
            new DictionaryDisposable<EventInstance, IDisposable>();

        TweenerCore<float, float, FloatOptions> musicAppearingTween;
        List<IntPtr> musicFadingEvents = new List<IntPtr>();

        static Action mainThreadCallback;

        public bool IsMusicEnabled => PlayerPrefs.GetInt(isMusicEnabled, 1) == 1;
        public bool IsSFXEnabled => PlayerPrefs.GetInt(isSFXEnabled, 1) == 1;

        public float CurrentMusicVolume
        {
            get
            {
                RESULT result = musicVca.getVolume(out float resultVolume);
                Debug.Assert(result == RESULT.OK, $"{AudioLog}: can't get CurrentMusicVolume: {result}");
                return resultVolume;
            }
        }

        public float CurrentSfxVolume
        {
            get
            {
                RESULT result = sfxVca.getVolume(out float resultVolume);
                Debug.Assert(result == RESULT.OK, $"{AudioLog}: can't get CurrentAmbientVolume: {result}");
                return resultVolume;
            }
        }

        //=============[ Lifecycle ]====================================================================================


        void Update()
        {
            mainThreadCallback?.Invoke();
            mainThreadCallback = null;
        }

        public void Initialize()
        {
            musicVca = RuntimeManager.GetVCA(musicVcaPath);
            sfxVca = RuntimeManager.GetVCA(sfxVcaPath);

            SetMusicVolume(1);
            SetSFXVolume(1);

            EnableMusic(IsMusicEnabled);
            EnableSFX(IsSFXEnabled);
        }

        void OnDisable()
        {
            stopSoundSubs.Dispose();
        }

        //=============[ Volume Control ]===============================================================================

        // "UI" and "World" VSAs are chidren of SFX VCA
        // VCA stands for Voltage Controlled Amplifier and is used to control volume

        public void EnableMusic(bool enable)
        {
            SetMusicVolume(enable ? 1 : 0);
            PlayerPrefs.SetInt(isMusicEnabled, enable ? 1 : 0);
        }

        public void EnableSFX(bool enable)
        {
            if (enable)
                SetSFXVolume(1);
            else
                Observable.Timer(TimeSpan.FromSeconds(0.2f)) //delay to play button_click sound before muting it
                    .Subscribe(_ => SetSFXVolume(0));

            PlayerPrefs.SetInt(isSFXEnabled, enable ? 1 : 0);
        }

        public void SetMusicVolume(float value) => musicVca.setVolume(Mathf.Clamp(value, 0, 1));
        public void SetSFXVolume(float value) => sfxVca.setVolume(Mathf.Clamp(value, 0, 1));

        public void IncreaseMusicVolume(float valueToAdd) => ClampedIncreaseVolume(musicVca, valueToAdd);
        public void IncreaseSfxVolume(float valueToAdd) => ClampedIncreaseVolume(sfxVca, valueToAdd);

        void ClampedIncreaseVolume(VCA target, float value)
        {
            target.getVolume(out float currentVolume);
            float targetVolume = currentVolume + value;
            float clampedTargetVolume = Mathf.Clamp(targetVolume, 0, 1);
            RESULT result = target.setVolume(clampedTargetVolume);

            Debug.Assert(result == RESULT.OK, $"{AudioLog}: ClampedIncreaseVolume: {result}");
        }

        //=============[ Sounds ]=======================================================================================

        FmodEvent GetEvent(SoundType soundType)
        {
            for (int i = 0; i < events.Length; i++)
            {
                if (events[i].SoundType == soundType)
                {
                    return events[i];
                }
            }

            Debug.LogError($"{AudioLog}: event wasn't found.");
            return null;
        }

        public EventInstance? PlaySound(SoundType soundType, bool fadeMusic = false)
        {
            FmodEvent fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            return PlaySound(fmodEvent.Instance.Value, fadeMusic);
        }

        public EventInstance? PlaySound(SoundType soundType, TimeSpan duration,
            STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT, bool fadeMusic = false)
        {
            EventInstance? instance = PlaySound(soundType, fadeMusic);

            if (instance == null)
                return null;

            stopSoundSubs.Add((EventInstance) instance,
                Observable.Timer(duration).Subscribe(_ => StopSound(soundType, stopMode)));

            return instance;
        }

        public EventInstance PlaySound(EventInstance instance, bool fadeMusic = false)
        {
            if (fadeMusic)
            {
                var callback = new EVENT_CALLBACK(FadeMusicCallback);
                instance.setCallback(callback, EVENT_CALLBACK_TYPE.SOUND_PLAYED | EVENT_CALLBACK_TYPE.SOUND_STOPPED);
            }

            instance.start();
            stopSoundSubs.Remove(instance);
            return instance;
        }

        public EventInstance? StopSound(SoundType soundType, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.Instance.Value.stop(stopMode);
            return fmodEvent.Instance;
        }

        public EventInstance? StopSoundImmediately(SoundType soundType) =>
            StopSound(soundType, STOP_MODE.IMMEDIATE);

        public EventInstance? PauseSound(SoundType soundType, bool pause = true)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.Instance.Value.setPaused(pause);

            return fmodEvent.Instance;
        }

        public void MuteSound(SoundType soundType, bool isMute)
        {
            GetEvent(soundType)?.Instance.Value.setVolume(isMute ? 0 : 1);
        }

        //Catch callback from FMOD unmanaged code which runs on the other thread and pass it to main thread 
        [MonoPInvokeCallback(typeof(EVENT_CALLBACK_TYPE))]
        static RESULT FadeMusicCallback(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
        {
            mainThreadCallback = () => Instance.FadeMusic(type, _event, parameters);
            return RESULT.OK;
        }

        void FadeMusic(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
        {
            if (!IsMusicEnabled)
                return;
            
            switch (type)
            {
                case EVENT_CALLBACK_TYPE.SOUND_PLAYED:
                    if (!IsSFXEnabled)
                        break;

                    // many calls from the same event can come in the same time,
                    // so to avoid duplicates add only unique events
                    if (!musicFadingEvents.Contains(_event))
                        musicFadingEvents.Add(_event);

                    musicAppearingTween?.Kill();
                    SetMusicVolume(0);
                    break;
                case EVENT_CALLBACK_TYPE.SOUND_STOPPED:
                    //remove event and also possible duplicates, just in case
                    while (musicFadingEvents.Contains(_event))
                        musicFadingEvents.Remove(_event);

                    if (musicFadingEvents.Count != 0)
                        break;

                    musicAppearingTween?.Kill();
                    musicAppearingTween = DOTween.To(() => CurrentMusicVolume, SetMusicVolume, 1, 5f);
                    break;
            }
        }

        //=============[ Parameters ]===================================================================================

        // example of work with parameters
        // public void SetZoomFactor(float rate)
        // {
        //     Debug.Assert(rate >= 0 && rate <= 1);
        //     SetFmodParameter(zoomFactorParamDescription, rate);
        // }

        void SetFmodParameter(PARAMETER_DESCRIPTION description, float value)
        {
            float a;
            RuntimeManager.StudioSystem.getParameterByID(description.id, out a);
            RESULT result = RuntimeManager.StudioSystem.setParameterByID(description.id, value);
            Debug.Assert(result == RESULT.OK, $"{AudioLog}: failed to set parameter {description}: result = {result}");
        }

        //=============[ Helpers ]======================================================================================

        [Button]
        void WhatIsNotHere()
        {
            List<SoundType> soundTypes = EnumExtensions.GetEnumValues<SoundType>().ToList();
            soundTypes.Remove(SoundType.None);
            for (int i = 0;
                i < soundTypes.Count;
                i++)
            {
                if (events.Count(ev => ev.SoundType == soundTypes[i]) > 1)
                    Debug.LogError($"{AudioLog}: more then one object of type {soundTypes[i]}");
            }

            for (int i = 0;
                i < events.Length;
                i++)
            {
                if (events[i] == null)
                    continue;

                soundTypes.Remove(events[i].SoundType);
            }

            if (soundTypes.Count == 0)
            {
                Debug.Log($"{AudioLog}: everything is set up.");
                return;
            }

            string result = $"{AudioLog}: lack of ";
            for (int i = 0;
                i < soundTypes.Count;
                i++)
            {
                if (i > 0)
                    result += ", ";

                result += soundTypes[i];
            }

            Debug.Log(result);
        }
    }
}