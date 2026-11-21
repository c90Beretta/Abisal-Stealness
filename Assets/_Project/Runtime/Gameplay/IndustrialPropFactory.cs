using System.Collections.Generic;
using UnityEngine;

namespace Zombtoy.Gameplay
{
    public static class IndustrialPropFactory
    {
        private static readonly Dictionary<string, GameObject> editorPrefabs = new Dictionary<string, GameObject>();

        public static GameObject SpawnEditorOrFallbackPrefab(string assetPath, Vector3 position, Vector3 targetSize, float yaw, Transform parent, Material fallbackMat, bool solid = true)
        {
#if UNITY_EDITOR
            if (!editorPrefabs.TryGetValue(assetPath, out var source) || source == null)
            {
                source = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                editorPrefabs[assetPath] = source;
            }
            if (source != null)
            {
                var wrapper = new GameObject("Prop / " + source.name);
                wrapper.transform.position = Vector3.zero;
                wrapper.transform.rotation = Quaternion.identity;
                wrapper.transform.localScale = Vector3.one;

                var model = Object.Instantiate(source, wrapper.transform, false);
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
                foreach (var c in model.GetComponentsInChildren<Collider>(true))
                {
                    c.enabled = false;
                    if (Application.isPlaying) Object.Destroy(c);
                    else Object.DestroyImmediate(c);
                }
                var renderers = model.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    foreach (var r in renderers) b.Encapsulate(r.bounds);
                    model.transform.localPosition -= b.center;
                    wrapper.transform.localScale = new Vector3(
                        targetSize.x / Mathf.Max(.05f, b.size.x),
                        targetSize.y / Mathf.Max(.05f, b.size.y),
                        targetSize.z / Mathf.Max(.05f, b.size.z));
                }
                wrapper.transform.SetParent(parent, false);
                wrapper.transform.position = position;
                wrapper.transform.rotation = Quaternion.Euler(0, yaw, 0);
                if (solid)
                {
                    var box = wrapper.AddComponent<BoxCollider>();
                    Vector3 s = wrapper.transform.localScale;
                    box.center = Vector3.zero;
                    box.size = new Vector3(
                        targetSize.x / Mathf.Max(.01f, Mathf.Abs(s.x)),
                        targetSize.y / Mathf.Max(.01f, Mathf.Abs(s.y)),
                        targetSize.z / Mathf.Max(.01f, Mathf.Abs(s.z)));
                }
                if (targetSize.y > 1.6f) wrapper.AddComponent<MapOccluder>();
                return wrapper;
            }
#endif
            var fallback = Box("Prop fallback", position, targetSize, fallbackMat, solid, parent);
            fallback.transform.rotation = Quaternion.Euler(0, yaw, 0);
            if (targetSize.y > 1.6f) fallback.AddComponent<MapOccluder>();
            return fallback;
        }

        public static void BarrelCluster(Transform parent, Vector3 center, Material rustMat, Material yellowMat, Material darkMat)
        {
            Vector3[] offsets = { new Vector3(-.42f, 0, -.35f), new Vector3(.42f, 0, -.30f), new Vector3(0, 0, .38f) };
            for (int i = 0; i < offsets.Length; i++)
            {
                Vector3 pos = center + offsets[i] + Vector3.up * .48f;
                SpawnEditorOrFallbackPrefab(
                    "Assets/SciFi Warehouse Kit/Prefabs/Props/Crates Barrels Pallets/Barrel.prefab",
                    pos, new Vector3(.62f, .96f, .62f), i * 35f, parent, i == 1 ? yellowMat : rustMat, true);
            }
            Box("Pallet base", center + Vector3.up * .06f, new Vector3(1.55f, .12f, 1.45f), darkMat, true, parent);
        }

        public static void CrateStack(Transform parent, Vector3 center, float yaw, Material rustMat, Material tealMat, Material darkMat)
        {
            var group = new GameObject("Industrial Crate Stack").transform;
            group.SetParent(parent, false);
            group.position = center;
            group.rotation = Quaternion.Euler(0, yaw, 0);

            Box("Pallet", center + Vector3.up * .06f, new Vector3(2.2f, .12f, 1.6f), darkMat, true, group);
            SpawnEditorOrFallbackPrefab(
                "Assets/SciFi Warehouse Kit/Prefabs/Props/Crates Barrels Pallets/Crate Long.prefab",
                center + group.rotation * new Vector3(-.45f, .52f, 0), new Vector3(1.1f, .80f, 1.3f), yaw, group, rustMat, true);
            SpawnEditorOrFallbackPrefab(
                "Assets/SciFi Warehouse Kit/Prefabs/Props/Crates Barrels Pallets/Crate Medium.prefab",
                center + group.rotation * new Vector3(.55f, .46f, 0), new Vector3(.85f, .70f, .95f), yaw + 15f, group, tealMat, true);
            SpawnEditorOrFallbackPrefab(
                "Assets/Data/Sci-fi_assets/Sci-Fi Old rusty props PBR/Prefabs/Supply_box.prefab",
                center + group.rotation * new Vector3(-.35f, 1.12f, 0), new Vector3(.75f, .42f, .65f), yaw, group, tealMat, false);
        }

