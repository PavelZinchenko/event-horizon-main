using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombatView
{
    // The sound output of the combat scene (migration phase 5b, owner question 12 (a)): the adapter implements it on
    // ISoundPlayer, so this module needs no database types. Sound ID 0 is an anonymous sound. Every sound plays at one
    // volume, wherever it happens, as in legacy.
    public interface ISoundOutput
    {
        void Play(AudioClip clip, int soundId, bool loop);
        void Stop(int soundId);
    }

    // A clip with its loop flag; the adapter resolves both from the clip name (legacy AudioClipId: a '*' loops).
    public readonly struct SoundClip
    {
        public SoundClip(AudioClip clip, bool loop)
        {
            Clip = clip;
            Loop = loop;
        }

        public readonly AudioClip Clip;
        public readonly bool Loop;
        public bool IsEmpty => !Clip;
    }

    // The sounds of one battle. A sound that its owner stops later (a loop, or a sound that ends with its bullet) gets
    // an ID of its own, so it never stops another; Dispose stops every one still running.
    public sealed class CombatSounds : IDisposable
    {
        public CombatSounds(ISoundOutput output)
        {
            _output = output;
        }

        public void Play(SoundClip sound)
        {
            if (_output != null && !sound.IsEmpty)
                _output.Play(sound.Clip, 0, sound.Loop);
        }

        // 0 without a clip or an output. Stop takes the ID.
        public int Start(SoundClip sound)
        {
            if (_output == null || sound.IsEmpty)
                return 0;

            var id = _nextId++;
            _running.Add(id);
            _output.Play(sound.Clip, id, sound.Loop);
            return id;
        }

        public void Stop(int id)
        {
            if (_running.Remove(id))
                _output.Stop(id);
        }

        public void Dispose()
        {
            foreach (var id in _running)
                _output.Stop(id);
            _running.Clear();
        }

        // Legacy combat uses object hash codes as IDs; it never runs together with this scene.
        private const int FirstId = 0x43560000;

        private readonly ISoundOutput _output;
        private readonly HashSet<int> _running = new HashSet<int>();
        private int _nextId = FirstId;
    }
}
