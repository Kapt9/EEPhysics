using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EEPhysics
{
    internal static class ItemId
    {
        public const int Piano = 77;
        public const int Drum = 83;
        public const int SwitchPurple = 113;
        public const int DoorPurple = 184;
        public const int GatePurple = 185;
        public const int DoorClub = 200;
        public const int GateClub = 201;
        public const int SpeedLeft = 114;
        public const int SpeedRight = 115;
        public const int SpeedUp = 116;
        public const int SpeedDown = 117;
        public const int Chain = 118;
        public const int Water = 119;
        public const int NinjaLadder = 120;
        public const int BrickComplete = 121;
        public const int Timedoor = 156;
        public const int Timegate = 157;
        public const int Coindoor = 43;
        public const int Coingate = 165;
        public const int BlueCoindoor = 213;
        public const int BlueCoingate = 214;
        public const int WineV = 98;
        public const int WineH = 99;
        public const int Diamond = 241;
        public const int Wave = 300;
        public const int Cake = 337;
        public const int Checkpoint = 360;
        public const int Spike = 361;
        public const int Fire = 368;
        public const int Mud = 369;
        public const int Lava = 416;
        public const int MudBubble = 370;
        public const int Portal = 242;
        public const int WorldPortal = 374;
        public const int ZombieGate = 206;
        public const int ZombieDoor = 207;
        public const int GlowylineBlueSlope = 375;
        public const int GlowyLineBlueStraight = 376;
        public const int GlowyLineYellowSlope = 377;
        public const int GlowyLineYellowStraight = 378;
        public const int GlowyLineGreenSlope = 379;
        public const int GlowyLineGreenStraight = 380;
        public const int GlowyLineRedSlope = 438;
        public const int GlowyLineRedStraight = 439;
        public const int PortalInvisible = 381;
        public const int TextSign = 385;
        public const int CyanKey = 408;
        public const int MagentaKey = 409;
        public const int YellowKey = 410;
        public const int InvisibleLeftArrow = 411;
        public const int InvisibleUpArrow = 412;
        public const int InvisibleRightArrow = 413;
        public const int InvisibleDot = 414;
        public const int OnewayCyan = 1001; 
        public const int OnewayOrange = 1002; 
        public const int OnewayYellow = 1003; 
        public const int OnewayPink = 1004;
        public const int OnewayGray = 1052;
        public const int OnewayBlue = 1053;
        public const int OnewayRed = 1054;
        public const int OnewayGreen = 1055;
        public const int OnewayBlack = 1056;
        public const int CyanDoor = 1005;
        public const int MagentaDoor = 1006;
        public const int YellowDoor = 1007;
        public const int CyanGate = 1008;
        public const int MagentaGate = 1009;
        public const int YellowGate = 1010;
        public const int DeathDoor = 1011;
        public const int DeathGate = 1012;
        public const int EffectJump = 417;
        public const int EffectFly = 418;
        public const int EffectRun = 419;
        public const int EffectProtection = 420;
        public const int EffectCurse = 421;
        public const int EffectZombie = 422;
        public const int EffectTeam = 423;
        public const int TeamDoor = 1027;
        public const int TeamGate = 1028;
        public const int Rope = 424;
        public const int MedievalShield = 273;
        public const int MedievalAxe = 275;
        public const int MedievalBbanner = 327;
        public const int MedievalCcoatfarms = 328;
        public const int MedievalSword = 329;
        public const int MedievalTimber = 440;
        public const int ToothBig = 338;
        public const int ToothSmall = 339;
        public const int ToothTriple = 340;
        public const int DojoLightLeft = 276;
        public const int DojoLightRight = 277;
        public const int DojoDarkLeft = 279;
        public const int DojoDarkRight = 280;
        public const int MedievalBanner = 327;
        public const int MedievalCoatofarms = 328;
        public const int Hologram = 397;
        public const int SlowDot = 459;
        public const int SlowDotInvisible = 460;
        public const int HalfBlockDomesticYellow = 1041;
        public const int HalfBlockDomesticBrown = 1042;
        public const int HalfBlockDomesticWhite = 1043;

        public static bool isSolid(int blockId)
        {
            return (9 <= blockId && blockId <= 97) || (122 <= blockId && blockId <= 217) || (1001 <= blockId && blockId <= 2000);
        }

        public static bool IsBackground(int blockId)
        {
            return blockId >= 500 && blockId <= 999;
        }

        public static bool isClimbable(int id)
        {
            switch (id)
            {
                case ItemId.NinjaLadder:
                case ItemId.Chain:
                case ItemId.WineV:
                case ItemId.WineH:
                case ItemId.Rope:
                case ItemId.SlowDot:
                case ItemId.SlowDotInvisible:
                    return true;
            }

            return false;
        }

        public static bool IsHalfBlock(int id)
        {
            switch (id)
            {
                case HalfBlockDomesticBrown:
                case HalfBlockDomesticWhite:
                case HalfBlockDomesticYellow:
                    return true;

                default:
                    return false;
            }
        }

        public static bool CanJumpThroughFromBelow(int itemId)
        {
            switch (itemId)
            {
                case 61:
                case 62:
                case 63:
                case 64:
                case 89:
                case 90:
                case 91:
                case 96:
                case 97:
                case 122:
                case 123:
                case 124:
                case 125:
                case 126:
                case 127:
                case 146:
                case 154:
                case 158:
                case 194:
                case 211:
                case 216:
                case OnewayCyan:
                case OnewayOrange:
                case OnewayYellow:
                case OnewayPink:
                case OnewayGray:
                case OnewayBlue:
                case OnewayRed:
                case OnewayGreen:
                case OnewayBlack:
                case 1050:
                case 1051:
                    return true;
            }
            return false;
        }
    }
}
