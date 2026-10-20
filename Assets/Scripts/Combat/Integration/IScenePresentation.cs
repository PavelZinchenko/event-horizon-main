using UnityEngine;

namespace Combat.Integration
{
    // Camera framing and map data for the camera and radar. Shared by combat and exploration scenes.
    public interface IScenePresentation
    {
        // The point the camera follows, in Unity units.
        Vector2 ViewPoint { get; }

        // The area the camera must show.
        Rect ViewRect { get; }

        float MapWidth { get; }
        float MapHeight { get; }
    }
}
