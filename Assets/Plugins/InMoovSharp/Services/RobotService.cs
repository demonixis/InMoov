using Demonixis.InMoovSharp.Settings;
using System;
using System.Collections;
using System.Collections.Generic;

#if INMOOV_UNITY
using UnityEngine;
#endif

namespace Demonixis.InMoovSharp.Services
{
    /// <summary>
    /// Base skeleton of a robot service.
    /// A service must have a type. By default it is supported on all platforms.
    /// </summary>
    public abstract class RobotService : IDisposable
    {
        public virtual RuntimePlatform[] SupportedPlateforms => new[]
        {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer,
            RuntimePlatform.LinuxPlayer,
            RuntimePlatform.LinuxEditor,
            RuntimePlatform.WindowsPlayer,
            RuntimePlatform.WindowsEditor,
            RuntimePlatform.OSXPlayer,
            RuntimePlatform.OSXEditor
        };

        protected Dictionary<string, string> _customSettings;

        public string ServiceName => GetType().Name;
        public virtual string SerializationFilename { get; } = string.Empty;
        public bool Started { get; protected set; }
        public bool Paused { get; protected set; }

        public RobotService()
        {
            _customSettings = new Dictionary<string, string>();
        }

        public virtual void Initialize()
        {
            Robot.Instance.WhenStarted(InternalInitialize);
        }

        /// <summary>
        /// Initialize the service if not already initialized.
        /// </summary>
        /// <returns>Returns false if already started, otherwise it returns true</returns>
        protected virtual void InternalInitialize()
        {
            if (Started) return;

            if (!string.IsNullOrEmpty(SerializationFilename))
                _customSettings = SaveGame.LoadData<Dictionary<string, string>>(SerializationFilename, "Config");

            if (_customSettings == null)
                _customSettings = new Dictionary<string, string>();

            SafeInitialize();

            Started = true;
        }

        protected virtual void SafeInitialize()
        {
        }

        public virtual void SetPaused(bool paused)
        {
            Paused = paused;
        }

        public virtual void Dispose()
        {
            Started = false;
        }

        public bool IsSupported()
        {
            return Array.IndexOf(SupportedPlateforms, Application.platform) > -1;
        }

        protected void StartCoroutine(IEnumerator coroutine)
        {
            Robot.Instance.CoroutineManager.Start(this, coroutine);
        }

        protected void StopAllCoroutines()
        {
            Robot.Instance.CoroutineManager.StopAll(this);
        }

        protected void AddSetting(string key, string value)
        {
            if (!_customSettings.ContainsKey(key))
                _customSettings.Add(key, value);
            else
                _customSettings[key] = value;
        }

        protected void RemoveSetting(string key)
        {
            if (_customSettings.ContainsKey(key))
                _customSettings.Remove(key);
        }
    }
}