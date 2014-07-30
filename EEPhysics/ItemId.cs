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
        /// The bric k_ complete.
        /// </summary>
        public const int BRICK_COMPLETE = 121;

        /// <summary>
        /// The cake.
        /// </summary>
        public const int CAKE = 337;

        /// <summary>
        /// The chain.
        /// </summary>
        public const int CHAIN = 118;

        /// <summary>
        /// The checkpoint.
        /// </summary>
        public const int CHECKPOINT = 360;

        /// <summary>
        /// The coindoor.
        /// </summary>
        public const int COINDOOR = 43;

        /// <summary>
        /// The coingate.
        /// </summary>
        public const int COINGATE = 165;

        /// <summary>
        /// The diamond.
        /// </summary>
        public const int DIAMOND = 241;

        /// <summary>
        /// The doo r_ club.
        /// </summary>
        public const int DOOR_CLUB = 200;

        /// <summary>
        /// The doo r_ purple.
        /// </summary>
        public const int DOOR_PURPLE = 184;

        /// <summary>
        /// The fire.
        /// </summary>
        public const int FIRE = 368;

        /// <summary>
        /// The gat e_ club.
        /// </summary>
        public const int GATE_CLUB = 201;

        /// <summary>
        /// The gat e_ purple.
        /// </summary>
        public const int GATE_PURPLE = 185;

        /// <summary>
        /// The glowylin e_ blu e_ slope.
        /// </summary>
        public const int GLOWYLINE_BLUE_SLOPE = 375;

        /// <summary>
        /// The glow y_ lin e_ blu e_ straight.
        /// </summary>
        public const int GLOWY_LINE_BLUE_STRAIGHT = 376;

        /// <summary>
        /// The glow y_ lin e_ gree n_ slope.
        /// </summary>
        public const int GLOWY_LINE_GREEN_SLOPE = 379;

        /// <summary>
        /// The glow y_ lin e_ gree n_ straight.
        /// </summary>
        public const int GLOWY_LINE_GREEN_STRAIGHT = 380;

        /// <summary>
        /// The glow y_ lin e_ yello w_ slope.
        /// </summary>
        public const int GLOWY_LINE_YELLOW_SLOPE = 377;

        /// <summary>
        /// The glow y_ lin e_ yello w_ straight.
        /// </summary>
        public const int GLOWY_LINE_YELLOW_STRAIGHT = 378;

        /// <summary>
        /// The mud.
        /// </summary>
        public const int MUD = 369;

        /// <summary>
        /// The mu d_ bubble.
        /// </summary>
        public const int MUD_BUBBLE = 370;

        /// <summary>
        /// The ninj a_ ladder.
        /// </summary>
        public const int NINJA_LADDER = 120;

        /// <summary>
        /// The portal.
        /// </summary>
        public const int PORTAL = 242;

        /// <summary>
        /// The porta l_ invisible.
        /// </summary>
        public const int PORTAL_INVISIBLE = 381;

        /// <summary>
        /// The spee d_ down.
        /// </summary>
        public const int SPEED_DOWN = 117;

        /// <summary>
        /// The spee d_ left.
        /// </summary>
        public const int SPEED_LEFT = 114;

        /// <summary>
        /// The spee d_ right.
        /// </summary>
        public const int SPEED_RIGHT = 115;

        /// <summary>
        /// The spee d_ up.
        /// </summary>
        public const int SPEED_UP = 116;

        /// <summary>
        /// The spike.
        /// </summary>
        public const int SPIKE = 361;

        /// <summary>
        /// The switc h_ purple.
        /// </summary>
        public const int SWITCH_PURPLE = 113;

        /// <summary>
        /// The tex t_ sign.
        /// </summary>
        public const int TEXT_SIGN = 385;

        /// <summary>
        /// The timedoor.
        /// </summary>
        public const int TIMEDOOR = 156;

        /// <summary>
        /// The timegate.
        /// </summary>
        public const int TIMEGATE = 157;

        /// <summary>
        /// The water.
        /// </summary>
        public const int WATER = 119;

        /// <summary>
        /// The wave.
        /// </summary>
        public const int WAVE = 300;

        /// <summary>
        /// The win e_ h.
        /// </summary>
        public const int WINE_H = 99;

        /// <summary>
        /// The win e_ v.
        /// </summary>
        public const int WINE_V = 98;

        /// <summary>
        /// The worl d_ portal.
        /// </summary>
        public const int WORLD_PORTAL = 374;

        /// <summary>
        /// The zombi e_ door.
        /// </summary>
        public const int ZOMBIE_DOOR = 207;

        /// <summary>
        /// The zombi e_ gate.
        /// </summary>
        public const int ZOMBIE_GATE = 206;

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
        public static bool isClimbable(int id)
        {
            switch (id)
            {
                case NINJA_LADDER:
                case CHAIN:
                case WINE_V:
                case WINE_H:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// The is solid.
        /// </summary>
        /// <param name="_arg1">
        /// The _arg 1.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool isSolid(int _arg1)
        {
            return (9 <= _arg1 && _arg1 <= 97) || (122 <= _arg1 && _arg1 <= 217);
        }

        #endregion
    }
}