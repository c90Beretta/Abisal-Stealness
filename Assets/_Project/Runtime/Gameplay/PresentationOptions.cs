using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Zombtoy.Gameplay
{
    public sealed class PresentationOptions : MonoBehaviour
    {
        public Volume volume;
        public bool Effects { get; private set; } = true;
        public static bool ReducedEffects { get; private set; }
        private SandboxSession session;
        private void Awake() { session = GetComponent<SandboxSession>(); Apply(); }
        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
            { Effects = !Effects; Apply(); session.SetMessage(Effects ? "Efectos de pantalla activados." : "Lectura clara: retro, grano, bloom y sacudida desactivados."); }
        }
        public void SetEffects(bool value) { Effects=value;Apply(); }
        private void Apply()
        {
            ReducedEffects = !Effects;Shader.SetGlobalFloat("_AbisalRetroStrength",Effects?1:0);
            if (volume == null) return;
            if (volume.profile.TryGet(out FilmGrain grain)) grain.active = Effects;
            if (volume.profile.TryGet(out Bloom bloom)) bloom.active = Effects;
        }
        private void OnDestroy() { ReducedEffects = false;Shader.SetGlobalFloat("_AbisalRetroStrength",0); }
    }
}
