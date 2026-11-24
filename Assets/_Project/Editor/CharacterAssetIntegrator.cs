using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Zombtoy.Gameplay;
using Object = UnityEngine.Object;

namespace Zombtoy.Editor
{
    /// <summary>
    /// Integracion v3 completa:
    /// 1) Extrae y enlaza TODAS las texturas PBR (Diffuse, Normal, Emissive) y meshes en URP/Lit para los 4 personajes
    /// 2) Calibra T-Pose vertical (sin inclinar pelvis/torso!) en Zombie1.fbx, Solider walk.glb y Clayton.glb
    /// 3) Usa Assets/Animations/Humanoid/Pro Rifle Pack con 8 direcciones completas (Walk, Run, Sprint, Crouch Walk)
    ///    y Based Upon: Body Orientation / Feet / Center of Mass para postura recta y pies firmes en el suelo
    /// 4) Integra los modelos 3D reales de armas del proyecto (SwatM4 + Kenney BlasterKit) en Resources/Weapons
    /// </summary>
    [InitializeOnLoad]
    public static class CharacterAssetIntegrator
    {
        private const string ResourcesRoot = "Assets/_Project/Resources";
        private const string CharactersDir = ResourcesRoot + "/Characters";
        private const string MaterialsDir = CharactersDir + "/Materials";
        private const string TexturesDir = CharactersDir + "/Textures";
        private const string WeaponsDir = ResourcesRoot + "/Weapons";
        private const string EnemiesDir = ResourcesRoot + "/Enemies";
        private const string EnemyClipsDir = EnemiesDir + "/Clips";
        private const string EnemyMaterialsDir = EnemiesDir + "/Materials";
        private const string CatalogPath = ResourcesRoot + "/SurvivorVisualCatalog.asset";
        private const string ControllerPath = CharactersDir + "/SurvivorAnimator.controller";
        private const string EnemyControllerPath = EnemiesDir + "/EnemyAnimator.controller";
        private const string EnemyUpperMaskPath = EnemiesDir + "/EnemyUpperBody.mask";

        private const string AnimPackDir = "Assets/Animations/Humanoid/Pro Rifle Pack";
        private const string AnimSourceSkeletonPath = AnimPackDir + "/Zombie1.fbx";
        private const string BurntZombieModelPath = "Assets/Models/Enemies/burner/zombie_theBurnt.fbx";

        private const string KaelModelPath = "Assets/Models/Humanoid/character.fbx";
        private const string ViktorModelPath = "Assets/Models/Humanoid/character (1).fbx";
        private const string VanceModelPath = "Assets/Models/Humanoid/solider-walk/source/Solider walk.glb";
        private const string VanceTexturesDir = "Assets/Models/Humanoid/solider-walk/textures";
        private const string ElenaModelPath = "Assets/Models/Humanoid/clayton-carmine-gears-ultimate-edition/source/Clayton.glb";
        private const string ElenaTexturesDir = "Assets/Models/Humanoid/clayton-carmine-gears-ultimate-edition/textures";

        private const string BlasterADir = "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format/blaster-a.fbx";
        private const string BlasterBDir = "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format/blaster-b.fbx";
        private const string BlasterCDir = "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format/blaster-c.fbx";
        private const string BlasterTexPath = "Assets/ThirdParty/Kenney/BlasterKit/Models/FBX format/Textures/colormap.png";

        private static bool autoBuildScheduled;

        static CharacterAssetIntegrator()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += EnsureIntegratedOnce;
        }

        private static void EnsureIntegratedOnce()
        {
            if (autoBuildScheduled || EditorApplication.isPlayingOrWillChangePlaymode) return;
            var catalog = AssetDatabase.LoadAssetAtPath<SurvivorVisualCatalog>(CatalogPath);
            if (catalog != null && catalog.IsComplete()) return;
            autoBuildScheduled = true;
            IntegrateAll(true);
        }

        [MenuItem("Abisal/Integrar Modelos y Animaciones de Personajes", priority = 3)]
        public static void IntegrateFromMenu()
        {
            IntegrateAll(true);
        }

        public static void IntegrateAll(bool forceRebuild)
        {
            try
            {
                EditorUtility.DisplayProgressBar("Abisal", "Preparando carpetas de personajes y armas...", 0.05f);
                Directory.CreateDirectory(ResourcesRoot);
                Directory.CreateDirectory(CharactersDir);
                Directory.CreateDirectory(MaterialsDir);
                Directory.CreateDirectory(TexturesDir);
                Directory.CreateDirectory(WeaponsDir);
                Directory.CreateDirectory(EnemiesDir);
                Directory.CreateDirectory(EnemyClipsDir);
                Directory.CreateDirectory(EnemyMaterialsDir);

                EditorUtility.DisplayProgressBar("Abisal", "Calibrando esqueleto fuente de Pro Rifle Pack (postura recta)...", 0.12f);
                Avatar animAvatar = ConfigureAnimationPackSourceAvatar(forceRebuild);
                var clips = ConfigureAndLoadProRifleClips(animAvatar, forceRebuild);

                EditorUtility.DisplayProgressBar("Abisal", "Construyendo AnimatorController 8-way (Pro Rifle Pack)...", 0.42f);
                var controller = BuildAnimatorController(clips);

                EditorUtility.DisplayProgressBar("Abisal", "Extrayendo texturas PBR de Kael y Viktor (FBX)...", 0.52f);
                ConfigureFbxHumanoidAndExtractTextures(KaelModelPath, TexturesDir + "/Kael", forceRebuild);
                ConfigureFbxHumanoidAndExtractTextures(ViktorModelPath, TexturesDir + "/Viktor", forceRebuild);

                var entries = new List<SurvivorVisualEntry>();

                EditorUtility.DisplayProgressBar("Abisal", "Construyendo Kael (Vanguardia) con texturas PBR...", 0.62f);
                entries.Add(BuildFbxCharacterEntry(
                    SurvivorId.KaelAssault, "Kael_Vanguardia", KaelModelPath, TexturesDir + "/Kael", "Ch15", 1.82f,
                    new Vector3(0.01f, 0.02f, 0.06f), Vector3.zero, "mixamorig:RightHand", "mixamorig:LeftHand", controller));

                EditorUtility.DisplayProgressBar("Abisal", "Construyendo Viktor (Sombra) con texturas PBR...", 0.70f);
                entries.Add(BuildFbxCharacterEntry(
                    SurvivorId.ViktorStealth, "Viktor_Sombra", ViktorModelPath, TexturesDir + "/Viktor", "Ch35", 1.80f,
                    new Vector3(0.01f, 0.02f, 0.06f), Vector3.zero, "mixamorig:RightHand", "mixamorig:LeftHand", controller));

                EditorUtility.DisplayProgressBar("Abisal", "Construyendo Dra. Vance con texturas PBR y T-Pose recto...", 0.78f);
                entries.Add(BuildVanceCharacterEntry(controller));

                EditorUtility.DisplayProgressBar("Abisal", "Construyendo Elena (Clayton) con texturas PBR y T-Pose recto...", 0.85f);
                entries.Add(BuildElenaCharacterEntry(controller));

                EditorUtility.DisplayProgressBar("Abisal", "Integrando modelos 3D de armas (SwatM4 + Kenney BlasterKit)...", 0.88f);
                var weaponPrefabs = BuildIntegratedWeaponPrefabs();

                EditorUtility.DisplayProgressBar("Abisal", "Integrando modelos 3D y animaciones de ataque/muerte de enemigos...", 0.94f);
                var enemyController = BuildEnemyAnimatorController(clips);
                var enemyEntries = BuildEnemyVisualEntries(enemyController, forceRebuild);

                var catalog = AssetDatabase.LoadAssetAtPath<SurvivorVisualCatalog>(CatalogPath);
                if (catalog == null)
                {
                    catalog = ScriptableObject.CreateInstance<SurvivorVisualCatalog>();
                    AssetDatabase.CreateAsset(catalog, CatalogPath);
                }

                catalog.version = SurvivorVisualCatalog.CurrentCatalogVersion;
                catalog.animatorController = controller;
                catalog.entries = entries.Where(e => e != null).ToArray();
                catalog.weaponPrefabs = weaponPrefabs;
                catalog.enemyAnimatorController = enemyController;
                catalog.enemyEntries = enemyEntries;
                EditorUtility.SetDirty(catalog);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"[Zombtoy] Integracion v{catalog.version} completada: {catalog.entries.Length}/4 personajes, {weaponPrefabs.Count(w => w != null)}/6 armas 3D, {enemyEntries.Length}/4 modelos de enemigos y {clips.Count} clips Pro Rifle Pack. Complete={catalog.IsComplete()}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Zombtoy] Error en integracion v3: {ex}");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // =====================================================================
        // 1. BONE MAPS
        // =====================================================================

        private static readonly (string human, string bone)[] CatBaseHumanMap =
        {
            ("Hips", "Base HumanPelvis"),
            ("Spine", "Base HumanSpine1"),
            ("Chest", "Base HumanSpine2"),
            ("UpperChest", "Base HumanRibcage"),
            ("Neck", "Base HumanNeck"),
            ("Head", "Base HumanHead"),
            ("LeftShoulder", "Base HumanLArmCollarbone"),
            ("LeftUpperArm", "Base HumanLArmUpperarm"),
            ("LeftLowerArm", "Base HumanLArmForearm"),
            ("LeftHand", "Base HumanLArmPalm"),
            ("RightShoulder", "Base HumanRArmCollarbone"),
            ("RightUpperArm", "Base HumanRArmUpperarm"),
            ("RightLowerArm", "Base HumanRArmForearm"),
            ("RightHand", "Base HumanRArmPalm"),
            ("LeftUpperLeg", "Base HumanLLegThigh"),
            ("LeftLowerLeg", "Base HumanLLegCalf"),
            ("LeftFoot", "Base HumanLLegFoot"),
            ("LeftToes", "Base HumanLLegDigit11"),
            ("RightUpperLeg", "Base HumanRThigh"),
            ("RightLowerLeg", "Base HumanRCalf"),
            ("RightFoot", "Base HumanRFoot"),
            ("RightToes", "Base HumanRDigit11"),
        };

        // In Solider walk.glb, NPCMedium_L/R_Clavicle are children of NPCMedium_Neck -> map Neck as UpperChest so shoulders are valid Humanoid children.
        private static readonly (string human, string bone)[] VanceBoneMap =
        {
            ("Hips", "NPCMedium_Pelvis"),
            ("Spine", "NPCMedium_Spine"),
            ("Chest", "NPCMedium_Spine2"),
            ("UpperChest", "NPCMedium_Neck"),
            ("Head", "NPCMedium_Head"),
            ("LeftShoulder", "NPCMedium_L_Clavicle"),
            ("LeftUpperArm", "NPCMedium_L_UpperArm"),
            ("LeftLowerArm", "NPCMedium_L_Forearm"),
            ("LeftHand", "NPCMedium_L_Hand"),
            ("RightShoulder", "NPCMedium_R_Clavicle"),
            ("RightUpperArm", "NPCMedium_R_UpperArm"),
            ("RightLowerArm", "NPCMedium_R_Forearm"),
            ("RightHand", "NPCMedium_R_Hand"),
            ("LeftUpperLeg", "NPCMedium_L_Thigh"),
            ("LeftLowerLeg", "NPCMedium_L_Calf"),
            ("LeftFoot", "NPCMedium_L_Foot"),
            ("LeftToes", "NPCMedium_L_Toe0"),
            ("RightUpperLeg", "NPCMedium_R_Thigh"),
            ("RightLowerLeg", "NPCMedium_R_Calf"),
            ("RightFoot", "NPCMedium_R_Foot"),
            ("RightToes", "NPCMedium_R_Toe0"),
        };

        private static readonly (string human, string bone)[] ClaytonBoneMap =
        {
            ("Hips", "b_MF_Pelvis"),
            ("Spine", "b_MF_Spine_01"),
            ("Chest", "b_MF_Spine_02"),
            ("UpperChest", "b_MF_Spine_03"),
            ("Neck", "b_MF_Neck"),
            ("Head", "b_MF_Head"),
            ("LeftShoulder", "b_MF_Clavicle_L"),
            ("LeftUpperArm", "b_MF_UpperArm_L"),
            ("LeftLowerArm", "b_MF_Forearm_L"),
            ("LeftHand", "b_MF_Hand_L"),
            ("RightShoulder", "b_MF_Clavicle_R"),
            ("RightUpperArm", "b_MF_UpperArm_R"),
            ("RightLowerArm", "b_MF_Forearm_R"),
            ("RightHand", "b_MF_Hand_R"),
            ("LeftUpperLeg", "b_MF_Thigh_L"),
            ("LeftLowerLeg", "b_MF_Calf_L"),
            ("LeftFoot", "b_MF_Foot_L"),
            ("LeftToes", "b_MF_Toe_L"),
            ("RightUpperLeg", "b_MF_Thigh_R"),
            ("RightLowerLeg", "b_MF_Calf_R"),
            ("RightFoot", "b_MF_Foot_R"),
            ("RightToes", "b_MF_Toe_R"),
        };

        // =====================================================================
        // 2. PRO RIFLE PACK SOURCE AVATAR & 37 CLIPS (UPRIGHT, FEET GROUNDED)
        // =====================================================================

        private static Avatar ConfigureAnimationPackSourceAvatar(bool forceRebuild)
        {
            var importer = AssetImporter.GetAtPath(AnimSourceSkeletonPath) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[Zombtoy] No se encontro esqueleto fuente en {AnimSourceSkeletonPath}");
                return null;
            }

            var sourceGo = AssetDatabase.LoadAssetAtPath<GameObject>(AnimSourceSkeletonPath);
            if (sourceGo == null) return null;

            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;

            // CRITICAL FIX: Zombie1.fbx already has an upright torso (Pelvis->Head sums to 0 deg).
            // ONLY straighten limbs (arms & legs) into T-Pose — NEVER rotate Pelvis or Ribcage!
            var temp = Object.Instantiate(sourceGo);
            temp.name = sourceGo.name;
            temp.transform.position = Vector3.zero;
            temp.transform.rotation = Quaternion.identity;
            temp.transform.localScale = Vector3.one;
            try
            {
                var desc = BuildHumanDescriptionFromHierarchy(temp, CatBaseHumanMap, straightenLimbsOnly: true);
                importer.humanDescription = desc;
            }
            finally
            {
                Object.DestroyImmediate(temp);
            }

            importer.SaveAndReimport();
            var avatar = LoadAvatarFromAsset(AnimSourceSkeletonPath);
            Debug.Log($"[Zombtoy] Avatar fuente Pro Rifle Pack (upright T-Pose): valid={avatar != null && avatar.isValid}, human={avatar != null && avatar.isHuman}");
            return avatar;
        }

