/// ---------------------------------------------
/// Opsive Shared
/// Copyright (c) Opsive. All Rights Reserved.
/// https://www.opsive.com
/// ---------------------------------------------

namespace Opsive.Shared.Integrations.MasterAudio
{
    using Opsive.Shared.Audio;
    using UnityEngine;

    /// <summary>
    /// Implements the AudioConfig for the Master Audio integration.
    /// </summary>
    [CreateAssetMenu(fileName = "MasterAudioConfig", menuName = "Opsive/Audio/Master Audio Config", order = 3)]

    public class MasterAudioConfig : AudioConfig
    {
        [Tooltip("The name of the Master Audio Group.")]
        [DarkTonic.MasterAudio.SoundGroupAttribute] [SerializeField] protected string m_AudioGroupName;

        public string AudioGroupName { get { return m_AudioGroupName; } set { m_AudioGroupName = value; } }
    }
}