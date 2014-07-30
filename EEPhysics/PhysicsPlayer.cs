// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhysicsPlayer.cs" company="">
//   
// </copyright>
// <summary>
//   The physics player.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EEPhysics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The physics player.
    /// </summary>
    public class PhysicsPlayer
    {
        #region Constants

        /// <summary>
        /// The height.
        /// </summary>
        internal const int Height = 16;

        /// <summary>
        /// The width.
        /// </summary>
        internal const int Width = 16;

        #endregion

        #region Fields

        /// <summary>
        /// The overlapy.
        /// </summary>
        internal int overlapy;

        /// <summary>
        /// The got blue coins.
        /// </summary>
        private readonly List<Point> gotBlueCoins = new List<Point>();

        /// <summary>
        /// The got coins.
        /// </summary>
        private readonly List<Point> gotCoins = new List<Point>();

        /// <summary>
        /// The gravity.
        /// </summary>
        private readonly int gravity;

        /// <summary>
        /// The queue.
        /// </summary>
        private readonly int[] queue = new int[PhysicsConfig.QueueLength];

        // private int lastTile;
        /// <summary>
        /// The current.
        /// </summary>
        private int current;

        /// <summary>
        /// The delayed.
        /// </summary>
        private int delayed;

        /// <summary>
        /// The donex.
        /// </summary>
        private bool donex;

        /// <summary>
        /// The doney.
        /// </summary>
        private bool doney;

        /// <summary>
        /// The is invulnerable.
        /// </summary>
        private bool isInvulnerable;

        /// <summary>
        /// The last portal.
        /// </summary>
        private Point lastPortal;

        /// <summary>
        /// The modifier x.
        /// </summary>
        private double modifierX;

        /// <summary>
        /// The modifier y.
        /// </summary>
        private double modifierY;

        /// <summary>
        /// The morx.
        /// </summary>
        private int morx;

        /// <summary>
        /// The mory.
        /// </summary>
        private int mory;

        /// <summary>
        /// The mox.
        /// </summary>
        private double mox;

        /// <summary>
        /// The moy.
        /// </summary>
        private double moy;

        /// <summary>
        /// The mx.
        /// </summary>
        private double mx;

        /// <summary>
        /// The my.
        /// </summary>
        private double my;

        /// <summary>
        /// The pastx.
        /// </summary>
        private int pastx;

        /// <summary>
        /// The pasty.
        /// </summary>
        private int pasty;

        /// <summary>
        /// The speed x.
        /// </summary>
        private double speedX;

        /// <summary>
        /// The speed y.
        /// </summary>
        private double speedY;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsPlayer"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public PhysicsPlayer(int id, string name)
        {
            this.ID = id;
            this.Name = name;
            this.X = 16;
            this.Y = 16;
            this.gravity = (int)PhysicsConfig.Gravity;
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The player event.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        public delegate void PlayerEvent(PlayerEventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        /// The on die.
        /// </summary>
        public event PlayerEvent OnDie = delegate { };

        /// <summary>
        ///     Will be called only when player hits a blue coin first time. After first time, only event OnHitBlueCoin will be
        ///     called.
        /// </summary>
        public event PlayerEvent OnGetBlueCoin = delegate { };

        /// <summary>
        ///     Will be called only when player hits a coin first time. After first time, only event OnHitCoin will be called.
        /// </summary>
        public event PlayerEvent OnGetCoin = delegate { };

        /// <summary>
        ///     Note: This will be called every time player hits blue coin, even if the coin is already got by that player. If you
        ///     want to get only first time coin is hit, use event OnGetBlueCoin.
        /// </summary>
        public event PlayerEvent OnHitBlueCoin = delegate { };

        /// <summary>
        /// The on hit blue key.
        /// </summary>
        public event PlayerEvent OnHitBlueKey = delegate { };

        /// <summary>
        /// The on hit cake.
        /// </summary>
        public event PlayerEvent OnHitCake = delegate { };

        /// <summary>
        /// The on hit checkpoint.
        /// </summary>
        public event PlayerEvent OnHitCheckpoint = delegate { };

        /// <summary>
        ///     Note: This will be called every time player hits coin, even if the coin is already got by that player. If you want
        ///     to get only first time coin is hit, use event OnGetCoin.
        /// </summary>
        public event PlayerEvent OnHitCoin = delegate { };

        /// <summary>
        /// The on hit complete level brick.
        /// </summary>
        public event PlayerEvent OnHitCompleteLevelBrick = delegate { };

        /// <summary>
        /// The on hit crown.
        /// </summary>
        public event PlayerEvent OnHitCrown = delegate { };

        /// <summary>
        /// The on hit diamond.
        /// </summary>
        public event PlayerEvent OnHitDiamond = delegate { };

        /// <summary>
        /// The on hit drum.
        /// </summary>
        public event PlayerEvent OnHitDrum = delegate { };

        /// <summary>
        /// The on hit green key.
        /// </summary>
        public event PlayerEvent OnHitGreenKey = delegate { };

        /// <summary>
        /// The on hit piano.
        /// </summary>
        public event PlayerEvent OnHitPiano = delegate { };

        /// <summary>
        /// The on hit red key.
        /// </summary>
        public event PlayerEvent OnHitRedKey = delegate { };

        /// <summary>
        /// The on hit switch.
        /// </summary>
        public event PlayerEvent OnHitSwitch = delegate { };

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the blue coins.
        /// </summary>
        public int BlueCoins
        {
            get
            {
                return this.gotBlueCoins.Count;
            }
        }

        /// <summary>
        /// Gets or sets the coins.
        /// </summary>
        public int Coins { get; set; }

        /// <summary>
        /// Gets the horizontal.
        /// </summary>
        public int Horizontal { get; internal set; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>Also includes moderatormode.</summary>
        public bool InGodMode { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether is club member.
        /// </summary>
        public bool IsClubMember { get; set; }

        /// <summary>
        /// Gets a value indicating whether is dead.
        /// </summary>
        public bool IsDead { get; internal set; }

        /// <summary>
        /// Gets the last checkpoint x.
        /// </summary>
        public int LastCheckpointX { get; private set; }

        /// <summary>
        /// Gets the last checkpoint y.
        /// </summary>
        public int LastCheckpointY { get; private set; }

        /// <summary>
        /// Gets the modifier x.
        /// </summary>
        public double ModifierX
        {
            get
            {
                return this.modifierX * PhysicsConfig.VariableMultiplier;
            }

            internal set
            {
                this.modifierX = value / PhysicsConfig.VariableMultiplier;
            }
        }

        /// <summary>
        /// Gets the modifier y.
        /// </summary>
        public double ModifierY
        {
            get
            {
                return this.modifierY * PhysicsConfig.VariableMultiplier;
            }

            internal set
            {
                this.modifierY = value / PhysicsConfig.VariableMultiplier;
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>Purple switch state.</summary>
        public bool Purple { get; internal set; }

        /// <summary>
        /// Gets the speed x.
        /// </summary>
        public double SpeedX
        {
            get
            {
                return this.speedX * PhysicsConfig.VariableMultiplier;
            }

            internal set
            {
                this.speedX = value / PhysicsConfig.VariableMultiplier;
            }
        }

        /// <summary>
        /// Gets the speed y.
        /// </summary>
        public double SpeedY
        {
            get
            {
                return this.speedY * PhysicsConfig.VariableMultiplier;
            }

            internal set
            {
                this.speedY = value / PhysicsConfig.VariableMultiplier;
            }
        }

        /// <summary>
        /// Gets the vertical.
        /// </summary>
        public int Vertical { get; internal set; }

        /// <summary>
        /// Gets the x.
        /// </summary>
        public double X { get; internal set; }

        /// <summary>
        /// Gets the y.
        /// </summary>
        public double Y { get; internal set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the gravity multiplier.
        /// </summary>
        internal double GravityMultiplier
        {
            get
            {
                return this.HostWorld.WorldGravity;
            }
        }

        /// <summary>
        /// Gets or sets the host world.
        /// </summary>
        internal PhysicsWorld HostWorld { get; set; }

        /// <summary>
        /// Gets the speed multiplier.
        /// </summary>
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

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The overlaps tile.
        /// </summary>
        /// <param name="tx">
        /// The tx.
        /// </param>
        /// <param name="ty">
        /// The ty.
        /// </param>
        /// <returns>
        /// True if player overlaps block at x,y.
        /// </returns>
        public bool OverlapsTile(int tx, int ty)
        {
            int xx = tx * 16;
            int yy = ty * 16;
            return (this.X > xx - 16 && this.X <= xx + 16) && (this.Y > yy - 16 && this.Y <= yy + 16);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The kill player.
        /// </summary>
        internal void killPlayer()
        {
            this.IsDead = true;
        }

        /// <summary>
        /// The remove blue coin.
        /// </summary>
        /// <param name="xx">
        /// The xx.
        /// </param>
        /// <param name="yy">
        /// The yy.
        /// </param>
        internal void removeBlueCoin(int xx, int yy)
        {
            for (int i = 0; i < this.gotCoins.Count; i++)
            {
                if (this.gotCoins[i].x == xx && this.gotCoins[i].y == yy)
                {
                    this.gotCoins.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// The remove coin.
        /// </summary>
        /// <param name="xx">
        /// The xx.
        /// </param>
        /// <param name="yy">
        /// The yy.
        /// </param>
        internal void removeCoin(int xx, int yy)
        {
            for (int i = 0; i < this.gotCoins.Count; i++)
            {
                if (this.gotCoins[i].x == xx && this.gotCoins[i].y == yy)
                {
                    this.gotCoins.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// The reset coins.
        /// </summary>
        internal void resetCoins()
        {
            this.gotCoins.Clear();
            this.gotBlueCoins.Clear();
        }

        /// <summary>
        /// The respawn.
        /// </summary>
        internal void respawn()
        {
            this.modifierX = 0;
            this.modifierY = 0;
            this.ModifierX = 0;
            this.ModifierY = 0;
            this.speedX = 0;
            this.speedY = 0;
            this.SpeedX = 0;
            this.SpeedY = 0;
            this.IsDead = false;
        }

        /// <summary>
        /// The tick.
        /// </summary>
        internal void tick()
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

            cx = (int)(this.X + 8) >> 4;
            cy = (int)(this.Y + 8) >> 4;

            this.current = this.HostWorld.GetBlock(cx, cy);
            if (this.current == 4 || ItemId.isClimbable(this.current))
            {
                this.delayed = this.queue[1];
                this.queue[0] = this.current;
            }
            else
            {
                this.delayed = this.queue[0];
                this.queue[0] = this.queue[1];
            }

            this.queue[1] = this.current;

            if (this.IsDead)
            {
                this.Horizontal = 0;
                this.Vertical = 0;
            }

            isGodMode = this.InGodMode;
            if (this.InGodMode)
            {
                this.morx = 0;
                this.mory = 0;
                this.mox = 0;
                this.moy = 0;
            }
            else
            {
                switch (this.current)
                {
                    case 1:
                        this.morx = -this.gravity;
                        this.mory = 0;
                        break;
                    case 2:
                        this.morx = 0;
                        this.mory = -this.gravity;
                        break;
                    case 3:
                        this.morx = this.gravity;
                        this.mory = 0;
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
                        this.morx = 0;
                        this.mory = 0;
                        break;
                    case ItemId.WATER:
                        this.morx = 0;
                        this.mory = (int)PhysicsConfig.WaterBuoyancy;
                        break;
                    case ItemId.MUD:
                        this.morx = 0;
                        this.mory = (int)PhysicsConfig.MudBuoyancy;
                        break;
                    case ItemId.FIRE:
                    case ItemId.SPIKE:
                        if (!this.IsDead && !this.isInvulnerable)
                        {
                            this.killPlayer();
                            this.OnDie(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                        }

                        ;
                        break;
                    default:
                        this.morx = 0;
                        this.mory = this.gravity;
                        break;
                }

                switch (this.delayed)
                {
                    case 1:
                        this.mox = -this.gravity;
                        this.moy = 0;
                        break;
                    case 2:
                        this.mox = 0;
                        this.moy = -this.gravity;
                        break;
                    case 3:
                        this.mox = this.gravity;
                        this.moy = 0;
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
                        this.mox = 0;
                        this.moy = 0;
                        break;
                    case ItemId.WATER:
                        this.mox = 0;
                        this.moy = PhysicsConfig.WaterBuoyancy;
                        break;
                    case ItemId.MUD:
                        this.mox = 0;
                        this.moy = PhysicsConfig.MudBuoyancy;
                        break;
                    default:
                        this.mox = 0;
                        this.moy = this.gravity;
                        break;
                }
            }

            if (this.moy == PhysicsConfig.WaterBuoyancy || this.moy == PhysicsConfig.MudBuoyancy)
            {
                this.mx = this.Horizontal;
                this.my = this.Vertical;
            }
            else
            {
                if (this.moy != 0)
                {
                    this.mx = this.Horizontal;
                    this.my = 0;
                }
                else
                {
                    if (this.mox != 0)
                    {
                        this.mx = 0;
                        this.my = this.Vertical;
                    }
                    else
                    {
                        this.mx = this.Horizontal;
                        this.my = this.Vertical;
                    }
                }
            }

            this.mx = this.mx * this.SpeedMultiplier;
            this.my = this.my * this.SpeedMultiplier;
            this.mox = this.mox * this.GravityMultiplier;
            this.moy = this.moy * this.GravityMultiplier;

            this.ModifierX = this.mox + this.mx;
            this.ModifierY = this.moy + this.my;

            if (this.speedX != 0 || this.modifierX != 0)
            {
                this.speedX = this.speedX + this.modifierX;
                this.speedX = this.speedX * PhysicsConfig.BaseDrag;
                if (((this.mx == 0 && this.moy != 0) || this.speedX < 0 && this.mx > 0)
                    || (this.speedX > 0 && this.mx < 0) || (ItemId.isClimbable(this.current) && !isGodMode))
                {
                    this.speedX = this.speedX * PhysicsConfig.NoModifierDrag;
                }
                else
                {
                    if (this.current == ItemId.WATER && !isGodMode)
                    {
                        this.speedX = this.speedX * PhysicsConfig.WaterDrag;
                    }
                    else
                    {
                        if (this.current == ItemId.MUD && !isGodMode)
                        {
                            this.speedX = this.speedX * PhysicsConfig.MudDrag;
                        }
                    }
                }

                if (this.speedX > 16)
                {
                    this.speedX = 16;
                }
                else
                {
                    if (this.speedX < -16)
                    {
                        this.speedX = -16;
                    }
                    else
                    {
                        if (this.speedX < 0.0001 && this.speedX > -0.0001)
                        {
                            this.speedX = 0;
                        }
                    }
                }
            }

            if (this.speedY != 0 || this.modifierY != 0)
            {
                this.speedY = this.speedY + this.modifierY;
                this.speedY = this.speedY * PhysicsConfig.BaseDrag;
                if ((this.my == 0 && this.mox != 0) || (this.speedY < 0 && this.my > 0)
                    || (this.speedY > 0 && this.my < 0) || (ItemId.isClimbable(this.current) && !isGodMode))
                {
                    this.speedY = this.speedY * PhysicsConfig.NoModifierDrag;
                }
                else
                {
                    if (this.current == ItemId.WATER && !isGodMode)
                    {
                        this.speedY = this.speedY * PhysicsConfig.WaterDrag;
                    }
                    else
                    {
                        if (this.current == ItemId.MUD && !isGodMode)
                        {
                            this.speedY = this.speedY * PhysicsConfig.MudDrag;
                        }
                    }
                }

                if (this.speedY > 16)
                {
                    this.speedY = 16;
                }
                else
                {
                    if (this.speedY < -16)
                    {
                        this.speedY = -16;
                    }
                    else
                    {
                        if (this.speedY < 0.0001 && this.speedY > -0.0001)
                        {
                            this.speedY = 0;
                        }
                    }
                }
            }

            if (!isGodMode)
            {
                switch (this.current)
                {
                    case ItemId.SPEED_LEFT:
                        this.speedX = -PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_RIGHT:
                        this.speedX = PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_UP:
                        this.speedY = -PhysicsConfig.Boost;
                        break;
                    case ItemId.SPEED_DOWN:
                        this.speedY = PhysicsConfig.Boost;
                        break;
                }
            }

            reminderX = this.X % 1;
            currentSX = this.speedX;
            reminderY = this.Y % 1;
            currentSY = this.speedY;
            this.donex = false;
            this.doney = false;

            while ((currentSX != 0 && !this.donex) || (currentSY != 0 && !this.doney))
            {
                

                double multiplier = 1.42;
                this.current = this.HostWorld.GetBlock(cx, cy);
                if (!isGodMode && (this.current == ItemId.PORTAL || this.current == ItemId.PORTAL_INVISIBLE))
                {
                    if (this.lastPortal == null)
                    {
                        this.lastPortal = new Point(cx, cy);
                        int[] data = this.HostWorld.GetBlockData(cx, cy);
                        if (data != null && data.Length == 3)
                        {
                            Point portalPoint = this.HostWorld.GetPortalById(data[2]);
                            if (portalPoint != null)
                            {
                                int rot1 = this.HostWorld.GetBlockData(this.lastPortal.x, this.lastPortal.y)[0];
                                int rot2 = this.HostWorld.GetBlockData(portalPoint.x, portalPoint.y)[0];
                                if (rot1 < rot2)
                                {
                                    rot1 += 4;
                                }

                                switch (rot1 - rot2)
                                {
                                    case 1:
                                        this.SpeedX = this.SpeedY * multiplier;
                                        this.SpeedY = -this.SpeedX * multiplier;
                                        this.ModifierX = this.ModifierY * multiplier;
                                        this.ModifierY = -this.ModifierX * multiplier;
                                        reminderY = -reminderY;
                                        currentSY = -currentSY;
                                        break;
                                    case 2:
                                        this.SpeedX = -this.SpeedX * multiplier;
                                        this.SpeedY = -this.SpeedY * multiplier;
                                        this.ModifierX = -(this.ModifierX) * multiplier;
                                        this.ModifierY = -(this.ModifierY) * multiplier;
                                        reminderY = -reminderY;
                                        currentSY = -currentSY;
                                        reminderX = -reminderX;
                                        currentSX = -currentSX;
                                        break;
                                    case 3:
                                        this.SpeedX = -this.SpeedY * multiplier;
                                        this.SpeedY = this.SpeedX * multiplier;
                                        this.ModifierX = -(this.ModifierY) * multiplier;
                                        this.ModifierY = this.ModifierX * multiplier;
                                        reminderX = -reminderX;
                                        currentSX = -currentSX;
                                        break;
                                }

                                this.X = portalPoint.x * 16;
                                this.Y = portalPoint.y * 16;
                                this.lastPortal = portalPoint;
                            }
                        }
                    }
                }
                else
                {
                    this.lastPortal = null;
                }

                

                ox = this.X;
                oy = this.Y;
                osx = currentSX;
                osy = currentSY;

                #region stepX()

                if (currentSX > 0)
                {
                    if ((currentSX + reminderX) >= 1)
                    {
                        this.X = this.X + (1 - reminderX);
                        this.X = (int)this.X >> 0;
                        currentSX = currentSX - (1 - reminderX);
                        reminderX = 0;
                    }
                    else
                    {
                        this.X = this.X + currentSX;
                        currentSX = 0;
                    }
                }
                else
                {
                    if (currentSX < 0)
                    {
                        if (reminderX != 0 && (reminderX + currentSX) < 0)
                        {
                            currentSX = currentSX + reminderX;
                            this.X = this.X - reminderX;
                            this.X = (int)this.X >> 0;
                            reminderX = 1;
                        }
                        else
                        {
                            this.X = this.X + currentSX;
                            currentSX = 0;
                        }
                    }
                }

                if (this.HostWorld.overlaps(this))
                {
                    this.X = ox;
                    this.speedX = 0;
                    currentSX = osx;
                    this.donex = true;
                }

                #endregion

                #region stepY()

                if (currentSY > 0)
                {
                    if ((currentSY + reminderY) >= 1)
                    {
                        this.Y = this.Y + (1 - reminderY);
                        this.Y = (int)this.Y >> 0;
                        currentSY = currentSY - (1 - reminderY);
                        reminderY = 0;
                    }
                    else
                    {
                        this.Y = this.Y + currentSY;
                        currentSY = 0;
                    }

                    ;
                }
                else
                {
                    if (currentSY < 0)
                    {
                        if ((!((reminderY == 0))) && (((reminderY + currentSY) < 0)))
                        {
                            this.Y = (this.Y - reminderY);
                            this.Y = ((int)this.Y >> 0);
                            currentSY = (currentSY + reminderY);
                            reminderY = 1;
                        }
                        else
                        {
                            this.Y = (this.Y + currentSY);
                            currentSY = 0;
                        }
                    }
                }

                if (this.HostWorld.overlaps(this))
                {
                    this.Y = oy;
                    this.speedY = 0;
                    currentSY = osy;
                    this.doney = true;
                }

                #endregion
            }

            if (!this.IsDead)
            {
                if (this.pastx != cx || this.pasty != cy)
                {
                    switch (this.current)
                    {
                        case 100: // coin
                            this.OnHitCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < this.gotCoins.Count; i++)
                            {
                                if (this.gotCoins[i].x == cx && this.gotCoins[i].y == cy)
                                {
                                    goto found;
                                }
                            }

                            this.OnGetCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            this.gotCoins.Add(new Point(cx, cy));
                            found:
                            break;
                        case 101: // bluecoin
                            this.OnHitBlueCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            for (int i = 0; i < this.gotBlueCoins.Count; i++)
                            {
                                if (this.gotBlueCoins[i].x == cx && this.gotBlueCoins[i].y == cy)
                                {
                                    goto found2;
                                }
                            }

                            this.OnGetBlueCoin(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            this.gotBlueCoins.Add(new Point(cx, cy));
                            found2:
                            break;
                        case 5:

                            // crown
                            this.OnHitCrown(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 6:

                            // red key
                            this.OnHitRedKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 7:

                            // green
                            this.OnHitGreenKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 8:

                            // blue
                            this.OnHitBlueKey(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.SWITCH_PURPLE:

                            // purple (switch)
                            this.Purple = !this.Purple;
                            this.OnHitSwitch(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 77:

                            // piano
                            this.OnHitPiano(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case 83:

                            // drum
                            this.OnHitDrum(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.DIAMOND:

                            // diamond
                            this.OnHitDiamond(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.CAKE:

                            // cake
                            this.OnHitCake(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                        case ItemId.CHECKPOINT:

                            // checkpoint
                            if (!isGodMode)
                            {
                                this.LastCheckpointX = cx;
                                this.LastCheckpointY = cy;
                                this.OnHitCheckpoint(new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            }

                            break;
                        case ItemId.BRICK_COMPLETE:

                            // level completed
                            this.OnHitCompleteLevelBrick(
                                new PlayerEventArgs { Player = this, BlockX = cx, BlockY = cy });
                            break;
                    }

                    this.pastx = cx;
                    this.pasty = cy;
                }
            }

            int imx = (int)this.speedX << 8;
            int imy = (int)this.speedY << 8;

            if (this.current != ItemId.WATER && this.current != ItemId.MUD)
            {
                if (imx == 0)
                {
                    if (this.modifierX < 0.1 && this.modifierX > -0.1)
                    {
                        tx = this.X % 16;
                        if (tx < 2)
                        {
                            if (tx < 0.2)
                            {
                                this.X = Math.Floor(this.X);
                            }
                            else
                            {
                                this.X = this.X - (tx / 15);
                            }

                            ;
                        }
                        else
                        {
                            if (tx > 14)
                            {
                                if (tx > 15.8)
                                {
                                    this.X = Math.Ceiling(this.X);
                                }
                                else
                                {
                                    this.X = this.X + ((tx - 14) / 15);
                                }
                            }
                        }
                    }
                }

                if (imy == 0)
                {
                    if ((this.modifierY < 0.1) && (this.modifierY > -0.1))
                    {
                        ty = this.Y % 16;
                        if (ty < 2)
                        {
                            if (ty < 0.2)
                            {
                                this.Y = Math.Floor(this.Y);
                            }
                            else
                            {
                                this.Y = this.Y - (ty / 15);
                            }
                        }
                        else
                        {
                            if (ty > 14)
                            {
                                if (ty > 15.8)
                                {
                                    this.Y = Math.Ceiling(this.Y);
                                }
                                else
                                {
                                    this.Y = this.Y + ((ty - 14) / 15);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// The player event args.
    /// </summary>
    public class PlayerEventArgs
    {
        #region Public Properties

        /// <summary>
        ///     Block X where event happened.
        /// </summary>
        public int BlockX { get; set; }

        /// <summary>
        ///     Block Y where event happened.
        /// </summary>
        public int BlockY { get; set; }

        /// <summary>
        ///     Player which caused the event.
        /// </summary>
        public PhysicsPlayer Player { get; set; }

        #endregion
    }

    /// <summary>
    /// The point.
    /// </summary>
    internal class Point
    {
        #region Fields

        /// <summary>
        /// The x.
        /// </summary>
        public int x;

        /// <summary>
        /// The y.
        /// </summary>
        public int y;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> class.
        /// </summary>
        /// <param name="xx">
        /// The xx.
        /// </param>
        /// <param name="yy">
        /// The yy.
        /// </param>
        public Point(int xx, int yy)
        {
            this.x = xx;
            this.y = yy;
        }

        #endregion
    }
}