        private static readonly (string key, string file, bool loop)[] RequiredClipSpecs =
        {
            ("idle", "idle.fbx", true),
            ("idle_aim", "idle aiming.fbx", true),
            ("crouch_idle", "idle crouching.fbx", true),
            ("crouch_aim", "idle crouching aiming.fbx", true),

            ("walk_f", "walk forward.fbx", true),
            ("walk_fr", "walk forward right.fbx", true),
            ("walk_r", "walk right.fbx", true),
            ("walk_br", "walk backward right.fbx", true),
            ("walk_b", "walk backward.fbx", true),
            ("walk_bl", "walk backward left.fbx", true),
            ("walk_l", "walk left.fbx", true),
            ("walk_fl", "walk forward left.fbx", true),

            ("run_f", "run forward.fbx", true),
            ("run_fr", "run forward right.fbx", true),
            ("run_r", "run right.fbx", true),
            ("run_br", "run backward right.fbx", true),
            ("run_b", "run backward.fbx", true),
            ("run_bl", "run backward left.fbx", true),
            ("run_l", "run left.fbx", true),
            ("run_fl", "run forward left.fbx", true),

            ("sprint_f", "sprint forward.fbx", true),
            ("sprint_fr", "sprint forward right.fbx", true),
            ("sprint_r", "sprint right.fbx", true),
            ("sprint_br", "sprint backward right.fbx", true),
            ("sprint_b", "sprint backward.fbx", true),
            ("sprint_bl", "sprint backward left.fbx", true),
            ("sprint_l", "sprint left.fbx", true),
            ("sprint_fl", "sprint forward left.fbx", true),

            ("cwalk_f", "walk crouching forward.fbx", true),
            ("cwalk_fr", "walk crouching forward right.fbx", true),
            ("cwalk_r", "walk crouching right.fbx", true),
            ("cwalk_br", "walk crouching backward right.fbx", true),
            ("cwalk_b", "walk crouching backward.fbx", true),
            ("cwalk_bl", "walk crouching backward left.fbx", true),
            ("cwalk_l", "walk crouching left.fbx", true),
            ("cwalk_fl", "walk crouching forward left.fbx", true),

            ("death", "death from the front.fbx", false),
            ("death_back", "death from the back.fbx", false),
            ("death_right", "death from right.fbx", false),
            ("death_headshot", "death from front headshot.fbx", false),
        };

        private static Dictionary<string, AnimationClip> ConfigureAndLoadProRifleClips(Avatar sourceAvatar, bool forceRebuild)
        {
            var result = new Dictionary<string, AnimationClip>();

            for (int i = 0; i < RequiredClipSpecs.Length; i++)
            {
                var spec = RequiredClipSpecs[i];
                string path = AnimPackDir + "/" + spec.file;
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;

                bool needsReimport = forceRebuild
                    || importer.animationType != ModelImporterAnimationType.Human
                    || (sourceAvatar != null && importer.sourceAvatar != sourceAvatar)
                    || importer.clipAnimations == null
                    || importer.clipAnimations.Length == 0;

                if (needsReimport)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    if (sourceAvatar != null)
                    {
                        importer.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                        importer.sourceAvatar = sourceAvatar;
                    }
                    else
                    {
                        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    }

                    importer.importAnimation = true;
                    var clipAnims = importer.clipAnimations;
                    if (clipAnims == null || clipAnims.Length == 0)
                        clipAnims = importer.defaultClipAnimations;

                    for (int c = 0; c < clipAnims.Length; c++)
                    {
                        clipAnims[c].name = spec.key;
                        clipAnims[c].loopTime = spec.loop;
                        clipAnims[c].loopPose = spec.loop;

                        // CRITICAL: All Pro Rifle Pack locomotion clips carry 1.8m-3.4m of horizontal root motion.
                        // - lockRootPositionXZ = FALSE for looping locomotion so Unity strips root XZ travel into
                        //   deltaPosition (which applyRootMotion=false discards), keeping characters 100% in-place
                        //   with ZERO erratic left/right/forward lurching or loop snapping!
                        // - lockRootPositionXZ = TRUE only for non-looping death clips so bodies fall naturally.
                        // - keepOriginalOrientation = TRUE so all 8 directions stay aligned to +Z consistently.
                        // - Position Y based upon Feet -> grounded at y=0.
                        clipAnims[c].lockRootRotation = true;
                        clipAnims[c].keepOriginalOrientation = true;
                        clipAnims[c].lockRootHeightY = true;
                        clipAnims[c].keepOriginalPositionY = false;
                        clipAnims[c].heightFromFeet = true;
                        clipAnims[c].lockRootPositionXZ = !spec.loop;
                        clipAnims[c].keepOriginalPositionXZ = false;
                    }

                    importer.clipAnimations = clipAnims;
                    importer.SaveAndReimport();
                }

                var loadedClip = LoadPrimaryClip(path);
                if (loadedClip != null)
                    result[spec.key] = loadedClip;
            }

