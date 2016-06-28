using System;

namespace EEPhysics
{
    /// <summary>
    /// Constant physics variables.
    /// </summary>
    internal static class PhysicsConfig
    {
        public const double MsPerTick = 10;
        public const double VariableMultiplier = 7.752;
        public static readonly double DragMultiplier = 1.00016093;
        public static readonly double BaseDrag = Math.Pow(0.9981, MsPerTick) * DragMultiplier;
        public static readonly double NoModifierDrag = Math.Pow(0.99, MsPerTick) * DragMultiplier;
        public static readonly double WaterDrag = Math.Pow(0.995, MsPerTick) * DragMultiplier;
        public static readonly double MudDrag = Math.Pow(0.975, MsPerTick) * DragMultiplier;
        public static readonly double LavaDrag = Math.Pow(0.98, MsPerTick) * DragMultiplier;
        public static readonly double IceNoModDrag = Math.Pow(0.9993, MsPerTick) * DragMultiplier;
        public static readonly double IceDrag = Math.Pow(0.9998, MsPerTick) * DragMultiplier;
        public const double JumpHeight = 26;
        public const double Gravity = 2;
        public const double Boost = 16;
        public const double WaterBuoyancy = -0.5;
        public const double MudBuoyancy = 0.4;
        public const double LavaBuoyancy = 0.2;
        public const int QueueLength = 2;

        public const int EffectJump = 0;
        public const int EffectFly = 1;
        public const int EffectRun = 2;
        public const int EffectProtection = 3;
        public const int EffectCurse = 4;
        public const int EffectZombie = 5;
        public const int EffectLowGravity = 7;
        public const int EffectFire = 8;
        public const int EffectMultiJump = 9;


    }
}
