using UnityEditor;
using UnityEngine;

namespace Zombtoy.Editor
{
    // Import settings live outside the vendor folder and are reproducible on clone.
    public sealed class KenneyImportSettings : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/ThirdParty/Kenney/")) return;
            var model = (ModelImporter)assetImporter;
            model.importCameras = false;
            model.importLights = false;
            model.addCollider = false;
            model.materialImportMode = ModelImporterMaterialImportMode.None;
            if (assetPath.Contains("/Survivors/"))
                model.animationType = ModelImporterAnimationType.Generic;
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/ThirdParty/Kenney/")) return;
            var texture = (TextureImporter)assetImporter;
            texture.filterMode = FilterMode.Point;
            texture.mipmapEnabled = false;
            texture.textureCompression = TextureImporterCompression.Uncompressed;
        }

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/ThirdParty/Kenney/")) return;
            var audio = (AudioImporter)assetImporter;
            audio.forceToMono = true;
        }
    }
}
