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
        private static readonly int StyleHash = Animator.StringToHash("Style");
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
        private float upperLayerWeight = 1f;
        private float zombieStyle = 1f;
        private bool initialized;

        public void Initialize(SandboxEnemy owner)
        {
            if (initialized) return;
            initialized = true;

            enemy = owner;
            variant = GetComponent<InfectedVariant>();
            visibility = GetComponent<InfectedVisibility>();
            propBlock = new MaterialPropertyBlock();

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

            // Variation between Drunken (1.0) and Simple Zombie (0.0), favoring Drunken (~70% drunken / mixed)
            int hash = Mathf.Abs(gameObject.name.GetHashCode() ^ ((int)kind * 397));
            int bucket = hash % 4;
            zombieStyle = kind == InfectedKind.Walker
                ? (bucket == 0 ? 0.0f : bucket == 1 ? 0.65f : 1.0f)
                : kind == InfectedKind.Brute
                    ? (bucket == 0 ? 0.5f : 1.0f)
                    : (bucket < 2 ? 1.0f : 0.0f);
            EnemyAnimator.SetFloat(StyleHash, zombieStyle);
            EnemyAnimator.Update(0.02f);

            modelRenderers = ModelInstance.GetComponentsInChildren<Renderer>(true);
            HideLegacyRenderers();
        }

        public void ApplySpecialScale(SpecialSpecies species, bool isBoss)
        {
            if (ModelInstance == null) return;

            // Always apply UNIFORM scaling on Humanoid Animator roots so zombies are never squashed or sheared.
            // Bosses and Colossi render simply as larger, imposing zombies with clean proportions.
            float mult = isBoss
                ? 1.45f
                : species == SpecialSpecies.Colossus
                    ? 1.28f
                    : 1.15f;
            ModelInstance.transform.localScale = baseModelScale * mult;
        }

        public void UpdateMovement(float currentSpeed, EnemyMode mode)
        {
            if (EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;

            if (mode != lastMode)
            {
                if (mode == EnemyMode.Windup)
                {
                    var sp = GetComponent<SpecialInfected>();
                    bool heavy = (variant != null && variant.kind == InfectedKind.Brute)
                        || (sp != null && sp.Species == SpecialSpecies.Colossus);
                    EnemyAnimator.ResetTrigger(StaggerHash);
                    EnemyAnimator.SetTrigger(heavy ? HeavyAttackHash : AttackHash);
                }
                else if (mode == EnemyMode.Stagger)
                {
                    EnemyAnimator.SetTrigger(StaggerHash);
                }
                lastMode = mode;
            }

            float targetSpeed;
            if (mode == EnemyMode.Dead || mode == EnemyMode.Windup || mode == EnemyMode.Recover || mode == EnemyMode.Stagger || currentSpeed <= 0.05f)
            {
                targetSpeed = 0f;
            }
            else if (currentSpeed <= 1.35f)
            {
                targetSpeed = Mathf.Lerp(0.45f, 0.70f, Mathf.InverseLerp(0.35f, 1.35f, currentSpeed));
            }
            else if (currentSpeed <= 2.35f)
            {
                targetSpeed = Mathf.Lerp(0.70f, 1.10f, Mathf.InverseLerp(1.35f, 2.35f, currentSpeed));
            }
            else
            {
                targetSpeed = Mathf.Lerp(1.10f, 1.50f, Mathf.InverseLerp(2.35f, 3.20f, currentSpeed));
            }

            float damp = 1f - Mathf.Exp(-10f * Time.deltaTime);
            smoothedSpeed = Mathf.Lerp(smoothedSpeed, targetSpeed, damp);
            if (smoothedSpeed < 0.01f && targetSpeed <= 0f) smoothedSpeed = 0f;
            EnemyAnimator.SetFloat(SpeedHash, smoothedSpeed);

            float targetLayerWeight = mode == EnemyMode.Dead ? 0f : 1f;
            upperLayerWeight = Mathf.MoveTowards(upperLayerWeight, targetLayerWeight, Time.deltaTime * 10f);
            if (EnemyAnimator.layerCount > 1)
                EnemyAnimator.SetLayerWeight(1, upperLayerWeight);
        }

        public void TriggerScream()
        {
            if (EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;
            EnemyAnimator.SetTrigger(ScreamHash);
        }

        public void OnDeath(bool headshot)
        {
            if (EnemyAnimator == null || !EnemyAnimator.isActiveAndEnabled) return;
            int deathKind = headshot ? 3 : Random.Range(0, 3);
            EnemyAnimator.SetInteger(DeathKindHash, deathKind);
            EnemyAnimator.SetBool(DeadHash, true);
            EnemyAnimator.SetFloat(SpeedHash, 0f);
            upperLayerWeight = 0f;
            if (EnemyAnimator.layerCount > 1) EnemyAnimator.SetLayerWeight(1, 0f);
            lastMode = EnemyMode.Dead;
        }

        public void OnRestore(bool dead)
        {
            if (EnemyAnimator == null) return;
            smoothedSpeed = 0f;
            upperLayerWeight = dead ? 0f : 1f;
            lastMode = dead ? EnemyMode.Dead : EnemyMode.Patrol;
            EnemyAnimator.SetBool(DeadHash, dead);
            EnemyAnimator.SetFloat(SpeedHash, 0f);
            EnemyAnimator.SetFloat(StyleHash, zombieStyle);
            EnemyAnimator.ResetTrigger(AttackHash);
            EnemyAnimator.ResetTrigger(HeavyAttackHash);
            EnemyAnimator.ResetTrigger(ScreamHash);
            EnemyAnimator.ResetTrigger(StaggerHash);
            if (EnemyAnimator.layerCount > 1)
                EnemyAnimator.SetLayerWeight(1, upperLayerWeight);
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
            if (modelRenderers == null) return;
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
            if (ModelInstance == null) return;

            HideLegacyRenderers();

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
