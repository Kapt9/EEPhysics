using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PlayerIOClient;

namespace EEPhysics
{
    public class PhysicsPlayer
    {
        internal PhysicsWorld HostWorld { get; set; }
        internal const int Width = 16;
        internal const int Height = 16;
        public double X { get; internal set; }
        public double Y { get; internal set; }
        public int Horizontal { get; internal set; }
        public int Vertical { get; internal set; }
        //private int lastTile;
        private int current;
        private double speedX = 0;
        private double speedY = 0;
        private double modifierX = 0;
        private double modifierY = 0;
        private int gravity;
        private double mox, moy;
        private int morx, mory;
        private int pastx, pasty;
        internal int overlapy;
        private double mx, my;
        private bool isInvulnerable;
        private bool donex, doney;
        private int[] queue = new int[PhysicsConfig.QueueLength];
        private int delayed;
        private Point lastPortal;
        private List<Point> gotCoins = new List<Point>();
        private List<Point> gotBlueCoins = new List<Point>();

        /// <summary>Purple switch state.</summary>
        public bool Purple { get; internal set; }
        /// <summary>Also includes moderatormode.</summary>
        public bool InGodMode { get; internal set; }
        public bool IsDead { get; internal set; }
        //public bool Zombie { get; internal set; }

        internal double GravityMultiplier { get { return HostWorld.WorldGravity; } }
        internal double SpeedMultiplier
        {
            get
            {
                /*double d = 1;
                if (Zombie) {
                    d *= 1.2;
                }
                return d;*/
                return 1;
            }
        }
        public double SpeedX { get { return speedX * PhysicsConfig.VariableMultiplier; } internal set { speedX = value / PhysicsConfig.VariableMultiplier; } }
        public double SpeedY { get { return speedY * PhysicsConfig.VariableMultiplier; } internal set { speedY = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierX { get { return modifierX * PhysicsConfig.VariableMultiplier; } internal set { modifierX = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierY { get { return modifierY * PhysicsConfig.VariableMultiplier; } internal set { modifierY = value / PhysicsConfig.VariableMultiplier; } }

        public int LastCheckpointX { get; private set; }
        public int LastCheckpointY { get; private set; }

        public int ID { get; private set; }
        public string Name { get; protected set; }
        public int Coins { get; set; }
        public int BlueCoins { get { return gotBlueCoins.Count; } }
        public bool IsClubMember { get; set; }


        public delegate void PlayerEvent(PlayerEventArgs e);

        public event PlayerEvent OnHitCrown = delegate { };

        /// <summary>
        /// Note: This will be called every time player hits coin, even if the coin is already got by that player. If you want to get only first time coin is hit, use event OnGetCoin.
        /// </summary>
        public event PlayerEvent OnHitCoin = delegate { };
        /// <summary>
        /// Will be called only when player hits a coin first time. After first time, only event OnHitCoin will be called.
        /// </summary>
        public event PlayerEvent OnGetCoin = delegate { };

        /// <summary>
        /// Note: This will be called every time player hits blue coin, even if the coin is already got by that player. If you want to get only first time coin is hit, use event OnGetBlueCoin.
        /// </summary>
        public event PlayerEvent OnHitBlueCoin = delegate { };
        /// <summary>
        /// Will be called only when player hits a blue coin first time. After first time, only event OnHitBlueCoin will be called.
        /// </summary>
        public event PlayerEvent OnGetBlueCoin = delegate { };

        public event PlayerEvent OnHitRedKey = delegate { };
        public event PlayerEvent OnHitBlueKey = delegate { };
        public event PlayerEvent OnHitGreenKey = delegate { };

        public event PlayerEvent OnHitPiano = delegate { };
        public event PlayerEvent OnHitDrum = delegate { };
        public event PlayerEvent OnHitSwitch = delegate { };
        public event PlayerEvent OnHitDiamond = delegate { };
        public event PlayerEvent OnHitCake = delegate { };

        public event PlayerEvent OnHitCompleteLevelBrick = delegate { };
        public event PlayerEvent OnHitCheckpoint = delegate { };

        public event PlayerEvent OnDie = delegate { };


        public PhysicsPlayer(int id, string name)
        {
            ID = id;
            Name = name;
            X = 16;
            Y = 16;
            gravity = (int)PhysicsConfig.Gravity;
        }

        /// <summary>
        /// Updates player's position. This is run automatically from World.run() method every 10ms. You shouldn't run this unless you know what you're doing.
        /// </summary>
        public void tick()
        {
            int cx = 0;
            int cy = 0;
            bool isGodMode = false;

            double reminderX = double.NaN;
            double currentSX = double.NaN;
            double osx = double.NaN;
            double ox = double.NaN;
            double tx = double.NaN;

            double reminderY = double.NaN;
            double currentSY = double.NaN;
            double osy = double.NaN;
            double oy = double.NaN;
            double ty = double.NaN;

            cx = ((int)(X + 8) >> 4);
            cy = ((int)(Y + 8) >> 4);

            current = HostWorld.GetBlock(cx, cy);
            if (current == 4 || ItemId.isClimbable(current))
            {
                delayed = queue[1];
                queue[0] = current;
            }
            else
            {
                delayed = queue[0];
                queue[0] = queue[1];
            }
            queue[1] = current;

            if (IsDead)
            {
                Horizontal = 0;
                Vertical = 0;
            }

            isGodMode = InGodMode;
            if (InGodMode)
            {
                morx = 0;
                mory = 0;
                mox = 0;
                moy = 0;
            }
            else
            {
                switch (current)
                {
                    case 1:
                        morx = -((int)gravity);
                        mory = 0;
                        break;
                    case 2:
                        morx = 0;
                        mory = -((int)gravity);
                        break;
                    case 3:
                        morx = (int)gravity;
                        mory = 0;
                        break;
                    case ItemId.SPEED_LEFT:
                    case ItemId.SPEED_RIGHT:
                    case ItemId.SPEED_UP:
                    case ItemId.SPEED_DOWN:
                    case ItemId.CHAIN:
                    case ItemId.NINJA_LADDER:
                    case ItemId.WINE_H:
                    case ItemId.WINE_V:
                    case 4:
                        morx = 0;
                        mory = 0;
                        break;
                    case ItemId.WATER:
                        morx = 0;
                        mory = (int)PhysicsConfig.WaterBuoyancy;
                        break;
                    case ItemId.MUD:
                        morx = 0;
                        mory = (int)PhysicsConfig.MudBuoyancy;
                        break;
                    case ItemId.FIRE:
                    case ItemId.SPIKE:
                        if (!IsDead && !isInvulnerable)
                        {
                            killPlayer();
                            OnDie(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                        };
                        break;
                    default:
                        morx = 0;
                        mory = (int)gravity;
                        break;
                }

                switch (delayed)
                {
                    case 1:
                        mox = -gravity;
                        moy = 0;
                        break;
                    case 2:
                        mox = 0;
                        moy = -gravity;
                        break;
                    case 3:
                        mox = gravity;
                        moy = 0;
                        break;
                    case ItemId.SPEED_LEFT:
                    case ItemId.SPEED_RIGHT:
                    case ItemId.SPEED_UP:
                    case ItemId.SPEED_DOWN:
                    case ItemId.CHAIN:
                    case ItemId.NINJA_LADDER:
                    case ItemId.WINE_H:
                    case ItemId.WINE_V:
                    case 4:
                        mox = 0;
                        moy = 0;
                        break;
                    case ItemId.WATER:
                        mox = 0;
                        moy = PhysicsConfig.WaterBuoyancy;
                        break;
                    case ItemId.MUD:
                        mox = 0;
                        moy = PhysicsConfig.MudBuoyancy;
                        break;
                    default:
                        mox = 0;
                        moy = gravity;
                        break;
                }
            }

            if (moy == PhysicsConfig.WaterBuoyancy || moy == PhysicsConfig.MudBuoyancy)
            {
                mx = Horizontal;
                my = Vertical;
            }
            else
            {
                if (moy != 0)
                {
                    mx = Horizontal;
                    my = 0;
                }
                else
                {
                    if (mox != 0)
                    {
                        mx = 0;
                        my = Vertical;
                    }
                    else
                    {
                        mx = Horizontal;
                        my = Vertical;
                    }
                }
            }
            mx = (mx * SpeedMultiplier);
            my = (my * SpeedMultiplier);
            mox = (mox * GravityMultiplier);
            moy = (moy * GravityMultiplier);

            ModifierX = (mox + mx);
            ModifierY = (moy + my);

            if (speedX != 0 || modifierX != 0)
            {
                speedX = (speedX + modifierX);
                speedX = (speedX * PhysicsConfig.BaseDrag);
                if (((mx == 0 && moy != 0) || speedX < 0 && mx > 0) || (speedX > 0 && mx < 0) || (ItemId.isClimbable(current) && !isGodMode))
                {
                    speedX = (speedX * PhysicsConfig.NoModifierDrag);
                }
                else
                {
                    if (current == ItemId.WATER && !isGodMode)
                    {
                        speedX = (speedX * PhysicsConfig.WaterDrag);
                    }
                    else
                    {
                        if (current == ItemId.MUD && !isGodMode)
                        {
                            speedX = (speedX * PhysicsConfig.MudDrag);
                        }
                    }
                }
                if (speedX > 16)
                {
                    speedX = 16;
                }
                else
                {
                    if (speedX < -16)
                    {
                        speedX = -16;
                    }
                    else
                    {
                        if (speedX < 0.0001 && speedX > -0.0001)
                        {
                            speedX = 0;
                        }
                    }
                }
            }
            if (speedY != 0 || modifierY != 0)
            {
                speedY = (speedY + modifierY);
                speedY = (speedY * PhysicsConfig.BaseDrag);
                if ((my == 0 && mox != 0) || (speedY < 0 && my > 0) || (speedY > 0 && my < 0) || (ItemId.isClimbable(current) && !isGodMode))
                {
                    speedY = (speedY * PhysicsConfig.NoModifierDrag);
                }
                else
                {
                    if (current == ItemId.WATER && !isGodMode)
                    {
                        speedY = (speedY * PhysicsConfig.WaterDrag);
                    }
                    else
                    {
                        if (current == ItemId.MUD && !isGodMode)
                        {
                            speedY = (speedY * PhysicsConfig.MudDrag);
                        }
                    }
                }
                if (speedY > 16)
                {
                    speedY = 16;
                }
                else
                {
                    if (speedY < -16)
                    {
                        speedY = -16;
                    }
                    else
                    {
                        if (speedY < 0.0001 && speedY > -0.0001)
                        {
                            speedY = 0;
                        }
                    }
                }
            }
            if (isGodMode)
            {
                switch (this.current)
                {
                    case ItemId.SPEED_LEFT:
                        speedX = -PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_RIGHT:
                        speedX = PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_UP:
                        speedY = -PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_DOWN:
                        speedY = PhysicsConfig.Boost;
                        break;
                }
            }

            reminderX = X % 1;
            currentSX = speedX;
            reminderY = Y % 1;
            currentSY = speedY;
            donex = false;
            doney = false;

            while ((currentSX != 0 && !donex) || (currentSY != 0 && !doney))
            {
                #region processPortals()
                double multiplier = 1.42;
                current = HostWorld.GetBlock(cx, cy);
                if (!isGodMode && (current == ItemId.PORTAL || current == ItemId.PORTAL_INVISIBLE))
                {
                    if (lastPortal == null)
                    {
                        lastPortal = new Point(cx, cy);
                        int[] data = HostWorld.GetBlockData(cx, cy);
                        if (data != null && data.Length == 3)
                        {
                            Point portalPoint = HostWorld.GetPortalById(data[2]);
                            if (portalPoint != null)
                            {
                                int rot1 = HostWorld.GetBlockData(lastPortal.x, lastPortal.y)[0];
                                int rot2 = HostWorld.GetBlockData(portalPoint.x, portalPoint.y)[0];
                                if (rot1 < rot2)
                                {
                                    rot1 += 4;
                                }
                                switch (rot1 - rot2)
                                {
                                    case 1:
                                        SpeedX = (SpeedY * multiplier);
                                        SpeedY = (-SpeedX * multiplier);
                                        ModifierX = (ModifierY * multiplier);
                                        ModifierY = (-ModifierX * multiplier);
                                        reminderY = -reminderY;
                                        currentSY = -currentSY;
                                        break;
                                    case 2:
                                        SpeedX = (-SpeedX * multiplier);
                                        SpeedY = (-SpeedY * multiplier);
                                        ModifierX = (-(ModifierX) * multiplier);
                                        ModifierY = (-(ModifierY) * multiplier);
                                        reminderY = -(reminderY);
                                        currentSY = -(currentSY);
                                        reminderX = -(reminderX);
                                        currentSX = -(currentSX);
                                        break;
                                    case 3:
                                        SpeedX = (-SpeedY * multiplier);
                                        SpeedY = (SpeedX * multiplier);
                                        ModifierX = (-(ModifierY) * multiplier);
                                        ModifierY = (ModifierX * multiplier);
                                        reminderX = -(reminderX);
                                        currentSX = -(currentSX);
                                        break;
                                }
                                X = portalPoint.x * 16;
                                Y = portalPoint.y * 16;
                                lastPortal = portalPoint;
                            }
                        }
                    }
                }
                else
                {
                    lastPortal = null;
                }
                #endregion

                ox = X;
                oy = Y;
                osx = currentSX;
                osy = currentSY;

                #region stepX()
                if (currentSX > 0)
                {
                    if ((currentSX + reminderX) >= 1)
                    {
                        X = (X + (1 - reminderX));
                        X = ((int)X >> 0);
                        currentSX = (currentSX - (1 - reminderX));
                        reminderX = 0;
                    }
                    else
                    {
                        X = (X + currentSX);
                        currentSX = 0;
                    }
                }
                else
                {
                    if (currentSX < 0)
                    {
                        if (reminderX != 0 && (reminderX + currentSX) < 0)
                        {
                            currentSX = (currentSX + reminderX);
                            X = (X - reminderX);
                            X = ((int)X >> 0);
                            reminderX = 1;
                        }
                        else
                        {
                            X = (X + currentSX);
                            currentSX = 0;
                        }
                    }
                }
                if (HostWorld.overlaps(this))
                {
                    X = ox;
                    speedX = 0;
                    currentSX = osx;
                    donex = true;
                }
                #endregion

                #region stepY()
                if (currentSY > 0)
                {
                    if ((currentSY + reminderY) >= 1)
                    {
                        Y = (Y + (1 - reminderY));
                        Y = ((int)Y >> 0);
                        currentSY = (currentSY - (1 - reminderY));
                        reminderY = 0;
                    }
                    else
                    {
                        Y = (Y + currentSY);
                        currentSY = 0;
                    };
                }
                else
                {
                    if (currentSY < 0)
                    {
                        if (((!((reminderY == 0))) && (((reminderY + currentSY) < 0))))
                        {
                            Y = (Y - reminderY);
                            Y = ((int)Y >> 0);
                            currentSY = (currentSY + reminderY);
                            reminderY = 1;
                        }
                        else
                        {
                            Y = (Y + currentSY);
                            currentSY = 0;
                        }
                    }
                }
                if (HostWorld.overlaps(this))
                {
                    Y = oy;
                    speedY = 0;
                    currentSY = osy;
                    doney = true;
                }
                #endregion
            }

            if (!IsDead)
            {
                if (pastx != cx || pasty != cy)
                {
                    switch (current)
                    {
                        case 100:   //coin
                            OnHitCoin(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < gotCoins.Count; i++)
                            {
                                if (gotCoins[i].x == cx && gotCoins[i].y == cy)
                                {
                                    goto found;
                                }
                            }
                            OnGetCoin(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            gotCoins.Add(new Point(cx, cy));
                        found:
                            break;
                        case 101:   // bluecoin
                            OnHitBlueCoin(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < gotBlueCoins.Count; i++)
                            {
                                if (gotBlueCoins[i].x == cx && gotBlueCoins[i].y == cy)
                                {
                                    goto found2;
                                }
                            }
                            OnGetBlueCoin(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            gotBlueCoins.Add(new Point(cx, cy));
                        found2:
                            break;
                        case 5:
                            // crown
                            OnHitCrown(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 6:
                            // red key
                            OnHitRedKey(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 7:
                            // green
                            OnHitGreenKey(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 8:
                            // blue
                            OnHitBlueKey(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.SWITCH_PURPLE:
                            // purple (switch)
                            Purple = !Purple;
                            OnHitSwitch(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 77:
                            // piano
                            OnHitPiano(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 83:
                            // drum
                            OnHitDrum(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.DIAMOND:
                            // diamond
                            OnHitDiamond(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.CAKE:
                            // cake
                            OnHitCake(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.CHECKPOINT:
                            // checkpoint
                            if (!isGodMode)
                            {
                                LastCheckpointX = cx;
                                LastCheckpointY = cy;
                                OnHitCheckpoint(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            }
                            break;
                        case ItemId.BRICK_COMPLETE:
                            // level completed
                            OnHitCompleteLevelBrick(new PlayerEventArgs() { Player = this, BlockX = cx, BlockY = cy });
                            break;
                    }
                    pastx = cx;
                    pasty = cy;
                }
            }

            var imx = ((int)speedX << 8);
            var imy = ((int)speedY << 8);

            if (current != ItemId.WATER && current != ItemId.MUD)
            {
                if (imx == 0)
                {
                    if (modifierX < 0.1 && modifierX > -0.1)
                    {
                        tx = (X % 16);
                        if (tx < 2)
                        {
                            if (tx < 0.2)
                            {
                                X = Math.Floor(X);
                            }
                            else
                            {
                                X = (X - (tx / 15));
                            };
                        }
                        else
                        {
                            if (tx > 14)
                            {
                                if (tx > 15.8)
                                {
                                    X = Math.Ceiling(X);
                                }
                                else
                                {
                                    X = (X + ((tx - 14) / 15));
                                }
                            }
                        }
                    }
                }

                if (imy == 0)
                {
                    if ((modifierY < 0.1) && (modifierY > -0.1))
                    {
                        ty = (Y % 16);
                        if (ty < 2)
                        {
                            if (ty < 0.2)
                            {
                                Y = Math.Floor(Y);
                            }
                            else
                            {
                                Y = (Y - (ty / 15));
                            }
                        }
                        else
                        {
                            if (ty > 14)
                            {
                                if (ty > 15.8)
                                {
                                    Y = Math.Ceiling(Y);
                                }
                                else
                                {
                                    Y = (Y + ((ty - 14) / 15));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <returns>True if player overlaps block at x,y.</returns>
        public bool OverlapsTile(int tx, int ty)
        {
            int xx = tx * 16;
            int yy = ty * 16;
            return ((X > xx - 16 && X <= xx + 16) && (Y > yy - 16 && Y <= yy + 16));
        }

        internal void respawn()
        {
            modifierX = 0;
            modifierY = 0;
            ModifierX = 0;
            ModifierY = 0;
            speedX = 0;
            speedY = 0;
            SpeedX = 0;
            SpeedY = 0;
            IsDead = false;
        }
        internal void killPlayer()
        {
            IsDead = true;
        }
        internal void resetCoins()
        {
            gotCoins.Clear();
            gotBlueCoins.Clear();
        }
        internal void removeCoin(int xx, int yy)
        {
            for (int i = 0; i < gotCoins.Count; i++)
            {
                if (gotCoins[i].x == xx && gotCoins[i].y == yy)
                {
                    gotCoins.RemoveAt(i);
                    break;
                }
            }
        }
        internal void removeBlueCoin(int xx, int yy)
        {
            for (int i = 0; i < gotCoins.Count; i++)
            {
                if (gotCoins[i].x == xx && gotCoins[i].y == yy)
                {
                    gotCoins.RemoveAt(i);
                    break;
                }
            }
        }
    }

    public class PlayerEventArgs
    {
        /// <summary>
        /// Player which caused the event.
        /// </summary>
        public PhysicsPlayer Player { get; set; }
        /// <summary>
        /// Block X where event happened.
        /// </summary>
        public int BlockX { get; set; }
        /// <summary>
        /// Block Y where event happened.
        /// </summary>
        public int BlockY { get; set; }
    }

    internal class Point
    {
        public int x, y;
        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }
    }
}
