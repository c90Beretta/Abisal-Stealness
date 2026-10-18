using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zombtoy.Core;

namespace Zombtoy.Gameplay
{
    public sealed class SandboxSession : MonoBehaviour
    {
        public SurvivalConfig config;
        public SandboxPlayer player;
        public Transform terminal;
        public Transform extraction;
        public Material tracerMaterial;
        public PortMission portMission;
        public string missionTitle = "ZOMBTOY / SANDBOX DE FUNDACION";
        public SurvivalState State = new SurvivalState();
        public int rounds = 12;
        public int reserve = 36;
        public int Kills { get; set; }
        public bool Injecting => injectionLeft > 0f;
        public bool Running => !paused && !completed && State.health > 0f;
        private bool paused;
        private bool completed;
        private bool terminalActivated;
        private float injectionLeft;
        private float reloadLeft;
        private string message = "Activa la terminal y llega a EXTRACCION.";
        private SandboxEnemy[] enemies;
        private GUIStyle text;

        private void Start()
        {
            enemies = FindObjectsByType<SandboxEnemy>();
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                paused = !paused;
            if (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame &&
                (completed || State.health <= 0f))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            Time.timeScale = Running ? 1f : 0f;
            if (!Running) return;
            State.Tick(Time.deltaTime, config);
            if (injectionLeft > 0f)
            {
                injectionLeft -= Time.deltaTime;
                if (injectionLeft <= 0f && State.Inject(config)) message = "Suero administrado.";
            }
            if (reloadLeft > 0f)
            {
                reloadLeft -= Time.deltaTime;
                if (reloadLeft <= 0f)
                {
                    int count = Mathf.Min(12 - rounds, reserve);
                    rounds += count;
                    reserve -= count;
                }
            }
        }

        public bool UseRound()
        {
            if (rounds <= 0 || reloadLeft > 0f) return false;
            rounds--;
            return true;
        }

        public void Reload()
        {
            if (rounds >= 12 || reserve <= 0 || reloadLeft > 0 || Injecting) return;
            reloadLeft = 1.5f;
            EmitNoise(player.transform.position, 4f);
        }

        public void BeginInjection()
        {
            if (State.doses <= 0 || Injecting || reloadLeft > 0f) return;
            injectionLeft = config.injectionSeconds;
            EmitNoise(player.transform.position, 7f);
        }

        public void Interact()
        {
            if (!Running) return;
            if (portMission != null)
            {
                portMission.Interact();
                return;
            }
            if (Vector3.Distance(player.transform.position, terminal.position) < 2.5f)
            {
                terminalActivated = true;
                message = "Terminal activada. Busca EXTRACCION (luz verde).";
                EmitNoise(terminal.position, 12f);
            }
            else if (Vector3.Distance(player.transform.position, extraction.position) < 2.5f)
            {
                if (terminalActivated) { completed = true; message = "Ruta completada. Enter: repetir."; }
                else message = "Extraccion bloqueada: activa primero la terminal.";
            }
        }

        public void SetMessage(string value) => message = value;

        public void CompleteRoute(string value)
        {
            completed = true;
            message = value;
        }

        public void EmitNoise(Vector3 position, float radius)
        {
            if (enemies == null) return;
            foreach (var enemy in enemies)
                if (enemy != null && enemy.gameObject.activeSelf) enemy.Hear(position, radius);
        }

        private void OnGUI()
        {
            if (text == null)
                text = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true,
                    normal = { textColor = new Color(0.75f, 0.88f, 0.77f) } };
            float scale = Mathf.Max(0.65f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.Box(new Rect(18, 18, 450, 212), GUIContent.none);
            GUI.Label(new Rect(32, 28, 420, 25), missionTitle, text);
            GUI.Label(new Rect(32, 62, 420, 60), $"SALUD {State.health:0}    SUERO {State.serum:0}    FASE {State.Phase}\nDOSIS {State.doses}    MUNICION {rounds}/{reserve}    BAJAS {Kills}", text);
            GUI.Label(new Rect(32, 120, 420, 85), paused ? "PAUSA / Esc para volver" :
                State.health <= 0 ? "HAS MUERTO. Enter: reiniciar." :
                Injecting ? "Inyectando..." : reloadLeft > 0 ? "Recargando..." :
                State.IsManic ? "MANIA / ruido y temblor aumentados" : message, text);
            GUI.Label(new Rect(22, Screen.height / scale - 70, 950, 60),
                "WASD mover · Shift correr · raton apuntar · clic disparar · R recargar\nQ suero · E terminal/extraccion · Esc pausa | Prototipo: faltan sistemas del MVP", text);
            if (portMission != null && Running)
            {
                GUI.Box(new Rect(18, 235, 450, 85), GUIContent.none);
                GUI.Label(new Rect(32, 235, 440, 70), portMission.Objective + "\n" + portMission.Prompt, text);
            }
        }

        private void OnDestroy() => Time.timeScale = 1f;
    }
}
