// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ItemId.cs" company="">
//   
// </copyright>
// <summary>
//   The item id.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EEPhysics
{
    /// <summary>
    /// The item id.
    /// </summary>
    internal static class ItemId
    {
        #region Constants

        /// <summary>
        /// The brick complete option.
        /// </summary>
        public const int BrickComplete = 121;

        /// <summary>
        /// The cake.
        /// </summary>
        public const int Cake = 337;

        /// <summary>
        /// The chain.
        /// </summary>
        public const int Chain = 118;

        /// <summary>
        /// The checkpoint.
        /// </summary>
        public const int Checkpoint = 360;

        /// <summary>
        /// The coin door.
        /// </summary>
        public const int Coindoor = 43;

        /// <summary>
        /// The coin gate.
        /// </summary>
        public const int Coingate = 165;

        /// <summary>
        /// The diamond.
        /// </summary>
        public const int Diamond = 241;

        /// <summary>
        /// The door club.
        /// </summary>
        public const int DoorClub = 200;

        /// <summary>
        /// The purple door.
        /// </summary>
        public const int DoorPurple = 184;

        /// <summary>
        /// The fire.
        /// </summary>
        public const int Fire = 368;

        /// <summary>
        /// The gate club.
        /// </summary>
        public const int GateClub = 201;

        /// <summary>
        /// The purple gate.
        /// </summary>
        public const int GatePurple = 185;

        /// <summary>
        /// The glowy line blue slope.
        /// </summary>
        public const int GlowylineBlueSlope = 375;

        /// <summary>
        /// The glowy line blue straight.
        /// </summary>
        public const int GlowyLineBlueStraight = 376;

        /// <summary>
        /// The glowy line green slope.
        /// </summary>
        public const int GlowyLineGreenSlope = 379;

        /// <summary>
        /// The glowy line green straight.
        /// </summary>
        public const int GlowyLineGreenStraight = 380;

        /// <summary>
        /// The glowy line yellow slope.
        /// </summary>
        public const int GlowyLineYellowSlope = 377;

        /// <summary>
        /// The glowy line yellow straight.
        /// </summary>
        public const int GlowyLineYellowStraight = 378;

        /// <summary>
        /// The mud.
        /// </summary>
        public const int Mud = 369;

        /// <summary>
        /// The mud bubble.
        /// </summary>
        public const int MudBubble = 370;

        /// <summary>
        /// The ninja ladder.
        /// </summary>
        public const int NinjaLadder = 120;

        /// <summary>
        /// The portal.
        /// </summary>
        public const int Portal = 242;

        /// <summary>
        /// The invisible portal.
        /// </summary>
        public const int PortalInvisible = 381;

        /// <summary>
        /// The speed down.
        /// </summary>
        public const int SpeedDown = 117;

        /// <summary>
        /// The speed left.
        /// </summary>
        public const int SpeedLeft = 114;

        /// <summary>
        /// The speed right.
        /// </summary>
        public const int SpeedRight = 115;

        /// <summary>
        /// The speed up.
        /// </summary>
        public const int SpeedUp = 116;

        /// <summary>
        /// The spike.
        /// </summary>
        public const int Spike = 361;

        /// <summary>
        /// The switch purple.
        /// </summary>
        public const int SwitchPurple = 113;

        /// <summary>
        /// The text sign.
        /// </summary>
        public const int TextSign = 385;

        /// <summary>
        /// The time door.
        /// </summary>
        public const int Timedoor = 156;

        /// <summary>
        /// The time gate.
        /// </summary>
        public const int Timegate = 157;

        /// <summary>
        /// The water.
        /// </summary>
        public const int Water = 119;

        /// <summary>
        /// The wave.
        /// </summary>
        public const int Wave = 300;

        /// <summary>
        /// The horizontal vine.
        /// </summary>
        public const int WineH = 99;

        /// <summary>
        /// The vertical vine.
        /// </summary>
        public const int WineV = 98;

        /// <summary>
        /// The world portal.
        /// </summary>
        public const int WorldPortal = 374;

        /// <summary>
        /// The zombie door.
        /// </summary>
        public const int ZombieDoor = 207;

        /// <summary>
        /// The zombie gate.
        /// </summary>
        public const int ZombieGate = 206;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The is climbable.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsClimbable(int id)
        {
            switch (id)
            {
                case NinjaLadder:
                case Chain:
                case WineV:
                case WineH:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The is solid.
        /// </summary>
        /// <param name="arg1">
        /// The first argument.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool IsSolid(int arg1)
        {
            return (9 <= arg1 && arg1 <= 97) || (122 <= arg1 && arg1 <= 217);
        }

        #endregion
    }
}