        public static void PipeBridge(Transform parent, Vector3 a, Vector3 b, float height, Material darkMat, Material rustMat, Material yellowMat)
        {
            var group = new GameObject("Overhead Pipe Bridge").transform;
            group.SetParent(parent, false);
            float dist = Vector3.Distance(a, b);
            Vector3 mid = (a + b) * 0.5f + Vector3.up * height;
            Quaternion rot = Quaternion.LookRotation((b - a).normalized, Vector3.up);

            Box("Support Pillar A", a + Vector3.up * (height * 0.5f), new Vector3(.38f, height, .38f), darkMat, true, group);
            Box("Support Pillar B", b + Vector3.up * (height * 0.5f), new Vector3(.38f, height, .38f), darkMat, true, group);
            Box("Support Base A", a + Vector3.up * .25f, new Vector3(.65f, .5f, .65f), yellowMat, false, group);
            Box("Support Base B", b + Vector3.up * .25f, new Vector3(.65f, .5f, .65f), yellowMat, false, group);

            var truss = Box("Gantry Truss", mid, new Vector3(.55f, .35f, dist), darkMat, false, group);
            truss.transform.rotation = rot;

            for (int i = -1; i <= 1; i += 2)
            {
                var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipe.name = "Overhead Steam Pipe";
                RemoveCollider(pipe);
                pipe.transform.SetParent(group, false);
                pipe.transform.position = mid + rot * new Vector3(i * .24f, .22f, 0);
                pipe.transform.rotation = rot * Quaternion.Euler(90, 0, 0);
                pipe.transform.localScale = new Vector3(.22f, dist * 0.5f, .22f);
                if (rustMat != null) pipe.GetComponent<Renderer>().sharedMaterial = rustMat;
            }
        }

        public static void FloodlightTower(Transform parent, Vector3 pos, float yaw, Material darkMat, Material yellowMat, Color lightColor)
        {
            var group = new GameObject("Floodlight Tower").transform;
            group.SetParent(parent, false);
            group.position = pos;
            group.rotation = Quaternion.Euler(0, yaw, 0);

            Box("Tower Concrete Base", pos + Vector3.up * .4f, new Vector3(.9f, .8f, .9f), yellowMat, true, group);
            Box("Tower Mast", pos + Vector3.up * 3.2f, new Vector3(.24f, 5.6f, .24f), darkMat, true, group);
            Box("Crossbar", pos + Vector3.up * 5.8f, new Vector3(1.6f, .18f, .22f), darkMat, false, group);
            Box("Lamp Head L", pos + group.rotation * new Vector3(-.55f, 5.95f, .15f), new Vector3(.45f, .28f, .25f), yellowMat, false, group);
            Box("Lamp Head R", pos + group.rotation * new Vector3(.55f, 5.95f, .15f), new Vector3(.45f, .28f, .25f), yellowMat, false, group);

            var lightGo = new GameObject("Tower Point Light");
            lightGo.transform.SetParent(group, false);
            lightGo.transform.position = pos + Vector3.up * 5.4f + group.forward * .6f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.range = 13f;
            light.intensity = 2.8f;
        }

        public static void JerseyBarrierRow(Transform parent, Vector3 start, Vector3 end, int segments, Material concreteMat, Material yellowMat)
        {
            for (int i = 0; i < segments; i++)
            {
                Vector3 p = Vector3.Lerp(start, end, (i + 0.5f) / segments);
                Quaternion rot = Quaternion.LookRotation((end - start).normalized, Vector3.up);
                var b = Box("Jersey Barrier", p + Vector3.up * .45f, new Vector3(.55f, .9f, 2.2f), concreteMat, true, parent);
                b.transform.rotation = rot;
                var stripe = Box("Hazard Stripe", p + Vector3.up * .88f, new Vector3(.58f, .08f, 2.0f), yellowMat, false, parent);
                stripe.transform.rotation = rot;
            }
        }

