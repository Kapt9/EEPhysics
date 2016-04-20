using System;
using System.Collections;
using System.Collections.Generic;

namespace EEPhysics
{
    public class PhysicsPlayer
    {
        internal PhysicsWorld HostWorld { get; set; }
        internal const int Width = 16;
        internal const int Height = 16;
        /// <summary>
        /// The precise player X position.
        /// </summary>
        public double X { get; internal set; }
        /// <summary>
        /// The precise player Y position.
        /// </summary>
        public double Y { get; internal set; }

        private double oldX, oldY, speedX, speedY, modifierX, modifierY;
        private readonly List<Point> gotBlueCoins = new List<Point>();
        private readonly List<Point> gotCoins = new List<Point>();
        private readonly Queue<int> tileQueue = new Queue<int>();
        private readonly Queue<int> queue = new Queue<int>();
        private const double PortalMultiplier = 1.42;
        public int Horizontal { get; internal set; }
        public int Vertical { get; internal set; }
        internal BitArray Switches { get; set; }
        private int current, currentBelow;
        private int onFireDeath = 200;
        private readonly int gravity;
        private int tx = -1, ty = -1;
        private bool isInvulnerable;
        private bool hasLastPortal;
        private float deathOffset;
        private bool donex, doney;
        private int pastx, pasty;
        private Point lastPortal;
        private double mox, moy;
        private int morx, mory;
        private bool isOnFire;
        internal int Overlapy;
        private double mx, my;
        internal int Deaths;
        private int oh, ov;

        private double slippery = 0;
        private int jumpCount = 0;
        private int maxJumps = 1;

        public bool SpeedBoostEffect { get; internal set; }
        public bool JumpBoostEffect { get; internal set; }
        public bool HasLevitation { get; internal set; }

        public bool DoubleJumpEffect { get; internal set; }
        public bool GravityEffect { get; internal set; }

        public bool CursedEffect { get; internal set; }
        public bool IsThrusting { get; internal set; }
        public bool Zombie { get; internal set; }
        private double currentThrust = MaxThrust;
        private const double MaxThrust = 0.2;
        private readonly double thrustBurnOff = 0.01;

        /// <summary>Also includes moderator and guardian mode.</summary>
        public bool InGodMode { get; internal set; }
        public bool HasCrown { get; internal set; }
        public bool HasChat { get; internal set; }
        public bool IsDead { get; internal set; }
        public bool IsMe { get; internal set; }
        public int Team { get; internal set; }
        public int TickId { get; set; }

        private long lastJump;
        public bool JustSpaceDown { get; set; }
        public bool SpaceDown { get; set; }

        internal double gravityMultiplier
        {
            get
            {
                double d = 1;

                if (GravityEffect) d *= 0.15;
                else d *= HostWorld.WorldGravity;

                return d;
            }
        }
        internal double GravityMultiplier => HostWorld.WorldGravity;

