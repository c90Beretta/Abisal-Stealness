using UnityEngine;

namespace Zombtoy.Gameplay
{
    public static class ModelUpgradeBuilder
    {
        private static readonly Color Gunmetal = new Color(.11f, .12f, .13f);
        private static readonly Color DarkSteel = new Color(.19f, .21f, .22f);
        private static readonly Color BrushedSteel = new Color(.46f, .49f, .51f);
        private static readonly Color Polymer = new Color(.05f, .055f, .06f);
        private static readonly Color OliveDrab = new Color(.22f, .26f, .18f);
        private static readonly Color ColorWalnut = new Color(.24f, .14f, .08f);
        private static readonly Color Brass = new Color(.68f, .52f, .18f);
        private static readonly Color HazardAmber = new Color(.92f, .62f, .12f);
        private static readonly Color OpticCyan = new Color(.20f, .92f, .88f);
        private static readonly Color OpticRed = new Color(.98f, .20f, .16f);
        private static readonly Color ShellRed = new Color(.72f, .14f, .12f);

        public static GameObject CreateFirearmTemplate(int index, string title, Transform parent, Material baseMaterial)
        {
            var root = new GameObject(title + " template");
            root.SetActive(false);
            root.transform.SetParent(parent, false);

            Material mat = baseMaterial;
            float muzzleZ = 0.78f;
            float muzzleY = 0.07f;

            switch (index)
            {
                case 0: // 1: Pistola Tactica DP-9 (Reflex sight + tactical light + compensator)
                    muzzleZ = 0.34f;
                    muzzleY = 0.06f;
                    Part(root.transform, "Frame", new Vector3(0, .025f, .10f), new Vector3(.052f, .042f, .24f), Polymer, mat);
                    Part(root.transform, "Slide", new Vector3(0, .062f, .12f), new Vector3(.060f, .058f, .28f), Gunmetal, mat);
                    Part(root.transform, "Slide Port", new Vector3(0, .082f, .15f), new Vector3(.042f, .022f, .08f), BrushedSteel, mat);
                    var grip0 = Part(root.transform, "Grip", new Vector3(0, -.055f, -.01f), new Vector3(.052f, .145f, .072f), Polymer, mat);
                    grip0.transform.localRotation = Quaternion.Euler(-14, 0, 0);
                    Part(root.transform, "Extended Mag Base", new Vector3(0, -.13f, -.03f), new Vector3(.058f, .035f, .082f), DarkSteel, mat);
                    Tube(root.transform, "Threaded Barrel", new Vector3(0, .062f, .22f), .014f, .20f, BrushedSteel, mat);
                    Part(root.transform, "Muzzle Compensator", new Vector3(0, .062f, .295f), new Vector3(.056f, .052f, .055f), DarkSteel, mat);
                    Part(root.transform, "Tactical Light", new Vector3(0, .018f, .23f), new Vector3(.046f, .036f, .095f), DarkSteel, mat);
                    Part(root.transform, "Light Lens", new Vector3(0, .018f, .28f), new Vector3(.036f, .028f, .012f), OpticCyan, mat, true);
                    Part(root.transform, "Micro Reflex Housing", new Vector3(0, .105f, .04f), new Vector3(.048f, .038f, .055f), Gunmetal, mat);
                    Part(root.transform, "Reflex Dot", new Vector3(0, .112f, .04f), new Vector3(.034f, .024f, .008f), OpticCyan, mat, true);
                    break;

                case 1: // 2: Carabina de Asalto C3 (Holographic optic, M-LOK handguard, foregrip, PEQ laser, curved mag)
                    muzzleZ = 0.79f;
                    muzzleY = 0.07f;
                    Part(root.transform, "Upper Receiver", new Vector3(0, .055f, .12f), new Vector3(.076f, .092f, .31f), Gunmetal, mat);
                    Part(root.transform, "Lower Receiver", new Vector3(0, .005f, .09f), new Vector3(.070f, .065f, .24f), DarkSteel, mat);
                    var grip1 = Part(root.transform, "Pistol Grip", new Vector3(0, -.075f, -.01f), new Vector3(.058f, .15f, .078f), Polymer, mat);
                    grip1.transform.localRotation = Quaternion.Euler(-15, 0, 0);
                    Part(root.transform, "Buffer Tube", new Vector3(0, .045f, -.14f), new Vector3(.045f, .045f, .22f), DarkSteel, mat);
                    Part(root.transform, "Crane Stock", new Vector3(0, .02f, -.24f), new Vector3(.072f, .135f, .22f), Polymer, mat);
                    Part(root.transform, "Stock Pad", new Vector3(0, .015f, -.355f), new Vector3(.076f, .15f, .028f), OliveDrab, mat);
                    Part(root.transform, "MLOK Handguard", new Vector3(0, .055f, .42f), new Vector3(.072f, .082f, .32f), DarkSteel, mat);
                    Tube(root.transform, "Heavy Barrel", new Vector3(0, .065f, .54f), .016f, .52f, Gunmetal, mat);
                    Part(root.transform, "Flash Hider", new Vector3(0, .065f, .76f), new Vector3(.038f, .038f, .065f), BrushedSteel, mat);
                    var mag1 = Part(root.transform, "Curved MagUpper", new Vector3(0, -.095f, .17f), new Vector3(.048f, .19f, .085f), OliveDrab, mat);
                    mag1.transform.localRotation = Quaternion.Euler(14, 0, 0);
                    Part(root.transform, "Vertical Foregrip", new Vector3(0, -.045f, .42f), new Vector3(.044f, .125f, .048f), Polymer, mat);
                    Part(root.transform, "Holo Sight Body", new Vector3(0, .125f, .14f), new Vector3(.068f, .058f, .11f), Gunmetal, mat);
                    Part(root.transform, "Holo Reticle Window", new Vector3(0, .132f, .14f), new Vector3(.052f, .038f, .012f), OpticRed, mat, true);
                    Part(root.transform, "PEQ Laser Box", new Vector3(.048f, .072f, .48f), new Vector3(.038f, .032f, .095f), OliveDrab, mat);
                    Part(root.transform, "Laser Emitter", new Vector3(.052f, .072f, .53f), new Vector3(.014f, .014f, .014f), OpticRed, mat, true);
                    break;

                case 2: // 3: Escopeta de Combate S12 (Perforated heatshield, side saddle shells, pump action, breacher choke)
                    muzzleZ = 0.80f;
                    muzzleY = 0.07f;
                    Part(root.transform, "Shotgun Receiver", new Vector3(0, .048f, .11f), new Vector3(.082f, .105f, .32f), Gunmetal, mat);
                    Part(root.transform, "Ejection Port", new Vector3(.042f, .058f, .15f), new Vector3(.008f, .038f, .095f), BrushedSteel, mat);
                    Part(root.transform, "Tactical Stock", new Vector3(0, .005f, -.20f), new Vector3(.068f, .13f, .32f), ColorWalnut, mat);
                    Part(root.transform, "Recoil Pad", new Vector3(0, 0f, -.365f), new Vector3(.076f, .16f, .035f), Polymer, mat);
                    var grip2 = Part(root.transform, "Pistol Grip", new Vector3(0, -.072f, -.01f), new Vector3(.060f, .15f, .080f), Polymer, mat);
                    grip2.transform.localRotation = Quaternion.Euler(-14, 0, 0);
                    Tube(root.transform, "Smoothbore Barrel", new Vector3(0, .072f, .48f), .024f, .60f, Gunmetal, mat);
                    Tube(root.transform, "Tubular Magazine", new Vector3(0, .022f, .45f), .021f, .52f, DarkSteel, mat);
                    Part(root.transform, "Heatshield", new Vector3(0, .092f, .43f), new Vector3(.062f, .028f, .42f), BrushedSteel, mat);
                    Part(root.transform, "Breacher Choke", new Vector3(0, .072f, .775f), new Vector3(.056f, .056f, .065f), BrushedSteel, mat);
                    Part(root.transform, "Ribbed Pump", new Vector3(0, -.005f, .40f), new Vector3(.088f, .072f, .24f), ColorWalnut, mat);
                    for (int s = 0; s < 5; s++)
                    {
                        Part(root.transform, "Shell " + s, new Vector3(-.046f, .052f, .04f + s * .038f), new Vector3(.018f, .058f, .022f), ShellRed, mat);
                        Part(root.transform, "Shell Brass " + s, new Vector3(-.046f, .018f, .04f + s * .038f), new Vector3(.019f, .016f, .023f), Brass, mat);
                    }
                    Part(root.transform, "Ghost Ring Rear", new Vector3(0, .112f, .01f), new Vector3(.052f, .028f, .024f), Gunmetal, mat);
                    break;

                case 3: // 4: Fusil de Precision M82 "Arpon" (Long fluted barrel, chevron muzzle brake, telescopic scope, bipod)
                    muzzleZ = 0.86f;
                    muzzleY = 0.07f;
                    Part(root.transform, "AntiMateriel Receiver", new Vector3(0, .052f, .14f), new Vector3(.088f, .112f, .42f), DarkSteel, mat);
                    Part(root.transform, "Marksman Stock", new Vector3(0, .022f, -.22f), new Vector3(.072f, .14f, .34f), OliveDrab, mat);
                    Part(root.transform, "Cheek Riser", new Vector3(0, .095f, -.18f), new Vector3(.078f, .045f, .16f), Polymer, mat);
                    var grip3 = Part(root.transform, "Sniper Grip", new Vector3(0, -.078f, -.02f), new Vector3(.058f, .16f, .078f), Polymer, mat);
                    grip3.transform.localRotation = Quaternion.Euler(-12, 0, 0);
                    Part(root.transform, "Heavy Box Mag", new Vector3(0, -.085f, .18f), new Vector3(.062f, .17f, .14f), Gunmetal, mat);
                    Part(root.transform, "Vented Barrel Shroud", new Vector3(0, .058f, .46f), new Vector3(.082f, .088f, .36f), Gunmetal, mat);
                    Tube(root.transform, "Fluted Long Barrel", new Vector3(0, .068f, .60f), .019f, .54f, BrushedSteel, mat);
                    Part(root.transform, "Chevron Muzzle Brake", new Vector3(0, .068f, .835f), new Vector3(.095f, .068f, .095f), Gunmetal, mat);
                    // Telescopic Scope
                    Part(root.transform, "Scope Mount", new Vector3(0, .125f, .14f), new Vector3(.055f, .045f, .22f), DarkSteel, mat);
                    Tube(root.transform, "Scope Tube", new Vector3(0, .165f, .15f), .026f, .34f, Gunmetal, mat);
                    Tube(root.transform, "Scope Objective Bell", new Vector3(0, .165f, .30f), .038f, .09f, Gunmetal, mat);
                    Part(root.transform, "Scope Cyan Lens", new Vector3(0, .165f, .345f), new Vector3(.058f, .058f, .008f), OpticCyan, mat, true);
                    // Folded Bipod
                    Part(root.transform, "Bipod Left", new Vector3(-.042f, -.005f, .54f), new Vector3(.022f, .022f, .22f), BrushedSteel, mat);
                    Part(root.transform, "Bipod Right", new Vector3(.042f, -.005f, .54f), new Vector3(.022f, .022f, .22f), BrushedSteel, mat);
                    break;

                case 4: // 5: Subfusil Silenciado MP-7 "Abisal" (Compact PDW, monolithic suppressor, extended mag, reflex sight)
                    muzzleZ = 0.62f;
                    muzzleY = 0.065f;
                    Part(root.transform, "PDW Receiver", new Vector3(0, .052f, .12f), new Vector3(.068f, .095f, .28f), Polymer, mat);
                    Part(root.transform, "Collapsed Stock Bars", new Vector3(0, .048f, -.08f), new Vector3(.074f, .022f, .18f), BrushedSteel, mat);
                    Part(root.transform, "PDW Buttpad", new Vector3(0, .035f, -.17f), new Vector3(.064f, .13f, .028f), Polymer, mat);
                    var grip4 = Part(root.transform, "Center Grip", new Vector3(0, -.068f, .06f), new Vector3(.054f, .15f, .075f), Polymer, mat);
                    grip4.transform.localRotation = Quaternion.Euler(-8, 0, 0);
                    Part(root.transform, "Extended Stick Mag", new Vector3(0, -.145f, .06f), new Vector3(.042f, .22f, .055f), Gunmetal, mat);
                    Part(root.transform, "Folding Foregrip", new Vector3(0, -.035f, .23f), new Vector3(.042f, .11f, .042f), Polymer, mat);
                    Tube(root.transform, "Integral Suppressor", new Vector3(0, .062f, .43f), .028f, .36f, DarkSteel, mat);
                    Part(root.transform, "Suppressor Heat Band", new Vector3(0, .062f, .44f), new Vector3(.062f, .062f, .08f), Brass, mat);
                    Part(root.transform, "PDW Reflex Sight", new Vector3(0, .118f, .12f), new Vector3(.052f, .044f, .085f), Gunmetal, mat);
                    Part(root.transform, "PDW Reflex Lens", new Vector3(0, .124f, .12f), new Vector3(.040f, .030f, .010f), OpticCyan, mat, true);
                    break;

                default: // 6: Ametralladora Pesada M249 "Titan" (Box ammo drum, brass belt feed, heavy cooling shroud, carry handle)
                    muzzleZ = 0.82f;
                    muzzleY = 0.07f;
                    Part(root.transform, "LMG Heavy Receiver", new Vector3(0, .055f, .14f), new Vector3(.105f, .125f, .38f), Gunmetal, mat);
                    Part(root.transform, "Top Feed Cover", new Vector3(0, .125f, .13f), new Vector3(.092f, .032f, .28f), DarkSteel, mat);
                    Part(root.transform, "Heavy Buttstock", new Vector3(0, .025f, -.20f), new Vector3(.085f, .15f, .30f), Polymer, mat);
                    Part(root.transform, "Shoulder Rest", new Vector3(0, .085f, -.28f), new Vector3(.092f, .025f, .16f), BrushedSteel, mat);
                    var grip5 = Part(root.transform, "Saw Grip", new Vector3(0, -.078f, -.02f), new Vector3(.064f, .155f, .085f), Polymer, mat);
                    grip5.transform.localRotation = Quaternion.Euler(-14, 0, 0);
                    // Ammo Box Drum & Belt
                    Part(root.transform, "Ammo Box Pouch", new Vector3(-.015f, -.115f, .16f), new Vector3(.155f, .185f, .165f), OliveDrab, mat);
                    for (int b = 0; b < 5; b++)
                        Part(root.transform, "Ammo Belt Link " + b, new Vector3(-.065f + b * .012f, -.01f + b * .022f, .16f), new Vector3(.018f, .018f, .075f), Brass, mat);
                    Part(root.transform, "Perforated Barrel Shroud", new Vector3(0, .055f, .47f), new Vector3(.095f, .105f, .34f), DarkSteel, mat);
                    Tube(root.transform, "Heavy LMG Barrel", new Vector3(0, .072f, .58f), .024f, .52f, Gunmetal, mat);
                    Tube(root.transform, "Gas Cylinder", new Vector3(0, .025f, .54f), .018f, .44f, BrushedSteel, mat);
                    Part(root.transform, "Carry Handle", new Vector3(.055f, .155f, .30f), new Vector3(.028f, .085f, .12f), Polymer, mat);
                    Part(root.transform, "Titan Muzzle Compensator", new Vector3(0, .072f, .80f), new Vector3(.068f, .068f, .075f), HazardAmber, mat);
                    break;
            }

            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(root.transform, false);
            muzzle.localPosition = new Vector3(0, muzzleY, muzzleZ);
            return root;
        }

