//-------------------------------------------------------------------------------
//                                                                               
//    This code was automatically generated.                                     
//    Changes to this file may cause incorrect behavior and will be lost if      
//    the code is regenerated.                                                   
//                                                                               
//-------------------------------------------------------------------------------

using GameDatabase.Model;

namespace GameDatabase.Storage
{
    public interface IJsonSerializer
    {
        T FromJson<T>(string data);
        string ToJson<T>(T item);
    }

    public partial interface IDataStorage
    {
        void LoadContent(IContentLoader loader);
    }

    public interface IContentLoader
    {
        void LoadJson(string name, string data);
        void LoadLocalization(string name, string data);
        void LoadImage(string name, IImageData image);
        void LoadAudioClip(string name, IAudioClipData audioClip);
    }
}
