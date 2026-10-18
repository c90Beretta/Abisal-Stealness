using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class PortMission : MonoBehaviour
    {
        public SandboxSession session;
        public Transform arrival;
        public Transform fuse;
        public Transform generator;
        public Transform supplies;
        public GameObject controlDoor;
        public GameObject boardingGate;
        public bool HasFuse { get; private set; }
        public bool PowerOn { get; private set; }
        public bool BoardingAuthorized { get; private set; }
        public bool SuppliesTaken { get; private set; }
        public string Prompt { get; private set; }
        public string Objective => BoardingAuthorized ? "04 / Embarca en el submarino por el muelle este." :
            PowerOn ? "03 / Autoriza el embarque en CONTROL." :
            HasFuse ? "02 / Instala el repuesto en el GENERADOR." :
            "01 / Busca el repuesto en ALMACEN 03.";

        private void Start() => session.SetMessage("Puerto sin energia. Busca el repuesto en el almacen.");

        private void Update()
        {
            if (!session.Running) return;
            Transform nearby = NearestReachable();
            Prompt = nearby == null ? string.Empty : "[E] " + Action(nearby);
            if (session.player.transform.position.y < -3f)
            {
                var controller = session.player.GetComponent<CharacterController>();
                controller.enabled = false;
                session.player.transform.position = arrival.position;
                controller.enabled = true;
                session.State.health = Mathf.Max(0, session.State.health - 10);
                session.SetMessage("Caida al agua. Regresas al desembarco (-10 salud).");
            }
        }

        public void Interact()
        {
            Transform target = NearestReachable();
            if (target == null) return;
            if (target == fuse)
            {
                HasFuse = true;
                fuse.gameObject.SetActive(false);
                session.SetMessage("Repuesto recogido. Llevalo al generador del patio norte.");
            }
            else if (target == supplies)
            {
                SuppliesTaken = true;
                session.reserve += 12;
                session.State.doses++;
                supplies.gameObject.SetActive(false);
                session.SetMessage("Suministros: +12 cartuchos y +1 dosis de suero.");
            }
            else if (target == generator)
            {
                if (!HasFuse) session.SetMessage("Falta un repuesto. Busca en ALMACEN 03.");
                else if (!PowerOn)
                {
                    PowerOn = true;
                    controlDoor.SetActive(false);
                    session.EmitNoise(generator.position, 20);
                    session.SetMessage("Energia restablecida. La sala de control esta abierta.");
                }
            }
            else if (target == session.terminal)
            {
                if (!PowerOn) session.SetMessage("Terminal sin energia. Repara primero el generador.");
                else if (!BoardingAuthorized)
                {
                    BoardingAuthorized = true;
                    boardingGate.SetActive(false);
                    session.EmitNoise(session.terminal.position, 14);
                    session.SetMessage("Embarque autorizado. Ve al muelle este, junto al submarino.");
                }
            }
            else if (target == session.extraction)
            {
                if (!BoardingAuthorized) session.SetMessage("Embarque bloqueado. Requiere autorizacion de CONTROL.");
                else session.CompleteRoute("Puerto completado. A bordo del submarino. Enter: repetir.");
            }
        }

        private Transform NearestReachable()
        {
            Transform result = null;
            float best = 2.3f;
            foreach (var point in new[] { fuse, supplies, generator, session.terminal, session.extraction })
            {
                if (point == null || !point.gameObject.activeInHierarchy) continue;
                if ((point == supplies && SuppliesTaken) || (point == fuse && HasFuse)) continue;
                Vector3 delta = point.position - session.player.transform.position;
                float distance = new Vector2(delta.x, delta.z).magnitude;
                if (distance >= best) continue;
                Vector3 origin = session.player.transform.position + Vector3.up * 0.9f;
                Vector3 end = point.position + Vector3.up * 0.25f;
                if (Physics.Linecast(origin, end, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore) &&
                    !hit.transform.IsChildOf(point) && hit.transform != point) continue;
                best = distance;
                result = point;
            }
            return result;
        }

        private string Action(Transform point)
        {
            if (point == fuse) return "Recoger repuesto";
            if (point == supplies) return "Recoger suministros";
            if (point == generator) return PowerOn ? "Generador operativo" : "Reparar generador";
            if (point == session.terminal) return "Autorizar embarque";
            return "Embarcar";
        }
    }
}