        public static GameObject CreateMeleeModel(int meleeKind, Transform parent, Material mat)
        {
            string[] names = { "Combat knife KM-9", "Boarding axe", "Heavy saw machete", "Pneumatic breach hammer" };
            var root = new GameObject(names[Mathf.Clamp(meleeKind, 0, 3)]);
            root.transform.SetParent(parent, false);

            switch (Mathf.Clamp(meleeKind, 0, 3))
            {
                case 0: // Cuchillo Tactico KM-9
                    Part(root.transform, "Grip", new Vector3(0, -.12f, 0), new Vector3(.042f, .18f, .048f), Polymer, mat);
                    Part(root.transform, "Pommel", new Vector3(0, -.22f, 0), new Vector3(.052f, .035f, .055f), Gunmetal, mat);
                    Part(root.transform, "Crossguard", new Vector3(0, -.02f, 0), new Vector3(.095f, .022f, .055f), Gunmetal, mat);
                    Part(root.transform, "Blade Spine", new Vector3(-.008f, .14f, 0), new Vector3(.038f, .29f, .018f), Gunmetal, mat);
                    Part(root.transform, "Keen Edge", new Vector3(.018f, .14f, 0), new Vector3(.022f, .28f, .009f), BrushedSteel, mat);
                    break;

                case 1: // Hacha de Abordaje
                    Part(root.transform, "Haft", new Vector3(0, -.08f, 0), new Vector3(.044f, .58f, .048f), ColorWalnut, mat);
                    Part(root.transform, "Rubber Grip", new Vector3(0, -.24f, 0), new Vector3(.050f, .24f, .054f), Polymer, mat);
                    Part(root.transform, "Axe Collar", new Vector3(0, .18f, 0), new Vector3(.068f, .14f, .062f), Gunmetal, mat);
                    Part(root.transform, "Axe Cheek", new Vector3(.095f, .19f, 0), new Vector3(.18f, .17f, .024f), DarkSteel, mat);
                    Part(root.transform, "Bevel Edge", new Vector3(.195f, .19f, 0), new Vector3(.045f, .21f, .012f), BrushedSteel, mat);
                    Part(root.transform, "Breach Spike", new Vector3(-.085f, .19f, 0), new Vector3(.12f, .055f, .024f), BrushedSteel, mat);
                    break;

                case 2: // Machete Sierra de Estiba
                    Part(root.transform, "Machete Grip", new Vector3(0, -.15f, 0), new Vector3(.046f, .22f, .054f), Polymer, mat);
                    Part(root.transform, "Knuckle D-Guard", new Vector3(.038f, -.14f, 0), new Vector3(.032f, .19f, .035f), HazardAmber, mat);
                    Part(root.transform, "Heavy Blade", new Vector3(.015f, .18f, 0), new Vector3(.068f, .44f, .016f), DarkSteel, mat);
                    Part(root.transform, "Razor Edge", new Vector3(.052f, .19f, 0), new Vector3(.024f, .43f, .008f), BrushedSteel, mat);
                    for (int t = 0; t < 6; t++)
                        Part(root.transform, "Saw Tooth " + t, new Vector3(-.026f, .03f + t * .058f, 0), new Vector3(.026f, .028f, .020f), HazardAmber, mat);
                    break;

                default: // Mazo Neumatico de Brecha
                    Part(root.transform, "Long Shaft", new Vector3(0, -.04f, 0), new Vector3(.048f, .72f, .048f), DarkSteel, mat);
                    Part(root.transform, "Lower Grip", new Vector3(0, -.26f, 0), new Vector3(.056f, .28f, .056f), Polymer, mat);
                    Part(root.transform, "Hammer Housing", new Vector3(0, .28f, 0), new Vector3(.28f, .15f, .14f), Gunmetal, mat);
                    Part(root.transform, "Impact Face", new Vector3(.165f, .28f, 0), new Vector3(.065f, .18f, .16f), HazardAmber, mat);
                    Part(root.transform, "Hydraulic Piston", new Vector3(-.14f, .28f, 0), new Vector3(.08f, .09f, .09f), BrushedSteel, mat);
                    Part(root.transform, "Kinetic Core", new Vector3(0, .28f, .076f), new Vector3(.12f, .045f, .015f), OpticRed, mat, true);
                    break;
            }
            return root;
        }

