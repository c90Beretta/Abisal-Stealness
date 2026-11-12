using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public enum SurvivorId
    {
        KaelAssault = 0,
        ViktorStealth = 1,
        VanceMedic = 2,
        ElenaEngineer = 3
    }

    public sealed class SurvivorProfile
    {
        public SurvivorId Id;
        public string ButtonId;
        public string CodeName;
        public string RoleTitle;
        public string ShortButton;
        public string PassiveSummary;
        public float MoveMultiplier;
        public float StaminaRegenMultiplier;
        public float NoiseMultiplier;
        public float FirearmDamageMultiplier;
        public float HeavyShotgunMultiplier;
        public float MeleeDamageMultiplier;
        public float ReloadSeconds;
        public float HealBonus;
        public float ActionSpeedMultiplier;
        public float SerumDrainMultiplier;
        public float UtilityDrainMultiplier;
        public float ArmorAbsorption;
        public float ExtraCapacityKg;
        public float DodgeCostMultiplier;
        public Color ClothColor;
        public Color ArmorColor;
        public Color VisorColor;
        public Color AccentColor;
    }

    public sealed class SurvivorRoster : MonoBehaviour
    {
        public static readonly SurvivorProfile[] Profiles =
        {
            new SurvivorProfile
            {
                Id = SurvivorId.KaelAssault,
                ButtonId = "Asalto",
                CodeName = "Kael \"Vanguardia\"",
                RoleTitle = "Asalto Tactico",
                ShortButton = "1  Kael / Asalto",
                PassiveSummary = "Recarga rapida (1.15s), +15% dano balistico y control de retroceso",
                MoveMultiplier = 1.0f,
                StaminaRegenMultiplier = 1.0f,
                NoiseMultiplier = 1.0f,
                FirearmDamageMultiplier = 1.15f,
                HeavyShotgunMultiplier = 1.05f,
                MeleeDamageMultiplier = 1.0f,
                ReloadSeconds = 1.15f,
                HealBonus = 0f,
                ActionSpeedMultiplier = 1.0f,
                SerumDrainMultiplier = 1.0f,
                UtilityDrainMultiplier = 1.0f,
                ArmorAbsorption = 0.35f,
                ExtraCapacityKg = 0f,
                DodgeCostMultiplier = 1.0f,
                ClothColor = new Color(.34f, .38f, .42f),
                ArmorColor = new Color(.16f, .19f, .22f),
                VisorColor = new Color(.22f, .95f, .55f),
                AccentColor = new Color(.35f, .75f, .52f)
            },
            new SurvivorProfile
            {
                Id = SurvivorId.ViktorStealth,
                ButtonId = "Explorador",
                CodeName = "Viktor \"Sombra\"",
                RoleTitle = "Explorador de Abordaje",
                ShortButton = "2  Viktor / Explorador",
                PassiveSummary = "+16% velocidad, +40% regen resistencia, -40% ruido, +35% dano melee y robo de vigor",
                MoveMultiplier = 1.16f,
                StaminaRegenMultiplier = 1.40f,
                NoiseMultiplier = 0.60f,
                FirearmDamageMultiplier = 1.05f,
                HeavyShotgunMultiplier = 1.0f,
                MeleeDamageMultiplier = 1.35f,
                ReloadSeconds = 1.20f,
                HealBonus = 5f,
                ActionSpeedMultiplier = 1.15f,
                SerumDrainMultiplier = 0.85f,
                UtilityDrainMultiplier = 0.85f,
                ArmorAbsorption = 0.35f,
                ExtraCapacityKg = 2f,
                DodgeCostMultiplier = 0.70f,
                ClothColor = new Color(.14f, .16f, .19f),
                ArmorColor = new Color(.09f, .10f, .12f),
                VisorColor = new Color(.98f, .22f, .18f),
                AccentColor = new Color(.92f, .25f, .22f)
            },
            new SurvivorProfile
            {
                Id = SurvivorId.VanceMedic,
                ButtonId = "Sanitario",
                CodeName = "Dra. Vance \"Bio-Control\"",
                RoleTitle = "Sanitario de Cuarentena",
                ShortButton = "3  Vance / Sanitario",
                PassiveSummary = "-35% degradacion de suero, botiquin +55 vida + cura sangrado, +30% velocidad medica",
                MoveMultiplier = 1.06f,
                StaminaRegenMultiplier = 1.15f,
                NoiseMultiplier = 0.9f,
                FirearmDamageMultiplier = 1.10f,
                HeavyShotgunMultiplier = 1.0f,
                MeleeDamageMultiplier = 1.10f,
                ReloadSeconds = 1.30f,
                HealBonus = 15f,
                ActionSpeedMultiplier = 1.35f,
                SerumDrainMultiplier = 0.65f,
                UtilityDrainMultiplier = 0.80f,
                ArmorAbsorption = 0.38f,
                ExtraCapacityKg = 2f,
                DodgeCostMultiplier = 0.9f,
                ClothColor = new Color(.78f, .82f, .84f),
                ArmorColor = new Color(.22f, .46f, .52f),
                VisorColor = new Color(.20f, .92f, .98f),
                AccentColor = new Color(.25f, .85f, .92f)
            },
            new SurvivorProfile
            {
                Id = SurvivorId.ElenaEngineer,
                ButtonId = "Ingeniera",
                CodeName = "Elena \"Bastion\"",
                RoleTitle = "Ingeniera Pesada",
                ShortButton = "4  Elena / Ingeniera",
                PassiveSummary = "Blindaje reforzado (50% abs), +6kg carga, -35% gasto O2/Luz, +25% escopeta/pesada",
                MoveMultiplier = 0.96f,
                StaminaRegenMultiplier = 1.05f,
                NoiseMultiplier = 1.05f,
                FirearmDamageMultiplier = 1.05f,
                HeavyShotgunMultiplier = 1.25f,
                MeleeDamageMultiplier = 1.15f,
                ReloadSeconds = 1.35f,
                HealBonus = 0f,
                ActionSpeedMultiplier = 1.1f,
                SerumDrainMultiplier = 0.9f,
                UtilityDrainMultiplier = 0.65f,
                ArmorAbsorption = 0.50f,
                ExtraCapacityKg = 6f,
                DodgeCostMultiplier = 1.0f,
                ClothColor = new Color(.68f, .44f, .16f),
                ArmorColor = new Color(.28f, .24f, .18f),
                VisorColor = new Color(1.0f, .72f, .18f),
                AccentColor = new Color(.95f, .62f, .15f)
            }
        };

        public SandboxSession Session { get; private set; }
        public int CurrentIndex { get; private set; }
        public SurvivorProfile Current => Profiles[Mathf.Clamp(CurrentIndex, 0, Profiles.Length - 1)];

        private readonly List<GameObject> customAttachments = new List<GameObject>();
        private SkinnedMeshRenderer playerSkin;
        private Transform headBone, chestBone, leftShoulderBone, rightShoulderBone, hipsBone;
        private Material baseMaterial;

        public void Initialize(SandboxSession session)
        {
            Session = session;
            if (session == null || session.player == null) return;
            playerSkin = session.player.GetComponentInChildren<SkinnedMeshRenderer>();
            foreach (var bone in session.player.GetComponentsInChildren<Transform>(true))
            {
                if (bone.name == "Head") headBone = bone;
                else if (bone.name == "Chest") chestBone = bone;
                else if (bone.name == "LeftShoulder") leftShoulderBone = bone;
                else if (bone.name == "RightShoulder") rightShoulderBone = bone;
                else if (bone.name == "Hips") hipsBone = bone;
            }
            var vest = session.player.GetComponentsInChildren<MeshRenderer>(true);
            if (vest.Length > 0) baseMaterial = vest[0].sharedMaterial;
            ApplyCharacter(0, true);
        }

        public void CycleCharacter(int delta)
        {
            int next = (CurrentIndex + delta + Profiles.Length) % Profiles.Length;
            ApplyCharacter(next, false);
        }

        public void ApplyCharacter(int index, bool silent = false)
        {
            CurrentIndex = Mathf.Clamp(index, 0, Profiles.Length - 1);
            var profile = Current;
            RebuildVisuals(profile);
            if (!silent && Session != null && Session.Running)
            {
                TacticalAudio.Play(Session.player.transform.position, TacticalCue.Swing, 1.15f);
                Session.SetMessage($"Personaje activo: {profile.CodeName} ({profile.RoleTitle}) — {profile.PassiveSummary}");
            }
        }

        public void OnMeleeKill()
        {
            if (Session == null || Session.Night == null) return;
            if (Current.Id == SurvivorId.ViktorStealth)
            {
                Session.Night.Inventory.stamina = Mathf.Min(100f, Session.Night.Inventory.stamina + 14f);
                Session.State.health = Mathf.Min(100f, Session.State.health + 4f);
            }
            else
            {
                Session.Night.Inventory.stamina = Mathf.Min(100f, Session.Night.Inventory.stamina + 5f);
            }
        }

        private void RebuildVisuals(SurvivorProfile profile)
        {
            foreach (var item in customAttachments)
                if (item != null) Destroy(item);
            customAttachments.Clear();

            if (Session == null || Session.player == null) return;

            if (playerSkin != null && playerSkin.sharedMaterials != null && playerSkin.sharedMaterials.Length >= 4)
            {
                var block = new MaterialPropertyBlock();
                playerSkin.GetPropertyBlock(block, 0);
                block.SetColor("_BaseColor", profile.ClothColor);
                block.SetColor("_Color", profile.ClothColor);
                playerSkin.SetPropertyBlock(block, 0);

                var armorBlock = new MaterialPropertyBlock();
                playerSkin.GetPropertyBlock(armorBlock, 3);
                armorBlock.SetColor("_BaseColor", profile.ArmorColor);
                armorBlock.SetColor("_Color", profile.ArmorColor);
                playerSkin.SetPropertyBlock(armorBlock, 3);
            }

            foreach (var r in Session.player.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r == null) continue;
                if (r.name == "Armoured vest" || r.name.StartsWith("Carrier") || r.name.StartsWith("Harness"))
                    TintRenderer(r, profile.ArmorColor);
                else if (r.name == "Mask lens")
                    TintRenderer(r, profile.VisorColor, true);
                else if (r.name == "Soldier oxygen tank")
                    TintRenderer(r, profile.Id == SurvivorId.VanceMedic ? new Color(.18f, .55f, .62f) : profile.ArmorColor);
            }

            Material mat = baseMaterial != null ? baseMaterial : Session.tracerMaterial;
            if (headBone != null)
            {
                switch (profile.Id)
                {
                    case SurvivorId.KaelAssault:
                        AddPart(headBone, "Kael NVG Mount", new Vector3(0, .14f, .10f), new Vector3(.13f, .04f, .06f), profile.ArmorColor, mat);
                        AddPart(headBone, "Kael NVG Left", new Vector3(-.045f, .12f, .135f), new Vector3(.042f, .035f, .065f), profile.VisorColor, mat);
                        AddPart(headBone, "Kael NVG Right", new Vector3(.045f, .12f, .135f), new Vector3(.042f, .035f, .065f), profile.VisorColor, mat);
                        if (leftShoulderBone != null)
                            AddPart(leftShoulderBone, "Kael Radio Antenna", new Vector3(-.06f, .18f, -.04f), new Vector3(.015f, .32f, .015f), profile.ArmorColor, mat);
                        break;

                    case SurvivorId.ElenaEngineer:
                        AddPart(headBone, "Elena Welder Shield", new Vector3(0, .13f, .095f), new Vector3(.19f, .075f, .11f), profile.ClothColor, mat);
                        AddPart(headBone, "Elena Headlamp", new Vector3(0, .17f, .13f), new Vector3(.06f, .045f, .04f), profile.VisorColor, mat);
                        if (leftShoulderBone != null)
                            AddPart(leftShoulderBone, "Elena Left Pauldron", new Vector3(-.07f, .03f, 0), new Vector3(.15f, .12f, .18f), profile.ClothColor, mat);
                        if (rightShoulderBone != null)
                            AddPart(rightShoulderBone, "Elena Right Pauldron", new Vector3(.07f, .03f, 0), new Vector3(.15f, .12f, .18f), profile.ClothColor, mat);
                        if (chestBone != null)
                            AddPart(chestBone, "Elena Power Pack", new Vector3(0, .06f, -.25f), new Vector3(.28f, .32f, .14f), profile.ArmorColor, mat);
                        break;

                    case SurvivorId.VanceMedic:
                        AddPart(headBone, "Vance Bio Hood", new Vector3(0, .11f, -.01f), new Vector3(.21f, .22f, .21f), profile.ClothColor, mat);
                        AddPart(headBone, "Vance Bio Visor", new Vector3(0, .09f, .115f), new Vector3(.16f, .09f, .03f), profile.VisorColor, mat);
                        if (chestBone != null)
                        {
                            AddPart(chestBone, "Vance Cryo Left", new Vector3(-.09f, .05f, -.23f), new Vector3(.08f, .34f, .08f), profile.VisorColor, mat, PrimitiveType.Cylinder);
                            AddPart(chestBone, "Vance Cryo Right", new Vector3(.09f, .05f, -.23f), new Vector3(.08f, .34f, .08f), profile.VisorColor, mat, PrimitiveType.Cylinder);
                        }
                        if (hipsBone != null)
                            AddPart(hipsBone, "Vance Med Case", new Vector3(-.21f, 0, .03f), new Vector3(.08f, .15f, .16f), Color.white, mat);
                        break;

                    case SurvivorId.ViktorStealth:
                        AddPart(headBone, "Viktor Recon Cowl", new Vector3(0, .12f, -.015f), new Vector3(.20f, .20f, .22f), profile.ClothColor, mat);
                        AddPart(headBone, "Viktor Red Optics", new Vector3(0, .085f, .12f), new Vector3(.14f, .032f, .03f), profile.VisorColor, mat);
                        if (chestBone != null)
                        {
                            var scabbardA = AddPart(chestBone, "Viktor Back Scabbard A", new Vector3(0, .08f, -.20f), new Vector3(.045f, .52f, .025f), profile.ArmorColor, mat);
                            scabbardA.transform.localRotation = Quaternion.Euler(0, 0, 28);
                            var scabbardB = AddPart(chestBone, "Viktor Back Scabbard B", new Vector3(0, .08f, -.21f), new Vector3(.045f, .52f, .025f), profile.VisorColor, mat);
                            scabbardB.transform.localRotation = Quaternion.Euler(0, 0, -28);
                        }
                        break;
                }
            }
        }

        private GameObject AddPart(Transform parent, string label, Vector3 localPos, Vector3 scale, Color color, Material mat, PrimitiveType type = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = label;
            var col = go.GetComponent<Collider>();
            if (col != null) { col.enabled = false; Destroy(col); }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = mat;
            TintRenderer(renderer, color);
            customAttachments.Add(go);
            return go;
        }

        private static void TintRenderer(Renderer renderer, Color color, bool emissive = false)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            if (emissive) block.SetColor("_EmissionColor", color * 1.4f);
            renderer.SetPropertyBlock(block);
        }
    }
}
