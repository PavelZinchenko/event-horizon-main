using GameDatabase.Model;
using UnityEngine;

namespace Services.Reources
{
    public interface IResourceLocator
    {
        AudioClip GetAudioClip(AudioClipId id);
        Sprite GetSprite(SpriteId sprite);
        Sprite GetSprite(string name);
    }

}
