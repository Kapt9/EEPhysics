// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhysicsConfig.cs" company="">
//   
// </copyright>
// <summary>
//   Constant physics variables.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EEPhysics
{
    using System;

    /// <summary>
    ///     Constant physics variables.
    /// </summary>
    internal static class PhysicsConfig
    {
        #region Constants

        /// <summary>
        /// The boost.
        /// </summary>
        public const double Boost = 16;

        /// <summary>
        /// The gravity.
        /// </summary>
        public const double Gravity = 2;

        /// <summary>
        /// The jump height.
        /// </summary>
        public const double JumpHeight = 26;

        /// <summary>
        /// The ms per tick.
        /// </summary>
        public const int MsPerTick = 10;

        /// <summary>
        /// The mud buoyancy.
        /// </summary>
        public const double MudBuoyancy = 0.4;

        /// <summary>
        /// The queue length.
        /// </summary>
        public const int QueueLength = 2;

        /// <summary>
        /// The variable multiplier.
        /// </summary>
        public const double VariableMultiplier = 7.752;

        /// <summary>
        /// The water buoyancy.
        /// </summary>
        public const double WaterBuoyancy = -0.5;

        #endregion

        #region Static Fields

        /// <summary>
        /// The base drag.
        /// </summary>
        public static readonly double BaseDrag = Math.Pow(0.9981, MsPerTick) * 1.00016093;

        /// <summary>
        /// The mud drag.
        /// </summary>
        public static readonly double MudDrag = Math.Pow(0.975, MsPerTick) * 1.00016093;

        /// <summary>
        /// The no modifier drag.
        /// </summary>
        public static readonly double NoModifierDrag = Math.Pow(0.99, MsPerTick) * 1.00016093;

        /// <summary>
        /// The water drag.
        /// </summary>
        public static readonly double WaterDrag = Math.Pow(0.995, MsPerTick) * 1.00016093;

        #endregion
    }
}