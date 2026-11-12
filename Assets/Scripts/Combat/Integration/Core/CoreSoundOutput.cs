using CombatView;
using GameDatabase.Model;
using Services.Audio;
using Services.Resources;
using UnityEngine;

namespace Combat.Integration.Core
{
    // The combat view's sound output on the game's ISoundPlayer (migration phase 5b, step 2, owner question 12 (a)).
    // It also resolves database clip names, as the adapter resolves sprites.
    public sealed class CoreSoundOutput : ISoundOutput
    {
        public CoreSoundOutput(ISoundPlayer soundPlayer, IResourceLocator resourceLocator)
        {
            _soundPlayer = soundPlayer;
            _resourceLocator = resourceLocator;
        }

        public void Play(AudioClip clip, int soundId, bool loop) => _soundPlayer.Play(clip, soundId, loop);
        public void Stop(int soundId) => _soundPlayer.Stop(soundId);

        // Empty for no name or a missing clip. Legacy AudioClipId: a '*' in the name loops the sound.
        public SoundClip Resolve(AudioClipId id) =>
            id ? new SoundClip(_resourceLocator.GetAudioClip(id), id.Loop) : default;

        private readonly ISoundPlayer _soundPlayer;
        private readonly IResourceLocator _resourceLocator;
    }
}
