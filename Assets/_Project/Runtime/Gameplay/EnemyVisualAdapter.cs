using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    /// <summary>
    /// Monta el modelo 3D Humanoid y el Animator de enemigo (idle, walk, run, sprint, ataque,
    /// golpe pesado, grito, stagger y 4 muertes) manteniendo intacta la jerarquia legacy
    /// (Ragdoll/Dismemberment) oculta mediante forceRenderingOff para preservar compatibilidad total.
    /// </summary>
    [DefaultExecutionOrder(165)]
    public sealed class EnemyVisualAdapter : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HeavyAttackHash = Animator.StringToHash("HeavyAttack");
        private static readonly int ScreamHash = Animator.StringToHash("Scream");
        private static readonly int StaggerHash = Animator.StringToHash("Stagger");
        private static readonly int DeadHash = Animator.StringToHash("Dead");
        private static readonly int DeathKindHash = Animator.StringToHash("DeathKind");

        public GameObject ModelInstance { get; private set; }
        public Animator EnemyAnimator { get; private set; }

        private SandboxEnemy enemy;
        private InfectedVariant variant;
        private InfectedVisibility visibility;
        private Renderer[] legacyRenderers;
        private Renderer[] modelRenderers;
        private MaterialPropertyBlock propBlock;
        private Vector3 baseModelScale = Vector3.one;
        private EnemyMode lastMode = EnemyMode.Patrol;
        private float smoothedSpeed;
        private float armsLayerWeight = 0.9f;
        private float actionHoldUntil;
        private bool suppressedByHound;
        private bool initialized;

        public void Initialize(SandboxEnemy owner)
        {
            if (initialized) return;
            initialized = true;

            enemy = owner;
            variant = GetComponent<InfectedVariant>();
            visibility = GetComponent<InfectedVisibility>();
            propBlock = new MaterialPropertyBlock();

            if (GetComponent<SpecialInfected>()?.Species == SpecialSpecies.Hound)
            {
                SuppressForHound();
                return;
            }

            var catalog = Resources.Load<SurvivorVisualCatalog>(SurvivorVisualCatalog.ResourcePath);
            if (catalog == null || catalog.enemyAnimatorController == null) return;

            InfectedKind kind = variant != null ? variant.kind : InfectedKind.Walker;
            var entry = catalog.GetEnemyEntry(kind);
            if (entry == null || entry.modelPrefab == null) return;

            // Collect existing legacy renderers BEFORE spawning the 3D model so they stay intact but invisible
            var legacyList = new List<Renderer>();
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                if (r != null) legacyList.Add(r);
            legacyRenderers = legacyList.ToArray();

            ModelInstance = Instantiate(entry.modelPrefab, transform, false);
            ModelInstance.name = "EnemyModel_" + kind;
            ModelInstance.transform.localPosition = Vector3.zero;
            ModelInstance.transform.localRotation = Quaternion.identity;
            baseModelScale = ModelInstance.transform.localScale;

            foreach (var smr in ModelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                smr.updateWhenOffscreen = true;

            EnemyAnimator = ModelInstance.GetComponent<Animator>();
            if (EnemyAnimator == null) EnemyAnimator = ModelInstance.AddComponent<Animator>();
            if (entry.avatar != null && entry.avatar.isValid) EnemyAnimator.avatar = entry.avatar;
            EnemyAnimator.runtimeAnimatorController = catalog.enemyAnimatorController;
            EnemyAnimator.applyRootMotion = false;
            EnemyAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            EnemyAnimator.Rebind();
            EnemyAnimator.Update(0.02f);

            modelRenderers = ModelInstance.GetComponentsInChildren<Renderer>(true);
            HideLegacyRenderers();
        }

        public void SuppressForHound()
        {
            suppressedByHound = true;
            if (ModelInstance != null)
            {
                Destroy(ModelInstance);
                ModelInstance = null;
                EnemyAnimator = null;
                modelRenderers = null;
            }
        }

        public void ApplySpecialScale(SpecialSpecies species, bool isBoss)
        {
            if (species == SpecialSpecies.Hound)
            {
                SuppressForHound();
                return;
            }
            if (ModelInstance == null) return;

            Vector3 mult = isBoss
                ? new Vector3(1.42f, 1.32f, 1.42f)
                : species == SpecialSpecies.Colossus
                    ? new Vector3(1.28f, 1.20f, 1.28f)
                    : new Vector3(1.38f, 1.02f, 1.38f); // Bloater wide silhouette
            ModelInstance.transform.localScale = Vector3.Scale(baseModelScale, mult);
        }

        public void UpdateMovement(float currentSpeed, EnemyMode mode)
        {
            if (suppressedByHound || EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;

            if (mode != lastMode)
            {
                if (mode == EnemyMode.Windup)
                {
                    bool heavy = (variant != null && variant.kind == InfectedKind.Brute)
                        || GetComponent<SpecialInfected>()?.Species == SpecialSpecies.Colossus;
                    EnemyAnimator.ResetTrigger(StaggerHash);
                    EnemyAnimator.SetTrigger(heavy ? HeavyAttackHash : AttackHash);
                    actionHoldUntil = Time.time + (heavy ? 0.95f : 0.65f);
                }
                else if (mode == EnemyMode.Stagger)
                {
                    EnemyAnimator.SetTrigger(StaggerHash);
                    actionHoldUntil = Time.time + 0.38f;
                }
                lastMode = mode;
            }

            float targetSpeed = mode == EnemyMode.Dead || mode == EnemyMode.Windup || mode == EnemyMode.Recover || mode == EnemyMode.Stagger
                ? 0f
                : Mathf.Clamp(currentSpeed / 1.8f, 0f, 1.55f);

            float damp = 1f - Mathf.Exp(-10f * Time.deltaTime);
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, damp);
            EnemyAnimator.SetFloat(SpeedHash, smoothedSpeed);

            // Fade out the ZombieArms reach layer during dedicated full-body actions (Attack/Scream/Stagger/Dead)
            bool inFullBodyAction = mode == EnemyMode.Dead
                || mode == EnemyMode.Windup
                || mode == EnemyMode.Stagger
                || Time.time < actionHoldUntil;
            float targetLayerWeight = inFullBodyAction ? 0f : (smoothedSpeed > 0.15f ? 0.90f : 0.45f);
            armsLayerWeight = Mathf.MoveTowards(armsLayerWeight, targetLayerWeight, Time.deltaTime * 6f);
            if (EnemyAnimator.layerCount > 1)
                EnemyAnimator.SetLayerWeight(1, armsLayerWeight);
        }

        public void TriggerScream()
        {
            if (suppressedByHound || EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;
            EnemyAnimator.SetTrigger(ScreamHash);
            actionHoldUntil = Time.time + 1.05f;
        }

        public void OnDeath(bool headshot)
        {
            if (suppressedByHound || EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;
            int deathKind = headshot ? 3 : Random.Range(0, 3);
            EnemyAnimator.SetInteger(DeathKindHash, deathKind);
            EnemyAnimator.SetBool(DeadHash, true);
            EnemyAnimator.SetFloat(SpeedHash, 0f);
            if (EnemyAnimator.layerCount > 1) EnemyAnimator.SetLayerWeight(1, 0f);
            lastMode = EnemyMode.Dead;
        }

        public void OnRestore(bool dead)
        {
            if (suppressedByHound || EnemyAnimator == null) return;
            smoothedSpeed = 0f;
            actionHoldUntil = 0f;
            lastMode = dead ? EnemyMode.Dead : EnemyMode.Patrol;
            EnemyAnimator.SetBool(DeadHash, dead);
            EnemyAnimator.SetFloat(SpeedHash, 0f);
            EnemyAnimator.ResetTrigger(AttackHash);
            EnemyAnimator.ResetTrigger(HeavyAttackHash);
            EnemyAnimator.ResetTrigger(ScreamHash);
            EnemyAnimator.ResetTrigger(StaggerHash);
            if (ModelInstance != null)
            {
                ModelInstance.transform.localPosition = Vector3.zero;
                ModelInstance.transform.localRotation = Quaternion.identity;
            }
            if (EnemyAnimator.isActiveAndEnabled)
                EnemyAnimator.Update(0.02f);
        }

        public void ApplyVisualFeedback(bool flashing, bool burning)
        {
            if (suppressedByHound || modelRenderers == null) return;
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                var r = modelRenderers[i];
                if (r == null) continue;
                propBlock.Clear();
                if (flashing)
                {
                    propBlock.SetColor("_EmissionColor", Color.red * 1.8f);
                    propBlock.SetColor("_BaseColor", new Color(1f, 0.2f, 0.15f));
                }
                else if (burning)
                {
                    propBlock.SetColor("_EmissionColor", new Color(1.6f, 0.55f, 0.1f));
                }
                r.SetPropertyBlock(propBlock);
            }
        }

        private void LateUpdate()
        {
            if (suppressedByHound || ModelInstance == null) return;

            HideLegacyRenderers();

            // Mirror flashlight / night perception visibility onto the 3D enemy model
            bool visible = visibility == null || visibility.Visible;
            if (modelRenderers != null)
            {
                for (int i = 0; i < modelRenderers.Length; i++)
                    if (modelRenderers[i] != null) modelRenderers[i].enabled = visible;
            }
        }

        private void HideLegacyRenderers()
        {
            if (legacyRenderers == null) return;
            for (int i = 0; i < legacyRenderers.Length; i++)
                if (legacyRenderers[i] != null) legacyRenderers[i].forceRenderingOff = true;
        }
    }
}