        public static void SubmarineHullRibs(Transform parent, float minX, float maxX, float stepX, float centerZ, float widthZ, float heightY, Material darkMat, Material rustMat)
        {
            for (float x = minX; x <= maxX; x += stepX)
            {
                Box("Sub Hull Rib L", new Vector3(x, heightY * 0.5f, centerZ - widthZ * 0.5f + .35f), new Vector3(.45f, heightY, .7f), darkMat, true, parent);
                Box("Sub Hull Rib R", new Vector3(x, heightY * 0.5f, centerZ + widthZ * 0.5f - .35f), new Vector3(.45f, heightY, .7f), darkMat, true, parent);
                Box("Sub Overhead Arch L", new Vector3(x, heightY - .3f, centerZ - widthZ * 0.35f), new Vector3(.42f, .45f, widthZ * 0.28f), rustMat, false, parent);
                Box("Sub Overhead Arch R", new Vector3(x, heightY - .3f, centerZ + widthZ * 0.35f), new Vector3(.42f, .45f, widthZ * 0.28f), rustMat, false, parent);
            }
        }

        public static void CaveRockFormation(Transform parent, Vector3 pos, Vector3 scale, Material rockMat, Material crystalMat, bool stalactite = false, bool glowingCrystal = false)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            rock.name = stalactite ? "Cave Stalagmite Pillar" : "Abyssal Basalt Formation";
            rock.transform.SetParent(parent, false);
            rock.transform.position = pos;
            rock.transform.rotation = Quaternion.Euler(Random.Range(-8f, 8f), Random.Range(0f, 360f), Random.Range(-8f, 8f));
            rock.transform.localScale = scale;
            if (rockMat != null) rock.GetComponent<Renderer>().sharedMaterial = rockMat;
            if (scale.y > 1.8f) rock.AddComponent<MapOccluder>();

            if (glowingCrystal && crystalMat != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    shard.name = "Bioluminescent Crystal Shard";
                    RemoveCollider(shard);
                    shard.transform.SetParent(parent, false);
                    float angle = i * 120f + 25f;
                    shard.transform.position = pos + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * scale.x * .38f, .55f + i * .22f, Mathf.Sin(angle * Mathf.Deg2Rad) * scale.z * .38f);
                    shard.transform.rotation = Quaternion.Euler(22f, angle, 18f);
                    shard.transform.localScale = new Vector3(.28f, .95f + i * .2f, .28f);
                    shard.GetComponent<Renderer>().sharedMaterial = crystalMat;
                }
            }
        }

        public static void CaveWaterfall(Transform parent, Vector3 topPos, float height, float width, Material waterMat, Material whiteMat)
        {
            var group = new GameObject("Abyssal Waterfall").transform;
            group.SetParent(parent, false);
            group.position = topPos;

            // Main falling water curtain
            Box("Waterfall Curtain", topPos + Vector3.down * (height * 0.5f), new Vector3(width, height, .35f), waterMat, false, group);
            // White foam streaks
            for (int i = -1; i <= 1; i++)
                Box("Waterfall Foam Streak", topPos + new Vector3(i * width * 0.28f, -height * 0.5f, .16f), new Vector3(width * 0.14f, height * 0.96f, .08f), whiteMat, false, group);
            // Splash pool foam ring at bottom
            Vector3 bottom = new Vector3(topPos.x, 0.06f, topPos.z + .5f);
            Box("Waterfall Splash Pool", bottom, new Vector3(width * 1.6f, .05f, 2.2f), whiteMat, false, group);
            Box("Waterfall Inner Pool", bottom + Vector3.up * .01f, new Vector3(width * 1.2f, .05f, 1.5f), waterMat, false, group);

            var lightGo = new GameObject("Waterfall Bioluminescence");
            lightGo.transform.SetParent(group, false);
            lightGo.transform.position = bottom + Vector3.up * 1.4f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(.20f, .88f, .95f);
            light.range = 10f;
            light.intensity = 2.6f;
        }

        public static GameObject Box(string name, Vector3 pos, Vector3 size, Material mat, bool solid, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = size;
            if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!solid) RemoveCollider(go);
            return go;
        }

        public static void RemoveCollider(GameObject go)
        {
            foreach (var col in go.GetComponents<Collider>())
            {
                if (col == null) continue;
                col.enabled = false;
                if (Application.isPlaying) Object.Destroy(col);
                else Object.DestroyImmediate(col);
            }
        }
    }
}
