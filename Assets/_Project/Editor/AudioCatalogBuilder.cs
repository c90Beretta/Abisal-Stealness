using System.IO;
using UnityEditor;
using UnityEngine;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    [InitializeOnLoad]
    public static class AudioCatalogBuilder
    {
        private const string CatalogAssetPath = "Assets/_Project/Resources/TacticalAudioCatalog.asset";

        static AudioCatalogBuilder()
        {
            EditorApplication.delayCall += EnsureCatalogUpToDate;
        }

        [MenuItem("Abisal/Actualizar catalogo de audio SFX")]
        public static void BuildCatalogMenu()
        {
            BuildOrUpdateCatalog(forceLog: true);
        }

        private static void EnsureCatalogUpToDate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var existing = AssetDatabase.LoadAssetAtPath<TacticalAudioCatalog>(CatalogAssetPath);
            if (existing != null && existing.version >= TacticalAudioCatalog.CurrentVersion && existing.IsComplete)
                return;

            BuildOrUpdateCatalog(forceLog: false);
        }

        public static void BuildOrUpdateCatalog(bool forceLog)
        {
            string dir = Path.GetDirectoryName(CatalogAssetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var catalog = AssetDatabase.LoadAssetAtPath<TacticalAudioCatalog>(CatalogAssetPath);
            bool created = false;
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TacticalAudioCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
                created = true;
            }

            catalog.airVentLoop = LoadClip(TacticalAudioCatalog.AirVentPath);
            catalog.aggressiveGrowlLoop = LoadClip(TacticalAudioCatalog.AggressiveGrowlPath);
            catalog.enemyAlertMonster = LoadClip(TacticalAudioCatalog.EnemyAlertMonsterPath);
            catalog.enemyAttackSwipe = LoadClip(TacticalAudioCatalog.EnemyAttackSwipePath);
            catalog.enemyDeathGurgle = LoadClip(TacticalAudioCatalog.EnemyDeathGurglePath);
            catalog.enemyHitImpactFlesh = LoadClip(TacticalAudioCatalog.EnemyHitImpactFleshPath);
            catalog.playerAttackWhoosh = LoadClip(TacticalAudioCatalog.PlayerAttackWhooshPath);
            catalog.playerDeathBodyFall = LoadClip(TacticalAudioCatalog.PlayerDeathBodyFallPath);
            catalog.playerInteract = LoadClip(TacticalAudioCatalog.PlayerInteractPath);
            catalog.playerTakeDamage = LoadClip(TacticalAudioCatalog.PlayerTakeDamagePath);
            catalog.footstepsConcrete = LoadClip(TacticalAudioCatalog.FootstepsConcretePath);
            catalog.footstepsRunning = LoadClip(TacticalAudioCatalog.FootstepsRunningPath);
            catalog.footstepsLimping = LoadClip(TacticalAudioCatalog.FootstepsLimpingPath);
            catalog.uiMenuClick = LoadClip(TacticalAudioCatalog.UiMenuClickPath);
            catalog.uiPauseSwitch = LoadClip(TacticalAudioCatalog.UiPauseSwitchPath);
            catalog.weaponGunshot = LoadClip(TacticalAudioCatalog.WeaponGunshotPath);
            catalog.version = TacticalAudioCatalog.CurrentVersion;

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            if (forceLog || created)
                Debug.Log($"[Zombtoy Audio] TacticalAudioCatalog actualizado en '{CatalogAssetPath}' (completo: {catalog.IsComplete}).");
        }

        private static AudioClip LoadClip(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }
    }
}
