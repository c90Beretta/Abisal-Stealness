using UnityEngine;

namespace Zombtoy.Gameplay
{
    [DefaultExecutionOrder(-20)]
    public sealed class NightSurvival : MonoBehaviour
    {
        public SandboxSession session;
        public Light flashlight, personalLight;
        public Light[] poweredLights;
        public FieldInventory Inventory = new FieldInventory();
        public HordeDirector hordes;
        public AudioClip breathing;
        private AudioSource breath;
        private Renderer vest, mask;
        private Renderer[] lenses;
        public LootContainer[] Containers { get; private set; }
        public float NoiseRadius { get; private set; }
        public bool LampLit => Inventory.lampOn && Inventory.battery > 0;
        public bool InGas => Vector3.Distance(session.player.transform.position, new Vector3(7, 0, 18)) < 5;
        private LineRenderer ring, pulse;
        private float noiseUntil, nextGasDamage;

        private void Awake()
        {
            session = GetComponent<SandboxSession>();
            foreach (var r in session.player.GetComponentsInChildren<Renderer>()) { if (r.name == "Armoured vest") vest = r; if (r.name == "Respirator mask") mask = r; }
            var foundLenses = new System.Collections.Generic.List<Renderer>(); foreach (var r in session.player.GetComponentsInChildren<Renderer>()) if (r.name == "Mask lens") foundLenses.Add(r); lenses = foundLenses.ToArray();
            Containers = FindObjectsByType<LootContainer>(FindObjectsInactive.Include);
            ring = Ring("Circulo de percepcion", new Color(.25f, .7f, .65f, .28f), .018f);
            pulse = Ring("Pulso de sonido", new Color(.9f, .56f, .28f, .55f), .025f);
        }
        private LineRenderer Ring(string label, Color color, float width)
        {
            var line = new GameObject(label).AddComponent<LineRenderer>(); line.transform.SetParent(transform);
            line.sharedMaterial = session.tracerMaterial; line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = width; line.positionCount = 65; line.loop = false;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; return line;
        }
        private void Start()
        {
            session.player.view.gameObject.AddComponent<CameraTrauma>();
            breath = session.player.gameObject.AddComponent<AudioSource>(); breath.clip = breathing; breath.loop = true; breath.volume = .09f; breath.Play();
        }
        private void Update()
        {
            if (vest != null) vest.enabled = Inventory.armorOn;
            if (mask != null) mask.enabled = Inventory.maskOn;
            foreach (var lens in lenses) lens.enabled = Inventory.maskOn;
            if (flashlight != null) flashlight.enabled = LampLit;
            if (poweredLights != null) foreach (var lamp in poweredLights) if (lamp != null) lamp.enabled = session.portMission.PowerOn;
            if (breath != null) breath.mute = !session.Running || !Inventory.maskOn || Inventory.air <= 0;
            if (!session.Running) return;
            var bag = Inventory;
            if (LampLit) bag.battery = Mathf.Max(0, bag.battery - Time.deltaTime * .065f);
            if (bag.maskOn && bag.air > 0) bag.air = Mathf.Max(0, bag.air - Time.deltaTime * (InGas ? .4f : .025f));
            bag.stamina = Mathf.Min(100, bag.stamina + Time.deltaTime * 13);
            if (InGas && (!bag.maskOn || bag.air <= 0) && Time.time >= nextGasDamage)
            { nextGasDamage = Time.time + 2; session.TakeDamage(6, false); session.SetMessage("Fuga toxica: activa la mascara o abandona el generador."); }
            if (Time.time >= noiseUntil) NoiseRadius = Mathf.MoveTowards(NoiseRadius, 0, Time.deltaTime * 7);
        }
        private void LateUpdate()
        {
            ring.enabled = pulse.enabled = session.Mode == SessionMode.Playing;
            Draw(ring, 2.7f); Draw(pulse, Mathf.Max(2.7f, Mathf.Min(NoiseRadius, 7)));
            pulse.enabled &= NoiseRadius > 3;
        }
        private void Draw(LineRenderer line, float radius)
        {
            Vector3 center = session.player.transform.position + Vector3.up * .08f;
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI * 2 / 64; Vector3 end = center + new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;
                Vector3 eye = center + Vector3.up * .5f;
                if (Physics.Linecast(eye, end + Vector3.up * .5f, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) && hit.collider.GetComponentInParent<SandboxPlayer>() == null && hit.collider.GetComponentInParent<SandboxEnemy>() == null)
                    end = center + (end - center).normalized * Mathf.Max(.2f, hit.distance - .1f);
                line.SetPosition(i, end);
            }
        }
        public void Noise(Vector3 position, float radius)
        {
            if (Vector3.Distance(position, session.player.transform.position) < 1.5f) { NoiseRadius = Mathf.Max(radius, NoiseRadius); noiseUntil = Time.time + .6f; }
            if (hordes != null) hordes.Noise(position, radius);
        }
        public bool Perceives(Vector3 point)
        {
            Vector3 from = session.player.transform.position + Vector3.up * 1.1f;
            Vector3 delta = point - from; delta.y = 0;
            if (delta.magnitude > 2.7f && (!LampLit || delta.magnitude > 15 || Vector3.Angle(session.player.transform.forward, delta) > 38)) return false;
            return !Physics.Linecast(from, point, out var hit, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<SandboxEnemy>() != null;
        }
        public bool SpendStamina(float amount) { if (Inventory.stamina < amount) return false; Inventory.stamina -= amount; return true; }
        public void ToggleLamp() { Inventory.lampOn = !Inventory.lampOn; session.SetMessage(Inventory.battery <= 0 ? "Linterna sin carga. Cambia la pila en TAB." : Inventory.lampOn ? "Linterna encendida." : "Linterna apagada."); }
        public LootContainer NearestLoot()
        {
            LootContainer best = null; float distance = 2.5f;
            foreach (var box in Containers)
                if (box != null && box.Reachable(session) && !box.stock.Empty)
                { float d = Vector3.Distance(box.transform.position, session.player.transform.position); if (d < distance) { best = box; distance = d; } }
            return best;
        }
        public bool UseSupply(int kind)
        {
            if (!session.Running || session.Busy || session.State.InventoryLocked) return false;
            if (kind == 0 && Inventory.cells > 0 && Inventory.battery <= 70) { Inventory.cells--; Inventory.battery = 100; }
            else if (kind == 1 && Inventory.oxygen > 0 && Inventory.air <= 70) { Inventory.oxygen--; Inventory.air = 100; }
            else if (kind == 2 && Inventory.plates > 0 && Inventory.armor <= 70) { Inventory.plates--; Inventory.armor = 100; }
            else { session.SetMessage("Requiere un repuesto y una carga inferior al 70%."); return false; }
            session.EmitNoise(session.player.transform.position, 3);
            session.SetMessage(kind == 0 ? "Pila cambiada: linterna al 100%." : kind == 1 ? "Oxigeno recargado al 100%." : "Placa cambiada: proteccion al 100%."); return true;
        }
        public float Absorb(float damage)
        {
            if (!Inventory.armorOn || Inventory.armor <= 0) return damage;
            float absorbed = Mathf.Min(Inventory.armor, damage * .35f); Inventory.armor -= absorbed; return damage - absorbed;
        }
        private void OnDestroy() { if (ring != null) Destroy(ring.gameObject); if (pulse != null) Destroy(pulse.gameObject); }
    }
}
