using System;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Core.Other;
using Core.Other.CSharpExtensions;
using DG.Tweening;
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
        const string uiVcaPath = "vca:/UI";
        const string worldVcaPath = "vca:/World";

        const string isMusicDisabled = "isMusicDisabled";
        const string isSFXDisabled = "isSFXDisabled";

        const string AudioLog = "[Audio]";

        VCA musicVca;
        VCA sfxVca;
        VCA uiVca;
        VCA worldVca;

        DictionaryDisposable<EventInstance, IDisposable> stopSoundSubs =
            new DictionaryDisposable<EventInstance, IDisposable>();

        IDisposable incomeMessageSub;

        public bool IsMusicEnabled
        {
            get => PlayerPrefs.GetInt(isMusicDisabled) == 0;
            set
            {
                SetMusicVolume(value ? 1 : 0);
                PlayerPrefs.SetInt(isMusicDisabled, value ? 0 : 1);
                PlayerPrefs.Save();
            }
        }

        public bool IsSFXEnabled
        {
            get => PlayerPrefs.GetInt(isSFXDisabled) == 0;
            set
            {
                if (value)
                    SetSFXVolume(1);
                else
                    Observable.Timer(TimeSpan.FromSeconds(0.2f)) //delay to play button_click sound before muting it
                        .Subscribe(_ => SetSFXVolume(0));

                PlayerPrefs.SetInt(isSFXDisabled, value ? 0 : 1);
                PlayerPrefs.Save();
            }
        }
        //=============[ Lifecycle ]=====================================================================================

        protected override void Awake()
        {
            base.Awake();
            enabled = false; //deactivate Update() loop
            Debug.Log($"{AudioLog}: audio has awaken.");
        }

        public void Initialize()
        {
            //volume control
            musicVca = RuntimeManager.GetVCA(musicVcaPath);
            sfxVca = RuntimeManager.GetVCA(sfxVcaPath);
            // uiVca = RuntimeManager.GetVCA(uiVcaPath);
            // worldVca = RuntimeManager.GetVCA(worldVcaPath);
            SetMusicVolume(1);
            SetSFXVolume(1);
            // SetUiVolume(1);
            // SetWorldVolume(1);

            IsMusicEnabled = IsMusicEnabled;
            IsSFXEnabled = IsSFXEnabled;

            PlaySound(SoundType.MainMusicTheme);
        }

        public void OnAppInited()
        {
            enabled = true; //activate Update() loop
        }
        
        void Update()
        {
            // for 3D sounds - FMOD StudioListener position is being set at the point on plane where the camera looks 
            // gameObject.transform.position = App.CameraController.PointInPlane;
        }

        void OnDisable()
        {
            stopSoundSubs.Dispose();
        }

        //=============[ Volume Control ]===============================================================================

        //"UI" and "World" VSAs are chidren of SFX VCA
        //VCA stands for Voltage Controlled Amplifier and is used to control volume

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

        public float CurrentUiVolume
        {
            get
            {
                RESULT result = uiVca.getVolume(out float resultVolume);
                Debug.Assert(result == RESULT.OK, $"{AudioLog}: can't get CurrentUiVolume: {result}");
                return resultVolume;
            }
        }
        
        public float CurrentWorldVolume
        {
            get
            {
                RESULT result = worldVca.getVolume(out float resultVolume);
                Debug.Assert(result == RESULT.OK, $"{AudioLog}: can't get CurrentWorldVolume: {result}");
                return resultVolume;
            }
        }

        public void SetMusicVolume(float value) => musicVca.setVolume(Mathf.Clamp(value, 0, 1));
        public void SetSFXVolume(float value) => sfxVca.setVolume(Mathf.Clamp(value, 0, 1));
        public void SetUiVolume(float value) => uiVca.setVolume(Mathf.Clamp(value, 0, 1));
        public void SetWorldVolume(float value) => worldVca.setVolume(Mathf.Clamp(value, 0, 1));


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

            Debug.LogError($"{AudioLog}: event wasn't found");
            return null;
        }

        public EventInstance? PlaySound(SoundType soundType)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.PlaySound();
            stopSoundSubs.Remove(fmodEvent.Instance.Value);
            return fmodEvent.Instance;
        }

        public EventInstance PlaySound(EventInstance instance)
        {
            instance.start();
            stopSoundSubs.Remove(instance);
            return instance;
        }

        public EventInstance? PlaySound(SoundType soundType, TimeSpan duration,
            STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.PlaySound();
            stopSoundSubs.Add(fmodEvent.Instance.Value,
                Observable.Timer(duration).Subscribe(_ => StopSound(soundType, stopMode)));
            return fmodEvent.Instance;
        }

        public EventInstance? StopSound(SoundType soundType, STOP_MODE stopMode = STOP_MODE.ALLOWFADEOUT)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.StopSound(stopMode);

            return fmodEvent.Instance;
        }

        public EventInstance? StopSoundImmediately(SoundType soundType) =>
            StopSound(soundType, STOP_MODE.IMMEDIATE);

        public EventInstance? PauseSound(SoundType soundType, bool pause = true)
        {
            var fmodEvent = GetEvent(soundType);

            if (fmodEvent == null)
                return null;

            fmodEvent.PauseSound(pause);

            return fmodEvent.Instance;
        }

        public void MuteSound(SoundType soundType, bool isMute)
        {
            GetEvent(soundType)?.Mute(isMute);
        }

//example of music fading when some sound plays
// var callback = new EVENT_CALLBACK(StaticFadeMusic);
// achievement3Instance.setCallback(callback,
//     EVENT_CALLBACK_TYPE.SOUND_PLAYED | EVENT_CALLBACK_TYPE.SOUND_STOPPED);

        [MonoPInvokeCallback(typeof(EVENT_CALLBACK_TYPE))]
        static RESULT StaticFadeMusic(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
        {
            Instance.FadeMusic(type, _event, parameters);
            return RESULT.OK;
        }

        void FadeMusic(EVENT_CALLBACK_TYPE type, IntPtr _event, IntPtr parameters)
        {
            Observable.Start(() => Unit.Default)
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    if (!IsMusicEnabled || !IsSFXEnabled) return; //Unity API can be used only in main thread

                    switch (type)
                    {
                        case EVENT_CALLBACK_TYPE.SOUND_PLAYED:
                            // DOTween.To(() => CurrentMusicVolume, SetMusicVolume, 0, 3f)
                            // .OnComplete(() => PauseSound(SoundType.Music));
                            SetMusicVolume(0);
                            // PauseSound(SoundType.Music);
                            break;
                        case EVENT_CALLBACK_TYPE.SOUND_STOPPED:
                            // PauseSound(SoundType.Music, false);
                            // SetMusicVolume(0);
                            DOTween.To(() => CurrentMusicVolume, SetMusicVolume, 1, 5f);
                            break;
                    }
                });
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

            for (int i = 0; i < events.Length; i++)
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