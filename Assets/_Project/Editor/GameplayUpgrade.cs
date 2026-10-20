using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Zombtoy.Core;
using Zombtoy.Gameplay;

namespace Zombtoy.Editor
{
    public static class GameplayUpgrade
    {
        private const string Root = "Assets/_Project";
        // Explicit authoring migration; does not run on domain reload or replace map geometry.
        [MenuItem("Abisal/Actualizar sistemas del puerto")]
        public static void Upgrade()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(PortMapBuilder.ScenePath);
            Directory.CreateDirectory(Root + "/Art/Animations"); AssetDatabase.Refresh();
            var idle = Clip("idle", "Idle"); var run = Clip("run", "Run");
            var slow = Definition("Shambler", "Errante", 70, 1.15f, 7, 12, 0.65f, 1.3f);
            var fast = Definition("Runner", "Corredor", 55, 2.25f, 9, 9, 0.55f, 1.1f);
            var session = Object.FindAnyObjectByType<SandboxSession>();
            session.State.medkits = 2; session.State.bandages = 3;
            foreach (var enemy in Object.FindObjectsByType<SandboxEnemy>())
            {
                enemy.definition = enemy.name.Contains("Runner") ? fast : slow;
                enemy.health = enemy.definition.health; enemy.speed = enemy.definition.speed; enemy.sightRange = enemy.definition.sightRange;
                Animate(enemy.gameObject, idle, run);
            }
            Animate(session.player.gameObject, idle, run);
            if (session.player.GetComponentInChildren<Light>() == null)
            {
                var light = new GameObject("Player flashlight").AddComponent<Light>();
                light.transform.SetParent(session.player.transform, false); light.transform.localPosition = new Vector3(0, 1.1f, 0.5f);
                light.type = LightType.Spot; light.range = 13; light.spotAngle = 65; light.intensity = 6; light.color = new Color(0.85f, 1, 0.87f);
            }
            Pickup("aid-arrival", "Botiquin y vendas", new Vector3(-21, 0.5f, -12), 0, 1, 2);
            Pickup("ammo-yard", "Municion y botiquin", new Vector3(8, 0.5f, -7), 18, 1, 0);
            Pickup("aid-control", "Botiquin de control", new Vector3(15, 0.5f, 17), 0, 1, 1);
            PlayerSettings.productName = "Abisal Stealness";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed; PlayerSettings.resizableWindow = true;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(PortMapBuilder.ScenePath, true), new EditorBuildSettingsScene(FoundationBuilder.ScenePath, true) };
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }
        private static AnimationClip Clip(string file, string clipName)
        {
            string path = Root + "/Art/Animations/" + clipName + ".anim";
            var result = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (result != null) { result.name = file == "idle" ? "idle" : "move"; EditorUtility.SetDirty(result); return result; }
            var source = AssetDatabase.LoadAllAssetsAtPath("Assets/ThirdParty/Kenney/Survivors/Animations/" + file + ".fbx").OfType<AnimationClip>().Single(c => c.name == "Root|" + clipName);
            result = Object.Instantiate(source); result.name = file == "idle" ? "idle" : "move"; result.legacy = true; result.wrapMode = WrapMode.Loop;
            var settings = AnimationUtility.GetAnimationClipSettings(result); settings.loopTime = true; AnimationUtility.SetAnimationClipSettings(result, settings);
            AssetDatabase.CreateAsset(result, path); return result;
        }
        private static void Animate(GameObject actor, AnimationClip idle, AnimationClip run)
        {
            var skeleton = actor.GetComponentsInChildren<Transform>().Single(t => t.name == "Root");
            var animation = skeleton.GetComponent<Animation>();
            if (animation == null) animation = skeleton.gameObject.AddComponent<Animation>();
            // AddClip creates embedded copies. Shared authored clips keep the scene small.
            AnimationUtility.SetAnimationClips(animation, new[] { idle, run }); animation.clip = idle;
            animation.playAutomatically = true; animation.cullingType = AnimationCullingType.AlwaysAnimate;
            var motion = actor.GetComponent<CharacterMotion>();
            if (motion == null) motion = actor.AddComponent<CharacterMotion>();
            motion.rig = animation;
            EditorUtility.SetDirty(animation); EditorUtility.SetDirty(motion);
        }
        private static EnemyDefinition Definition(string file, string title, float health, float speed, float sight, float damage, float windup, float recovery)
        {
            string path = Root + "/Data/Enemy_" + file + ".asset";
            var result = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (result == null) { result = ScriptableObject.CreateInstance<EnemyDefinition>(); AssetDatabase.CreateAsset(result, path); }
            result.displayName = title; result.health = health; result.speed = speed; result.sightRange = sight; result.damage = damage; result.attackWindup = windup; result.attackRecovery = recovery;
            EditorUtility.SetDirty(result); return result;
        }
        private static void Pickup(string id, string label, Vector3 position, int ammo, int medkits, int bandages)
        {
            if (Object.FindObjectsByType<WorldPickup>().Any(p => p.id == id)) return;
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = "Supply " + id; go.transform.position = position; go.transform.localScale = new Vector3(0.65f, 0.7f, 0.65f);
            go.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Art/Materials/Port/Signal_green.mat");
            var pickup = go.AddComponent<WorldPickup>(); pickup.id = id; pickup.label = label; pickup.ammo = ammo; pickup.medkits = medkits; pickup.bandages = bandages;
            var cross = new GameObject("Medical marker").AddComponent<TextMesh>(); cross.transform.SetParent(go.transform, false); cross.transform.localPosition = new Vector3(0, 1.3f, 0); cross.transform.rotation = Quaternion.Euler(30, 45, 0);
            cross.text = "+"; cross.anchor = TextAnchor.MiddleCenter; cross.characterSize = 0.18f; cross.fontSize = 48; cross.color = Color.white;
        }
    }
}