            return result;
        }

        private static AnimationClip LoadPrimaryClip(string assetPath)
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                    return clip;
            }
            return null;
        }

        private static Avatar LoadAvatarFromAsset(string assetPath)
        {
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj is Avatar avatar)
                    return avatar;
            }
            return null;
        }

        // =====================================================================
        // 3. 8-WAY DIRECTIONAL ANIMATOR CONTROLLER (PRO RIFLE PACK)
        // =====================================================================

        private static AnimatorController BuildAnimatorController(Dictionary<string, AnimationClip> clips)
        {
            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
                AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("MoveX", AnimatorControllerParameterType.Float);
            controller.AddParameter("MoveY", AnimatorControllerParameterType.Float);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Aiming", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Crouching", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            var sm = controller.layers[0].stateMachine;

            AnimationClip idleAim = clips.TryGetValue("idle_aim", out var ia) ? ia : (clips.TryGetValue("idle", out var r0) ? r0 : null);
            AnimationClip idleRelax = idleAim != null ? idleAim : (clips.TryGetValue("idle", out var r) ? r : null);
            AnimationClip crouchIdle = clips.TryGetValue("crouch_aim", out var ca0) ? ca0 : (clips.TryGetValue("crouch_idle", out var ci) ? ci : idleRelax);
            AnimationClip crouchAim = clips.TryGetValue("crouch_aim", out var ca) ? ca : crouchIdle;

            // 1) Standing Locomotion (idle + 8-way Walk + 8-way Run + 8-way Sprint)
            // Pro Rifle Pack run/sprint clips are 0.50s (4.56 m/s & 6.85 m/s) while gameplay speeds are 3.0 / 5.0 m/s,
            // so timeScale is calibrated to match actual ground speed smoothly without frantic thrashing.
            var standState = controller.CreateBlendTreeInController("Standing_Locomotion", out BlendTree standTree, 0);
            Configure2DTree(standTree, BlendTreeType.FreeformDirectional2D);
            if (idleRelax != null) standTree.AddChild(idleRelax, Vector2.zero);
            Add8WayRing(standTree, clips, "walk", 0.45f, idleRelax, 0.95f);
            Add8WayRing(standTree, clips, "run", 1.0f, idleRelax, 0.68f);
            Add8WayRing(standTree, clips, "sprint", 1.5f, idleRelax, 0.76f);
            sm.defaultState = standState;

            // 2) Standing Aiming (ADS idle aiming + 8-way Walk + 8-way Run)
            var aimState = controller.CreateBlendTreeInController("Standing_Aiming", out BlendTree aimTree, 0);
            Configure2DTree(aimTree, BlendTreeType.FreeformDirectional2D);
            if (idleAim != null) aimTree.AddChild(idleAim, Vector2.zero);
            Add8WayRing(aimTree, clips, "walk", 0.50f, idleAim, 0.90f);
            Add8WayRing(aimTree, clips, "run", 1.0f, idleAim, 0.66f);

            // 3) Crouching Locomotion (crouch idle + 8-way Crouch Walk)
            var crouchState = controller.CreateBlendTreeInController("Crouching_Locomotion", out BlendTree crouchTree, 0);
            Configure2DTree(crouchTree, BlendTreeType.FreeformDirectional2D);
            if (crouchIdle != null) crouchTree.AddChild(crouchIdle, Vector2.zero);
            Add8WayRing(crouchTree, clips, "cwalk", 0.60f, crouchIdle, 0.90f);

            // 4) Crouching Aiming (crouch aiming idle + 8-way Crouch Walk)
            var crouchAimState = controller.CreateBlendTreeInController("Crouching_Aiming", out BlendTree crouchAimTree, 0);
            Configure2DTree(crouchAimTree, BlendTreeType.FreeformDirectional2D);
            if (crouchAim != null) crouchAimTree.AddChild(crouchAim, Vector2.zero);
            Add8WayRing(crouchAimTree, clips, "cwalk", 0.60f, crouchAim, 0.90f);

            AddStanceTransition(standState, aimState, aiming: true, crouching: false);
            AddStanceTransition(standState, crouchState, aiming: false, crouching: true);
            AddStanceTransition(standState, crouchAimState, aiming: true, crouching: true);

            AddStanceTransition(aimState, standState, aiming: false, crouching: false);
            AddStanceTransition(aimState, crouchState, aiming: false, crouching: true);
            AddStanceTransition(aimState, crouchAimState, aiming: true, crouching: true);

            AddStanceTransition(crouchState, standState, aiming: false, crouching: false);
            AddStanceTransition(crouchState, aimState, aiming: true, crouching: false);
            AddStanceTransition(crouchState, crouchAimState, aiming: true, crouching: true);

            AddStanceTransition(crouchAimState, standState, aiming: false, crouching: false);
            AddStanceTransition(crouchAimState, aimState, aiming: true, crouching: false);
            AddStanceTransition(crouchAimState, crouchState, aiming: false, crouching: true);

            // 5) Death state
            var deadState = sm.AddState("Dead", new Vector3(360, 220, 0));
            deadState.motion = clips.TryGetValue("death", out var dc) ? dc : idleRelax;

            var toDead = sm.AddAnyStateTransition(deadState);
            toDead.hasExitTime = false;
            toDead.canTransitionToSelf = false;
            toDead.duration = 0.10f;
            toDead.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            var revive = deadState.AddTransition(standState);
            revive.hasExitTime = false;
            revive.duration = 0.15f;
            revive.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void Configure2DTree(BlendTree tree, BlendTreeType type)
        {
            tree.blendType = type;
            tree.blendParameter = "MoveX";
            tree.blendParameterY = "MoveY";
            tree.useAutomaticThresholds = false;
        }

        private static void AddStanceTransition(AnimatorState from, AnimatorState to, bool aiming, bool crouching)
        {
            var tr = from.AddTransition(to);
            tr.hasExitTime = false;
            tr.duration = 0.11f;
            tr.AddCondition(aiming ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "Aiming");
            tr.AddCondition(crouching ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, "Crouching");
        }

        private static void Add8WayRing(BlendTree tree, Dictionary<string, AnimationClip> clips, string prefix, float radius, AnimationClip fallback, float timeScale = 1f)
        {
            float d = radius * 0.70710678f;
            var directions = new (string suffix, Vector2 pos)[]
            {
                ("_f",  new Vector2(0f, radius)),
                ("_fr", new Vector2(d, d)),
                ("_r",  new Vector2(radius, 0f)),
                ("_br", new Vector2(d, -d)),
                ("_b",  new Vector2(0f, -radius)),
                ("_bl", new Vector2(-d, -d)),
                ("_l",  new Vector2(-radius, 0f)),
                ("_fl", new Vector2(-d, d)),
            };

            var children = new List<ChildMotion>(tree.children);
            foreach (var (suffix, pos) in directions)
            {
                AnimationClip chosen = (clips.TryGetValue(prefix + suffix, out var clip) && clip != null) ? clip : fallback;
                if (chosen == null) continue;
                children.Add(new ChildMotion
                {
                    motion = chosen,
                    position = pos,
                    timeScale = timeScale,
                    cycleOffset = 0f,
                    directBlendParameter = "Blend"
                });
            }
            tree.children = children.ToArray();
        }

        // =====================================================================
        // 4. FBX CHARACTERS WITH FULL EXTRACTED PBR TEXTURES (KAEL & VIKTOR)
        // =====================================================================

        private static void ConfigureFbxHumanoidAndExtractTextures(string fbxPath, string extractDir, bool forceRebuild)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null) return;

            Directory.CreateDirectory(extractDir);
            AssetDatabase.Refresh();
            bool hasExtracted = Directory.GetFiles(extractDir, "*.png").Length > 0 || Directory.GetFiles(extractDir, "*.jpg").Length > 0;
            if (forceRebuild || !hasExtracted)
            {
                importer.ExtractTextures(extractDir);
                AssetDatabase.Refresh();
                MarkNormalMapsInFolder(extractDir);
            }

            var existingAvatar = LoadAvatarFromAsset(fbxPath);
            if (importer.animationType != ModelImporterAnimationType.Human || existingAvatar == null || !existingAvatar.isValid || !existingAvatar.isHuman)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = false;
                importer.SaveAndReimport();
            }
        }

        private static SurvivorVisualEntry BuildFbxCharacterEntry(
            SurvivorId id, string slug, string fbxPath, string textureFolder, string texPrefix, float targetHeight,
            Vector3 handOffset, Vector3 eulerOffset, string rightHandBone, string leftHandBone,
            RuntimeAnimatorController controller)
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var avatar = LoadAvatarFromAsset(fbxPath);
            if (sourcePrefab == null || avatar == null || !avatar.isValid)
            {
                Debug.LogWarning($"[Zombtoy] No se pudo preparar FBX {slug} ({fbxPath})");
                return null;
            }

            var instance = Object.Instantiate(sourcePrefab);
            instance.name = "Survivor_" + slug;
            try
            {
                foreach (var legacyAnim in instance.GetComponentsInChildren<Animation>(true))
                    Object.DestroyImmediate(legacyAnim);
                foreach (var childAnim in instance.GetComponentsInChildren<Animator>(true))
                    if (childAnim.gameObject != instance) Object.DestroyImmediate(childAnim);

                EnableAllRenderersAndMeshes(instance);
                ApplyMixamoPbrMaterials(instance, slug, textureFolder, texPrefix);
                NormalizeSingleRootScale(instance, targetHeight);
                ConfigureAnimatorAndRenderers(instance, avatar, controller);

                string prefabPath = $"{CharactersDir}/Survivor_{(int)id}_{slug}.prefab";
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

                return new SurvivorVisualEntry
                {
                    id = id,
                    label = slug,
                    modelPrefab = savedPrefab,
                    avatar = avatar,
                    targetHeight = targetHeight,
                    weaponHandOffset = handOffset,
                    weaponEulerOffset = eulerOffset,
                    rightHandBoneName = rightHandBone,
                    leftHandBoneName = leftHandBone,
                    hiddenChildNames = Array.Empty<string>()
                };
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void ApplyMixamoPbrMaterials(GameObject root, string slug, string textureFolder, string prefix)
        {
            // Build dedicated URP/Lit materials for each 1001/1002/1003 texture set extracted from the Mixamo FBX
            var sets = new List<Material>();
            for (int setNum = 1001; setNum <= 1004; setNum++)
            {
                string tag = $"{prefix}_{setNum}";
                Texture2D diffuse = FindTextureInFolder(textureFolder, tag + "_Diffuse");
                if (diffuse == null) continue;
                Texture2D normal = FindTextureInFolder(textureFolder, tag + "_Normal");
                Texture2D emissive = FindTextureInFolder(textureFolder, tag + "_Emissive");
                var mat = CreateOrUpdateUrpLitMaterial($"{slug}_{tag}", diffuse, normal, emissive, Color.white, metallic: 0.18f, smoothness: 0.42f);
                sets.Add(mat);
            }

            if (sets.Count == 0) return;

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                var current = r.sharedMaterials;
                if (current == null || current.Length == 0) continue;

                var updated = new Material[current.Length];
                for (int i = 0; i < current.Length; i++)
                {
                    string matName = current[i] != null ? current[i].name : string.Empty;
                    Material chosen = null;
                    // Match explicit slot tag (e.g. body1 -> 1002, body2 -> 1003, or explicit 1001/1002/1003)
                    for (int s = 0; s < sets.Count; s++)
                    {
                        string num = (1001 + s).ToString();
                        if (matName.Contains(num)) { chosen = sets[s]; break; }
                    }
                    if (chosen == null)
                    {
                        if (matName.EndsWith("2") && sets.Count >= 3) chosen = sets[2];
                        else if (matName.EndsWith("1") && sets.Count >= 2) chosen = sets[1];
                        else chosen = sets[Mathf.Clamp(i, 0, sets.Count - 1)];
                    }
                    updated[i] = chosen;
                }
                r.sharedMaterials = updated;
            }
        }

        // =====================================================================
        // 5. GLB CHARACTERS WITH EXPLICIT PBR TEXTURES & UPRIGHT T-POSE (VANCE & ELENA)
        // =====================================================================

        private static SurvivorVisualEntry BuildVanceCharacterEntry(RuntimeAnimatorController controller)
        {
            MarkNormalMapsInFolder(VanceTexturesDir);

            var eyeMat = CreateOrUpdateUrpLitMaterial("Vance_Eye",
                FindTextureInFolder(VanceTexturesDir, "Eye_D_0"), null, null, Color.white, 0.05f, 0.75f);
            var legsMat = CreateOrUpdateUrpLitMaterial("Vance_Legs",
                FindTextureInFolder(VanceTexturesDir, "ID_3_D_clean_2"),
                FindTextureInFolder(VanceTexturesDir, "id_3_N_1"), null, Color.white, 0.12f, 0.30f);
            var faceMat = CreateOrUpdateUrpLitMaterial("Vance_Face",
                FindTextureInFolder(VanceTexturesDir, "Swat_head_D_3"), null, null, Color.white, 0.02f, 0.25f);
            var helmetMat = CreateOrUpdateUrpLitMaterial("Vance_Helmet",
                FindTextureInFolder(VanceTexturesDir, "helmet_D_clean_5"),
                FindTextureInFolder(VanceTexturesDir, "helmet_N_4"), null, Color.white, 0.22f, 0.45f);
            var torsoMat = CreateOrUpdateUrpLitMaterial("Vance_Torso",
                FindTextureInFolder(VanceTexturesDir, "ID_2_D_clean_7"),
                FindTextureInFolder(VanceTexturesDir, "ID_2_N_6"), null, Color.white, 0.15f, 0.35f);

            var matMap = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase)
            {
                ["Stevenson_eye"] = eyeMat,
                ["Stenvenson_ID_3"] = legsMat,
                ["stevenson_face_2"] = faceMat,
                ["Swat_helmet"] = helmetMat,
                ["Stevenson_ID_2"] = torsoMat,
                ["__slot_0"] = eyeMat,
                ["__slot_1"] = legsMat,
                ["__slot_2"] = faceMat,
                ["__slot_3"] = helmetMat,
                ["__slot_4"] = torsoMat,
            };

            return BuildGlbCharacterEntry(
                SurvivorId.VanceMedic, "Vance_BioControl", VanceModelPath, 1.80f,
                VanceBoneMap, new[] { "SwatM4" }, matMap,
                new Vector3(0.01f, 0.02f, 0.05f), Vector3.zero, "NPCMedium_R_Hand", "NPCMedium_L_Hand", controller);
        }

        private static SurvivorVisualEntry BuildElenaCharacterEntry(RuntimeAnimatorController controller)
        {
            MarkNormalMapsInFolder(ElenaTexturesDir);

            var bodyMat = CreateOrUpdateUrpLitMaterial("Elena_Clayton_Body",
                FindTextureInFolder(ElenaTexturesDir, "COG_ClaytonCarmine_Delta_Tex_2"),
                FindTextureInFolder(ElenaTexturesDir, "COG_ClaytonCarmine_Delta_Normal_1"),
                null, Color.white, 0.28f, 0.44f);
            var helmetMat = CreateOrUpdateUrpLitMaterial("Elena_Clayton_Helmet",
                FindTextureInFolder(ElenaTexturesDir, "COG_ClaytonCarmine_Helmet_Delta_Tex_5"),
                FindTextureInFolder(ElenaTexturesDir, "COG_ClaytonCarmine_Helmet_Delta_Normal_4"),
                null, Color.white, 0.32f, 0.50f);

            var matMap = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase)
            {
                ["GOW3_Clayton_Carmine_GP01_Delta_MAT_MIC"] = bodyMat,
                ["GOW3_Clayton_Carmine_Helmet_GP01_Delta_MAT_MIC"] = helmetMat,
                ["__slot_0"] = bodyMat,
                ["__slot_1"] = helmetMat,
            };

            return BuildGlbCharacterEntry(
                SurvivorId.ElenaEngineer, "Elena_Bastion", ElenaModelPath, 1.88f,
                ClaytonBoneMap, Array.Empty<string>(), matMap,
                new Vector3(0.02f, 0.02f, 0.06f), Vector3.zero, "b_MF_Hand_R", "b_MF_Hand_L", controller);
        }

        private static SurvivorVisualEntry BuildGlbCharacterEntry(
            SurvivorId id, string slug, string glbPath, float targetHeight,
            (string human, string bone)[] boneMap, string[] hiddenChildren,
            Dictionary<string, Material> explicitMaterials,
            Vector3 handOffset, Vector3 eulerOffset, string rightHandBone, string leftHandBone,
            RuntimeAnimatorController controller)
        {
            var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
            if (sourcePrefab == null)
            {
                Debug.LogWarning($"[Zombtoy] Modelo GLB no encontrado: {glbPath}");
                return null;
            }

            var root = new GameObject("Survivor_" + slug);
            var modelChild = Object.Instantiate(sourcePrefab, root.transform, false);
            modelChild.name = sourcePrefab.name;

            try
            {
                EnableAllRenderersAndMeshes(root);
                DisableNamedChildren(root, hiddenChildren);

                // Strip ALL legacy Animation and nested Animator components added by glTFast
                foreach (var legacyAnim in root.GetComponentsInChildren<Animation>(true))
                    Object.DestroyImmediate(legacyAnim);
                foreach (var nestedAnim in root.GetComponentsInChildren<Animator>(true))
                    Object.DestroyImmediate(nestedAnim);

                EnsureFacingPositiveZ(root, modelChild.transform, boneMap);
                NormalizeModelBounds(root, modelChild.transform, targetHeight);

                ApplyExplicitMaterialMap(root, explicitMaterials);

                Avatar avatar = BuildAndSaveGlbHumanoidAvatar(root, slug, boneMap);
                if (avatar == null || !avatar.isValid || !avatar.isHuman)
                    Debug.LogWarning($"[Zombtoy] Avatar GLB para {slug} invalido o no humano!");

                ConfigureAnimatorAndRenderers(root, avatar, controller);

                string prefabPath = $"{CharactersDir}/Survivor_{(int)id}_{slug}.prefab";
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

                return new SurvivorVisualEntry
                {
                    id = id,
                    label = slug,
                    modelPrefab = savedPrefab,
                    avatar = avatar,
                    targetHeight = targetHeight,
                    weaponHandOffset = handOffset,
                    weaponEulerOffset = eulerOffset,
                    rightHandBoneName = rightHandBone,
                    leftHandBoneName = leftHandBone,
                    hiddenChildNames = hiddenChildren
                };
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void ApplyExplicitMaterialMap(GameObject root, Dictionary<string, Material> matMap)
        {
            if (matMap == null || matMap.Count == 0) return;

            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null || !r.gameObject.activeInHierarchy) continue;
                var mats = r.sharedMaterials;
                if (mats == null || mats.Length == 0) continue;

                var updated = new Material[mats.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    string name = mats[i] != null ? mats[i].name : string.Empty;
                    if (matMap.TryGetValue(name, out var exact) && exact != null)
                    {
                        updated[i] = exact;
                    }
                    else if (matMap.TryGetValue("__slot_" + i, out var bySlot) && bySlot != null)
                    {
                        updated[i] = bySlot;
                    }
                    else
                    {
                        // Partial name match fallback
                        Material partial = null;
                        foreach (var kvp in matMap)
                        {
                            if (!kvp.Key.StartsWith("__", StringComparison.Ordinal) &&
                                !string.IsNullOrEmpty(name) &&
                                name.IndexOf(kvp.Key, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                partial = kvp.Value;
                                break;
                            }
                        }
                        updated[i] = partial != null ? partial : mats[i];
                    }
                }
                r.sharedMaterials = updated;
            }
        }

        private static Avatar BuildAndSaveGlbHumanoidAvatar(GameObject sourceRoot, string slug, (string human, string bone)[] boneMap)
        {
            var clone = Object.Instantiate(sourceRoot);
            clone.name = sourceRoot.name;
            clone.transform.position = Vector3.zero;
            clone.transform.rotation = Quaternion.identity;
            clone.transform.localScale = Vector3.one;

            try
            {
                var desc = BuildHumanDescriptionFromHierarchy(clone, boneMap, straightenLimbsOnly: true);
                var avatar = AvatarBuilder.BuildHumanAvatar(clone, desc);
                avatar.name = "Avatar_" + slug;

                string avatarPath = $"{CharactersDir}/Avatar_{slug}.asset";
                if (AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath) != null)
                    AssetDatabase.DeleteAsset(avatarPath);
                AssetDatabase.CreateAsset(avatar, avatarPath);
                return AssetDatabase.LoadAssetAtPath<Avatar>(avatarPath);
            }
            finally
            {
                Object.DestroyImmediate(clone);
            }
        }

        private static HumanDescription BuildHumanDescriptionFromHierarchy(GameObject root, (string human, string bone)[] boneMap, bool straightenLimbsOnly)
        {
            var byName = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!byName.ContainsKey(t.name))
                    byName.Add(t.name, t);
            }

            if (straightenLimbsOnly)
                EnforceUprightLimbTPose(byName, boneMap);

            var humanBones = new List<HumanBone>();
            foreach (var (human, bone) in boneMap)
            {
                if (!byName.ContainsKey(bone)) continue;
                humanBones.Add(new HumanBone
                {
                    humanName = human,
                    boneName = bone,
                    limit = new HumanLimit { useDefaultValues = true }
                });
            }

            var skeletonBones = new List<SkeletonBone>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                skeletonBones.Add(new SkeletonBone
                {
                    name = t.name,
                    position = t.localPosition,
                    rotation = t.localRotation,
                    scale = t.localScale
                });
            }

            return new HumanDescription
            {
                human = humanBones.ToArray(),
                skeleton = skeletonBones.ToArray(),
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };
        }

        /// <summary>
        /// Straightens arms (-X / +X), legs (-Y), and feet (+Z) into a clean Unity T-Pose
        /// WITHOUT pitching/tilting Pelvis or short Spine segments (which previously caused a 77 deg Ribcage tilt!).
        /// </summary>
        private static void EnforceUprightLimbTPose(Dictionary<string, Transform> byName, (string human, string bone)[] boneMap)
        {
            Transform Get(string humanName)
            {
                foreach (var pair in boneMap)
                {
                    if (pair.human == humanName && byName.TryGetValue(pair.bone, out var found))
                        return found;
                }
                return null;
            }

            var hips = Get("Hips");
            var head = Get("Head");
            var lUpperArm = Get("LeftUpperArm");
            var lLowerArm = Get("LeftLowerArm");
            var lHand = Get("LeftHand");
            var rUpperArm = Get("RightUpperArm");
            var rLowerArm = Get("RightLowerArm");
            var rHand = Get("RightHand");
            var lUpperLeg = Get("LeftUpperLeg");
            var lLowerLeg = Get("LeftLowerLeg");
            var lFoot = Get("LeftFoot");
            var lToes = Get("LeftToes");
            var rUpperLeg = Get("RightUpperLeg");
            var rLowerLeg = Get("RightLowerLeg");
            var rFoot = Get("RightFoot");
            var rToes = Get("RightToes");

            float approxHeight = hips != null && head != null ? Mathf.Max(0.5f, Mathf.Abs(head.position.y - hips.position.y) * 2.1f) : 1.8f;
            float minSegment = approxHeight * 0.045f;

            // 1) Yaw-only check on Hips (NEVER pitch Pelvis!)
            if (hips != null && lUpperLeg != null && rUpperLeg != null)
            {
                Vector3 hipRight = rUpperLeg.position - lUpperLeg.position;
                hipRight.y = 0f;
                if (hipRight.sqrMagnitude > 0.0001f)
                {
                    float yawAngle = Vector3.SignedAngle(hipRight.normalized, Vector3.right, Vector3.up);
                    if (Mathf.Abs(yawAngle) > 2f)
                        hips.rotation = Quaternion.AngleAxis(yawAngle, Vector3.up) * hips.rotation;
                }
            }

            // 2) Straighten Arms horizontally: Left -> -X, Right -> +X
            AlignLimbSegmentSafe(lUpperArm, lLowerArm, Vector3.left, minSegment);
            AlignLimbSegmentSafe(lLowerArm, lHand, Vector3.left, minSegment);
            AlignLimbSegmentSafe(rUpperArm, rLowerArm, Vector3.right, minSegment);
            AlignLimbSegmentSafe(rLowerArm, rHand, Vector3.right, minSegment);

            // 3) Straighten Legs vertically (-Y) and Feet forward (+Z)
            AlignLimbSegmentSafe(lUpperLeg, lLowerLeg, Vector3.down, minSegment);
            AlignLimbSegmentSafe(lLowerLeg, lFoot, Vector3.down, minSegment);
            AlignLimbSegmentSafe(lFoot, lToes, Vector3.forward, minSegment * 0.5f);

            AlignLimbSegmentSafe(rUpperLeg, rLowerLeg, Vector3.down, minSegment);
            AlignLimbSegmentSafe(rLowerLeg, rFoot, Vector3.down, minSegment);
            AlignLimbSegmentSafe(rFoot, rToes, Vector3.forward, minSegment * 0.5f);
        }

        private static void AlignLimbSegmentSafe(Transform joint, Transform child, Vector3 worldTargetDir, float minLength)
        {
            if (joint == null || child == null) return;
            Vector3 current = child.position - joint.position;
            if (current.magnitude < minLength) return;
            Quaternion delta = Quaternion.FromToRotation(current.normalized, worldTargetDir.normalized);
            joint.rotation = delta * joint.rotation;
        }

        // =====================================================================
        // 6. REAL 3D WEAPON PREFABS FROM ASSET PACKS (SWATM4 + KENNEY BLASTERKIT)
        // =====================================================================

        private static GameObject[] BuildIntegratedWeaponPrefabs()
        {
            var result = new GameObject[6];

            MarkNormalMapsInFolder(VanceTexturesDir);
            var m4BaseMat = CreateOrUpdateUrpLitMaterial("Weapon_M4_PBR",
                FindTextureInFolder(VanceTexturesDir, "M4_9"),
                FindTextureInFolder(VanceTexturesDir, "m4_N_8"),
                null, Color.white, 0.35f, 0.45f);
            var m4SniperMat = CreateOrUpdateUrpLitMaterial("Weapon_M4_Precision_PBR",
                FindTextureInFolder(VanceTexturesDir, "M4_9"),
                FindTextureInFolder(VanceTexturesDir, "m4_N_8"),
                null, new Color(0.76f, 0.80f, 0.74f), 0.40f, 0.48f);
            var m4HeavyMat = CreateOrUpdateUrpLitMaterial("Weapon_M4_Heavy_PBR",
                FindTextureInFolder(VanceTexturesDir, "M4_9"),
                FindTextureInFolder(VanceTexturesDir, "m4_N_8"),
                null, new Color(0.68f, 0.66f, 0.62f), 0.45f, 0.40f);

            var blasterTex = AssetDatabase.LoadAssetAtPath<Texture2D>(BlasterTexPath);
            var pistolMat = CreateOrUpdateUrpLitMaterial("Weapon_Pistol_PBR", blasterTex, null, null, new Color(0.62f, 0.66f, 0.70f), 0.45f, 0.45f);
            var smgMat = CreateOrUpdateUrpLitMaterial("Weapon_SMG_PBR", blasterTex, null, null, new Color(0.55f, 0.60f, 0.64f), 0.48f, 0.42f);
            var shotgunMat = CreateOrUpdateUrpLitMaterial("Weapon_Shotgun_PBR", blasterTex, null, null, new Color(0.70f, 0.64f, 0.56f), 0.35f, 0.38f);

            Mesh m4Mesh = ExtractStaticMeshFromAsset(VanceModelPath, "SwatM4");

            // 0: Pistola DP-9 (Kenney blaster-b compact pistol)
            result[0] = BuildBlasterWeaponPrefab(0, "Weapon_0_Pistol_DP9", BlasterBDir, pistolMat,
                targetLength: 0.36f, targetScale: new Vector3(1f, 1f, 1f), addScope: false, addDrum: false, addSuppressor: false);

            // 1: Carabina C3 (SwatM4 real textured 3D M4 rifle from Solider walk.glb)
            result[1] = m4Mesh != null
                ? BuildM4WeaponPrefab(1, "Weapon_1_Carbine_M4", m4Mesh, m4BaseMat,
                    scale: new Vector3(1.0f, 1.0f, 1.0f), addScope: false, addDrum: false)
                : BuildBlasterWeaponPrefab(1, "Weapon_1_Carbine_C3", BlasterADir, pistolMat,
                    targetLength: 0.74f, targetScale: Vector3.one, addScope: false, addDrum: false, addSuppressor: false);

            // 2: Escopeta S12 (Kenney blaster-c heavy shotgun)
            result[2] = BuildBlasterWeaponPrefab(2, "Weapon_2_Shotgun_S12", BlasterCDir, shotgunMat,
                targetLength: 0.80f, targetScale: new Vector3(1.08f, 1.05f, 1.0f), addScope: false, addDrum: false, addSuppressor: false);

            // 3: Fusil de precision (SwatM4 Marksman DMR with long barrel & telescopic scope)
            result[3] = m4Mesh != null
                ? BuildM4WeaponPrefab(3, "Weapon_3_Precision_DMR", m4Mesh, m4SniperMat,
                    scale: new Vector3(0.96f, 0.98f, 1.24f), addScope: true, addDrum: false)
                : BuildBlasterWeaponPrefab(3, "Weapon_3_Precision", BlasterADir, m4SniperMat,
                    targetLength: 0.92f, targetScale: new Vector3(0.92f, 0.95f, 1.15f), addScope: true, addDrum: false, addSuppressor: false);

            // 4: Subfusil (Kenney blaster-a compact PDW + suppressor)
            result[4] = BuildBlasterWeaponPrefab(4, "Weapon_4_SMG_Abisal", BlasterADir, smgMat,
                targetLength: 0.56f, targetScale: new Vector3(0.92f, 0.92f, 0.95f), addScope: false, addDrum: false, addSuppressor: true);

            // 5: Ametralladora pesada (SwatM4 Heavy SAW variant with box ammo drum)
            result[5] = m4Mesh != null
                ? BuildM4WeaponPrefab(5, "Weapon_5_Heavy_SAW", m4Mesh, m4HeavyMat,
                    scale: new Vector3(1.22f, 1.18f, 1.10f), addScope: false, addDrum: true)
                : BuildBlasterWeaponPrefab(5, "Weapon_5_Heavy", BlasterCDir, m4HeavyMat,
                    targetLength: 0.88f, targetScale: new Vector3(1.2f, 1.15f, 1.1f), addScope: false, addDrum: true, addSuppressor: false);

            return result;
        }

        private static Mesh ExtractStaticMeshFromAsset(string assetPath, string targetNodeName)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (source == null) return null;
            foreach (var smr in source.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (smr.sharedMesh != null && (string.Equals(smr.name, targetNodeName, StringComparison.OrdinalIgnoreCase) || smr.sharedMesh.name == "Térháló"))
                    return smr.sharedMesh;
            }
            foreach (var mf in source.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh != null && string.Equals(mf.name, targetNodeName, StringComparison.OrdinalIgnoreCase))
                    return mf.sharedMesh;
            }
            return null;
        }

        private static GameObject BuildM4WeaponPrefab(int index, string prefabName, Mesh m4Mesh, Material mat, Vector3 scale, bool addScope, bool addDrum)
        {
            var root = new GameObject(prefabName);
            try
            {
                var body = new GameObject("M4_Mesh", typeof(MeshFilter), typeof(MeshRenderer));
                body.transform.SetParent(root.transform, false);
                body.GetComponent<MeshFilter>().sharedMesh = m4Mesh;
                body.GetComponent<MeshRenderer>().sharedMaterial = mat;

                // SwatM4 mesh is long along X (stock on shorter end, barrel on longer end).
                // Orient so barrel always points cleanly along +Z!
                Bounds raw = m4Mesh.bounds;
                float yaw = Mathf.Abs(raw.min.x) > Mathf.Abs(raw.max.x) ? 90f : -90f;
                body.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
                body.transform.localScale = scale;

                // Measure oriented bounds and place rear grip near z=0, barrel extending along +Z
                Bounds b = body.GetComponent<MeshRenderer>().bounds;
                float len = Mathf.Max(0.1f, b.size.z);
                float desiredLen = index == 3 ? 0.90f : index == 5 ? 0.84f : 0.76f;
                float norm = desiredLen / len;
                body.transform.localScale = Vector3.Scale(body.transform.localScale, Vector3.one * norm);

                b = body.GetComponent<MeshRenderer>().bounds;
                // Place weapon so receiver/grip is near (0, 0.04, 0.14) and barrel extends forward
                Vector3 shift = new Vector3(-b.center.x, 0.04f - b.center.y, 0.22f - b.center.z);
                body.transform.position += shift;
                b = body.GetComponent<MeshRenderer>().bounds;

                if (addScope)
                {
                    var scopeMat = CreateOrUpdateUrpLitMaterial("Weapon_Scope_Dark", null, null, null, new Color(0.12f, 0.13f, 0.14f), 0.65f, 0.55f);
                    AddAccessoryTube(root.transform, "PrecisionScope", new Vector3(0f, b.max.y + 0.025f, b.center.z - 0.04f), 0.022f, 0.30f, scopeMat);
                }
                if (addDrum)
                {
                    var drumMat = CreateOrUpdateUrpLitMaterial("Weapon_Drum_Olive", null, null, null, new Color(0.22f, 0.26f, 0.18f), 0.25f, 0.35f);
                    AddAccessoryBox(root.transform, "HeavyAmmoBox", new Vector3(0f, b.min.y + 0.02f, b.center.z - 0.02f), new Vector3(0.13f, 0.15f, 0.14f), drumMat);
                }

                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(root.transform, false);
                muzzle.localPosition = new Vector3(0f, b.center.y + 0.02f, Mathf.Clamp(b.max.z + 0.02f, 0.30f, 0.86f));

                string path = $"{WeaponsDir}/{prefabName}.prefab";
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static GameObject BuildBlasterWeaponPrefab(int index, string prefabName, string fbxPath, Material mat, float targetLength, Vector3 targetScale, bool addScope, bool addDrum, bool addSuppressor)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (source == null) return null;

            var root = new GameObject(prefabName);
            try
            {
                var visual = Object.Instantiate(source, root.transform, false);
                visual.name = "Model";
                foreach (var col in visual.GetComponentsInChildren<Collider>(true))
                    Object.DestroyImmediate(col);

                foreach (var r in visual.GetComponentsInChildren<Renderer>(true))
                    r.sharedMaterial = mat;

                if (TryComputeVisibleBounds(visual, out Bounds raw))
                {
                    // Rotate longest horizontal axis onto +Z if needed
                    if (raw.size.x > raw.size.z * 1.15f)
                        visual.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                }

                if (TryComputeVisibleBounds(visual, out Bounds b) && b.size.z > 0.01f)
                {
                    float norm = targetLength / b.size.z;
                    visual.transform.localScale = Vector3.Scale(targetScale, Vector3.one * norm);
                }

                if (TryComputeVisibleBounds(visual, out b))
                {
                    float zTarget = index == 0 ? 0.14f : 0.24f;
                    visual.transform.position += new Vector3(-b.center.x, 0.04f - b.center.y, zTarget - b.center.z);
                }

                TryComputeVisibleBounds(visual, out b);

                if (addSuppressor)
                {
                    var supMat = CreateOrUpdateUrpLitMaterial("Weapon_Suppressor", null, null, null, new Color(0.10f, 0.11f, 0.12f), 0.6f, 0.45f);
                    AddAccessoryTube(root.transform, "Suppressor", new Vector3(0f, b.center.y + 0.015f, b.max.z + 0.09f), 0.022f, 0.20f, supMat);
                    b.Encapsulate(new Vector3(0f, b.center.y, b.max.z + 0.19f));
                }
                if (addScope)
                {
                    var scopeMat = CreateOrUpdateUrpLitMaterial("Weapon_Scope_Dark", null, null, null, new Color(0.12f, 0.13f, 0.14f), 0.65f, 0.55f);
                    AddAccessoryTube(root.transform, "Scope", new Vector3(0f, b.max.y + 0.02f, b.center.z), 0.022f, 0.28f, scopeMat);
                }
                if (addDrum)
                {
                    var drumMat = CreateOrUpdateUrpLitMaterial("Weapon_Drum_Olive", null, null, null, new Color(0.22f, 0.26f, 0.18f), 0.25f, 0.35f);
                    AddAccessoryBox(root.transform, "HeavyAmmoBox", new Vector3(0f, b.min.y, b.center.z), new Vector3(0.13f, 0.15f, 0.14f), drumMat);
                }

                var muzzle = new GameObject("Muzzle").transform;
                muzzle.SetParent(root.transform, false);
                muzzle.localPosition = new Vector3(0f, b.center.y + 0.015f, Mathf.Clamp(b.max.z + 0.02f, 0.28f, 0.86f));

                string path = $"{WeaponsDir}/{prefabName}.prefab";
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void AddAccessoryTube(Transform parent, string name, Vector3 pos, float radius, float length, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            go.transform.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void AddAccessoryBox(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        // =====================================================================
        // 7. SHARED UTILITIES (TEXTURES, MATERIALS, BOUNDS, RENDERERS)
        // =====================================================================

        private static void MarkNormalMapsInFolder(string folder)
        {
            if (!Directory.Exists(folder)) return;
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                string name = Path.GetFileNameWithoutExtension(file);
                bool looksNormal = name.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.EndsWith("_N_1", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_N_4", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_N_6", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_N_8", StringComparison.OrdinalIgnoreCase)
                    || name.EndsWith("_N", StringComparison.OrdinalIgnoreCase);
                if (!looksNormal) continue;

                string assetPath = file.Replace('\\', '/');
                var ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null && ti.textureType != TextureImporterType.NormalMap)
                {
                    ti.textureType = TextureImporterType.NormalMap;
                    ti.SaveAndReimport();
                }
            }
        }

        private static Texture2D FindTextureInFolder(string folder, string partialName)
        {
            if (!Directory.Exists(folder)) return null;
            foreach (string file in Directory.GetFiles(folder))
            {
                if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
                string fn = Path.GetFileNameWithoutExtension(file);
                if (fn.IndexOf(partialName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(file.Replace('\\', '/'));
                    if (tex != null) return tex;
                }
            }
            return null;
        }

        private static Material CreateOrUpdateUrpLitMaterial(string name, Texture2D diffuse, Texture2D normal, Texture2D emissive, Color tint, float metallic, float smoothness)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            string path = $"{MaterialsDir}/{SanitizeFileName(name)}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(urpLit);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (urpLit != null)
            {
                mat.shader = urpLit;
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            if (diffuse != null) mat.SetTexture("_BaseMap", diffuse);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (emissive != null)
            {
                mat.SetTexture("_EmissionMap", emissive);
                mat.SetColor("_EmissionColor", Color.white * 1.4f);
                mat.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void EnableAllRenderersAndMeshes(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null) continue;
                r.gameObject.SetActive(true);
                r.enabled = true;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                r.receiveShadows = true;
            }
        }

        private static void EnsureFacingPositiveZ(GameObject root, Transform modelChild, (string human, string bone)[] boneMap)
        {
            Transform leftArm = null, rightArm = null;
            string leftName = null, rightName = null;
            foreach (var pair in boneMap)
            {
                if (pair.human == "LeftUpperArm") leftName = pair.bone;
                else if (pair.human == "RightUpperArm") rightName = pair.bone;
            }

            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == leftName) leftArm = t;
                else if (t.name == rightName) rightArm = t;
            }

            if (leftArm != null && rightArm != null)
            {
                Vector3 leftLocal = root.transform.InverseTransformPoint(leftArm.position);
                Vector3 rightLocal = root.transform.InverseTransformPoint(rightArm.position);
                if (rightLocal.x < leftLocal.x)
                    modelChild.Rotate(0f, 180f, 0f, Space.World);
            }
        }

        private static void DisableNamedChildren(GameObject root, string[] names)
        {
            if (names == null || names.Length == 0) return;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (string.Equals(t.name, names[i], StringComparison.OrdinalIgnoreCase))
                        t.gameObject.SetActive(false);
                }
            }
        }

        private static void NormalizeSingleRootScale(GameObject root, float targetHeight)
        {
            if (!TryComputeVisibleBounds(root, out Bounds bounds)) return;
            if (bounds.size.y > 0.05f)
            {
                float scale = targetHeight / bounds.size.y;
                if (Mathf.Abs(scale - 1f) > 0.02f)
                    root.transform.localScale = Vector3.one * scale;
            }
        }

        private static void NormalizeModelBounds(GameObject outerRoot, Transform modelChild, float targetHeight)
        {
            if (!TryComputeVisibleBounds(outerRoot, out Bounds bounds)) return;
            if (bounds.size.y > 0.05f)
            {
                float scale = targetHeight / bounds.size.y;
                modelChild.localScale *= scale;
            }

            if (TryComputeVisibleBounds(outerRoot, out bounds))
            {
                Vector3 shift = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
                modelChild.position += shift;
            }
        }

        private static bool TryComputeVisibleBounds(GameObject root, out Bounds total)
        {
            total = default;
            bool hasBounds = false;
            foreach (var r in root.GetComponentsInChildren<Renderer>(false))
            {
                if (r == null || !r.enabled) continue;
                if (!hasBounds) { total = r.bounds; hasBounds = true; }
                else total.Encapsulate(r.bounds);
            }
            return hasBounds;
        }

        private static void ConfigureAnimatorAndRenderers(GameObject root, Avatar avatar, RuntimeAnimatorController controller)
        {
            foreach (var smr in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                smr.updateWhenOffscreen = true;

            foreach (var col in root.GetComponentsInChildren<Collider>(true))
            {
                col.enabled = false;
                Object.DestroyImmediate(col);
            }

            var animator = root.GetComponent<Animator>();
            if (animator == null) animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.updateMode = AnimatorUpdateMode.Normal;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "Default";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Replace(' ', '_').Replace(':', '_');
        }

        // =====================================================================
        // 8. ENEMY 3D MODELS + GROUNDED MOCAP BASE + UPPER-BODY ZOMBIE LAYER
        // =====================================================================

        private static readonly (string human, string bone)[] MakeHumanBurntMap =
        {
            ("Hips", "root"),
            ("Spine", "spine05"),
            ("Chest", "spine03"),
            ("UpperChest", "spine01"),
            ("Neck", "neck01"),
            ("Head", "head"),
            ("LeftShoulder", "clavicle_L"),
            ("LeftUpperArm", "upperarm01_L"),
            ("LeftLowerArm", "lowerarm01_L"),
            ("LeftHand", "wrist_L"),
            ("RightShoulder", "clavicle_R"),
            ("RightUpperArm", "upperarm01_R"),
            ("RightLowerArm", "lowerarm01_R"),
            ("RightHand", "wrist_R"),
            ("LeftUpperLeg", "upperleg01_L"),
            ("LeftLowerLeg", "lowerleg01_L"),
            ("LeftFoot", "foot_L"),
            ("LeftToes", "toe1-1_L"),
            ("RightUpperLeg", "upperleg01_R"),
            ("RightLowerLeg", "lowerleg01_R"),
            ("RightFoot", "foot_R"),
            ("RightToes", "toe1-1_R"),
        };

        private static AnimatorController BuildEnemyAnimatorController(Dictionary<string, AnimationClip> proClips)
        {
            // Auto-detect optional real Mixamo Drunken / Zombie clips if present in Assets/Animations (outside Rifle packs)
            AnimationClip customDrunkIdle = FindCustomEnemyClip(loop: true, "drunk idle", "drunken idle");
            AnimationClip customDrunkWalk = FindCustomEnemyClip(loop: true, "drunk walk", "drunken walk", "drunk", "drunken", "stumble");
            AnimationClip customDrunkRun = FindCustomEnemyClip(loop: true, "drunk run", "drunken run");
            AnimationClip customZombieIdle = FindCustomEnemyClip(loop: true, "zombie idle");
            AnimationClip customZombieWalk = FindCustomEnemyClip(loop: true, "zombie walk");
            AnimationClip customZombieRun = FindCustomEnemyClip(loop: true, "zombie run", "zombie sprint");

            // Base Layer uses EXCLUSIVELY real MoCap clips (in-place via lockRootPositionXZ = false) so RootT.y
            // and feet remain 100% grounded. Calibrate timeScale to match enemy ground speeds cleanly.
            AnimationClip baseIdle = customZombieIdle ?? (proClips.TryGetValue("idle_aim", out var bia)
                ? bia
                : (proClips.TryGetValue("idle", out var bi) ? bi : null));
            AnimationClip drunkBaseIdle = customDrunkIdle ?? baseIdle;
            AnimationClip walkClip = customZombieWalk ?? (proClips.TryGetValue("walk_f", out var wf) ? wf : baseIdle);
            AnimationClip drunkWalkClip = customDrunkWalk ?? walkClip;
            AnimationClip runClip = customZombieRun ?? (proClips.TryGetValue("run_f", out var rf) ? rf : walkClip);
            AnimationClip drunkRunClip = customDrunkRun ?? customDrunkWalk ?? runClip;
            AnimationClip sprintClip = customZombieRun ?? (proClips.TryGetValue("sprint_f", out var sf) ? sf : runClip);

            AnimationClip death0 = proClips.TryGetValue("death", out var d0) ? d0 : baseIdle;
            AnimationClip death1 = proClips.TryGetValue("death_back", out var d1) ? d1 : death0;
            AnimationClip death2 = proClips.TryGetValue("death_right", out var d2) ? d2 : death0;
            AnimationClip death3 = proClips.TryGetValue("death_headshot", out var d3) ? d3 : death0;

            // Layer 1 (ZombieUpperBody): 2 distinct enemy style variations controlled by Style (0 = Simple Zombie, 1 = Drunken)
            var idleUpperClip = CreateOrUpdateMuscleClip("Enemy_ZombieIdle", 2.4f, loop: true, BuildZombieIdleUpperCurves);
            var walkUpperClip = CreateOrUpdateMuscleClip("Enemy_ZombieArmsReach", 1.2f, loop: true, BuildZombieWalkUpperCurves);
            var chaseUpperClip = CreateOrUpdateMuscleClip("Enemy_ZombieChaseUpper", 0.9f, loop: true, BuildZombieChaseUpperCurves);

            var drunkIdleUpperClip = CreateOrUpdateMuscleClip("Enemy_DrunkenIdleUpper", 2.8f, loop: true, BuildDrunkenIdleUpperCurves);
            var drunkWalkUpperClip = CreateOrUpdateMuscleClip("Enemy_DrunkenWalkUpper", 1.8f, loop: true, BuildDrunkenWalkUpperCurves);
            var drunkChaseUpperClip = CreateOrUpdateMuscleClip("Enemy_DrunkenChaseUpper", 1.2f, loop: true, BuildDrunkenChaseUpperCurves);

            var attackClip = FindCustomEnemyClip(loop: false, "attack", "claw", "bite", "swipe", "punch")
                ?? CreateOrUpdateMuscleClip("Enemy_AttackSwipe", 0.62f, loop: false, BuildZombieAttackSwipeCurves);
            var heavyAttackClip = FindCustomEnemyClip(loop: false, "heavy", "slam", "smash")
                ?? CreateOrUpdateMuscleClip("Enemy_HeavySlam", 0.92f, loop: false, BuildZombieHeavySlamCurves);
            var screamClip = FindCustomEnemyClip(loop: false, "scream", "roar", "howl", "taunt")
                ?? CreateOrUpdateMuscleClip("Enemy_Scream", 1.05f, loop: false, BuildZombieScreamCurves);
            var staggerClip = FindCustomEnemyClip(loop: false, "stagger", "hit", "flinch", "reaction")
                ?? CreateOrUpdateMuscleClip("Enemy_Stagger", 0.36f, loop: false, BuildZombieStaggerCurves);

            var upperMask = CreateOrUpdateEnemyUpperMask();

            if (AssetDatabase.LoadAssetAtPath<AnimatorController>(EnemyControllerPath) != null)
                AssetDatabase.DeleteAsset(EnemyControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(EnemyControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Style", AnimatorControllerParameterType.Float); // 0 = Simple Zombie, 1 = Drunken
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Scream", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Stagger", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);
            controller.AddParameter("DeathKind", AnimatorControllerParameterType.Int);

            var sm = controller.layers[0].stateMachine;

            // Base Layer (Layer 0): 2D Cartesian Locomotion (Speed on X, Style 0..1 on Y) with calibrated timeScales
            var locoState = controller.CreateBlendTreeInController("Locomotion", out BlendTree locoTree, 0);
            locoTree.blendType = BlendTreeType.FreeformCartesian2D;
            locoTree.blendParameter = "Speed";
            locoTree.blendParameterY = "Style";
            locoTree.useAutomaticThresholds = false;

            var baseMotions = new List<ChildMotion>();
            // Style = 0 (Simple Zombie)
            if (baseIdle != null) baseMotions.Add(new ChildMotion { motion = baseIdle, position = new Vector2(0.0f, 0f), timeScale = 1.0f, directBlendParameter = "Blend" });
            if (walkClip != null) baseMotions.Add(new ChildMotion { motion = walkClip, position = new Vector2(0.55f, 0f), timeScale = 0.72f, directBlendParameter = "Blend" });
            if (runClip != null) baseMotions.Add(new ChildMotion { motion = runClip, position = new Vector2(1.05f, 0f), timeScale = 0.56f, directBlendParameter = "Blend" });
            if (sprintClip != null) baseMotions.Add(new ChildMotion { motion = sprintClip, position = new Vector2(1.50f, 0f), timeScale = 0.64f, directBlendParameter = "Blend" });
            // Style = 1 (Drunken variation: heavier/slower deliberate cadence, or real Drunken FBX clips when present)
            if (drunkBaseIdle != null) baseMotions.Add(new ChildMotion { motion = drunkBaseIdle, position = new Vector2(0.0f, 1f), timeScale = 0.85f, directBlendParameter = "Blend" });
            if (drunkWalkClip != null) baseMotions.Add(new ChildMotion { motion = drunkWalkClip, position = new Vector2(0.55f, 1f), timeScale = 0.62f, directBlendParameter = "Blend" });
            if (drunkRunClip != null) baseMotions.Add(new ChildMotion { motion = drunkRunClip, position = new Vector2(1.05f, 1f), timeScale = 0.52f, directBlendParameter = "Blend" });
            if (sprintClip != null) baseMotions.Add(new ChildMotion { motion = sprintClip, position = new Vector2(1.50f, 1f), timeScale = 0.60f, directBlendParameter = "Blend" });
            locoTree.children = baseMotions.ToArray();
            sm.defaultState = locoState;

            // 4 Real MoCap Death variants on Base Layer (front, back, side, headshot)
            AnimationClip[] deaths = { death0, death1, death2, death3 };
            for (int i = 0; i < deaths.Length; i++)
            {
                var dState = sm.AddState("Dead_" + i, new Vector3(520, -40 + i * 70, 0));
                dState.motion = deaths[i];

                var toDead = sm.AddAnyStateTransition(dState);
                toDead.hasExitTime = false;
                toDead.canTransitionToSelf = false;
                toDead.duration = 0.08f;
                toDead.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
                toDead.AddCondition(AnimatorConditionMode.Equals, i, "DeathKind");

                var revive = dState.AddTransition(locoState);
                revive.hasExitTime = false;
                revive.duration = 0.12f;
                revive.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
            }

            // Layer 1: Upper-Body Zombie/Drunken Posture + Actions (Body + Head + Arms + Fingers ONLY; Root & Legs untouched)
            var upperSm = new AnimatorStateMachine { name = "ZombieUpperBody", hideFlags = HideFlags.HideInHierarchy };
            AssetDatabase.AddObjectToAsset(upperSm, controller);
            var upperLayer = new AnimatorControllerLayer
            {
                name = "ZombieUpperBody",
                avatarMask = upperMask,
                blendingMode = AnimatorLayerBlendingMode.Override,
                defaultWeight = 1.0f,
                stateMachine = upperSm
            };
            controller.AddLayer(upperLayer);

            var upperLocoTree = new BlendTree
            {
                name = "UpperLocomotionTree",
                blendType = BlendTreeType.FreeformCartesian2D,
                blendParameter = "Speed",
                blendParameterY = "Style",
                useAutomaticThresholds = false,
                hideFlags = HideFlags.HideInHierarchy
            };
            AssetDatabase.AddObjectToAsset(upperLocoTree, controller);
            upperLocoTree.children = new[]
            {
                // Style = 0 (Simple Zombie)
                new ChildMotion { motion = idleUpperClip, position = new Vector2(0.0f, 0f), timeScale = 1f, directBlendParameter = "Blend" },
                new ChildMotion { motion = walkUpperClip, position = new Vector2(0.55f, 0f), timeScale = 1f, directBlendParameter = "Blend" },
                new ChildMotion { motion = chaseUpperClip, position = new Vector2(1.20f, 0f), timeScale = 1f, directBlendParameter = "Blend" },
                // Style = 1 (Drunken Zombie variation)
                new ChildMotion { motion = drunkIdleUpperClip, position = new Vector2(0.0f, 1f), timeScale = 1f, directBlendParameter = "Blend" },
                new ChildMotion { motion = drunkWalkUpperClip, position = new Vector2(0.55f, 1f), timeScale = 1f, directBlendParameter = "Blend" },
                new ChildMotion { motion = drunkChaseUpperClip, position = new Vector2(1.20f, 1f), timeScale = 1f, directBlendParameter = "Blend" },
            };

            var upperLocoState = upperSm.AddState("UpperLocomotion", new Vector3(40, 40, 0));
            upperLocoState.motion = upperLocoTree;
            upperSm.defaultState = upperLocoState;

            var attackState = AddEnemyActionState(upperSm, "Attack", attackClip, new Vector3(320, -80, 0), upperLocoState, "Attack", 0.08f, 0.88f);
            var heavyState = AddEnemyActionState(upperSm, "HeavyAttack", heavyAttackClip, new Vector3(320, 0, 0), upperLocoState, "HeavyAttack", 0.10f, 0.90f);
            var screamState = AddEnemyActionState(upperSm, "Scream", screamClip, new Vector3(320, 80, 0), upperLocoState, "Scream", 0.10f, 0.90f);
            var staggerState = AddEnemyActionState(upperSm, "Stagger", staggerClip, new Vector3(320, 160, 0), upperLocoState, "Stagger", 0.05f, 0.85f);

            AddInterruptTrigger(attackState, staggerState, "Stagger", 0.05f);
            AddInterruptTrigger(heavyState, staggerState, "Stagger", 0.05f);
            AddInterruptTrigger(screamState, staggerState, "Stagger", 0.05f);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState AddEnemyActionState(
            AnimatorStateMachine sm, string name, AnimationClip clip, Vector3 pos,
            AnimatorState returnTo, string triggerParam, float enterDuration, float exitTime)
        {
            var state = sm.AddState(name, pos);
            state.motion = clip;

            var enter = returnTo.AddTransition(state);
            enter.hasExitTime = false;
            enter.duration = enterDuration;
            enter.AddCondition(AnimatorConditionMode.If, 0f, triggerParam);
            enter.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");

            var exit = state.AddTransition(returnTo);
            exit.hasExitTime = true;
            exit.exitTime = exitTime;
            exit.duration = 0.12f;
            return state;
        }

        private static void AddInterruptTrigger(AnimatorState from, AnimatorState to, string triggerParam, float duration)
        {
            var tr = from.AddTransition(to);
            tr.hasExitTime = false;
            tr.duration = duration;
            tr.AddCondition(AnimatorConditionMode.If, 0f, triggerParam);
            tr.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");
        }

        private static AnimationClip FindCustomEnemyClip(bool loop, params string[] keywords)
        {
            if (!Directory.Exists("Assets/Animations")) return null;
            foreach (string file in Directory.GetFiles("Assets/Animations", "*.fbx", SearchOption.AllDirectories))
            {
                string norm = file.Replace('\\', '/');
                if (norm.Contains("/Pro Rifle Pack/") || norm.Contains("/Rifle 8-Way Locomotion Pack/") || norm.Contains("/Lite Rifle Pack/"))
                    continue;
                string lower = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                for (int i = 0; i < keywords.Length; i++)
                {
                    if (lower.Contains(keywords[i]))
                    {
                        var imp = AssetImporter.GetAtPath(norm) as ModelImporter;
                        if (imp != null)
                        {
                            bool dirty = imp.animationType != ModelImporterAnimationType.Human || imp.clipAnimations == null || imp.clipAnimations.Length == 0;
                            if (dirty)
                            {
                                imp.animationType = ModelImporterAnimationType.Human;
                                imp.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                                imp.importAnimation = true;
                                var clips = imp.clipAnimations != null && imp.clipAnimations.Length > 0 ? imp.clipAnimations : imp.defaultClipAnimations;
                                for (int c = 0; c < clips.Length; c++)
                                {
                                    clips[c].loopTime = loop;
                                    clips[c].loopPose = loop;
                                    clips[c].lockRootRotation = true;
                                    clips[c].keepOriginalOrientation = true;
                                    clips[c].lockRootHeightY = true;
                                    clips[c].keepOriginalPositionY = false;
                                    clips[c].heightFromFeet = true;
                                    clips[c].lockRootPositionXZ = !loop;
                                    clips[c].keepOriginalPositionXZ = false;
                                }
                                imp.clipAnimations = clips;
                                imp.SaveAndReimport();
                            }
                        }
                        var clip = LoadPrimaryClip(norm);
                        if (clip != null) return clip;
                    }
                }
            }
            return null;
        }

        private static AvatarMask CreateOrUpdateEnemyUpperMask()
        {
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(EnemyUpperMaskPath);
            if (mask == null)
            {
                mask = new AvatarMask();
                AssetDatabase.CreateAsset(mask, EnemyUpperMaskPath);
            }

            // Disable EVERYTHING first (especially Root, LeftLeg, RightLeg, FootIK, HandIK)
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);

            // Enable Body (Spine/Chest/UpperChest) + Head + Arms + Fingers so rifle sideways twist is fully replaced
            // while Base Layer retains 100% control over RootT.y, RootQ, Pelvis, Legs, and Foot grounding.
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Body, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.Head, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightArm, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFingers, true);
            mask.SetHumanoidBodyPartActive(AvatarMaskBodyPart.RightFingers, true);
            EditorUtility.SetDirty(mask);
            return mask;
        }

        private static AnimationClip CreateOrUpdateMuscleClip(string clipName, float duration, bool loop, Action<AnimationClip, float> populate)
        {
            string path = $"{EnemyClipsDir}/{clipName}.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { name = clipName, frameRate = 30f };
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                clip.ClearCurves();
                clip.frameRate = 30f;
            }

            populate(clip, duration);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.loopBlend = loop;
            settings.stopTime = duration;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static HashSet<string> validMuscles;

        private static void SetMuscle(AnimationClip clip, string muscleName, params (float t, float v)[] keys)
        {
            if (validMuscles == null)
                validMuscles = new HashSet<string>(HumanTrait.MuscleName, StringComparer.Ordinal);
            if (!validMuscles.Contains(muscleName)) return;

            var curve = new AnimationCurve();
            for (int i = 0; i < keys.Length; i++)
                curve.AddKey(new Keyframe(keys[i].t, keys[i].v));
            for (int i = 0; i < curve.length; i++)
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);

            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), muscleName), curve);
        }

        /// <summary>
        /// Explicitly sets neutral baselines on all upper-body muscles so the rifle-holding torso twist
        /// and arm rotations from Layer 0 never bleed through onto the zombie upper body.
        /// </summary>
        private static void NeutralizeUpperBodyBase(AnimationClip clip, float d)
        {
            string[] zeroed =
            {
                "Spine Front-Back", "Spine Left-Right", "Spine Twist Left-Right",
                "Chest Front-Back", "Chest Left-Right", "Chest Twist Left-Right",
                "UpperChest Front-Back", "UpperChest Left-Right", "UpperChest Twist Left-Right",
                "Neck Nod Down-Up", "Neck Tilt Left-Right", "Neck Turn Left-Right",
                "Head Nod Down-Up", "Head Tilt Left-Right", "Head Turn Left-Right",
                "Left Shoulder Down-Up", "Left Shoulder Front-Back",
                "Right Shoulder Down-Up", "Right Shoulder Front-Back",
                "Left Arm Down-Up", "Left Arm Front-Back", "Left Arm Twist In-Out",
                "Right Arm Down-Up", "Right Arm Front-Back", "Right Arm Twist In-Out",
                "Left Forearm Stretch", "Left Forearm Twist In-Out",
                "Right Forearm Stretch", "Right Forearm Twist In-Out",
                "Left Hand Down-Up", "Left Hand In-Out",
                "Right Hand Down-Up", "Right Hand In-Out",
            };
            for (int i = 0; i < zeroed.Length; i++)
                SetMuscle(clip, zeroed[i], (0f, 0f), (d, 0f));
        }

        private static void BuildZombieIdleUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Steady, upright-with-slight-slouch zombie idle (ZERO side-to-side swaying, only subtle breathing)
            SetMuscle(clip, "Spine Front-Back", (0f, -0.08f), (d * 0.5f, -0.11f), (d, -0.08f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.06f), (d * 0.5f, -0.09f), (d, -0.06f));
            SetMuscle(clip, "Neck Nod Down-Up", (0f, 0.05f), (d * 0.5f, 0.08f), (d, 0.05f));
            SetMuscle(clip, "Left Shoulder Front-Back", (0f, 0.10f), (d, 0.10f));
            SetMuscle(clip, "Right Shoulder Front-Back", (0f, 0.10f), (d, 0.10f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.60f), (d * 0.5f, -0.57f), (d, -0.60f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.60f), (d * 0.5f, -0.57f), (d, -0.60f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.16f), (d, 0.16f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.16f), (d, 0.16f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.28f), (d, -0.28f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.28f), (d, -0.28f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.18f), (d, -0.18f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.18f), (d, -0.18f));
        }

        private static void BuildZombieWalkUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Steady forward-facing zombie walk upper body (ZERO artificial lateral wobble/sway)
            SetMuscle(clip, "Spine Front-Back", (0f, -0.12f), (d * 0.5f, -0.14f), (d, -0.12f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.08f), (d * 0.5f, -0.10f), (d, -0.08f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0.06f), (d, 0.06f));
            SetMuscle(clip, "Left Shoulder Front-Back", (0f, 0.18f), (d, 0.18f));
            SetMuscle(clip, "Right Shoulder Front-Back", (0f, 0.18f), (d, 0.18f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.34f), (d * 0.5f, -0.30f), (d, -0.34f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.30f), (d * 0.5f, -0.34f), (d, -0.30f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.40f), (d * 0.5f, 0.44f), (d, 0.40f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.44f), (d * 0.5f, 0.40f), (d, 0.44f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.30f), (d, -0.30f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.30f), (d, -0.30f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.20f), (d, -0.20f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.20f), (d, -0.20f));
        }

        private static void BuildZombieChaseUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Steady feral chase upper body (forward lean + outstretched arms, ZERO lateral swaying)
            SetMuscle(clip, "Spine Front-Back", (0f, -0.18f), (d * 0.5f, -0.20f), (d, -0.18f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.12f), (d * 0.5f, -0.14f), (d, -0.12f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0.12f), (d, 0.12f));
            SetMuscle(clip, "Left Shoulder Front-Back", (0f, 0.28f), (d, 0.28f));
            SetMuscle(clip, "Right Shoulder Front-Back", (0f, 0.28f), (d, 0.28f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.14f), (d * 0.5f, -0.10f), (d, -0.14f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.10f), (d * 0.5f, -0.14f), (d, -0.10f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.56f), (d * 0.5f, 0.62f), (d, 0.56f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.62f), (d * 0.5f, 0.56f), (d, 0.62f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.26f), (d, -0.26f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.26f), (d, -0.26f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.22f), (d, -0.22f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.22f), (d, -0.22f));
        }

        private static void BuildDrunkenIdleUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Drunken lopsided infected idle: asymmetric dropped shoulder, slumped chest, tilted head, heavy loose arms
            SetMuscle(clip, "Spine Front-Back", (0f, -0.14f), (d * 0.5f, -0.20f), (d, -0.14f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.10f), (d * 0.5f, -0.15f), (d, -0.10f));
            SetMuscle(clip, "Spine Left-Right", (0f, 0.14f), (d * 0.5f, 0.08f), (d, 0.14f));
            SetMuscle(clip, "Chest Left-Right", (0f, 0.10f), (d * 0.5f, 0.06f), (d, 0.10f));
            SetMuscle(clip, "Neck Tilt Left-Right", (0f, -0.18f), (d * 0.5f, -0.12f), (d, -0.18f));
            SetMuscle(clip, "Head Tilt Left-Right", (0f, -0.24f), (d * 0.5f, -0.16f), (d, -0.24f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, -0.12f), (d * 0.5f, -0.04f), (d, -0.12f));
            SetMuscle(clip, "Left Shoulder Down-Up", (0f, -0.20f), (d, -0.20f));
            SetMuscle(clip, "Right Shoulder Down-Up", (0f, 0.12f), (d, 0.12f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.68f), (d * 0.5f, -0.62f), (d, -0.68f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.45f), (d * 0.5f, -0.40f), (d, -0.45f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.24f), (d, 0.24f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.08f), (d, 0.08f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.18f), (d, -0.18f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.42f), (d, -0.42f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.28f), (d, -0.28f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.15f), (d, -0.15f));
        }

        private static void BuildDrunkenWalkUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Drunken stumbling walk: sluggish asymmetric lurch, dropped left shoulder, head lolling, heavy hanging arms
            SetMuscle(clip, "Spine Front-Back", (0f, -0.18f), (d * 0.5f, -0.25f), (d, -0.18f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.14f), (d * 0.5f, -0.20f), (d, -0.14f));
            SetMuscle(clip, "Spine Left-Right", (0f, 0.16f), (d * 0.5f, 0.06f), (d, 0.16f));
            SetMuscle(clip, "Chest Left-Right", (0f, 0.12f), (d * 0.5f, 0.04f), (d, 0.12f));
            SetMuscle(clip, "Spine Twist Left-Right", (0f, 0.10f), (d * 0.5f, 0.02f), (d, 0.10f));
            SetMuscle(clip, "Neck Tilt Left-Right", (0f, -0.20f), (d * 0.5f, -0.12f), (d, -0.20f));
            SetMuscle(clip, "Head Tilt Left-Right", (0f, -0.26f), (d * 0.5f, -0.14f), (d, -0.26f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, -0.06f), (d * 0.5f, 0.08f), (d, -0.06f));
            SetMuscle(clip, "Left Shoulder Down-Up", (0f, -0.22f), (d, -0.22f));
            SetMuscle(clip, "Right Shoulder Down-Up", (0f, 0.14f), (d, 0.14f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.58f), (d * 0.5f, -0.46f), (d, -0.58f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.28f), (d * 0.5f, -0.38f), (d, -0.28f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.34f), (d * 0.5f, 0.46f), (d, 0.34f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.20f), (d * 0.5f, 0.36f), (d, 0.20f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.16f), (d, -0.16f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.40f), (d, -0.40f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.30f), (d, -0.30f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.18f), (d, -0.18f));
        }

        private static void BuildDrunkenChaseUpperCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Drunken aggressive lunge: heavy off-kilter forward plunge with asymmetric grabbing arms
            SetMuscle(clip, "Spine Front-Back", (0f, -0.24f), (d * 0.5f, -0.30f), (d, -0.24f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.18f), (d * 0.5f, -0.22f), (d, -0.18f));
            SetMuscle(clip, "Spine Left-Right", (0f, 0.12f), (d * 0.5f, 0.04f), (d, 0.12f));
            SetMuscle(clip, "Head Tilt Left-Right", (0f, -0.20f), (d * 0.5f, -0.10f), (d, -0.20f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0.14f), (d, 0.14f));
            SetMuscle(clip, "Left Shoulder Down-Up", (0f, -0.16f), (d, -0.16f));
            SetMuscle(clip, "Right Shoulder Down-Up", (0f, 0.12f), (d, 0.12f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.32f), (d * 0.5f, -0.22f), (d, -0.32f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.06f), (d * 0.5f, -0.16f), (d, -0.06f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.52f), (d * 0.5f, 0.62f), (d, 0.52f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.64f), (d * 0.5f, 0.52f), (d, 0.64f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.22f), (d, -0.22f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.34f), (d, -0.34f));
            SetMuscle(clip, "Left Hand Down-Up", (0f, -0.26f), (d, -0.26f));
            SetMuscle(clip, "Right Hand Down-Up", (0f, -0.20f), (d, -0.20f));
        }

        private static void BuildZombieAttackSwipeCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Anticipation windup -> violent forward claw lunge & horizontal slash -> follow-through
            SetMuscle(clip, "Spine Front-Back", (0f, -0.12f), (d * 0.32f, 0.16f), (d * 0.62f, -0.36f), (d, -0.14f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.10f), (d * 0.32f, 0.18f), (d * 0.62f, -0.34f), (d, -0.12f));
            SetMuscle(clip, "Spine Twist Left-Right", (0f, 0f), (d * 0.32f, 0.38f), (d * 0.65f, -0.45f), (d, 0f));
            SetMuscle(clip, "Chest Twist Left-Right", (0f, 0f), (d * 0.32f, 0.42f), (d * 0.65f, -0.45f), (d, 0f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.25f), (d * 0.32f, 0.45f), (d * 0.60f, -0.15f), (d, -0.35f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.30f), (d * 0.32f, -0.25f), (d * 0.62f, 0.78f), (d, 0.35f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.35f), (d * 0.32f, -0.60f), (d * 0.62f, -0.10f), (d, -0.35f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.30f), (d * 0.32f, 0.15f), (d * 0.65f, -0.25f), (d, -0.35f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.35f), (d * 0.32f, 0.45f), (d * 0.65f, 0.65f), (d, 0.35f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.30f), (d * 0.32f, -0.40f), (d * 0.65f, -0.20f), (d, -0.30f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0.05f), (d * 0.32f, 0.25f), (d * 0.62f, -0.22f), (d, 0.05f));
        }

        private static void BuildZombieHeavySlamCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Brute / Large Boss two-handed overhead windup and heavy slam
            SetMuscle(clip, "Spine Front-Back", (0f, -0.14f), (d * 0.42f, 0.30f), (d * 0.72f, -0.48f), (d, -0.16f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.12f), (d * 0.42f, 0.28f), (d * 0.72f, -0.42f), (d, -0.14f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.25f), (d * 0.42f, 0.78f), (d * 0.72f, -0.40f), (d, -0.30f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.25f), (d * 0.42f, 0.78f), (d * 0.72f, -0.40f), (d, -0.30f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.35f), (d * 0.42f, 0.20f), (d * 0.72f, 0.75f), (d, 0.35f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.35f), (d * 0.42f, 0.20f), (d * 0.72f, 0.75f), (d, 0.35f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.35f), (d * 0.42f, -0.50f), (d * 0.72f, -0.10f), (d, -0.30f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.35f), (d * 0.42f, -0.50f), (d * 0.72f, -0.10f), (d, -0.30f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0.05f), (d * 0.42f, 0.35f), (d * 0.72f, -0.30f), (d, 0.05f));
        }

        private static void BuildZombieScreamCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Screamer arches torso backward, throws head up, flares arms outward
            SetMuscle(clip, "Spine Front-Back", (0f, -0.14f), (d * 0.25f, 0.34f), (d * 0.72f, 0.28f), (d, -0.14f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.12f), (d * 0.25f, 0.36f), (d * 0.72f, 0.30f), (d, -0.12f));
            SetMuscle(clip, "Neck Nod Down-Up", (0f, 0f), (d * 0.25f, 0.48f), (d * 0.75f, 0.42f), (d, 0f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0f), (d * 0.25f, 0.55f), (d * 0.75f, 0.48f), (d, 0f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.30f), (d * 0.25f, 0.15f), (d * 0.75f, 0.08f), (d, -0.30f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.30f), (d * 0.25f, 0.15f), (d * 0.75f, 0.08f), (d, -0.30f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.30f), (d * 0.25f, -0.35f), (d * 0.75f, -0.28f), (d, 0.30f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.30f), (d * 0.25f, -0.35f), (d * 0.75f, -0.28f), (d, 0.30f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.30f), (d * 0.25f, -0.45f), (d, -0.30f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.30f), (d * 0.25f, -0.45f), (d, -0.30f));
        }

        private static void BuildZombieStaggerCurves(AnimationClip clip, float d)
        {
            NeutralizeUpperBodyBase(clip, d);
            // Sharp upper-body recoil when shot or parried (legs stay firmly grounded)
            SetMuscle(clip, "Spine Front-Back", (0f, -0.10f), (d * 0.35f, 0.36f), (d, -0.12f));
            SetMuscle(clip, "Chest Front-Back", (0f, -0.10f), (d * 0.35f, 0.40f), (d, -0.12f));
            SetMuscle(clip, "Head Nod Down-Up", (0f, 0f), (d * 0.35f, 0.48f), (d, 0.05f));
            SetMuscle(clip, "Left Arm Down-Up", (0f, -0.25f), (d * 0.35f, 0.25f), (d, -0.30f));
            SetMuscle(clip, "Right Arm Down-Up", (0f, -0.25f), (d * 0.35f, 0.30f), (d, -0.30f));
            SetMuscle(clip, "Left Arm Front-Back", (0f, 0.35f), (d * 0.35f, -0.28f), (d, 0.30f));
            SetMuscle(clip, "Right Arm Front-Back", (0f, 0.35f), (d * 0.35f, -0.28f), (d, 0.30f));
            SetMuscle(clip, "Left Forearm Stretch", (0f, -0.30f), (d * 0.35f, -0.45f), (d, -0.30f));
            SetMuscle(clip, "Right Forearm Stretch", (0f, -0.30f), (d * 0.35f, -0.45f), (d, -0.30f));
        }

        private static EnemyVisualEntry[] BuildEnemyVisualEntries(RuntimeAnimatorController enemyController, bool forceRebuild)
        {
            // Configure zombie_theBurnt.fbx with an explicit upright T-Pose MakeHuman avatar
            // (its raw FBX bind pose is crouched mid-stride with arms pointing forward!)
            ConfigureBurntZombieHumanoid(BurntZombieModelPath, TexturesDir + "/BurntZombie", forceRebuild);

            var clothTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Industrial/Textures/WornCloth_v1.png");
            var burntTex = FindTextureInFolder(TexturesDir + "/BurntZombie", "Diffuse")
                ?? FindTextureInFolder(TexturesDir + "/BurntZombie", "Albedo")
                ?? FindTextureInFolder(TexturesDir + "/BurntZombie", "BaseColor")
                ?? clothTex;
            var burntNorm = FindTextureInFolder(TexturesDir + "/BurntZombie", "Normal");

            var walkerMat = CreateEnemyMaterial("Enemy_Walker_Burnt", burntTex, burntNorm, new Color(0.32f, 0.28f, 0.25f), Color.black, 0.08f, 0.22f);
            var bruteMat = CreateEnemyMaterial("Enemy_Brute_CharredEmber", burntTex, burntNorm, new Color(0.20f, 0.16f, 0.15f), new Color(0.45f, 0.12f, 0.04f), 0.18f, 0.30f);
            var runnerMat = CreateEnemyMaterial("Enemy_Runner_Feral", clothTex, null, new Color(0.48f, 0.24f, 0.22f), new Color(0.08f, 0.01f, 0.01f), 0.05f, 0.32f);
            var screamerMat = CreateEnemyMaterial("Enemy_Screamer_Bio", clothTex, null, new Color(0.56f, 0.62f, 0.50f), new Color(0.08f, 0.26f, 0.12f), 0.05f, 0.28f);

            // Always use UNIFORM scaling on every Humanoid enemy prefab so limbs and torsos are never squashed!
            var list = new List<EnemyVisualEntry>();
            list.Add(BuildEnemyPrefabEntry(InfectedKind.Walker, "Walker_Burnt", BurntZombieModelPath, 1.80f, walkerMat, enemyController));
            list.Add(BuildEnemyPrefabEntry(InfectedKind.Runner, "Runner_Feral", AnimSourceSkeletonPath, 1.74f, runnerMat, enemyController));
            list.Add(BuildEnemyPrefabEntry(InfectedKind.Brute, "Brute_BurntHeavy", BurntZombieModelPath, 2.06f, bruteMat, enemyController));
            list.Add(BuildEnemyPrefabEntry(InfectedKind.Screamer, "Screamer_Bio", AnimSourceSkeletonPath, 1.84f, screamerMat, enemyController));
            return list.Where(e => e != null).ToArray();
        }

        private static void ConfigureBurntZombieHumanoid(string fbxPath, string extractDir, bool forceRebuild)
        {
            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer == null) return;

            Directory.CreateDirectory(extractDir);
            AssetDatabase.Refresh();
            bool hasExtracted = Directory.GetFiles(extractDir, "*.png").Length > 0 || Directory.GetFiles(extractDir, "*.jpg").Length > 0;
            if (forceRebuild || !hasExtracted)
            {
                importer.ExtractTextures(extractDir);
                AssetDatabase.Refresh();
                MarkNormalMapsInFolder(extractDir);
            }

            var sourceGo = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            if (sourceGo == null) return;

            var existingAvatar = LoadAvatarFromAsset(fbxPath);
            bool hasExplicitMap = importer.humanDescription.human != null && importer.humanDescription.human.Length >= 15;
            if (forceRebuild || importer.animationType != ModelImporterAnimationType.Human || !hasExplicitMap || existingAvatar == null || !existingAvatar.isValid || !existingAvatar.isHuman)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = false;

                var temp = Object.Instantiate(sourceGo);
                temp.name = sourceGo.name;
                temp.transform.position = Vector3.zero;
                temp.transform.rotation = Quaternion.identity;
                temp.transform.localScale = Vector3.one;
                try
                {
                    var byName = new Dictionary<string, Transform>(StringComparer.Ordinal);
                    foreach (var t in temp.GetComponentsInChildren<Transform>(true))
                    {
                        if (!byName.ContainsKey(t.name))
                            byName.Add(t.name, t);
                    }

                    EnforceMakeHumanBurntTPose(byName);
                    var desc = BuildHumanDescriptionFromHierarchy(temp, MakeHumanBurntMap, straightenLimbsOnly: false);
                    importer.humanDescription = desc;
                }
                finally
                {
                    Object.DestroyImmediate(temp);
                }

                importer.SaveAndReimport();
                var rebuilt = LoadAvatarFromAsset(fbxPath);
                Debug.Log($"[Zombtoy] Avatar zombie_theBurnt (upright MakeHuman T-Pose): valid={rebuilt != null && rebuilt.isValid}, human={rebuilt != null && rebuilt.isHuman}");
            }
        }

        /// <summary>
        /// Straightens zombie_theBurnt.fbx into a clean upright Unity T-Pose by rotating ONLY mapped Humanoid bones
        /// (never modifying unmapped MakeHuman in-between bones like upperleg02/lowerleg02/spine04 so Unity reports zero rig errors).
        /// </summary>
        private static void EnforceMakeHumanBurntTPose(Dictionary<string, Transform> byName)
        {
            Transform Get(string boneName) => byName.TryGetValue(boneName, out var t) ? t : null;

            const float minSeg = 0.02f;

            var root = Get("root");
            var spine = Get("spine05");
            var chest = Get("spine03");
            var upperChest = Get("spine01");
            var neck = Get("neck01");
            var head = Get("head");

            var lUpperArm = Get("upperarm01_L");
            var lLowerArm = Get("lowerarm01_L");
            var lHand = Get("wrist_L");

            var rUpperArm = Get("upperarm01_R");
            var rLowerArm = Get("lowerarm01_R");
            var rHand = Get("wrist_R");

            var lThigh = Get("upperleg01_L");
            var lCalf = Get("lowerleg01_L");
            var lFoot = Get("foot_L");
            var lToe = Get("toe1-1_L");

            var rThigh = Get("upperleg01_R");
            var rCalf = Get("lowerleg01_R");
            var rFoot = Get("foot_R");
            var rToe = Get("toe1-1_R");

            // 1) Yaw-align root so hips face +Z (Left -> -X, Right -> +X)
            if (root != null && lThigh != null && rThigh != null)
            {
                Vector3 hipRight = rThigh.position - lThigh.position;
                hipRight.y = 0f;
                if (hipRight.sqrMagnitude > 0.0001f)
                {
                    float yaw = Vector3.SignedAngle(hipRight.normalized, Vector3.right, Vector3.up);
                    if (Mathf.Abs(yaw) > 1f)
                        root.rotation = Quaternion.AngleAxis(yaw, Vector3.up) * root.rotation;
                }
            }

            // 2) Straighten ONLY mapped spine/neck bones vertically along +Y
            AlignLimbSegmentSafe(spine, chest, Vector3.up, minSeg);
            AlignLimbSegmentSafe(chest, upperChest, Vector3.up, minSeg);
            AlignLimbSegmentSafe(upperChest, neck, Vector3.up, minSeg);
            AlignLimbSegmentSafe(neck, head, Vector3.up, minSeg);

            // 3) Straighten ONLY mapped arm bones horizontally to -X / +X
            AlignLimbSegmentSafe(lUpperArm, lLowerArm, Vector3.left, minSeg);
            AlignLimbSegmentSafe(lLowerArm, lHand, Vector3.left, minSeg);
            AlignLimbSegmentSafe(rUpperArm, rLowerArm, Vector3.right, minSeg);
            AlignLimbSegmentSafe(rLowerArm, rHand, Vector3.right, minSeg);

            // 4) Straighten ONLY mapped leg bones vertically to -Y and feet forward to +Z
            AlignLimbSegmentSafe(lThigh, lCalf, Vector3.down, minSeg);
            AlignLimbSegmentSafe(lCalf, lFoot, Vector3.down, minSeg);
            AlignLimbSegmentSafe(lFoot, lToe, Vector3.forward, minSeg * 0.5f);

            AlignLimbSegmentSafe(rThigh, rCalf, Vector3.down, minSeg);
            AlignLimbSegmentSafe(rCalf, rFoot, Vector3.down, minSeg);
            AlignLimbSegmentSafe(rFoot, rToe, Vector3.forward, minSeg * 0.5f);

            // 5) Lift root vertically so feet rest cleanly near y = 0.09
            if (root != null && lFoot != null && rFoot != null)
            {
                float lowestFootY = Mathf.Min(lFoot.position.y, rFoot.position.y);
                float lift = 0.09f - lowestFootY;
                if (Mathf.Abs(lift) > 0.01f)
                    root.position += new Vector3(0f, lift, 0f);
            }
        }

        private static float MeasureStandingHeight(GameObject root)
        {
            var byName = new Dictionary<string, Transform>(StringComparer.Ordinal);
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (!byName.ContainsKey(t.name))
                    byName.Add(t.name, t);
            }

            // For MakeHuman burnt zombie, sum actual straightened bone lengths (raw pose is crouched!)
            if (byName.ContainsKey("spine05") && byName.ContainsKey("upperleg01_L") && byName.ContainsKey("foot_L") && byName.ContainsKey("head"))
            {
                float legLen = ChainLength(byName, "upperleg01_L", "upperleg02_L", "lowerleg01_L", "lowerleg02_L", "foot_L");
                float spineLen = ChainLength(byName, "root", "spine05", "spine04", "spine03", "spine02", "spine01", "neck01", "neck02", "neck03", "head");
                float total = legLen + spineLen;
                if (total > 0.3f)
                    return total * 1.14f; // +14% for foot sole + skull crown
            }

            if (TryComputeVisibleBounds(root, out Bounds b) && b.size.y > 0.05f)
                return b.size.y;

            return 1.80f;
        }

        private static float ChainLength(Dictionary<string, Transform> byName, params string[] names)
        {
            float sum = 0f;
            for (int i = 0; i < names.Length - 1; i++)
            {
                if (byName.TryGetValue(names[i], out var a) && byName.TryGetValue(names[i + 1], out var b))
                    sum += Vector3.Distance(a.position, b.position);
            }
            return sum;
        }

        private static EnemyVisualEntry BuildEnemyPrefabEntry(
            InfectedKind kind, string slug, string fbxPath, float targetHeight,
            Material mat, RuntimeAnimatorController controller)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            var avatar = LoadAvatarFromAsset(fbxPath);
            if (source == null || avatar == null || !avatar.isValid || !avatar.isHuman)
            {
                // Fallback to Zombie1.fbx if burnt zombie avatar failed
                source = AssetDatabase.LoadAssetAtPath<GameObject>(AnimSourceSkeletonPath);
                avatar = LoadAvatarFromAsset(AnimSourceSkeletonPath);
                if (source == null || avatar == null) return null;
            }

            var instance = Object.Instantiate(source);
            instance.name = "Enemy_" + slug;
            try
            {
                // Strip cameras, lights, legacy animations, and nested animators (e.g. Blender Camera/Lamp in zombie_theBurnt.fbx)
                foreach (var cam in instance.GetComponentsInChildren<Camera>(true)) Object.DestroyImmediate(cam.gameObject);
                foreach (var lgt in instance.GetComponentsInChildren<Light>(true)) Object.DestroyImmediate(lgt.gameObject);
                foreach (var legacy in instance.GetComponentsInChildren<Animation>(true)) Object.DestroyImmediate(legacy);
                foreach (var childAnim in instance.GetComponentsInChildren<Animator>(true))
                    if (childAnim.gameObject != instance) Object.DestroyImmediate(childAnim);

                EnableAllRenderersAndMeshes(instance);

                // In zombie_theBurnt.fbx, disable overlapping low_poly proxy if high-detail male_generic mesh is present
                var allSmrs = instance.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                bool hasHighRes = allSmrs.Any(s => s.name.IndexOf("male_generic", StringComparison.OrdinalIgnoreCase) >= 0);
                foreach (var smr in allSmrs)
                {
                    if (hasHighRes && smr.name.IndexOf("low_poly", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        smr.gameObject.SetActive(false);
                        continue;
                    }
                    if (mat != null)
                    {
                        var mats = new Material[Mathf.Max(1, smr.sharedMaterials.Length)];
                        for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                        smr.sharedMaterials = mats;
                    }
                }

                foreach (var mr in instance.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (mat != null && mr.gameObject.activeInHierarchy)
                    {
                        var mats = new Material[Mathf.Max(1, mr.sharedMaterials.Length)];
                        for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                        mr.sharedMaterials = mats;
                    }
                }

                // Always use UNIFORM scale based on true standing height so Humanoid rigs never squash or shear
                float standingHeight = MeasureStandingHeight(instance);
                if (standingHeight > 0.05f)
                {
                    float uniform = targetHeight / standingHeight;
                    instance.transform.localScale = Vector3.one * uniform;
                }

                ConfigureAnimatorAndRenderers(instance, avatar, controller);

                string prefabPath = $"{EnemiesDir}/Enemy_{(int)kind}_{slug}.prefab";
                var saved = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);

                return new EnemyVisualEntry
                {
                    kind = kind,
                    label = slug,
                    modelPrefab = saved,
                    avatar = avatar,
                    targetHeight = targetHeight
                };
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static Material CreateEnemyMaterial(string name, Texture2D diffuse, Texture2D normal, Color tint, Color emission, float metallic, float smoothness)
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            string path = $"{EnemyMaterialsDir}/{SanitizeFileName(name)}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(urpLit);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (urpLit != null)
            {
                mat.shader = urpLit;
            }

            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Metallic", metallic);
            mat.SetFloat("_Smoothness", smoothness);
            if (diffuse != null) mat.SetTexture("_BaseMap", diffuse);
            if (normal != null)
            {
                mat.SetTexture("_BumpMap", normal);
                mat.SetFloat("_BumpScale", 1f);
                mat.EnableKeyword("_NORMALMAP");
            }
            if (emission.maxColorComponent > 0.001f)
            {
                mat.SetColor("_EmissionColor", emission);
                mat.EnableKeyword("_EMISSION");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