        internal double SpeedMultiplier
        {
            get
            {
                double d = 1.0;
                if (Zombie) d *= 0.6;
                if (SpeedBoostEffect) d *= 1.5;
                return d;
            }
        }
        public double SpeedX { get { return speedX * PhysicsConfig.VariableMultiplier; } internal set { speedX = value / PhysicsConfig.VariableMultiplier; } }
        public double SpeedY { get { return speedY * PhysicsConfig.VariableMultiplier; } internal set { speedY = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierX { get { return modifierX * PhysicsConfig.VariableMultiplier; } internal set { modifierX = value / PhysicsConfig.VariableMultiplier; } }
        public double ModifierY { get { return modifierY * PhysicsConfig.VariableMultiplier; } internal set { modifierY = value / PhysicsConfig.VariableMultiplier; } }
        internal double JumpMultiplier
        {
            get
            {
                double d = 1.0;

                if (JumpBoostEffect) d *= 1.3;
                if (slippery > 0) d *= 0.88;
                if (Zombie) d *= 0.75;

                return d;
            }
        }

        public int LastCheckpointX { get; private set; }
        public int LastCheckpointY { get; private set; }

        /// <summary>
        /// The player ID in PlayerIO Messages
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Player name in the world.
        /// </summary>
        public string Name { get; protected set; }
        public bool IsClubMember { get; set; }
        public int BlueCoins { get; set; }
        public int Coins { get; set; }


        public delegate void PlayerEvent(PlayerEventArgs e);

        private readonly Dictionary<int, PlayerEvent> blockIdEvents = new Dictionary<int, PlayerEvent>();
        private readonly Dictionary<int, PlayerEvent> bgblockIdEvents = new Dictionary<int, PlayerEvent>();
        private readonly Dictionary<int, PlayerEvent> touchBlockEvents = new Dictionary<int, PlayerEvent>();
        private readonly List<Point> touchedPoints = new List<Point>();

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

        /// <summary>
        /// Includes invisible portals.
        /// </summary>
        public event PlayerEvent OnHitPortal = delegate { };

        public event PlayerEvent OnHitRedKey = delegate { };
        public event PlayerEvent OnHitBlueKey = delegate { };
        public event PlayerEvent OnHitGreenKey = delegate { };
        public event PlayerEvent OnHitCyanKey = delegate { };
        public event PlayerEvent OnHitMagentaKey = delegate { };
        public event PlayerEvent OnHitYellowKey = delegate { };

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
            Id = id;
            Name = name;
            X = 16;
            Y = 16;
            gravity = (int)PhysicsConfig.Gravity;
            Switches = new BitArray(100);
        }

        internal void Tick()
        {
            double tx;
            double ty;
            bool spacedown = SpaceDown;

            if (IsDead)
            {
                if (IsMe && HostWorld.Connected)
                {
                    deathOffset += 0.3f;
                    if (deathOffset >= 16.0f)
                    {
                        HostWorld.Connection.Send("death");
                        deathOffset = 0;
                    }
                }
            }

            int cx = ((int)(X + 8) >> 4);
            int cy = ((int)(Y + 8) >> 4);

            var delayed = queue.Count > 0 ? queue.Dequeue() : 0;
            current = HostWorld.GetBlock(0, cx, cy);

            if (ItemId.IsHalfBlock(current))
            {
                var rot = HostWorld.GetBlockData(cx, cy)[0];

                if (rot == 1) cy -= 1;
                if (rot == 0) cx -= 1;

                current = HostWorld.GetBlock(0, cx, cy);
            }

            if (this.tx != -1) UpdateTeamDoors(this.tx, this.ty);

            currentBelow = 0;
            if (current == 1 || current == 411) currentBelow = HostWorld.GetBlock(0, cx - 1, cy);
            else if (current == 2 || current == 412) currentBelow = HostWorld.GetBlock(0, cx, cy - 1);
            else if (current == 3 || current == 413) currentBelow = HostWorld.GetBlock(0, cx + 1, cy);
            else currentBelow = HostWorld.GetBlock(0, cx, cy + 1);

            queue.Enqueue(current);
            if (current == 4 || current == 414 || ItemId.IsClimbable(current))
            {
                delayed = queue.Dequeue();
                queue.Enqueue(current);
            }

            // not needed, client side only
            /*while (tileQueue.Count > 0)
            {
                UpdatePurpleSwitches(tileQueue.Dequeue());
            }*/

            // TODO: Change fire to effect
            if (isOnFire && !isInvulnerable)
            {
                if (onFireDeath <= 0)
                {
                    onFireDeath = 200;
                    KillPlayer();
                }
                else
                {
                    onFireDeath--;
                }
            }
            else
            {
                onFireDeath = 200;
            }

            if (IsDead)
            {
                Horizontal = 0;
                Vertical = 0;
                spacedown = false;
                JustSpaceDown = false;
            }

            bool isGodMode = InGodMode;
            if (InGodMode)
            {
                morx = 0;
                mory = 0;
                mox = 0;
                moy = 0;
            }
            else
            {
                if (ItemId.IsClimbable(current))
                {
                    morx = 0;
                    mory = 0;
                }
                else
                {
                    switch (current)
                    {
                        case 1:
                        case ItemId.InvisibleLeftArrow:
                            morx = -gravity;
                            mory = 0;
                            break;
                        case 2:
                        case ItemId.InvisibleUpArrow:
                            morx = 0;
                            mory = -gravity;
                            break;
                        case 3:
                        case ItemId.InvisibleRightArrow:
                            morx = gravity;
                            mory = 0;
                            break;
                        case ItemId.SpeedLeft:
                        case ItemId.SpeedRight:
                        case ItemId.SpeedUp:
                        case ItemId.SpeedDown:
                        case ItemId.InvisibleDot:
                        case 4:
                            morx = 0;
                            mory = 0;
                            break;
                        case ItemId.Water:
                            morx = 0;
                            mory = (int)PhysicsConfig.WaterBuoyancy;
                            if (isOnFire)
                            {
                                isOnFire = false;
                            }
                            break;
                        case ItemId.Mud:
                            morx = 0;
                            mory = (int)PhysicsConfig.MudBuoyancy;
                            if (isOnFire)
                            {
                                isOnFire = false;
                            }
                            break;
                        case ItemId.Lava:
                            morx = 0;
                            mory = (int)PhysicsConfig.LavaBuoyancy;
                            if (!isOnFire && !isInvulnerable)
                            {
                                isOnFire = true;
                            }
                            break;
                        case ItemId.Fire:
                        case ItemId.Spike:
                            morx = 0;
                            mory = gravity;
                            if (!IsDead && !isInvulnerable) KillPlayer();
                            break;
                        case ItemId.EffectProtection:
                            morx = 0;
                            mory = gravity;
                            if (HostWorld.GetOnStatus(cx, cy) && isOnFire)
                            {
                                Zombie = false;
                                isOnFire = false;
                                CursedEffect = false;
                            }
                            break;
                        default:
                            morx = 0;
                            mory = gravity;
                            break;
                    }
                }

                if (ItemId.IsClimbable(delayed))
                {
                    mox = 0;
                    moy = 0;
                }
                else
                {
                    switch (delayed)
                    {
                        case 1:
                        case ItemId.InvisibleLeftArrow:
                            mox = -gravity;
                            moy = 0;
                            break;
                        case 2:
                        case ItemId.InvisibleUpArrow:
                            mox = 0;
                            moy = -gravity;
                            break;
                        case 3:
                        case ItemId.InvisibleRightArrow:
                            mox = gravity;
                            moy = 0;
                            break;
                        case ItemId.SpeedLeft:
                        case ItemId.SpeedRight:
                        case ItemId.SpeedUp:
                        case ItemId.SpeedDown:
                        case ItemId.InvisibleDot:
                        case 4:
                            mox = 0;
                            moy = 0;
                            break;
                        case ItemId.Water:
                            mox = 0;
                            moy = PhysicsConfig.WaterBuoyancy;
                            break;
                        case ItemId.Mud:
                            mox = 0;
                            moy = PhysicsConfig.MudBuoyancy;
                            break;
                        case ItemId.Lava:
                            mox = 0;
                            moy = PhysicsConfig.LavaBuoyancy;
                            break;
                        default:
                            mox = 0;
                            moy = gravity;
                            break;
                    }
                }
            }

            if (moy == PhysicsConfig.WaterBuoyancy || moy == PhysicsConfig.MudBuoyancy || moy == PhysicsConfig.LavaBuoyancy)
            {
                mx = Horizontal;
                my = Vertical;
            }
            else if (moy != 0)
            {
                mx = Horizontal;
                my = 0;
            }
            else if (mox != 0)
            {
                mx = 0;
                my = Vertical;
            }
            else
            {
                mx = Horizontal;
                my = Vertical;
            }

            mx *= SpeedMultiplier;
            my *= SpeedMultiplier;
            mox *= GravityMultiplier;
            moy *= GravityMultiplier;

            ModifierX = (mox + mx);
            ModifierY = (moy + my);

            if (!DoubleIsEqual(speedX, 0) || !DoubleIsEqual(modifierX, 0))
            {
                speedX = (speedX + modifierX);
                speedX = (speedX * PhysicsConfig.BaseDrag);
                if (!isGodMode)
                {
                    if ((mx == 0 && moy != 0) || (speedX < 0 && mx > 0) || (speedX > 0 && mx < 0) || ItemId.IsClimbable(current))
                    {
                        speedX = (speedX * PhysicsConfig.NoModifierDrag);
                    }
                    else if (current == ItemId.Water)
                    {
                        speedX = (speedX * PhysicsConfig.WaterDrag);
                    }
                    else if (current == ItemId.Mud)
                    {
                        speedX = (speedX * PhysicsConfig.MudDrag);
                    }
                    else if (current == ItemId.Lava)
                    {
                        speedX = (speedX * PhysicsConfig.LavaDrag);
                    }
                }

                if (speedX > 16)
                {
                    speedX = 16;
                }
                else if (speedX < -16)
                {
                    speedX = -16;
                }
                else if (speedX < 0.0001 && speedX > -0.0001)
                {
                    speedX = 0;
                }
            }
            if (!DoubleIsEqual(speedY, 0) || !DoubleIsEqual(modifierY, 0))
            {
                speedY = (speedY + modifierY);
                speedY = (speedY * PhysicsConfig.BaseDrag);
                if (!isGodMode)
                {
                    if ((my == 0 && mox != 0) || (speedY < 0 && my > 0) || (speedY > 0 && my < 0) || ItemId.IsClimbable(current))
                    {
                        speedY = (speedY * PhysicsConfig.NoModifierDrag);
                    }
                    else if (current == ItemId.Water)
                    {
                        speedY = (speedY * PhysicsConfig.WaterDrag);
                    }
                    else if (current == ItemId.Mud)
                    {
                        speedY = (speedY * PhysicsConfig.MudDrag);
                    }
                    else if (current == ItemId.Lava)
                    {
                        speedY = (speedY * PhysicsConfig.LavaDrag);
                    }
                }

                if (speedY > 16)
                {
                    speedY = 16;
                }
                else if (speedY < -16)
                {
                    speedY = -16;
                }
                else if (speedY < 0.0001 && speedY > -0.0001)
                {
                    speedY = 0;
                }
            }

            if (!isGodMode)
            {
                switch (current)
                {
                    case ItemId.SpeedLeft: speedX = -PhysicsConfig.Boost; break;
                    case ItemId.SpeedRight: speedX = PhysicsConfig.Boost; break;
                    case ItemId.SpeedUp: speedY = -PhysicsConfig.Boost; break;
                    case ItemId.SpeedDown: speedY = PhysicsConfig.Boost; break;
                }
            }

            var reminderX = X % 1;
            var currentSx = speedX;
            var reminderY = Y % 1;
            var currentSy = speedY;
            donex = false;
            doney = false;

            while ((currentSx != 0 && !donex) || (currentSy != 0 && !doney))
            {
                #region processPortals()
                current = HostWorld.GetBlock(cx, cy);
                if (!isGodMode && (current == ItemId.Portal || current == ItemId.PortalInvisible))
                {
                    if (!hasLastPortal)
                    {
                        OnHitPortal(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                        lastPortal = new Point(cx, cy);
                        hasLastPortal = true;
                        int[] data = HostWorld.GetBlockData(cx, cy);
                        if (data != null && data.Length == 3)
                        {
                            Point portalPoint;
                            if (data[2] != data[1] && // target != itself
                                HostWorld.TryGetPortalById(data[2], out portalPoint))
                            {
                                int[] data2 = HostWorld.GetBlockData(lastPortal.x, lastPortal.y);
                                int[] data3 = HostWorld.GetBlockData(portalPoint.x, portalPoint.y);
                                if (data2 != null && data2.Length == 3 &&
                                    data3 != null && data3.Length == 3)
                                {
                                    int rot1 = data2[0];
                                    int rot2 = data3[0];
                                    if (rot1 < rot2)
                                    {
                                        rot1 += 4;
                                    }
                                    switch (rot1 - rot2)
                                    {
                                        case 1:
                                            SpeedX = SpeedY * PortalMultiplier;
                                            SpeedY = -SpeedX * PortalMultiplier;
                                            ModifierX = ModifierY * PortalMultiplier;
                                            ModifierY = -ModifierX * PortalMultiplier;
                                            reminderY = -reminderY;
                                            currentSy = -currentSy;
                                            break;
                                        case 2:
                                            SpeedX = -SpeedX * PortalMultiplier;
                                            SpeedY = -SpeedY * PortalMultiplier;
                                            ModifierX = -ModifierX * PortalMultiplier;
                                            ModifierY = -ModifierY * PortalMultiplier;
                                            reminderY = -reminderY;
                                            currentSy = -currentSy;
                                            reminderX = -reminderX;
                                            currentSx = -currentSx;
                                            break;
                                        case 3:
                                            SpeedX = -SpeedY * PortalMultiplier;
                                            SpeedY = SpeedX * PortalMultiplier;
                                            ModifierX = -ModifierY * PortalMultiplier;
                                            ModifierY = ModifierX * PortalMultiplier;
                                            reminderX = -reminderX;
                                            currentSx = -currentSx;
                                            break;
                                    }
                                    X = portalPoint.x * 16;
                                    Y = portalPoint.y * 16;
                                    lastPortal = portalPoint;
                                }
                            }
                        }
                    }
                }
                else
                {
                    hasLastPortal = false;
                }
                #endregion

                var ox = X;
                var oy = Y;
                var osx = currentSx;
                var osy = currentSy;

                #region stepX()
                if (currentSx > 0)
                {
                    if ((currentSx + reminderX) >= 1)
                    {
                        X += 1 - reminderX;
                        X = Math.Floor(X);
                        currentSx -= 1 - reminderX;
                        reminderX = 0;
                    }
                    else
                    {
                        X += currentSx;
                        currentSx = 0;
                    }
                }
                else
                {
                    if (currentSx < 0)
                    {
                        if (!DoubleIsEqual(reminderX, 0) && (reminderX + currentSx) < 0)
                        {
                            currentSx += reminderX;
                            X -= reminderX;
                            X = Math.Floor(X);
                            reminderX = 1;
                        }
                        else
                        {
                            X += currentSx;
                            currentSx = 0;
                        }
                    }
                }
                if (HostWorld.Overlaps(this))
                {
                    X = ox;
                    speedX = 0;
                    currentSx = osx;
                    donex = true;
                }
                #endregion

                #region stepY()
                if (currentSy > 0)
                {
                    if ((currentSy + reminderY) >= 1)
                    {
                        Y += 1 - reminderY;
                        Y = Math.Floor(Y);
                        currentSy -= 1 - reminderY;
                        reminderY = 0;
                    }
                    else
                    {
                        Y += currentSy;
                        currentSy = 0;
                    }
                }
                else
                {
                    if (currentSy < 0)
                    {
                        if (!DoubleIsEqual(reminderY, 0) && (reminderY + currentSy) < 0)
                        {
                            Y -= reminderY;
                            Y = Math.Floor(Y);
                            currentSy += reminderY;
                            reminderY = 1;
                        }
                        else
                        {
                            Y += currentSy;
                            currentSy = 0;
                        }
                    }
                }
                if (HostWorld.Overlaps(this))
                {
                    Y = oy;
                    speedY = 0;
                    currentSy = osy;
                    doney = true;
                }
                #endregion
            }


            if (!IsDead)
            {
                if (IsMe)
                {
                    int mod = 1;
                    bool injump = false;
                    bool changed = false;
                    if (JustSpaceDown)
                    {
                        lastJump = -HostWorld.Sw.ElapsedMilliseconds;
                        injump = true;
                        mod = -1;
                    }
                    if (SpaceDown)
                    {
                        if (HasLevitation)
                        {
                            if (IsThrusting)
                            {
                                changed = true;
                            }
                            IsThrusting = true;
                            ApplyThrust();
                        }
                        else if (lastJump < 0)
                        {
                            if (HostWorld.Sw.ElapsedMilliseconds + lastJump > 750)
                            {
                                injump = true;
                            }
                        }
                        else if (HostWorld.Sw.ElapsedMilliseconds - lastJump > 150)
                        {
                            injump = true;
                        }
                    }
                    else if (HasLevitation)
                    {
                        if (IsThrusting)
                        {
                            changed = true;
                        }
                        IsThrusting = true;
                    }
                    if (injump && !HasLevitation)
                    {
                        if (SpeedX == 0 && morx != 0 && mox != 0 && (X % 16 == 0 || X % 8 == 0))
                        {
                            SpeedX -= morx * PhysicsConfig.JumpHeight * JumpMultiplier;
                            changed = true;
                            lastJump = HostWorld.Sw.ElapsedMilliseconds * mod;
                        }
                        if (SpeedY == 0 && mory != 0 && moy != 0 && (Y % 16 == 0 || Y % 8 == 0))
                        {
                            SpeedY -= mory * PhysicsConfig.JumpHeight * JumpMultiplier;
                            changed = true;
                            lastJump = HostWorld.Sw.ElapsedMilliseconds * mod;
                        }
                    }
                    if (changed || oh != Horizontal || ov != Vertical)
                    {
                        oh = Horizontal;
                        ov = Vertical;
                        HostWorld.Connection.Send("m", X, Y, SpeedX, SpeedY, (int)ModifierX, (int)ModifierY,
                            Horizontal, Vertical, GravityMultiplier, spacedown, JustSpaceDown, TickId);
                    }
                    TickId++;
                    JustSpaceDown = false;
                }
                if (pastx != cx || pasty != cy)
                {
                    PlayerEvent e;
                    if (blockIdEvents.Count != 0 && blockIdEvents.TryGetValue(current, out e))
                    {
                        e(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                    }
                    if (bgblockIdEvents.Count != 0 && bgblockIdEvents.TryGetValue(HostWorld.GetBlock(1, cx, cy), out e))
                    {
                        e(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                    }

                    // Might remove specific events soon, because you can make them now with void AddBlockEvent. (except OnGetCoin and OnGetBlueCoin)
                    switch (current)
                    {
                        case 100:   //coin
                            OnHitCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < gotCoins.Count; i++)
                            {
                                if (gotCoins[i].x == cx && gotCoins[i].y == cy)
                                {
                                    goto found;
                                }
                            }
                            OnGetCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            gotCoins.Add(new Point(cx, cy));
                            if (IsMe && HostWorld.Connected)
                            {
                                HostWorld.Connection.Send("c", ++Coins, BlueCoins, cx, cy);
                            }
                            found:
                            break;
                        case 101:   // bluecoin
                            OnHitBlueCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < gotBlueCoins.Count; i++)
                            {
                                if (gotBlueCoins[i].x == cx && gotBlueCoins[i].y == cy)
                                {
                                    goto found2;
                                }
                            }
                            OnGetBlueCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            gotBlueCoins.Add(new Point(cx, cy));
                            if (IsMe && HostWorld.Connected)
                            {
                                HostWorld.Connection.Send("c", Coins, ++BlueCoins, cx, cy);
                            }
                            found2:
                            break;
                        case 5:
                            // crown
                            OnHitCrown(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode && !HasCrown)
                            {
                                HostWorld.Connection.Send("crown", cx, cy);
                            }
                            break;
                        case 6:
                            // red key
                            OnHitRedKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "red");
                            }
                            break;
                        case 7:
                            // green key
                            OnHitGreenKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "green");
                            }
                            break;
                        case 8:
                            // blue key
                            OnHitBlueKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "blue");
                            }
                            break;
                        case ItemId.CyanKey:
                            OnHitCyanKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "cyan");
                            }
                            break;
                        case ItemId.MagentaKey:
                            OnHitMagentaKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "magenta");
                            }
                            break;
                        case ItemId.YellowKey:
                            OnHitYellowKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("pressKey", cx, cy, "yellow");
                            }
                            break;
                        case ItemId.SwitchPurple:
                            int sid = HostWorld.GetBlockData(cx, cy)[0];
                            UpdatePurpleSwitches(sid);
                            OnHitSwitch(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.Piano:
                            OnHitPiano(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.Drum:
                            OnHitDrum(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.Diamond:
                            OnHitDiamond(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("diamondtouch", cx, cy);
                            }
                            break;
                        case ItemId.Cake:
                            OnHitCake(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("caketouch", cx, cy);
                            }
                            break;
                        case ItemId.Hologram:
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("hologramtouch", cx, cy);
                            }
                            break;
                        case ItemId.Checkpoint:
                            if (!isGodMode)
                            {
                                LastCheckpointX = cx;
                                LastCheckpointY = cy;
                                OnHitCheckpoint(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                                if (IsMe && HostWorld.Connected)
                                {
                                    HostWorld.Connection.Send("checkpoint", cx, cy);
                                }
                            }
                            break;
                        case ItemId.BrickComplete:
                            OnHitCompleteLevelBrick(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            if (IsMe && HostWorld.Connected && !InGodMode)
                            {
                                HostWorld.Connection.Send("levelcomplete", cx, cy);
                            }
                            break;

                        case ItemId.EffectProtection:
                            if (!InGodMode)
                            {
                                var status = HostWorld.GetOnStatus(cx, cy);
                                if (isInvulnerable != status)
                                {
                                    isInvulnerable = status;
                                    if (isInvulnerable)
                                    {
                                        Zombie = false;
                                        isOnFire = false;
                                        CursedEffect = false;
                                    }
                                    if (IsMe && HostWorld.Connected && !InGodMode) HostWorld.Connection.Send("effect", cx, cy, PhysicsConfig.EffectProtection);
                                }
                            }
                            break;

                    }
                    pastx = cx;
                    pasty = cy;
                }
                if (touchBlockEvents.Count > 0)
                {
                    PlayerEvent e;
                    Point p;
                    if (oldX != X || oldY != Y)
                    {
                        for (int i = 0; i < touchedPoints.Count; i++)
                        {
                            if (touchedPoints[i].y == cy)
                            {
                                if (X % 16 == 0 && (touchedPoints[i].x == cx - 1 || touchedPoints[i].x == cx + 1) && touchedPoints[i].y == cy)
                                {

                                }
                                else
                                {
                                    touchedPoints.RemoveAt(i--);
                                }
                            }
                            else if (touchedPoints[i].x == cx)
                            {
                                if (Y % 16 == 0 && (touchedPoints[i].y == cy - 1 || touchedPoints[i].y == cy + 1) && touchedPoints[i].x == cx)
                                {

                                }
                                else
                                {
                                    touchedPoints.RemoveAt(i--);
                                }
                            }
                            else
                            {
                                touchedPoints.RemoveAt(i--);
                            }
                        }
                        if (X % 16 == 0)
                        {
                            p = new Point(cx - 1, cy);
                            if (ItemId.IsSolid(HostWorld.GetBlock(0, p.x, p.y)) && touchBlockEvents.TryGetValue(HostWorld.GetBlock(0, p.x, p.y), out e))
                            {
                                if (!touchedPoints.Contains(p))
                                {
                                    touchedPoints.Add(p);
                                    e(new PlayerEventArgs { Player = this, BlockX = p.x, BlockY = p.y });
                                }
                            }
                            p = new Point(cx + 1, cy);
                            if (ItemId.IsSolid(HostWorld.GetBlock(0, p.x, p.y)) && touchBlockEvents.TryGetValue(HostWorld.GetBlock(0, p.x, p.y), out e))
                            {
                                if (!touchedPoints.Contains(p))
                                {
                                    touchedPoints.Add(p);
                                    e(new PlayerEventArgs { Player = this, BlockX = p.x, BlockY = p.y });
                                }
                            }
                        }
                        if (DoubleIsEqual(Y % 16, 0))
                        {
                            p = new Point(cx, cy - 1);
                            if (ItemId.IsSolid(HostWorld.GetBlock(0, p.x, p.y)) && touchBlockEvents.TryGetValue(HostWorld.GetBlock(0, p.x, p.y), out e))
                            {
                                if (!touchedPoints.Contains(p))
                                {
                                    touchedPoints.Add(p);
                                    e(new PlayerEventArgs { Player = this, BlockX = p.x, BlockY = p.y });
                                }
                            }
                            p = new Point(cx, cy + 1);
                            if (ItemId.IsSolid(HostWorld.GetBlock(0, p.x, p.y)) && touchBlockEvents.TryGetValue(HostWorld.GetBlock(0, p.x, p.y), out e))
                            {
                                if (!touchedPoints.Contains(p))
                                {
                                    touchedPoints.Add(p);
                                    e(new PlayerEventArgs { Player = this, BlockX = p.x, BlockY = p.y });
                                }
                            }
                        }
                    }
                }
            }

            if (HasLevitation)
            {
                UpdateThrust();
            }

            int imx = ((int)speedX << 8);
            int imy = ((int)speedX << 8);
            if (imx != 0 || ((current == ItemId.Water || current == ItemId.Mud || current == ItemId.Lava) && !InGodMode))
            {

            }
            else if (modifierY < 0.1 && modifierX > -0.1)
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
                        X -= tx / 15;
                    }
                }
                else
                {
                    if (tx > 14)
                    {
                        if (tx > 15.8)
                        {
                            X = Math.Floor(X);
                            X++;
                        }
                        else
                        {
                            X += (tx - 14) / 15;
                        }
                    }
                }
            }
            if (imx != 0 || ((current == ItemId.Water || current == ItemId.Mud || current == ItemId.Lava) && !InGodMode))
            {

            }
            else if (modifierY < 0.1 && modifierY > -0.1)
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
                        Y -= ty / 15;
                    }
                }
                else
                {
                    if (ty > 14)
                    {
                        if (ty > 15.8)
                        {
                            Y = Math.Floor(Y);
                            Y++;
                        }
                        else
                        {
                            Y += (ty - 14) / 15;
                        }
                    }
                }
            }

            oldX = X;
            oldY = Y;
        }

        /// <summary>
        /// Set horizontal movement direction of the bot. Allowed only for the bot player. Allowed only for the bot player. Also you have to initialize PhysicsWorld with the PlayerIO Connection.
        /// </summary>
        /// <param name="horizontal">-1 = left, 1 = right</param>
        public void SetHorizontal(int horizontal)
        {
            if (!IsMe) throw new Exception("Allowed only for the bot player.");
            if (!HostWorld.Connected) throw new Exception("EEPhysics needs connection to move the bot. Make sure you initialized PhysicsWorld with PlayerIO Connection.");

            Horizontal = horizontal;
        }
        /// <summary>
        /// Set horizontal movement direction of the bot. Allowed only for the bot player. Also you have to initialize PhysicsWorld with the PlayerIO Connection.
        /// </summary>
        /// <param name="vertical">-1 = up, 1 = down</param>
        public void SetVertical(int vertical)
        {
            if (!IsMe) throw new Exception("Allowed only for the bot player.");
            if (!HostWorld.Connected) throw new Exception("EEPhysics needs connection to move the bot. Make sure you initialized PhysicsWorld with PlayerIO Connection.");

            Vertical = vertical;
        }

        /// <summary>
        /// Makes PhysicsPlayer raise event when player moves inside blockId block. Event is not raised every tick, but only at when player first touches the block.
        /// (Touching doesn't count! Only the block that is at center of player is checked, no multiple blocks at same time.)
        /// </summary>
        /// <param name="blockId">Block ID to check for.</param>
        /// <param name="e">Method which is run when event occurs.</param>
        public void AddBlockEvent(int blockId, PlayerEvent e)
        {
            if (!ItemId.IsBackground(blockId)) blockIdEvents[blockId] = e;
            else bgblockIdEvents[blockId] = e;
        }
        /// <returns>Whether there's block event with specified blockId.</returns>
        public bool HasBlockEvent(int blockId)
        {
            if (!ItemId.IsBackground(blockId)) return blockIdEvents.ContainsKey(blockId);
            return bgblockIdEvents.ContainsKey(blockId);
        }

        /// <summary>
        /// Removes block event added with AddBlockEvent with specified blockId.
        /// </summary>
        public void RemoveBlockEvent(int blockId)
        {
            if (!ItemId.IsBackground(blockId)) blockIdEvents.Remove(blockId);
            else bgblockIdEvents.Remove(blockId);
        }

        public bool GetSwitchState(int switchId)
        {
            return Switches[switchId];
        }

        /// <summary>
        /// Makes PhysicsPlayer raise event when player touches blockId block (doesn't need to overlap). Event is not raised every tick, but at least when player first touches the block. It is possible that it's raised multiple times for same block in some cases.
        /// </summary>
        /// <param name="blockId">Block ID to check for.</param>
        /// <param name="e">Method which is run when event occurs.</param>
        public void AddBlockTouchEvent(int blockId, PlayerEvent e)
        {
            touchBlockEvents[blockId] = e;
        }
        /// <returns>Whether there's block event with specified blockId.</returns>
        public bool HasBlockTouchEvent(int blockId)
        {
            return touchBlockEvents.ContainsKey(blockId);
        }
        /// <summary>
        /// Removes block event added with AddBlockTouchEvent with specified blockId.
        /// </summary>
        public void RemoveBlockTouchEvent(int blockId)
        {
            touchBlockEvents.Remove(blockId);
        }

        /// <returns>True if player overlaps block at x,y.</returns>
        public bool OverlapsTile(int tx, int ty)
        {
            int xx = tx * 16;
            int yy = ty * 16;
            return ((X > xx - 16 && X <= xx + 16) && (Y > yy - 16 && Y <= yy + 16));
        }

        internal void Respawn()
        {
            ModifierX = 0;
            ModifierY = 0;
            SpeedX = 0;
            SpeedY = 0;
            IsDead = false;
        }
        internal void KillPlayer()
        {
            deathOffset = 0;
            Deaths++;
            IsDead = true;
            isOnFire = false;
            onFireDeath = 200;
            OnDie(new PlayerEventArgs { Player = this, BlockX = ((int)(X + 8) >> 4), BlockY = ((int)(Y + 8) >> 4) });
        }
        internal void Reset()
        {
            gotCoins.Clear();
            gotBlueCoins.Clear();
            BlueCoins = 0;
            Coins = 0;
            Deaths = 0;
        }
        internal void RemoveCoin(int xx, int yy)
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
        internal void RemoveBlueCoin(int xx, int yy)
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
        internal void SetEffect(int effectId, bool active)
        {
            switch (effectId)
            {
                case PhysicsConfig.EffectJump: JumpBoostEffect = active; break;
                case PhysicsConfig.EffectFly: HasLevitation = active; break;
                case PhysicsConfig.EffectRun: SpeedBoostEffect = active; break;
                case PhysicsConfig.EffectProtection: isInvulnerable = active; break;
                case PhysicsConfig.EffectCurse: CursedEffect = active; break;
                case PhysicsConfig.EffectZombie: Zombie = active; break;
            }
        }

        internal void UpdatePurpleSwitches(int id)
        {
            Switches[id] = !Switches[id];
            if (HostWorld.Overlaps(this))
            {
                Switches[id] = !Switches[id];
                tileQueue.Enqueue(id);
            }
        }
        internal void UpdateTeamDoors(int x, int y)
        {
            int data = HostWorld.GetBlockData(x, y)[0];
            int team = Team;
            if (Team != data)
            {
                Team = data;
                if (!HostWorld.Overlaps(this))
                {
                    tx = -1;
                    ty = -1;
                }
                else
                {
                    Team = team;
                    tx = x;
                    ty = y;
                }
            }
        }
        internal void UpdateThrust()
        {
            if (mory != 0) SpeedY = SpeedY - currentThrust * PhysicsConfig.JumpHeight / 2 * mory * 0.5;
            if (morx != 0) SpeedX = SpeedX - currentThrust * PhysicsConfig.JumpHeight / 2 * morx * 0.5;

            if (!IsThrusting)
            {
                if (currentThrust > 0) currentThrust = currentThrust - thrustBurnOff;
                else currentThrust = 0;
            }
        }
        public void ApplyThrust() { currentThrust = MaxThrust; }

        // this is used because: http://stackoverflow.com/questions/3103782/rule-of-thumb-to-test-the-equality-of-two-doubles-in-c
        internal bool DoubleIsEqual(double d1, double d2) { return Math.Abs(d1 - d2) < 0.00000001; }
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

    internal struct Point
    {
        public int x, y;
        public Point(int xx, int yy)
        {
            x = xx;
            y = yy;
        }

        public override bool Equals(object o)
        {
            if (o is Point) return (x == ((Point)o).x && y == ((Point)o).y);
            return base.Equals(o);
        }

        public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode(); }

        public static bool operator ==(Point p, Point p2) { return p.Equals(p2); }
        public static bool operator !=(Point p, Point p2) { return !p.Equals(p2); }
    }
}
