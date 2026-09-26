using System;
using UnityEngine;
namespace SaileachStudios.Mirlini.Core {
    // One production profile, loaded at startup; never stored per level or modified by gameplay.
    [CreateAssetMenu(menuName="Mirlini/Gameplay feel")]
    public sealed class GameplayFeel : ScriptableObject {
        [Min(.1f)] public float Speed = 9.5f;
        [Min(.1f)] public float Response = 12f;
        [Min(.1f)] public float ReleaseResponse = 18f;
        [Min(.1f)] public float MaxAcceleration = 120f;
        [Min(0)] public float VelocityDeadband = .02f;
        [Min(0)] public float LinearDamping = .5f;
        [Min(0)] public float KeyboardMultiplier = 100f;
        [Min(0)] public float TouchMultiplier = 1f;
        [Min(0)] public float GyroMultiplier = 2f;
        [Range(0,20)] public float TiltDegrees = 6f;
        [Min(.1f)] public float TiltResponse = 5f;
        public Material WorldFeedbackMaterial;
        public bool IsValid {
            get {
                foreach(float v in new[]{Speed,Response,ReleaseResponse,MaxAcceleration,VelocityDeadband,LinearDamping,
                    KeyboardMultiplier,TouchMultiplier,GyroMultiplier,TiltDegrees,TiltResponse})
                    if(float.IsNaN(v)||float.IsInfinity(v)||v<0) return false;
                return Speed>0 && Response>0 && ReleaseResponse>0 && MaxAcceleration>0 && TiltResponse>0 &&
                    VelocityDeadband<Speed && TiltDegrees<=20 && WorldFeedbackMaterial!=null;
            }
        }
        public static GameplayFeel Load() {
            var profile=Resources.Load<GameplayFeel>("Gameplay Feel");
            if(profile==null || !profile.IsValid) throw new InvalidOperationException("Valid Resources/Gameplay Feel profile is required.");
            return profile;
        }
    }
}
