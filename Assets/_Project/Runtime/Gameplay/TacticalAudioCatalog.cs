using UnityEngine;

namespace Zombtoy.Gameplay
{
    public sealed class TacticalAudioCatalog : ScriptableObject
    {
        public const string ResourcePath = "TacticalAudioCatalog";
        public const int CurrentVersion = 1;

        public const string AirVentPath = "Assets/_Project/Audio/Music/Ambience/ambience_air_vent.mp3";
        public const string AggressiveGrowlPath = "Assets/_Project/Audio/Music/Ambience/ambience_aggressive_growl.mp3";
        public const string EnemyAlertMonsterPath = "Assets/_Project/Audio/SFX/Enemy/enemy_alert_monster.mp3";
        public const string EnemyAttackSwipePath = "Assets/_Project/Audio/SFX/Enemy/enemy_attack_swipe.mp3";
        public const string EnemyDeathGurglePath = "Assets/_Project/Audio/SFX/Enemy/enemy_death_gurgle.mp3";
        public const string EnemyHitImpactFleshPath = "Assets/_Project/Audio/SFX/Enemy/enemy_hit_impact_flesh.mp3";
        public const string PlayerAttackWhooshPath = "Assets/_Project/Audio/SFX/Player/player_attack_whoosh.mp3";
        public const string PlayerDeathBodyFallPath = "Assets/_Project/Audio/SFX/Player/player_death_body_fall.mp3";
        public const string PlayerInteractPath = "Assets/_Project/Audio/SFX/Player/player_interact.wav";
        public const string PlayerTakeDamagePath = "Assets/_Project/Audio/SFX/Player/player_take_damage.mp3";
        public const string FootstepsConcretePath = "Assets/_Project/Audio/SFX/Steps/footsteps_concrete.mp3";
        public const string FootstepsRunningPath = "Assets/_Project/Audio/SFX/Steps/footsteps_running.mp3";
        public const string FootstepsLimpingPath = "Assets/_Project/Audio/SFX/Steps/footsteps_limping.mp3";
        public const string UiMenuClickPath = "Assets/_Project/Audio/SFX/UI/ui_menu_click.mp3";
        public const string UiPauseSwitchPath = "Assets/_Project/Audio/SFX/UI/ui_pause_switch.mp3";
        public const string WeaponGunshotPath = "Assets/_Project/Audio/SFX/Weapon/weapon_gunshot.mp3";

        public int version;

        [Header("Subtle Environmental Layers (Existing Soundtrack Preserved)")]
        public AudioClip airVentLoop;
        public AudioClip aggressiveGrowlLoop;

        [Header("Enemy SFX")]
        public AudioClip enemyAlertMonster;
        public AudioClip enemyAttackSwipe;
        public AudioClip enemyDeathGurgle;
        public AudioClip enemyHitImpactFlesh;

        [Header("Player SFX")]
        public AudioClip playerAttackWhoosh;
        public AudioClip playerDeathBodyFall;
        public AudioClip playerInteract;
        public AudioClip playerTakeDamage;

        [Header("Movement Footstep Loops")]
        public AudioClip footstepsConcrete;
        public AudioClip footstepsRunning;
        public AudioClip footstepsLimping;

        [Header("UI & Weapon SFX")]
        public AudioClip uiMenuClick;
        public AudioClip uiPauseSwitch;
        public AudioClip weaponGunshot;

        public bool IsComplete =>
            airVentLoop != null &&
            aggressiveGrowlLoop != null &&
            enemyAlertMonster != null &&
            enemyAttackSwipe != null &&
            enemyDeathGurgle != null &&
            enemyHitImpactFlesh != null &&
            playerAttackWhoosh != null &&
            playerDeathBodyFall != null &&
            playerInteract != null &&
            playerTakeDamage != null &&
            footstepsConcrete != null &&
            footstepsRunning != null &&
            footstepsLimping != null &&
            uiMenuClick != null &&
            uiPauseSwitch != null &&
            weaponGunshot != null;
    }
}
