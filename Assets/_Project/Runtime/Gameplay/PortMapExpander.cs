using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Zombtoy.Gameplay
{
    public enum GameMapId
    {
        PortCargo = 0,
        MassiveSubmarine = 1,
        AbyssalWaterCave = 2
    }

    [Serializable]
    public sealed class SectorSnapshot
    {
        public int mapId;
        public int supplyDropCharges = 1;
        public int armoryUses = 2;
        public int bioUses = 2;

        public bool Valid => mapId >= 0 && mapId <= 2 && supplyDropCharges >= 0 && supplyDropCharges <= 10 && armoryUses >= 0 && armoryUses <= 10 && bioUses >= 0 && bioUses <= 10;
    }

    public sealed class PortMapExpander : MonoBehaviour
    {
        public static readonly string[] MapTitles =
        {
            "01 / Puerto Carguero Expandido",
            "02 / Submarino Masivo B-07",
            "03 / Cueva Submarina Abisal"
        };

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

        public const string MapRootName = "Port_Expanded_Sectors";

        public GameMapId CurrentMap { get; private set; } = GameMapId.PortCargo;

        private SandboxSession session;
        private Transform root;
        private GameObject basePortEnv;
        private NavMeshSurface basePortNavSurface;
        private NavMeshSurface dynamicMapNavSurface;
        private Material concreteMat, darkMat, tealMat, rustMat, yellowMat, whiteMat, greenMat, redMat, waterMat;
        private readonly Dictionary<string, GameObject> propTemplates = new Dictionary<string, GameObject>();
        private readonly List<(GameObject go, bool initialActive)> basePortChildren = new List<(GameObject, bool)>();
        private readonly List<GameObject> perimeterWalls = new List<GameObject>();

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

        private readonly List<Transform> animatedWaterRipples = new List<Transform>();

        public int SupplyDropCharges => supplyDropCharges;
        public bool TurretActive => Time.time < turretActiveUntil;

        public void Initialize(SandboxSession value, GameMapId mapId = GameMapId.PortCargo)
        {
            session = value != null ? value : (GetComponent<SandboxSession>() ?? FindAnyObjectByType<SandboxSession>());
            if (session == null || session.portMission == null) return;
            EnsureCached();
            BuildMap(mapId, false);
        }

        public void BuildMap(GameMapId mapId, bool announceAndResetPlayer = false)
        {
            if (session == null) session = GetComponent<SandboxSession>() ?? FindAnyObjectByType<SandboxSession>();
            if (session == null || session.portMission == null) return;

            EnsureCached();
            CurrentMap = mapId;
            animatedWaterRipples.Clear();
            supplyDropPod = null;
            supplyDropBeacon = null;
            southArmoryStation = null;
            westBioStation = null;
            northTurretStation = null;
            turretLight = null;

            DestroyAllExistingMapRoots();

            // Toggle ALL base Port_Environment_3D children + stray root port bulkheads cleanly
            SetBasePortActive(mapId == GameMapId.PortCargo);
            Physics.SyncTransforms();

            // Keep Port_Expanded_Sectors at scene root so it is never entangled with Port_Environment_3D
            root = new GameObject(MapRootName).transform;

            if (mapId == GameMapId.PortCargo)
            {
                OpenPerimeterPassages();
                session.missionTitle = "ABISAL / 01 - PUERTO CARGUERO";
                PopulateMainPortDeckWithDenseProps();
                BuildSouthHelipadAndContainerMaze();
                BuildWestBioQuarantineWing();
                BuildNorthPowerSubstation();
                BuildEastDockyardDetails();
            }
            else if (mapId == GameMapId.MassiveSubmarine)
            {
                session.missionTitle = "ABISAL / 02 - SUBMARINO MASIVO B-07";
                BuildMassiveSubmarineMap();
            }
            else
            {
                session.missionTitle = "ABISAL / 03 - CUEVA SUBMARINA ABISAL";
                BuildAbyssalWaterCaveMap();
            }

            Physics.SyncTransforms();
            UpdateNavigationForActiveMap(mapId);

            if (announceAndResetPlayer && session.player != null)
            {
                TeleportPlayer(session.portMission.arrival.position, MapTitles[(int)mapId]);
            }
        }

        private void UpdateNavigationForActiveMap(GameMapId mapId)
        {
            if (basePortNavSurface == null && basePortEnv != null)
                basePortNavSurface = basePortEnv.GetComponent<NavMeshSurface>();

            if (mapId == GameMapId.PortCargo)
            {
                if (dynamicMapNavSurface != null)
                {
                    dynamicMapNavSurface.RemoveData();
                    dynamicMapNavSurface = null;
                }
                if (basePortNavSurface != null && !basePortNavSurface.enabled)
                    basePortNavSurface.enabled = true;
                return;
            }

            if (basePortNavSurface != null)
                basePortNavSurface.enabled = false;

            if (root != null)
            {
                dynamicMapNavSurface = root.gameObject.AddComponent<NavMeshSurface>();
                dynamicMapNavSurface.collectObjects = CollectObjects.Children;
                dynamicMapNavSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                dynamicMapNavSurface.BuildNavMesh();
            }
        }

        private void DestroyAllExistingMapRoots()
        {
            if (dynamicMapNavSurface != null)
            {
                dynamicMapNavSurface.RemoveData();
                dynamicMapNavSurface = null;
            }

            var toDestroy = new List<GameObject>();
            if (root != null) toDestroy.Add(root.gameObject);
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t != null && (t.name == MapRootName || t.name == "__Disposed_Map_Root") && !toDestroy.Contains(t.gameObject))
                    toDestroy.Add(t.gameObject);
            }
            root = null;
            foreach (var go in toDestroy)
            {
                if (go == null) continue;
                var nav = go.GetComponent<NavMeshSurface>();
                if (nav != null) nav.RemoveData();
                // Immediately disable every collider AND deactivate BEFORE Destroy so zero ghost colliders remain
                foreach (var col in go.GetComponentsInChildren<Collider>(true))
                    if (col != null) col.enabled = false;
                go.SetActive(false);
                go.name = "__Disposed_Map_Root";
                if (Application.isPlaying) Destroy(go);
                else DestroyImmediate(go);
            }
        }

        public void DeliverHordeSupplyDrop()
        {
            supplyDropCharges = Mathf.Min(3, supplyDropCharges + 1);
            if (supplyDropPod != null) supplyDropPod.gameObject.SetActive(true);
            if (supplyDropBeacon != null) supplyDropBeacon.color = new Color(.2f, 1f, .55f);
        }

        public SectorSnapshot Capture() => new SectorSnapshot
        {
            mapId = (int)CurrentMap,
            supplyDropCharges = supplyDropCharges,
            armoryUses = armoryUses,
            bioUses = bioUses
        };

        public void Restore(SectorSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.Valid) return;
            var targetMap = (GameMapId)Mathf.Clamp(snapshot.mapId, 0, 2);
            if (targetMap != CurrentMap) BuildMap(targetMap, false);
            supplyDropCharges = snapshot.supplyDropCharges;
            armoryUses = snapshot.armoryUses;
            bioUses = snapshot.bioUses;
            if (supplyDropBeacon != null)
                supplyDropBeacon.color = supplyDropCharges > 0 ? new Color(.2f, 1f, .55f) : new Color(.4f, .45f, .5f);
        }

        public string GetNearbyPrompt()
        {
            if (session == null || session.player == null || !session.Running) return string.Empty;
            Vector3 p = session.player.transform.position;

            if (supplyDropPod != null && supplyDropCharges > 0 && FlatDistance(p, supplyDropPod.position) < 2.6f)
                return $"[E] Suministros de Horda ({supplyDropCharges} disp.)";
            if (southArmoryStation != null && armoryUses > 0 && FlatDistance(p, southArmoryStation.position) < 2.5f)
                return $"[E] Armeria Tactica: +18 cartuchos y +1 placa ({armoryUses} disp.)";
            if (westBioStation != null && bioUses > 0 && FlatDistance(p, westBioStation.position) < 2.5f)
                return $"[E] Bio-Sintesis: +1 suero y +1 botiquin ({bioUses} disp.)";
            if (northTurretStation != null && FlatDistance(p, northTurretStation.position) < 2.6f)
            {
                if (Time.time < turretActiveUntil) return "[E] Pilono Defensivo operativo (descargando)";
                if (Time.time < turretCooldownUntil) return $"[E] Pilono Defensivo recargando ({Mathf.CeilToInt(turretCooldownUntil - Time.time)}s)";
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
                session.SetMessage("Suministros recogidos: +24 municion, +1 botiquin, +2 vendas y +1 pila.");
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
                session.SetMessage($"Armeria utilizada: +18 municion y blindaje al 100% ({armoryUses} restantes).");
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
                session.SetMessage("Pilono Electro-Defensivo activado (25s): electrocuta infectados cercanos.");
                return true;
            }

            return false;
        }

        private void Update()
        {
            if (session == null || !session.Running) return;

            var kb = Keyboard.current;
            if (kb != null && session.player != null)
            {
                if (kb.f6Key.wasPressedThisFrame) TeleportPlayer(new Vector3(11f, 0.1f, -31f), "Sector Sur (Helipuerto / Hangar / Laguna)");
                else if (kb.f7Key.wasPressedThisFrame) TeleportPlayer(new Vector3(-39f, 0.1f, 0f), "Sector Oeste (Bio-Cuarentena / Sala Torpedos / Gruta)");
                else if (kb.f8Key.wasPressedThisFrame) TeleportPlayer(new Vector3(-1f, 0.1f, 30f), "Sector Norte (Subestacion / Reactor / Geiseres)");
                else if (kb.f10Key.wasPressedThisFrame) BuildMap(GameMapId.PortCargo, true);
                else if (kb.f11Key.wasPressedThisFrame) BuildMap(GameMapId.MassiveSubmarine, true);
                else if (kb.f12Key.wasPressedThisFrame) BuildMap(GameMapId.AbyssalWaterCave, true);
            }

            if (supplyDropBeacon != null)
                supplyDropBeacon.intensity = supplyDropCharges > 0 ? 3.2f + Mathf.Sin(Time.time * 5f) * 1.4f : 1.0f;

            if (turretLight != null)
                turretLight.color = Time.time < turretActiveUntil ? new Color(.25f, .92f, 1f) : new Color(.95f, .55f, .15f);

            for (int i = 0; i < animatedWaterRipples.Count; i++)
            {
                var r = animatedWaterRipples[i];
                if (r == null) continue;
                float pulse = 1f + Mathf.Sin(Time.time * 2.6f + i * 1.3f) * 0.12f;
                r.localScale = new Vector3(r.localScale.x, 0.02f, pulse * 3.2f);
            }

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

        // =====================================================================
        // MAP 01: DENSE PORT CARGO EXPANSION
        // =====================================================================
        private void PopulateMainPortDeckWithDenseProps()
        {
            // Keep NavMesh test routes clear:
            // Route A: (-24,-15) -> (-22, 15.2) along x ~ -23
            // Route B: (-22, 15.2) -> (5, 13.2) along z ~ 12..15
            // Route C: (5, 13.2) -> (22, 16.2) along z ~ 11..15
            // Route D: (22, 16.2) -> (39, -9.8) along x ~ 22..26 -> z = -8
            // Sightline at (-10, -17) -> (-10, -15) kept open!

            // Second-story stacked containers on existing central yard containers:
            SpawnContainer(new Vector3(-18f, 2.42f, -2f), 0f, rustMat);
            SpawnContainer(new Vector3(0f, 2.42f, -2f), 0f, rustMat);
            SpawnContainer(new Vector3(-6f, 2.42f, 5f), 0f, rustMat);

            // Additional yard containers in dead zones (east-central & south-east yard)
            SpawnContainer(new Vector3(9f, 0f, -2f), 0f, tealMat);
            SpawnContainer(new Vector3(9f, 2.42f, -2f), 0f, rustMat);
            SpawnContainer(new Vector3(11f, 0f, 5f), 0f, rustMat);
            SpawnContainer(new Vector3(2f, 0f, -14f), 0f, tealMat);
            SpawnContainer(new Vector3(13f, 0f, -14f), 0f, rustMat);

            // Barrel clusters and industrial crate stacks across the main dock
            IndustrialPropFactory.BarrelCluster(root, new Vector3(-14f, 0f, -4.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(-2.5f, 0f, 2.2f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(15.5f, 0f, -2.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(-16f, 0f, -13f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(7.5f, 0f, -11.5f), rustMat, yellowMat, darkMat);

            IndustrialPropFactory.CrateStack(root, new Vector3(-12.5f, 0f, 2.2f), 0f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(4.5f, 0f, 1.8f), 90f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(16.5f, 0f, 3.5f), 180f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(-4.5f, 0f, -14.5f), 0f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(17.5f, 0f, -14.5f), 90f, rustMat, tealMat, darkMat);

            // Overhead steam pipe bridges linking Warehouse -> Generator -> Control Room
            IndustrialPropFactory.PipeBridge(root, new Vector3(-14f, 0f, 17f), new Vector3(2f, 0f, 17f), 3.8f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.PipeBridge(root, new Vector3(7f, 0f, 17f), new Vector3(14f, 0f, 17f), 3.8f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.PipeBridge(root, new Vector3(-19f, 0f, -10.5f), new Vector3(-19f, 0f, 8.5f), 4.0f, darkMat, rustMat, yellowMat);

            // Jersey concrete barriers along security lanes
            IndustrialPropFactory.JerseyBarrierRow(root, new Vector3(-16f, 0f, -18.5f), new Vector3(-12f, 0f, -18.5f), 2, concreteMat, yellowMat);
            IndustrialPropFactory.JerseyBarrierRow(root, new Vector3(-3f, 0f, -8.5f), new Vector3(3f, 0f, -8.5f), 2, concreteMat, yellowMat);
            IndustrialPropFactory.JerseyBarrierRow(root, new Vector3(15f, 0f, -8.5f), new Vector3(20f, 0f, -8.5f), 2, concreteMat, yellowMat);

            // Floodlight towers illuminating yard
            IndustrialPropFactory.FloodlightTower(root, new Vector3(-13.5f, 0f, -12.5f), 45f, darkMat, yellowMat, new Color(1f, .78f, .45f));
            IndustrialPropFactory.FloodlightTower(root, new Vector3(6.5f, 0f, -6.5f), -35f, darkMat, yellowMat, new Color(1f, .82f, .50f));
            IndustrialPropFactory.FloodlightTower(root, new Vector3(19.5f, 0f, 4.5f), 210f, darkMat, yellowMat, new Color(.75f, .92f, .82f));

            // Extra props around Generator & Crane North Yard
            CloneTemplate("Oxy", new Vector3(1.2f, 0.72f, 16.5f), 0f);
            CloneTemplate("Oxy", new Vector3(8.8f, 0.72f, 16.5f), 180f);
            CloneTemplate("Electrobox", new Vector3(2.5f, 1.2f, 19.5f), 0f);
            CloneTemplate("Cart", new Vector3(-8.5f, 0.48f, 16.8f), 25f);
            CloneTemplate("Cart", new Vector3(12.5f, 0.48f, -5.5f), -40f);
        }

        private void BuildSouthHelipadAndContainerMaze()
        {
            // Main South Deck: x from -24 to 28 (width 52), z from -20 to -48 (depth 28)
            Box("South expansion deck", new Vector3(2f, -0.5f, -34f), new Vector3(52f, 1f, 28f), concreteMat, true);

            // Outer guardrails
            Rail("South outer rail", new Vector3(2f, 0.45f, -48f), new Vector3(52f, 0.9f, 0.3f));
            Rail("South west rail", new Vector3(-24f, 0.45f, -34f), new Vector3(0.3f, 0.9f, 28f));
            Rail("South east rail", new Vector3(28f, 0.45f, -34f), new Vector3(0.3f, 0.9f, 28f));

            // Road stripes & hazard zones
            for (int i = 0; i < 5; i++)
            {
                Box("South lane stripe W", new Vector3(-7.5f, 0.015f, -22f - i * 5f), new Vector3(0.22f, 0.02f, 2.4f), yellowMat, false);
                Box("South lane stripe E", new Vector3(11.5f, 0.015f, -22f - i * 5f), new Vector3(0.22f, 0.02f, 2.4f), yellowMat, false);
            }

            // Dense Container Labyrinth (2-story stacks, angled corridors, crate barricades)
            SpawnContainer(new Vector3(-16f, 0f, -25f), 0f, rustMat);
            SpawnContainer(new Vector3(-16f, 2.42f, -25f), 0f, tealMat);
            SpawnContainer(new Vector3(-19f, 0f, -32f), 90f, tealMat);
            SpawnContainer(new Vector3(-2f, 0f, -25f), 0f, tealMat);
            SpawnContainer(new Vector3(-2f, 2.42f, -25f), 0f, rustMat);
            SpawnContainer(new Vector3(4.5f, 0f, -31f), 90f, rustMat);
            SpawnContainer(new Vector3(-3.5f, 0f, -37f), 0f, tealMat);
            SpawnContainer(new Vector3(-15f, 0f, -43f), 0f, rustMat);
            SpawnContainer(new Vector3(-4f, 0f, -44f), 0f, rustMat);
            SpawnContainer(new Vector3(23.5f, 0f, -26f), 90f, tealMat);
            SpawnContainer(new Vector3(23.5f, 0f, -38f), 90f, rustMat);

            IndustrialPropFactory.CrateStack(root, new Vector3(-11.5f, 0f, -28.5f), 45f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(1.5f, 0f, -29.5f), 0f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(-7.5f, 0f, -41.5f), 90f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(8.5f, 0f, -42.5f), 0f, rustMat, tealMat, darkMat);

            IndustrialPropFactory.BarrelCluster(root, new Vector3(-12.5f, 0f, -23.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(7.5f, 0f, -25.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(-18.5f, 0f, -38.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(19.5f, 0f, -43.5f), rustMat, yellowMat, darkMat);

            IndustrialPropFactory.PipeBridge(root, new Vector3(-14f, 0f, -21f), new Vector3(-14f, 0f, -36f), 3.9f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.FloodlightTower(root, new Vector3(8.5f, 0f, -27.5f), 135f, darkMat, yellowMat, new Color(1f, .78f, .42f));
            IndustrialPropFactory.FloodlightTower(root, new Vector3(21.5f, 0f, -41.5f), -45f, darkMat, yellowMat, new Color(.45f, .95f, .68f));

            // Helipad H-01 at (15, 0, -34)
            Box("Helipad tarmac", new Vector3(15f, 0.018f, -34f), new Vector3(12f, 0.025f, 12f), darkMat, false);
            Box("Helipad border N", new Vector3(15f, 0.03f, -28.2f), new Vector3(11.6f, 0.02f, 0.25f), yellowMat, false);
            Box("Helipad border S", new Vector3(15f, 0.03f, -39.8f), new Vector3(11.6f, 0.02f, 0.25f), yellowMat, false);
            Box("Helipad border W", new Vector3(9.2f, 0.03f, -34f), new Vector3(0.25f, 0.02f, 11.6f), yellowMat, false);
            Box("Helipad border E", new Vector3(20.8f, 0.03f, -34f), new Vector3(0.25f, 0.02f, 11.6f), yellowMat, false);
            Box("Helipad H Left", new Vector3(13.2f, 0.032f, -34f), new Vector3(0.45f, 0.02f, 4.2f), whiteMat, false);
            Box("Helipad H Right", new Vector3(16.8f, 0.032f, -34f), new Vector3(0.45f, 0.02f, 4.2f), whiteMat, false);
            Box("Helipad H Cross", new Vector3(15f, 0.032f, -34f), new Vector3(3.6f, 0.02f, 0.45f), whiteMat, false);
            WorldLabel("07 / HELIPUERTO H-01", new Vector3(15f, 0.05f, -26.5f), yellowMat, true);

            CreateSupplyDropPod(new Vector3(15f, 0f, -34f));
            CreateArmoryStation(new Vector3(-10f, 0f, -37f), "ARMERIA SUR [E]");
        }

        private void BuildWestBioQuarantineWing()
        {
            // West Wing Deck: x from -52 to -28 (width 24), z from -12 to 12 (depth 24)
            Box("West quarantine deck", new Vector3(-40f, -0.5f, 0f), new Vector3(24f, 1f, 24f), concreteMat, true);
            Box("West cleanroom floor", new Vector3(-40f, 0.015f, 0f), new Vector3(21f, 0.02f, 21f), tealMat, false);

            Rail("West wing N rail", new Vector3(-40f, 0.45f, 12f), new Vector3(24f, 0.9f, 0.3f));
            Rail("West wing S rail", new Vector3(-40f, 0.45f, -12f), new Vector3(24f, 0.9f, 0.3f));
            Rail("West wing W rail", new Vector3(-52f, 0.45f, 0f), new Vector3(0.3f, 0.9f, 24f));
            Rail("West wing E-S rail", new Vector3(-28f, 0.45f, -10f), new Vector3(0.3f, 0.9f, 4f));
            Rail("West wing E-N rail", new Vector3(-28f, 0.45f, 10f), new Vector3(0.3f, 0.9f, 4f));

            // Decontamination Archways
            foreach (float x in new[] { -30.5f, -35f, -39.5f })
            {
                Box("Decon Arch L", new Vector3(x, 1.4f, -2.6f), new Vector3(0.4f, 2.8f, 0.4f), yellowMat, true);
                Box("Decon Arch R", new Vector3(x, 1.4f, 2.6f), new Vector3(0.4f, 2.8f, 0.4f), yellowMat, true);
                Box("Decon Arch Top", new Vector3(x, 2.9f, 0f), new Vector3(0.45f, 0.35f, 5.6f), yellowMat, false);
            }

            // 6 Cryo Bio-Containers & Medical Racks
            for (int side = -1; side <= 1; side += 2)
            {
                for (int idx = 0; idx < 3; idx++)
                {
                    float x = -33.5f - idx * 4.5f;
                    float z = side * 6.5f;
                    var tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    tank.name = "Bio Cryo Cylinder";
                    tank.transform.SetParent(root, false);
                    tank.transform.position = new Vector3(x, 1.3f, z);
                    tank.transform.localScale = new Vector3(1.5f, 1.3f, 1.5f);
                    tank.GetComponent<Renderer>().sharedMaterial = greenMat;
                    Box("Cryo Cap", new Vector3(x, 2.65f, z), new Vector3(1.65f, 0.22f, 1.65f), darkMat, false);
                    Box("Cryo Base", new Vector3(x, 0.15f, z), new Vector3(1.75f, 0.30f, 1.75f), darkMat, true);
                }
                CloneTemplate("Shelf", new Vector3(-47f, 1.1f, side * 8.5f), 0f);
                CloneTemplate("Oxy", new Vector3(-31.5f, 0.75f, side * 9.2f), 0f);
                CloneTemplate("Cart", new Vector3(-38f, 0.48f, side * 9.0f), 90f);
            }

            IndustrialPropFactory.BarrelCluster(root, new Vector3(-48.5f, 0f, -8.5f), rustMat, yellowMat, darkMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(-48.5f, 0f, 8.5f), 90f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.FloodlightTower(root, new Vector3(-32f, 0f, -9.5f), 45f, darkMat, yellowMat, new Color(.35f, .95f, .88f));
            IndustrialPropFactory.FloodlightTower(root, new Vector3(-32f, 0f, 9.5f), 135f, darkMat, yellowMat, new Color(.35f, .95f, .88f));

            WorldLabel("08 / BIO-CUARENTENA", new Vector3(-35f, 0.05f, 0f), whiteMat, true);
            CreateBioStation(new Vector3(-45f, 0f, 0f), "BIO-SINTESIS [E]");
        }

        private void BuildNorthPowerSubstation()
        {
            // North Substation Deck: x from -14 to 14 (width 28), z from 24 to 46 (depth 22)
            Box("North substation deck", new Vector3(0f, -0.5f, 35f), new Vector3(28f, 1f, 22f), concreteMat, true);

            Rail("North sub W rail", new Vector3(-14f, 0.45f, 35f), new Vector3(0.3f, 0.9f, 22f));
            Rail("North sub E rail", new Vector3(14f, 0.45f, 35f), new Vector3(0.3f, 0.9f, 22f));
            Rail("North sub N rail", new Vector3(0f, 0.45f, 46f), new Vector3(28f, 0.9f, 0.3f));

            // 4 High-Voltage Transformer Banks & Cooling Towers
            foreach (float x in new[] { -9f, 8f })
            {
                for (int r = 0; r < 2; r++)
                {
                    float z = 30f + r * 8.5f;
                    Box("Transformer Unit", new Vector3(x, 1.35f, z), new Vector3(2.8f, 2.7f, 3.4f), darkMat, true);
                    Box("Transformer Coils", new Vector3(x, 2.95f, z), new Vector3(2.0f, 0.65f, 2.6f), rustMat, false);
                    Box("Cooling Fin", new Vector3(x, 1.35f, z - 1.75f), new Vector3(2.2f, 2.1f, 0.15f), yellowMat, false);
                    CloneTemplate("Electrobox", new Vector3(x, 1.3f, z - 1.85f), 0f);
                }
            }

            IndustrialPropFactory.PipeBridge(root, new Vector3(-9f, 0f, 30f), new Vector3(8f, 0f, 30f), 4.0f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.PipeBridge(root, new Vector3(-9f, 0f, 38.5f), new Vector3(8f, 0f, 38.5f), 4.0f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(-5.5f, 0f, 42.5f), 0f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(4.5f, 0f, 42.5f), rustMat, yellowMat, darkMat);

            WorldLabel("09 / SUBESTACION", new Vector3(-1f, 0.05f, 27.5f), yellowMat, true);
            CreateTeslaPylon(new Vector3(-1f, 0f, 34f), "PILONO DEFENSIVO [E]");
        }

        private void BuildEastDockyardDetails()
        {
            // Extra mooring bollards, cargo staging along East Quay (leaving boarding pier z=-8 clear)
            IndustrialPropFactory.CrateStack(root, new Vector3(25.5f, 0f, 2.5f), 90f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(25.5f, 0f, -15.5f), rustMat, yellowMat, darkMat);
        }

        // =====================================================================
        // MAP 02: MASSIVE NUCLEAR SUBMARINE B-07 "LEVIATAN" INTERIOR
        // =====================================================================
        private void BuildMassiveSubmarineMap()
        {
            // Massive Submarine Hull Floor & Bilge Trenches covering all play zones
            Box("Submarine Main Hull Deck", new Vector3(2f, -0.52f, 0f), new Vector3(106f, 1f, 96f), darkMat, true);
            Box("Submarine Central Grating Walkway", new Vector3(0f, 0.02f, 0f), new Vector3(84f, 0.03f, 18f), concreteMat, false);

            // Outer Pressure Hull Bulkheads & Curved Hull Ribs
            Box("Sub Hull Wall South", new Vector3(2f, 2.2f, -46f), new Vector3(104f, 4.4f, 0.8f), darkMat, true);
            Box("Sub Hull Wall North", new Vector3(2f, 2.2f, 44f), new Vector3(104f, 4.4f, 0.8f), darkMat, true);
            Box("Sub Hull Wall West", new Vector3(-48f, 2.2f, -1f), new Vector3(0.8f, 4.4f, 90f), darkMat, true);
            Box("Sub Hull Wall East", new Vector3(52f, 2.2f, -1f), new Vector3(0.8f, 4.4f, 90f), darkMat, true);

            // Repeating pressure hull ribs along the entire submarine length
            IndustrialPropFactory.SubmarineHullRibs(root, -44f, 44f, 8f, 0f, 88f, 4.8f, darkMat, rustMat);
            IndustrialPropFactory.SubmarineHullRibs(root, -28f, 26f, 6f, 2f, 44f, 4.2f, darkMat, tealMat);

            // Bilge water channels along submarine sides
            Box("Submarine Bilge Water S", new Vector3(0f, 0.015f, -23f), new Vector3(52f, 0.02f, 3.2f), waterMat, false);
            Box("Submarine Bilge Water N", new Vector3(0f, 0.015f, 23f), new Vector3(52f, 0.02f, 3.2f), waterMat, false);

            // 1. Proa / Torpedo Room (around Arrival & West Sector)
            WorldLabel("01 / SALA DE TORPEDOS PROA", new Vector3(-23f, 0.06f, -17.5f), yellowMat, true);
            for (int t = 0; t < 4; t++)
            {
                float z = -6f + t * 3.8f;
                var tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tube.name = "Heavy Torpedo Tube " + t;
                tube.transform.SetParent(root, false);
                tube.transform.position = new Vector3(-41f, 1.15f, z);
                tube.transform.rotation = Quaternion.Euler(0, 0, 90);
                tube.transform.localScale = new Vector3(1.1f, 3.6f, 1.1f);
                tube.GetComponent<Renderer>().sharedMaterial = tealMat;

                var warhead = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                warhead.name = "Torpedo Warhead";
                IndustrialPropFactory.RemoveCollider(warhead);
                warhead.transform.SetParent(root, false);
                warhead.transform.position = new Vector3(-34f, 0.85f, z);
                warhead.transform.rotation = Quaternion.Euler(0, 0, 90);
                warhead.transform.localScale = new Vector3(0.65f, 1.8f, 0.65f);
                warhead.GetComponent<Renderer>().sharedMaterial = yellowMat;
                Box("Torpedo Cradle", new Vector3(-34f, 0.35f, z), new Vector3(2.4f, 0.5f, 0.8f), darkMat, true);
            }
            CreateBioStation(new Vector3(-45f, 0f, 0f), "ENFERMERIA NAVAL [E]");

            // 2. Magazine & Ballast Control (around Warehouse / Fuse at -22, 17)
            WorldLabel("02 / BUNKER DE OJIVAS Y REPUESTO", new Vector3(-20f, 0.06f, 12f), yellowMat, true);
            IndustrialPropFactory.PipeBridge(root, new Vector3(-26f, 0f, 10f), new Vector3(-12f, 0f, 10f), 3.6f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.CrateStack(root, new Vector3(-16f, 0f, 15f), 0f, rustMat, tealMat, darkMat);
            IndustrialPropFactory.BarrelCluster(root, new Vector3(-16f, 0f, 3.5f), rustMat, yellowMat, darkMat);

            // 3. Central Nuclear Reactor Chamber (around Generator at 5, 15 and North Sector)
            WorldLabel("03 / CAMARA DEL REACTOR NUCLEAR", new Vector3(5f, 0.06f, 11f), greenMat, true);
            var reactorCore = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            reactorCore.name = "Cherenkov Nuclear Reactor Vessel";
            reactorCore.transform.SetParent(root, false);
            reactorCore.transform.position = new Vector3(-1f, 2.4f, 28f);
            reactorCore.transform.localScale = new Vector3(5.2f, 2.6f, 5.2f);
            reactorCore.GetComponent<Renderer>().sharedMaterial = greenMat;
            Box("Reactor Upper Shield", new Vector3(-1f, 4.9f, 28f), new Vector3(6.2f, 0.6f, 6.2f), darkMat, false);
            Box("Reactor Lower Containment", new Vector3(-1f, 0.5f, 28f), new Vector3(6.4f, 1.0f, 6.4f), yellowMat, true);
            AddPointLight(new Vector3(-1f, 3.2f, 25f), new Color(.15f, .92f, 1f), 14f, 4.2f);
            CreateTeslaPylon(new Vector3(-1f, 0f, 35f), "BOBINA DEL REACTOR [E]");

            // Internal submarine bulkhead dividers in central hull (leaving navigation lanes clear!)
            Box("Sub Bulkhead Divider W", new Vector3(-13f, 1.8f, -2f), new Vector3(4.5f, 3.6f, 1.2f), darkMat, true);
            Box("Sub Bulkhead Divider M", new Vector3(-2f, 1.8f, -2f), new Vector3(5.0f, 3.6f, 1.2f), tealMat, true);
            Box("Sub Bulkhead Divider E", new Vector3(10f, 1.8f, -2f), new Vector3(4.5f, 3.6f, 1.2f), darkMat, true);
            IndustrialPropFactory.PipeBridge(root, new Vector3(-18f, 0f, -2f), new Vector3(16f, 0f, -2f), 3.7f, darkMat, rustMat, yellowMat);
            IndustrialPropFactory.PipeBridge(root, new Vector3(-16f, 0f, 6f), new Vector3(14f, 0f, 6f), 3.7f, darkMat, rustMat, yellowMat);

            // 4. Command Bridge / CIC & Periscope (around Control Room at 22, 18)
            WorldLabel("04 / PUENTE DE MANDO CIC", new Vector3(18.5f, 0.06f, 14f), greenMat, true);
            var periscope = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            periscope.name = "Attack Periscope Column";
            periscope.transform.SetParent(root, false);
            periscope.transform.position = new Vector3(18.5f, 2.5f, 16f);
            periscope.transform.localScale = new Vector3(0.55f, 2.5f, 0.55f);
            periscope.GetComponent<Renderer>().sharedMaterial = tealMat;
            Box("Holo Sonar Plotter", new Vector3(15f, 0.65f, 15.5f), new Vector3(2.2f, 1.1f, 2.2f), darkMat, true);
            Box("Holo Sonar Screen", new Vector3(15f, 1.25f, 15.5f), new Vector3(1.8f, 0.06f, 1.8f), greenMat, false);

            // 5. South Moonpool & Mini-Sub Hangar
            Box("Moonpool Water Bay", new Vector3(4f, 0.02f, -34f), new Vector3(14f, 0.04f, 10f), waterMat, false);
            Box("Moonpool Warning Curb N", new Vector3(4f, 0.12f, -28.8f), new Vector3(14.4f, 0.22f, 0.35f), yellowMat, false);
            Box("Moonpool Warning Curb S", new Vector3(4f, 0.12f, -39.2f), new Vector3(14.4f, 0.22f, 0.35f), yellowMat, false);
            var miniSub = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            miniSub.name = "Docked Recon Mini-Sub";
            miniSub.transform.SetParent(root, false);
            miniSub.transform.position = new Vector3(4f, 1.3f, -34f);
            miniSub.transform.rotation = Quaternion.Euler(0, 0, 90);
            miniSub.transform.localScale = new Vector3(2.4f, 4.2f, 2.4f);
            miniSub.GetComponent<Renderer>().sharedMaterial = yellowMat;
            WorldLabel("05 / BAHIA MOONPOOL Y ARMERIA", new Vector3(15f, 0.06f, -27f), yellowMat, true);
            CreateSupplyDropPod(new Vector3(16f, 0f, -34f));
            CreateArmoryStation(new Vector3(-11f, 0f, -36f), "TALLER DE TORPEDOS [E]");

            // 6. Aft Engine Turbines & Escape Pod (around Extraction at 39, -8)
            for (int side = -1; side <= 1; side += 2)
            {
                var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                shaft.name = "Main Propulsion Drive Shaft";
                shaft.transform.SetParent(root, false);
                shaft.transform.position = new Vector3(33f, 1.2f, -8f + side * 4.2f);
                shaft.transform.rotation = Quaternion.Euler(0, 0, 90);
                shaft.transform.localScale = new Vector3(1.6f, 6.5f, 1.6f);
                shaft.GetComponent<Renderer>().sharedMaterial = rustMat;
            }
            WorldLabel("06 / CAPSULA DE ESCAPE B-07", new Vector3(33f, 0.06f, -8f), greenMat, true);

            // Submarine Red Emergency Klaxon Lights
            foreach (Vector3 lp in new[]
            {
                new Vector3(-22f, 3.4f, -4f), new Vector3(-6f, 3.4f, 4f),
                new Vector3(12f, 3.4f, -4f), new Vector3(24f, 3.4f, 2f),
                new Vector3(-8f, 3.4f, -28f), new Vector3(16f, 3.4f, -28f)
            })
            {
                AddPointLight(lp, new Color(1f, .25f, .14f), 12f, 2.8f);
            }
        }

        // =====================================================================
        // MAP 03: ABYSSAL UNDERWATER CAVE WITH ABUNDANT WATER ELEMENTS
        // =====================================================================
        private void BuildAbyssalWaterCaveMap()
        {
            // Walkable Cavern Floor + Shallow Bioluminescent Water Sheet across low basins
            Box("Cavern Basalt Floor", new Vector3(2f, -0.52f, 0f), new Vector3(110f, 1f, 98f), darkMat, true);

            // Abundant shallow wading lagoons & glowing water channels (walkable!)
            CreateWaterLagoon(new Vector3(-10f, 0.016f, -8f), new Vector3(18f, 0.02f, 12f));
            CreateWaterLagoon(new Vector3(8f, 0.016f, 2f), new Vector3(16f, 0.02f, 14f));
            CreateWaterLagoon(new Vector3(4f, 0.018f, -32f), new Vector3(36f, 0.025f, 22f));
            CreateWaterLagoon(new Vector3(-38f, 0.018f, 0f), new Vector3(18f, 0.025f, 18f));
            CreateWaterLagoon(new Vector3(0f, 0.018f, 33f), new Vector3(22f, 0.025f, 16f));

            // Raised Stone Causeways & Steel Pontoon Walkways over the water
            Box("Pontoon Causeway West", new Vector3(-23f, 0.03f, 0f), new Vector3(4.2f, 0.04f, 34f), concreteMat, false);
            Box("Pontoon Causeway North", new Vector3(0f, 0.03f, 14f), new Vector3(44f, 0.04f, 4.2f), concreteMat, false);
            Box("Pontoon Causeway East", new Vector3(24f, 0.03f, 2f), new Vector3(4.2f, 0.04f, 28f), concreteMat, false);
            Box("Pontoon Bridge South", new Vector3(12f, 0.035f, -27f), new Vector3(4.5f, 0.05f, 16f), yellowMat, false);

            // Outer Cavern Rock Walls & Stalagmite Perimeter
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                Vector3 rim = new Vector3(Mathf.Cos(angle) * 46f, 2.2f, Mathf.Sin(angle) * 42f);
                // Keep East Bathyscaphe dock (x > 30, z ~ -8) open
                if (rim.x > 26f && Mathf.Abs(rim.z + 8f) < 9f) continue;
                IndustrialPropFactory.CaveRockFormation(root, rim, new Vector3(6.5f, 5.5f, 6.5f), darkMat, greenMat, false, i % 2 == 0);
            }

            // Interior Cavern Rock Pillars & Glowing Bioluminescent Crystal Clusters (keeping test routes clear!)
            Vector3[] crystalNodes =
            {
                new Vector3(-16f, 1.4f, -3f),
                new Vector3(-6f, 1.6f, 5f),
                new Vector3(3f, 1.5f, -3f),
                new Vector3(12f, 1.7f, 4f),
                new Vector3(-12f, 1.5f, -26f),
                new Vector3(2f, 1.8f, -36f),
                new Vector3(-34f, 1.6f, -6f),
                new Vector3(-34f, 1.6f, 6f),
                new Vector3(-8f, 1.6f, 34f),
                new Vector3(8f, 1.6f, 34f)
            };
            foreach (var node in crystalNodes)
            {
                IndustrialPropFactory.CaveRockFormation(root, node, new Vector3(2.4f, 3.2f, 2.4f), darkMat, greenMat, true, true);
                AddPointLight(node + Vector3.up * 2.2f, new Color(.18f, .95f, .88f), 11f, 2.6f);
            }

            // 6 Cascading Subterranean Waterfalls around the Cavern!
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(-27f, 8f, -2f), 8f, 3.2f, waterMat, whiteMat);
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(-10f, 8f, 21f), 8f, 3.6f, waterMat, whiteMat);
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(11f, 8f, 21f), 8f, 3.4f, waterMat, whiteMat);
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(26f, 8f, -18f), 8f, 3.5f, waterMat, whiteMat);
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(-4f, 9f, -43f), 9f, 4.5f, waterMat, whiteMat);
            IndustrialPropFactory.CaveWaterfall(root, new Vector3(-46f, 8f, 6f), 8f, 3.8f, waterMat, whiteMat);

            // Half-Sunken Shipwreck / Submarine Wreckage in the South Lagoon
            var wreck = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            wreck.name = "Sunken Abyssal Hull Wreck";
            wreck.transform.SetParent(root, false);
            wreck.transform.position = new Vector3(-4f, 0.6f, -33f);
            wreck.transform.rotation = Quaternion.Euler(18f, 32f, 78f);
            wreck.transform.localScale = new Vector3(3.6f, 7.5f, 3.6f);
            wreck.GetComponent<Renderer>().sharedMaterial = rustMat;

            // Zone Labels & Interactive Stations
            WorldLabel("01 / CAMPAMENTO CAMPANA DE BUCEO", new Vector3(-23f, 0.06f, -17.5f), whiteMat, true);
            WorldLabel("02 / GRUTA DE CRISTALES (CELULA)", new Vector3(-20f, 0.06f, 12f), greenMat, true);
            WorldLabel("03 / TURBINA HIDROTERMAL", new Vector3(5f, 0.06f, 11f), yellowMat, true);
            WorldLabel("04 / BIODOMO SUMERGIDO", new Vector3(18.5f, 0.06f, 14f), greenMat, true);
            WorldLabel("05 / LAGUNA DE LOS PECIOS", new Vector3(15f, 0.06f, -26.5f), whiteMat, true);
            WorldLabel("06 / ESCLUSA DEL BATISCAFO", new Vector3(33f, 0.06f, -8f), yellowMat, true);

            CreateSupplyDropPod(new Vector3(15f, 0f, -34f));
            CreateArmoryStation(new Vector3(-11f, 0f, -37f), "DEPOSITO DE ARPONES [E]");
            CreateBioStation(new Vector3(-44f, 0f, 0f), "PURIFICADOR HIDRO-BIO [E]");
            CreateTeslaPylon(new Vector3(-1f, 0f, 34f), "GEISER BIO-ELECTRICO [E]");
        }

        private void CreateWaterLagoon(Vector3 center, Vector3 size)
        {
            var pool = Box("Cavern Water Lagoon", center, size, waterMat, false);
            // Glowing shoreline foam & animated ripple ring
            var ripple = Box("Water Ripple Ring", center + Vector3.up * 0.008f, new Vector3(size.x * 0.65f, 0.02f, 3.2f), greenMat, false);
            animatedWaterRipples.Add(ripple.transform);
        }

        private void CreateSupplyDropPod(Vector3 pos)
        {
            supplyDropPod = new GameObject("Horde Supply Drop Pod").transform;
            supplyDropPod.SetParent(root, false);
            supplyDropPod.position = pos;
            Box("Supply Crate Base", pos + Vector3.up * 0.45f, new Vector3(1.5f, 0.9f, 1.2f), tealMat, true, supplyDropPod);
            Box("Supply Crate Lid", pos + Vector3.up * 0.95f, new Vector3(1.6f, 0.14f, 1.3f), yellowMat, false, supplyDropPod);
            Box("Supply Signal Flare", pos + Vector3.up * 1.15f, new Vector3(0.22f, 0.28f, 0.22f), greenMat, false, supplyDropPod);
            supplyDropBeacon = AddPointLight(pos + Vector3.up * 1.8f, new Color(.2f, 1f, .55f), 8f, 3.2f);
            WorldLabel("SUMINISTROS DE HORDA [E]", pos + Vector3.up * 1.85f, greenMat, false);
        }

        private void CreateArmoryStation(Vector3 pos, string title)
        {
            southArmoryStation = new GameObject("South Tactical Armory").transform;
            southArmoryStation.SetParent(root, false);
            southArmoryStation.position = pos;
            Box("Armory Locker", pos + Vector3.up * 1.1f, new Vector3(2.2f, 2.2f, 0.9f), darkMat, true, southArmoryStation);
            Box("Armory Trim", pos + new Vector3(0f, 1.2f, 0.48f), new Vector3(1.8f, 0.35f, 0.06f), yellowMat, false, southArmoryStation);
            CloneTemplate("Shelf", pos + new Vector3(-3.0f, 1.1f, 0f), 0f);
            AddPointLight(pos + new Vector3(0f, 2.8f, 1.5f), new Color(1f, .78f, .42f), 8.5f, 2.8f);
            WorldLabel(title, pos + Vector3.up * 2.55f, yellowMat, false);
        }

        private void CreateBioStation(Vector3 pos, string title)
        {
            westBioStation = new GameObject("West Bio Synthesis Station").transform;
            westBioStation.SetParent(root, false);
            westBioStation.position = pos;
            Box("Bio Lab Bench", pos + Vector3.up * 0.55f, new Vector3(1.4f, 1.1f, 3.4f), darkMat, true, westBioStation);
            Box("Bio Display", pos + new Vector3(0.75f, 1.35f, 0f), new Vector3(0.08f, 0.55f, 1.6f), greenMat, false, westBioStation);
            CloneTemplate("Computer", pos + Vector3.up * 1.1f, 90f);
            AddPointLight(pos + new Vector3(2.5f, 2.9f, 0f), new Color(.35f, .95f, .88f), 9f, 3.0f);
            WorldLabel(title, pos + Vector3.up * 2.35f, greenMat, false);
        }

        private void CreateTeslaPylon(Vector3 pos, string title)
        {
            northTurretStation = new GameObject("North Tesla Defense Pylon").transform;
            northTurretStation.SetParent(root, false);
            northTurretStation.position = pos;
            Box("Pylon Base", pos + Vector3.up * 0.6f, new Vector3(1.8f, 1.2f, 1.8f), darkMat, true, northTurretStation);
            Box("Pylon Spire", pos + Vector3.up * 2.1f, new Vector3(0.55f, 2.2f, 0.55f), tealMat, true, northTurretStation);
            Box("Pylon Emitter Ring", pos + Vector3.up * 3.1f, new Vector3(1.2f, 0.25f, 1.2f), greenMat, false, northTurretStation);
            turretLight = AddPointLight(pos + Vector3.up * 3.5f, new Color(.95f, .55f, .15f), 10f, 3.2f);
            WorldLabel(title, pos + Vector3.up * 3.7f, greenMat, false);
        }

        private void EnsureCached()
        {
            if (basePortEnv == null)
                basePortEnv = GameObject.Find("Port_Environment_3D") ?? GameObject.Find("Port_Environment");

            if (basePortChildren.Count == 0)
            {
                if (basePortEnv != null)
                {
                    for (int i = 0; i < basePortEnv.transform.childCount; i++)
                    {
                        var child = basePortEnv.transform.GetChild(i).gameObject;
                        if (child.name == MapRootName || child.name == "__Disposed_Map_Root") continue;
                        basePortChildren.Add((child, child.activeSelf));
                    }
                }

                // Also catch stray port-only static structures saved at scene root (e.g. BulkHead(Clone), Industrial / BulkHead)
                var scene = SceneManager.GetActiveScene();
                if (scene.IsValid())
                {
                    foreach (var rootGo in scene.GetRootGameObjects())
                    {
                        if (rootGo == null || rootGo == basePortEnv) continue;
                        string n = rootGo.name;
                        if (n.StartsWith("BulkHead") || n.StartsWith("Industrial /"))
                            basePortChildren.Add((rootGo, rootGo.activeSelf));
                    }
                }
            }

            if (perimeterWalls.Count == 0)
            {
                foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
                {
                    if (t == null) continue;
                    string n = t.name;
                    if (n == "South perimeter rebuilt" || n == "South perimeter" ||
                        n == "West perimeter rebuilt" || n == "West perimeter" ||
                        n == "North perimeter rebuilt" || n == "North perimeter")
                    {
                        perimeterWalls.Add(t.gameObject);
                    }
                }
            }

            if (concreteMat == null)
                CacheSceneMaterialsAndTemplates();
        }

        private void SetBasePortActive(bool active)
        {
            bool anyInitiallyActive = false;
            for (int i = 0; i < basePortChildren.Count; i++)
                if (basePortChildren[i].initialActive) { anyInitiallyActive = true; break; }

            if (basePortEnv != null)
            {
                foreach (var c in basePortEnv.GetComponents<Collider>())
                    if (c != null) c.enabled = active;
            }

            for (int i = 0; i < basePortChildren.Count; i++)
            {
                var (go, initialActive) = basePortChildren[i];
                if (go == null) continue;
                bool shouldBeActive = active && (anyInitiallyActive ? initialActive : true);
                if (!shouldBeActive)
                {
                    foreach (var c in go.GetComponentsInChildren<Collider>(true))
                        if (c != null) c.enabled = false;
                }
                go.SetActive(shouldBeActive);
                if (shouldBeActive)
                {
                    foreach (var c in go.GetComponentsInChildren<Collider>(true))
                        if (c != null) c.enabled = true;
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
            waterMat = FindMat("Water") ?? tealMat;

            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t == null || t.name == MapRootName) continue;
                if (basePortEnv != null && !t.IsChildOf(basePortEnv.transform)) continue;
                if (t.name == "Cargo container" && !propTemplates.ContainsKey("Container")) propTemplates["Container"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Shelf") && !propTemplates.ContainsKey("Shelf")) propTemplates["Shelf"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Electroboxes") && !propTemplates.ContainsKey("Electrobox")) propTemplates["Electrobox"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Oxy_agregate") && !propTemplates.ContainsKey("Oxy")) propTemplates["Oxy"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Old_computer") && !propTemplates.ContainsKey("Computer")) propTemplates["Computer"] = t.gameObject;
                else if (t.name.StartsWith("Industrial / Cart") && !propTemplates.ContainsKey("Cart")) propTemplates["Cart"] = t.gameObject;
            }

            var water = FindObjectByNameIncludeInactive("Water");
            if (water != null) water.transform.localScale = new Vector3(220f, 0.15f, 200f);
        }

        private GameObject FindObjectByNameIncludeInactive(string objectName)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t != null && t.name == objectName) return t.gameObject;
            }
            return null;
        }

        private Material FindMat(string objectName)
        {
            var go = GameObject.Find(objectName) ?? FindObjectByNameIncludeInactive(objectName);
            var r = go != null ? go.GetComponentInChildren<Renderer>(true) : null;
            return r != null ? r.sharedMaterial : null;
        }

        private void OpenPerimeterPassages()
        {
            bool openedSouth = false, openedWest = false, openedNorth = false;
            foreach (var wall in perimeterWalls)
            {
                if (wall == null) continue;
                wall.SetActive(false);
                if (wall.name.StartsWith("South")) openedSouth = true;
                else if (wall.name.StartsWith("West")) openedWest = true;
                else if (wall.name.StartsWith("North")) openedNorth = true;
            }

            if (openedSouth)
            {
                Rail("South rail W", new Vector3(-14.5f, 0.45f, -20f), new Vector3(7f, 0.9f, 0.3f));
                Rail("South rail Mid", new Vector3(2f, 0.45f, -20f), new Vector3(12f, 0.9f, 0.3f));
                Rail("South rail E", new Vector3(21.5f, 0.45f, -20f), new Vector3(13f, 0.9f, 0.3f));
            }
            if (openedWest)
            {
                Rail("West rail S", new Vector3(-28f, 0.45f, -5.5f), new Vector3(0.3f, 0.9f, 5f));
                Rail("West rail N", new Vector3(-28f, 0.45f, 5.5f), new Vector3(0.3f, 0.9f, 5f));
            }
            if (openedNorth)
            {
                Rail("North rail W", new Vector3(-8.5f, 0.45f, 24f), new Vector3(9f, 0.9f, 0.3f));
                Rail("North rail E", new Vector3(6.5f, 0.45f, 24f), new Vector3(9f, 0.9f, 0.3f));
            }
        }

        private void SpawnContainer(Vector3 pos, float yaw, Material mat)
        {
            if (propTemplates.TryGetValue("Container", out var tpl) && tpl != null)
            {
                var inst = Instantiate(tpl, root);
                inst.SetActive(true);
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
            clone.SetActive(true);
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
            if (!solid) IndustrialPropFactory.RemoveCollider(go);
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
            session.SetMessage("Mapa / Sector activo: " + sectorLabel + ".");
        }

        private static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
