using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Locks project-authored captain and Three Body faction icons to Unity's
/// single-Sprite import path.  This is intentionally limited to generated
/// assets so it cannot change the original game's multi-sprite icon imports.
/// </summary>
internal sealed class ThreeBodyGeneratedIconImporter : AssetPostprocessor
{
    private const string CaptainPath = "Assets/Resources/Textures/UI/captain.png";
    private const string FactionDirectory = "Assets/Resources/Textures/Factions/";

    private void OnPreprocessTexture()
    {
        if (!IsManagedIcon(assetPath))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 512;
    }

    private static bool IsManagedIcon(string path)
    {
        if (string.Equals(path, CaptainPath, StringComparison.OrdinalIgnoreCase))
            return true;
        if (!path.StartsWith(FactionDirectory, StringComparison.OrdinalIgnoreCase) ||
            !path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            return false;

        var fileName = Path.GetFileNameWithoutExtension(path);
        if (!fileName.StartsWith("faction_", StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(fileName.Substring("faction_".Length), out var factionId))
            return false;
        return factionId >= 21 && factionId <= 28;
    }
}
