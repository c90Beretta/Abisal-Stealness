using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    [System.Serializable] public sealed class SectorSnapshot
    {
        public int charges,armory,bio;
        public float active,cooldown;
        public bool Valid => charges>=0&&charges<=3&&armory>=0&&armory<=2&&bio>=0&&bio<=2&&float.IsFinite(active)&&active>=0&&active<=60&&float.IsFinite(cooldown)&&cooldown>=0&&cooldown<=180;
    }
    public sealed class PortMapExpander : MonoBehaviour
    {
        public static readonly Vector3[] ExpandedSpawnPoints =
        {
            new Vector3(-8f, 0.1f, -32f),
            new Vector3(14f, 0.1f, -36f),
            new Vector3(2f, 0.1f, -28f),
            new Vector3(-38f, 0.1f, -4f),
            new Vector3(-42f, 0.1f, 4f),
            new Vector3(-5f, 0.1f, 34f),
            new Vector3(4f, 0.1f, 36f)
        };

        private SandboxSession session;
        private Transform root;
        private Material concreteMat, darkMat, tealMat, rustMat, yellowMat, whiteMat, greenMat, redMat;
        private readonly Dictionary<string, GameObject> propTemplates = new Dictionary<string, GameObject>();

        private Transform supplyDropPod;
        private Light supplyDropBeacon;
        private int supplyDropCharges = 1;

        private Transform southArmoryStation;
        private int armoryUses = 2;

        private Transform westBioStation;
        private int bioUses = 2;

        private Transform northTurretStation;
        private Light turretLight;
        private float turretActiveUntil, nextTurretZap, turretCooldownUntil;

        public int SupplyDropCharges => supplyDropCharges;
        public bool TurretActive => Time.time < turretActiveUntil;

        public void Initialize(SandboxSession value)
        {
            session = value;
            if (session == null || session.portMission == null) return;

            var existing = GameObject.Find("Port_Expanded_Sectors");
            if (existing != null)
            {
                if (Application.isPlaying) Destroy(existing);
                else DestroyImmediate(existing);
            }

            var env = GameObject.Find("Port_Environment") ?? GameObject.Find("Port_Environment_3D");
            root = new GameObject("Port_Expanded_Sectors").transform;
            if (env != null) root.SetParent(env.transform, false);

            CacheSceneMaterialsAndTemplates();
            OpenPerimeterPassages();
            BuildSouthHelipadAndContainerMaze();
            BuildWestBioQuarantineWing();
            BuildNorthPowerSubstation();
            Physics.SyncTransforms();
            var surface=env!=null?env.GetComponent<Unity.AI.Navigation.NavMeshSurface>():null;
            if(surface!=null)surface.BuildNavMesh();
        }

        public SectorSnapshot Capture() => new SectorSnapshot {charges=supplyDropCharges,armory=armoryUses,bio=bioUses,active=Mathf.Max(0,turretActiveUntil-Time.time),cooldown=Mathf.Max(0,turretCooldownUntil-Time.time)};
        public void Restore(SectorSnapshot value)
        {
            if(value==null||!value.Valid)return;
            supplyDropCharges=value.charges;armoryUses=value.armory;bioUses=value.bio;
            turretActiveUntil=Time.time+value.active;turretCooldownUntil=Time.time+value.cooldown;
        }
        public void DeliverHordeSupplyDrop()
        {
            supplyDropCharges = Mathf.Min(3, supplyDropCharges + 1);
            if (supplyDropPod != null) supplyDropPod.gameObject.SetActive(true);
            if (supplyDropBeacon != null) supplyDropBeacon.color = new Color(.2f, 1f, .55f);
        }

        public string GetNearbyPrompt()
        {
            if (session == null || session.player == null || !session.Running) return string.Empty;
            Vector3 p = session.player.transform.position;

            if (supplyDropPod != null && supplyDropCharges > 0 && FlatDistance(p, supplyDropPod.position) < 2.6f)
                return $"[E] Caja de Suministros de Horda ({supplyDropCharges} disp.)";
            if (southArmoryStation != null && armoryUses > 0 && FlatDistance(p, southArmoryStation.position) < 2.5f)
                return $"[E] Armeria Sur: +18 cartuchos y +1 placa ({armoryUses} disp.)";
            if (westBioStation != null && bioUses > 0 && FlatDistance(p, westBioStation.position) < 2.5f)
                return $"[E] Bio-Sintesis: +1 suero y +1 botiquin ({bioUses} disp.)";
            if (northTurretStation != null && FlatDistance(p, northTurretStation.position) < 2.6f)
            {
                if (Time.time < turretActiveUntil) return "[E] Pilono Tesla operativo (descargando)";
                if (Time.time < turretCooldownUntil) return $"[E] Pilono Tesla recargando ({Mathf.CeilToInt(turretCooldownUntil - Time.time)}s)";
                return "[E] Activar Pilono Electro-Defensivo (25s)";
            }
            return string.Empty;
        }

        public bool TryInteract()
        {
            if (session == null || session.player == null || !session.Running) return false;
            Vector3 p = session.player.transform.position;

            if (supplyDropPod != null && supplyDropCharges > 0 && FlatDistance(p, supplyDropPod.position) < 2.6f)
            {
                supplyDropCharges--;
                session.reserve = Mathf.Min(240, session.reserve + 24);
                session.State.bandages = Mathf.Min(12, session.State.bandages + 2);
                session.State.medkits = Mathf.Min(8, session.State.medkits + 1);
                if (session.Night != null)
                {
                    session.Night.Inventory.cells = Mathf.Min(8, session.Night.Inventory.cells + 1);
                    session.Night.Inventory.stamina = 100f;
                }
                TacticalAudio.Play(supplyDropPod.position, TacticalCue.Impact, 1.2f);
                session.SetMessage("Suministros de Horda recogidos: +24 municion, +1 botiquin, +2 vendas y +1 pila.");
                if (supplyDropBeacon != null && supplyDropCharges == 0)
                    supplyDropBeacon.color = new Color(.9f, .55f, .15f);
                return true;
            }

            if (southArmoryStation != null && armoryUses > 0 && FlatDistance(p, southArmoryStation.position) < 2.5f)
            {
                armoryUses--;
                session.reserve = Mathf.Min(240, session.reserve + 18);
                if (session.Night != null)
                {
                    session.Night.Inventory.plates = Mathf.Min(4, session.Night.Inventory.plates + 1);
                    session.Night.Inventory.armor = 100f;
                }
                TacticalAudio.Play(southArmoryStation.position, TacticalCue.Swing, 0.9f);
                session.SetMessage($"Armeria Sur utilizada: +18 municion y blindaje al 100% ({armoryUses} restantes).");
                return true;
            }

            if (westBioStation != null && bioUses > 0 && FlatDistance(p, westBioStation.position) < 2.5f)
            {
                bioUses--;
                session.State.doses = Mathf.Min(8, session.State.doses + 1);
                session.State.medkits = Mathf.Min(8, session.State.medkits + 1);
                session.State.bleeding = 0f;
                session.State.health = Mathf.Min(100f, session.State.health + 25f);
                TacticalAudio.Play(westBioStation.position, TacticalCue.Swing, 1.3f);
                session.SetMessage($"Bio-Sintesis completada: +1 suero, +1 botiquin y +25 vida ({bioUses} restantes).");
                return true;
            }

            if (northTurretStation != null && FlatDistance(p, northTurretStation.position) < 2.6f)
            {
                if (Time.time < turretActiveUntil || Time.time < turretCooldownUntil)
                {
                    session.SetMessage("El Pilono Electro-Defensivo se esta recargando.");
                    return true;
                }
                turretActiveUntil = Time.time + 25f;
                turretCooldownUntil = Time.time + 45f;
                session.EmitNoise(northTurretStation.position, 22f);
                TacticalAudio.Play(northTurretStation.position, TacticalCue.Burst, 1.4f);
                session.SetMessage("Pilono Electro-Defensivo activado (25s): electrocuta infectados en el sector norte.");
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (session == null || !session.Running) return;

            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && session.player != null)
            {
                if (kb.f6Key.wasPressedThisFrame) TeleportPlayer(new Vector3(11f, 0.1f, -31f), "Sector 07: Helipuerto Sur y Laberinto de Contenedores");
                else if (kb.f7Key.wasPressedThisFrame) TeleportPlayer(new Vector3(-39f, 0.1f, 0f), "Sector 08: Ala Oeste de Bio-Cuarentena");
                else if (kb.f8Key.wasPressedThisFrame) TeleportPlayer(new Vector3(-1f, 0.1f, 30f), "Sector 09: Subestacion Electrica Norte");
            }

            if (supplyDropBeacon != null)
                supplyDropBeacon.intensity = supplyDropCharges > 0 ? 3.2f + Mathf.Sin(Time.time * 5f) * 1.4f : 1.0f;

            if (turretLight != null)
                turretLight.color = Time.time < turretActiveUntil ? new Color(.25f, .92f, 1f) : new Color(.95f, .55f, .15f);

            if (Time.time < turretActiveUntil && Time.time >= nextTurretZap && northTurretStation != null && session.Enemies != null)
            {
                nextTurretZap = Time.time + 0.65f;
                Vector3 origin = northTurretStation.position + Vector3.up * 2.4f;
                foreach (var enemy in session.Enemies)
                {
                    if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy) continue;
                    if (Vector3.Distance(enemy.transform.position, northTurretStation.position) > 16f) continue;
                    enemy.Hit(22f, enemy.transform.position + Vector3.up * 1.2f, false, true);
                    enemy.Stagger(0.8f);
                    CombatEffects.ParryBurst(enemy.transform.position + Vector3.up * 1.1f, session.tracerMaterial);
                    var arc = new GameObject("Tesla Arc").AddComponent<LineRenderer>();
                    arc.sharedMaterial = session.tracerMaterial;
                    arc.startWidth = 0.06f;
                    arc.endWidth = 0.02f;
                    arc.startColor = arc.endColor = new Color(.3f, .95f, 1f);
                    arc.positionCount = 2;
                    arc.SetPosition(0, origin);
                    arc.SetPosition(1, enemy.transform.position + Vector3.up * 1.1f);
                    Destroy(arc.gameObject, 0.10f);
                    TacticalAudio.Play(enemy.transform.position, TacticalCue.Impact, 1.5f);
                    break;
                }
            }
        }

        private void CacheSceneMaterialsAndTemplates()
        {
            concreteMat = FindMat("Quay deck") ?? session.tracerMaterial;
            darkMat = FindMat("Submarine boarding deck") ?? concreteMat;
            tealMat = FindMat("Generator - E") ?? concreteMat;
            rustMat = FindMat("Container body") ?? concreteMat;
            yellowMat = FindMat("Quay safety stripe") ?? concreteMat;
            whiteMat = FindMat("Control approach marking") ?? concreteMat;
            greenMat = FindMat("Embarcar") ?? concreteMat;
            redMat = FindMat("Control rolling door") ?? concreteMat;

            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t == null) continue;
                if (t.name == "Cargo container" && !propTemplates.ContainsKey("Container")) propTemplates["Container"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Shelf") && !propTemplates.ContainsKey("Shelf")) propTemplates["Shelf"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Electroboxes") && !propTemplates.ContainsKey("Electrobox")) propTemplates["Electrobox"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Oxy_agregate") && !propTemplates.ContainsKey("Oxy")) propTemplates["Oxy"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Old_computer") && !propTemplates.ContainsKey("Computer")) propTemplates["Computer"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Cart") && !propTemplates.ContainsKey("Cart")) propTemplates["Cart"] = t.gameObject;
            }

            var water = GameObject.Find("Water");
            if (water != null) water.transform.localScale = new Vector3(180f, 0.15f, 160f);
        }

        private Material FindMat(string objectName)
        {
            var go = GameObject.Find(objectName);
            var r = go != null ? go.GetComponentInChildren<Renderer>() : null;
            return r != null ? r.sharedMaterial : null;
        }

        private void OpenPerimeterPassages()
        {
            // Open two wide passages in the South Perimeter (x = -11..-4 and x = 8..15)
            var south = GameObject.Find("South perimeter rebuilt") ?? GameObject.Find("South perimeter");
            if (south != null)
            {
                south.SetActive(false);
                Rail("South rail W", new Vector3(-14.5f, 0.45f, -20f), new Vector3(7f, 0.9f, 0.3f));
                Rail("South rail Mid", new Vector3(2f, 0.45f, -20f), new Vector3(12f, 0.9f, 0.3f));
                Rail("South rail E", new Vector3(21.5f, 0.45f, -20f), new Vector3(13f, 0.9f, 0.3f));
            }

            // Open a 6m passage in the West Perimeter (z = -3..3)
            var west = GameObject.Find("West perimeter rebuilt") ?? GameObject.Find("West perimeter");
            if (west != null)
            {
                west.SetActive(false);
                Rail("West rail S", new Vector3(-28f, 0.45f, -5.5f), new Vector3(0.3f, 0.9f, 5f));
                Rail("West rail N", new Vector3(-28f, 0.45f, 5.5f), new Vector3(0.3f, 0.9f, 5f));
            }

            // Open a 6m passage in the North Perimeter (x = -4..2)
            var north = GameObject.Find("North perimeter rebuilt") ?? GameObject.Find("North perimeter");
            if (north != null)
            {
                north.SetActive(false);
                Rail("North rail W", new Vector3(-8.5f, 0.45f, 24f), new Vector3(9f, 0.9f, 0.3f));
                Rail("North rail E", new Vector3(6.5f, 0.45f, 24f), new Vector3(9f, 0.9f, 0.3f));
            }
        }

        private void BuildSouthHelipadAndContainerMaze()
        {
            // Main South Deck: x from -16 to 26 (width 42), z from -20 to -44 (depth 24)
            Box("South expansion deck", new Vector3(5f, -0.5f, -32f), new Vector3(42f, 1f, 24f), concreteMat, true);

            // Outer guardrails
            Rail("South outer rail", new Vector3(5f, 0.45f, -44f), new Vector3(42f, 0.9f, 0.3f));
            Rail("South west rail", new Vector3(-16f, 0.45f, -32f), new Vector3(0.3f, 0.9f, 24f));
            Rail("South east rail", new Vector3(26f, 0.45f, -32f), new Vector3(0.3f, 0.9f, 24f));

            // Approach road markings
            for (int i = 0; i < 4; i++)
            {
                Box("South lane stripe W", new Vector3(-7.5f, 0.015f, -22f - i * 5f), new Vector3(0.22f, 0.02f, 2.4f), yellowMat, false);
                Box("South lane stripe E", new Vector3(11.5f, 0.015f, -22f - i * 5f), new Vector3(0.22f, 0.02f, 2.4f), yellowMat, false);
            }

            // Container Maze in South-West & Central South
            SpawnContainer(new Vector3(-12f, 0f, -26f), 0f, rustMat);
            SpawnContainer(new Vector3(-2f, 0f, -26f), 0f, tealMat);
            SpawnContainer(new Vector3(4f, 0f, -33f), 90f, rustMat);
            SpawnContainer(new Vector3(-3f, 0f, -39f), 0f, tealMat);

            // Helipad H-01 at (15, 0, -34)
            Box("Helipad tarmac", new Vector3(15f, 0.018f, -34f), new Vector3(12f, 0.025f, 12f), darkMat, false);
            Box("Helipad border N", new Vector3(15f, 0.03f, -28.2f), new Vector3(11.6f, 0.02f, 0.25f), yellowMat, false);
            Box("Helipad border S", new Vector3(15f, 0.03f, -39.8f), new Vector3(11.6f, 0.02f, 0.25f), yellowMat, false);
            Box("Helipad H Left", new Vector3(13.2f, 0.032f, -34f), new Vector3(0.45f, 0.02f, 4.2f), whiteMat, false);
            Box("Helipad H Right", new Vector3(16.8f, 0.032f, -34f), new Vector3(0.45f, 0.02f, 4.2f), whiteMat, false);
            Box("Helipad H Cross", new Vector3(15f, 0.032f, -34f), new Vector3(3.6f, 0.02f, 0.45f), whiteMat, false);
            WorldLabel("07 / HELIPUERTO H-01", new Vector3(15f, 0.05f, -26.5f), yellowMat, true);

            // Horde Supply Drop Pod on Helipad
            supplyDropPod = new GameObject("Horde Supply Drop Pod").transform;
            supplyDropPod.SetParent(root, false);
            supplyDropPod.position = new Vector3(15f, 0f, -34f);
            Box("Supply Crate Base", new Vector3(15f, 0.45f, -34f), new Vector3(1.5f, 0.9f, 1.2f), tealMat, true, supplyDropPod);
            Box("Supply Crate Lid", new Vector3(15f, 0.95f, -34f), new Vector3(1.6f, 0.14f, 1.3f), yellowMat, false, supplyDropPod);
            Box("Supply Signal Flare", new Vector3(15f, 1.15f, -34f), new Vector3(0.22f, 0.28f, 0.22f), greenMat, false, supplyDropPod);
            supplyDropBeacon = AddPointLight(new Vector3(15f, 1.8f, -34f), new Color(.2f, 1f, .55f), 8f, 3.2f);
            WorldLabel("SUMINISTROS DE HORDA [E]", new Vector3(15f, 1.85f, -34f), greenMat, false);

            // South Armory Station at (-10, 0, -37)
            southArmoryStation = new GameObject("South Tactical Armory").transform;
            southArmoryStation.SetParent(root, false);
            southArmoryStation.position = new Vector3(-10f, 0f, -37f);
            Box("Armory Locker", new Vector3(-10f, 1.1f, -37f), new Vector3(2.2f, 2.2f, 0.9f), darkMat, true, southArmoryStation);
            Box("Armory Trim", new Vector3(-10f, 1.2f, -36.52f), new Vector3(1.8f, 0.35f, 0.06f), yellowMat, false, southArmoryStation);
            CloneTemplate("Shelf", new Vector3(-13.2f, 1.1f, -37f), 0f);
            AddPointLight(new Vector3(-10f, 2.8f, -35.5f), new Color(1f, .78f, .42f), 8.5f, 2.8f);
            WorldLabel("ARMERIA SUR [E]", new Vector3(-10f, 2.55f, -37f), yellowMat, false);
        }

        private void BuildWestBioQuarantineWing()
        {
            // West Wing Deck: x from -48 to -28 (width 20), z from -8 to 8 (depth 16)
            Box("West quarantine deck", new Vector3(-38f, -0.5f, 0f), new Vector3(20f, 1f, 16f), concreteMat, true);
            Box("West cleanroom floor", new Vector3(-38f, 0.015f, 0f), new Vector3(18f, 0.02f, 14f), tealMat, false);

            Rail("West wing N rail", new Vector3(-38f, 0.45f, 8f), new Vector3(20f, 0.9f, 0.3f));
            Rail("West wing S rail", new Vector3(-38f, 0.45f, -8f), new Vector3(20f, 0.9f, 0.3f));
            Rail("West wing W rail", new Vector3(-48f, 0.45f, 0f), new Vector3(0.3f, 0.9f, 16f));

            // Decontamination Archways
            foreach (float x in new[] { -31f, -36f })
            {
                Box("Decon Arch L", new Vector3(x, 1.4f, -2.6f), new Vector3(0.4f, 2.8f, 0.4f), yellowMat, true);
                Box("Decon Arch R", new Vector3(x, 1.4f, 2.6f), new Vector3(0.4f, 2.8f, 0.4f), yellowMat, true);
                Box("Decon Arch Top", new Vector3(x, 2.9f, 0f), new Vector3(0.45f, 0.35f, 5.6f), yellowMat, false);
            }

            // Cryo Bio-Containers
            for (int i = -1; i <= 1; i += 2)
            {
                var tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tank.name = "Bio Cryo Cylinder";
                tank.transform.SetParent(root, false);
                tank.transform.position = new Vector3(-39f, 1.3f, i * 5.2f);
                tank.transform.localScale = new Vector3(1.6f, 1.3f, 1.6f);
                tank.GetComponent<Renderer>().sharedMaterial = greenMat;
                CloneTemplate("Oxy", new Vector3(-43f, 0.75f, i * 5.2f), 0f);
            }

            // Bio-Synthesis Station at (-44, 0, 0)
            westBioStation = new GameObject("West Bio Synthesis Station").transform;
            westBioStation.SetParent(root, false);
            westBioStation.position = new Vector3(-44f, 0f, 0f);
            Box("Bio Lab Bench", new Vector3(-44f, 0.55f, 0f), new Vector3(1.4f, 1.1f, 3.4f), darkMat, true, westBioStation);
            Box("Bio Display", new Vector3(-43.25f, 1.35f, 0f), new Vector3(0.08f, 0.55f, 1.6f), greenMat, false, westBioStation);
            CloneTemplate("Computer", new Vector3(-44f, 1.1f, 0f), 90f);
            AddPointLight(new Vector3(-41.5f, 2.9f, 0f), new Color(.35f, .95f, .88f), 9f, 3.0f);
            WorldLabel("08 / BIO-CUARENTENA", new Vector3(-35f, 0.05f, 0f), whiteMat, true);
            WorldLabel("BIO-SINTESIS [E]", new Vector3(-44f, 2.35f, 0f), greenMat, false);
        }

        private void BuildNorthPowerSubstation()
        {
            // North Substation Deck: x from -12 to 10 (width 22), z from 24 to 42 (depth 18)
            Box("North substation deck", new Vector3(-1f, -0.5f, 33f), new Vector3(22f, 1f, 18f), concreteMat, true);

            Rail("North sub W rail", new Vector3(-12f, 0.45f, 33f), new Vector3(0.3f, 0.9f, 18f));
            Rail("North sub E rail", new Vector3(10f, 0.45f, 33f), new Vector3(0.3f, 0.9f, 18f));
            Rail("North sub N rail", new Vector3(-1f, 0.45f, 42f), new Vector3(22f, 0.9f, 0.3f));

            // Transformer banks
            foreach (float x in new[] { -8f, 6f })
            {
                Box("Transformer Unit", new Vector3(x, 1.25f, 35f), new Vector3(2.6f, 2.5f, 3.2f), darkMat, true);
                Box("Transformer Coils", new Vector3(x, 2.75f, 35f), new Vector3(1.8f, 0.6f, 2.4f), rustMat, false);
                CloneTemplate("Electrobox", new Vector3(x, 1.3f, 33.2f), 0f);
            }

            // Tesla Defense Pylon at (-1, 0, 33)
            northTurretStation = new GameObject("North Tesla Defense Pylon").transform;
            northTurretStation.SetParent(root, false);
            northTurretStation.position = new Vector3(-1f, 0f, 33f);
            Box("Pylon Base", new Vector3(-1f, 0.6f, 33f), new Vector3(1.8f, 1.2f, 1.8f), darkMat, true, northTurretStation);
            Box("Pylon Spire", new Vector3(-1f, 2.1f, 33f), new Vector3(0.55f, 2.2f, 0.55f), tealMat, true, northTurretStation);
            Box("Pylon Emitter Ring", new Vector3(-1f, 3.1f, 33f), new Vector3(1.2f, 0.25f, 1.2f), greenMat, false, northTurretStation);
            turretLight = AddPointLight(new Vector3(-1f, 3.5f, 33f), new Color(.95f, .55f, .15f), 10f, 3.2f);
            WorldLabel("09 / SUBESTACION", new Vector3(-1f, 0.05f, 27.5f), yellowMat, true);
            WorldLabel("PILONO DEFENSIVO [E]", new Vector3(-1f, 3.7f, 33f), greenMat, false);
        }

        private void SpawnContainer(Vector3 pos, float yaw, Material mat)
        {
            if (propTemplates.TryGetValue("Container", out var tpl) && tpl != null)
            {
                var inst = Instantiate(tpl, root);
                inst.transform.position = pos;
                inst.transform.rotation = Quaternion.Euler(0, yaw, 0);
                var body = inst.transform.Find("Container body");
                if (body != null && mat != null) body.GetComponent<Renderer>().sharedMaterial = mat;
                return;
            }
            var box = Box("Cargo container", pos + Vector3.up * 1.2f, new Vector3(6f, 2.4f, 3f), mat, true);
            box.transform.rotation = Quaternion.Euler(0, yaw, 0);
        }

        private void CloneTemplate(string key, Vector3 pos, float yaw)
        {
            if (!propTemplates.TryGetValue(key, out var tpl) || tpl == null) return;
            var clone = Instantiate(tpl, root);
            clone.transform.position = pos;
            clone.transform.rotation = Quaternion.Euler(0, yaw, 0);
        }

        private void Rail(string name, Vector3 pos, Vector3 size)
        {
            Box(name, pos, size, darkMat, true);
            Box(name + " cap", pos + Vector3.up * (size.y * 0.5f), new Vector3(size.x, 0.07f, size.z), yellowMat, false);
        }

        private GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, bool solid, Transform parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent != null ? parent : root, false);
            go.transform.position = pos;
            go.transform.localScale = size;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!solid)
            {
                if (Application.isPlaying) { go.GetComponent<Collider>().enabled=false; Destroy(go.GetComponent<Collider>()); }
                else DestroyImmediate(go.GetComponent<Collider>());
            }
            return go;
        }

        private Light AddPointLight(Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject("Expanded sector light");
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            return light;
        }

        private void WorldLabel(string text, Vector3 point, Material mat, bool floor)
        {
            var item = new GameObject(text).AddComponent<TextMesh>();
            item.transform.SetParent(root, false);
            item.transform.position = point;
            item.transform.rotation = floor ? Quaternion.Euler(90, 0, 0) : Quaternion.Euler(30, 45, 0);
            item.text = text;
            item.fontSize = 44;
            item.characterSize = floor ? 0.07f : 0.042f;
            item.anchor = TextAnchor.MiddleCenter;
            item.color = mat != null && mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : Color.white;
        }

        private void TeleportPlayer(Vector3 destination, string sectorLabel)
        {
            var cc = session.player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            session.player.transform.position = destination;
            if (cc != null) cc.enabled = true;
            Physics.SyncTransforms();
            session.SetMessage("Traslado rapido a " + sectorLabel + ".");
        }

        private static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