        public static void EnhanceSpecialInfected(SpecialInfected special, SandboxSession session)
        {
            if (special == null || session == null) return;
            Material mat = session.tracerMaterial;
            if (special.Species == SpecialSpecies.Bloater)
            {
                var chest = FindBone(special.transform, "Chest") ?? special.transform;
                Color toxic = new Color(.48f, .88f, .22f);
                Part(chest, "Toxic Sac Back", new Vector3(0, .08f, -.18f), new Vector3(.34f, .36f, .26f), toxic, mat, true, PrimitiveType.Sphere);
                Part(chest, "Toxic Pustule L", new Vector3(-.16f, .16f, -.10f), new Vector3(.18f, .18f, .18f), HazardAmber, mat, true, PrimitiveType.Sphere);
                Part(chest, "Toxic Pustule R", new Vector3(.17f, .02f, .14f), new Vector3(.20f, .20f, .20f), toxic, mat, true, PrimitiveType.Sphere);
            }
            else if (special.Species == SpecialSpecies.Colossus)
            {
                var chest = FindBone(special.transform, "Chest") ?? special.transform;
                var head = FindBone(special.transform, "Head");
                Part(chest, "Colossus Chest Plate", new Vector3(0, .06f, .16f), new Vector3(.44f, .38f, .12f), Gunmetal, mat);
                Part(chest, "Colossus Bio Core", new Vector3(0, .08f, .23f), new Vector3(.14f, .14f, .04f), OpticRed, mat, true);
                Part(chest, "Colossus Pauldron L", new Vector3(-.26f, .18f, 0), new Vector3(.20f, .18f, .26f), DarkSteel, mat);
                Part(chest, "Colossus Pauldron R", new Vector3(.26f, .18f, 0), new Vector3(.20f, .18f, .26f), DarkSteel, mat);
                if (head != null)
                    Part(head, "Colossus Riot Mask", new Vector3(0, .08f, .08f), new Vector3(.22f, .20f, .18f), Gunmetal, mat);
            }
        }

        private static Transform FindBone(Transform root, string name)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static GameObject Part(Transform parent, string name, Vector3 pos, Vector3 scale, Color color, Material mat, bool emissive = false, PrimitiveType primitive = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(primitive);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) { col.enabled = false; Object.Destroy(col); }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            ApplyColor(go.GetComponent<Renderer>(), mat, color, emissive);
            return go;
        }

        private static GameObject Tube(Transform parent, string name, Vector3 pos, float radius, float length, Color color, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            var col = go.GetComponent<Collider>();
            if (col != null) { col.enabled = false; Object.Destroy(col); }
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            go.transform.localScale = new Vector3(radius * 2f, length * 0.5f, radius * 2f);
            ApplyColor(go.GetComponent<Renderer>(), mat, color, false);
            return go;
        }

        private static void ApplyColor(Renderer renderer, Material mat, Color color, bool emissive)
        {
            if (renderer == null) return;
            if (mat != null) renderer.sharedMaterial = mat;
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor("_BaseColor", color);
            block.SetColor("_Color", color);
            if (emissive) block.SetColor("_EmissionColor", color * 1.8f);
            renderer.SetPropertyBlock(block);
        }
    }
}
