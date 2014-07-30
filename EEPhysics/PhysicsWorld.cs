// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PhysicsWorld.cs" company="">
//   
// </copyright>
// <summary>
//   The physics world.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace EEPhysics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using PlayerIOClient;

    /// <summary>
    /// The physics world.
    /// </summary>
    public class PhysicsWorld
    {
        #region Constants

        /// <summary>
        /// The size.
        /// </summary>
        internal const int Size = 16;

        #endregion

        #region Fields

        /// <summary>
        /// The world gravity.
        /// </summary>
        internal double WorldGravity = 1;

        /// <summary>
        /// The sw.
        /// </summary>
        internal Stopwatch sw = new Stopwatch();

        /// <summary>
        /// The early messages.
        /// </summary>
        private List<Message> earlyMessages = new List<Message>();

        /// <summary>
        /// The foreground tiles.
        /// </summary>
        private int[][] foregroundTiles;

        /// <summary>
        /// The hide blue.
        /// </summary>
        private bool hideBlue;

        /// <summary>
        /// The hide green.
        /// </summary>
        private bool hideGreen;

        /// <summary>
        /// The hide red.
        /// </summary>
        private bool hideRed;

        /// <summary>
        /// The hide timedoor.
        /// </summary>
        private bool hideTimedoor;

        /// <summary>
        /// The inited.
        /// </summary>
        private bool inited;

        /// <summary>
        /// The physics thread.
        /// </summary>
        private Thread physicsThread;

        /// <summary>
        /// The running.
        /// </summary>
        private bool running;

        /// <summary>
        /// The tile data.
        /// </summary>
        private int[][][] tileData;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsWorld"/> class.
        /// </summary>
        public PhysicsWorld()
        {
            this.AutoStart = true;
            this.AddBotPlayer = true;
            this.Players = new ConcurrentDictionary<int, PhysicsPlayer>();
        }

        #endregion

        #region Public Events

        /// <summary>
        /// The on tick.
        /// </summary>
        public event EventHandler OnTick = delegate { };

        #endregion

        #region Public Properties

        /// <summary>
        ///     Whether bot adds itself from init message. Defaults to true.
        /// </summary>
        public bool AddBotPlayer { get; set; }

        /// <summary>
        ///     Whether bot automatically starts the physics simulation when it gets init message. Defaults to true.
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        ///     Whether physics simulation thread has been started.
        /// </summary>
        public bool PhysicsRunning { get; private set; }

        /// <summary>
        ///     You shouldn't add or remove any items from this dictionary outside EEPhysics.
        /// </summary>
        public ConcurrentDictionary<int, PhysicsPlayer> Players { get; private set; }

        /// <summary>
        /// Gets the world height.
        /// </summary>
        public int WorldHeight { get; private set; }

        /// <summary>
        /// Gets the world key.
        /// </summary>
        public string WorldKey { get; private set; }

        /// <summary>
        /// Gets the world width.
        /// </summary>
        public int WorldWidth { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get block.
        /// </summary>
        /// <param name="xx">
        /// The xx.
        /// </param>
        /// <param name="yy">
        /// The yy.
        /// </param>
        /// <returns>
        /// Foreground block ID
        /// </returns>
        public int GetBlock(int xx, int yy)
        {
            if (xx < 0 || xx >= this.foregroundTiles.Length || yy < 0 || yy >= this.foregroundTiles[0].Length)
            {
                return 0;
            }

            return this.foregroundTiles[xx][yy];
        }

        /// <summary>
        /// The get block data.
        /// </summary>
        /// <param name="xx">
        /// The xx.
        /// </param>
        /// <param name="yy">
        /// The yy.
        /// </param>
        /// <returns>
        /// Extra block data, eg. rotation, id and target id from portals.
        /// </returns>
        public int[] GetBlockData(int xx, int yy)
        {
            if (xx < 0 || xx >= this.foregroundTiles.Length || yy < 0 || yy >= this.foregroundTiles[0].Length)
            {
                return null;
            }

            return this.tileData[xx][yy];
        }

        /// <summary>
        /// The handle message.
        /// </summary>
        /// <param name="m">
        /// The m.
        /// </param>
        public void HandleMessage(Message m)
        {
            if (!this.inited && m.Type != "init")
            {
                this.earlyMessages.Add(m);
                return;
            }

            switch (m.Type)
            {
                case "m":
                    {
                        PhysicsPlayer p;
                        if (this.Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.X = m.GetDouble(1);
                            p.Y = m.GetDouble(2);
                            p.SpeedX = m.GetDouble(3);
                            p.SpeedY = m.GetDouble(4);
                            p.ModifierX = m.GetDouble(5);
                            p.ModifierY = m.GetDouble(6);
                            p.Horizontal = m.GetInt(7);
                            p.Vertical = m.GetInt(8);
                            p.Coins = m.GetInt(9);
                            p.Purple = m.GetBoolean(10);
                            p.IsDead = false;
                        }
                    }

                    break;
                case "b":
                    {
                        if (m.GetInt(0) == 0)
                        {
                            var xx = m.GetInt(1);
                            var yy = m.GetInt(2);
                            var blockId = m.GetInt(3);
                            this.foregroundTiles[xx][yy] = blockId;
                            if (blockId == 100)
                            {
                                foreach (var pair in this.Players)
                                {
                                    pair.Value.removeCoin(xx, yy);
                                }
                            }
                            else if (blockId == 101)
                            {
                                foreach (var pair in this.Players)
                                {
                                    pair.Value.removeBlueCoin(xx, yy);
                                }
                            }
                        }
                    }

                    break;
                case "add":
                    {
                        var p = new PhysicsPlayer(m.GetInt(0), m.GetString(1));
                        p.HostWorld = this;
                        p.X = m.GetDouble(3);
                        p.Y = m.GetDouble(4);
                        p.Coins = m.GetInt(8);
                        p.Purple = m.GetBoolean(9);
                        p.InGodMode = m.GetBoolean(5);
                        p.IsClubMember = m.GetBoolean(12);

                        this.Players.TryAdd(p.ID, p);
                    }

                    break;
                case "left":
                    {
                        PhysicsPlayer p;
                        this.Players.TryRemove(m.GetInt(0), out p);
                    }

                    break;
                case "show":
                case "hide":
                    {
                        var b = m.Type == "hide";
                        switch (m.GetString(0))
                        {
                            case "timedoor":
                                this.hideTimedoor = b;
                                break;
                            case "blue":
                                this.hideBlue = b;
                                break;
                            case "red":
                                this.hideRed = b;
                                break;
                            case "green":
                                this.hideGreen = b;
                                break;
                        }
                    }

                    break;
                case "bc":
                case "br":
                case "bs":
                    {
                        var xx = m.GetInt(0);
                        var yy = m.GetInt(1);
                        this.foregroundTiles[xx][yy] = m.GetInt(2);
                        this.tileData[xx][yy] = new int[m.Count - 4];
                        for (uint i = 3; i < 4; i++)
                        {
                            this.tileData[xx][yy][i - 3] = m.GetInt(i);
                        }
                    }

                    break;
                case "pt":
                    {
                        var xx = m.GetInt(0);
                        var yy = m.GetInt(1);
                        this.foregroundTiles[xx][yy] = m.GetInt(2);
                        this.tileData[xx][yy] = new int[m.Count - 3];
                        for (uint i = 3; i < 6; i++)
                        {
                            this.tileData[xx][yy][i - 3] = m.GetInt(i);
                        }
                    }

                    break;
                case "god":
                case "mod":
                    {
                        PhysicsPlayer p;
                        if (this.Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.InGodMode = m.GetBoolean(1);
                            if (p.InGodMode)
                            {
                                p.respawn();
                            }
                        }
                    }

                    break;
                case "tele":
                    {
                        var b = m.GetBoolean(0);
                        uint i = 1;
                        while (i + 2 < m.Count)
                        {
                            PhysicsPlayer p;
                            if (this.Players.TryGetValue(m.GetInt(i), out p))
                            {
                                p.X = m.GetInt(i + 1);
                                p.Y = m.GetInt(i + 2);
                                if (b)
                                {
                                    p.respawn();
                                }
                            }

                            i += 3;
                        }
                    }

                    break;
                case "teleport":
                    {
                        PhysicsPlayer p;
                        if (this.Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.X = m.GetInt(1);
                            p.Y = m.GetInt(2);
                        }
                    }

                    break;
                case "reset":
                    {
                        this.desBlocks(m, 0);
                        foreach (var pair in this.Players)
                        {
                            pair.Value.resetCoins();
                        }

                        /*for (int i = 0; i < players.Count; i++) {
                            players[i].Coins = 0;
                            players[i].respawn();
                        }*/
                    }

                    break;
                case "clear":
                    {
                        var border = m.GetInt(2);
                        var fill = m.GetInt(3);
                        for (var i = 0; i < this.foregroundTiles.Length; i++)
                        {
                            for (var ii = 0; ii < this.foregroundTiles[i].Length; ii++)
                            {
                                if (i == 0 || ii == 0 || i == this.WorldWidth - 1 || ii == this.WorldHeight - 1)
                                {
                                    this.foregroundTiles[i][ii] = border;
                                }
                                else
                                {
                                    this.foregroundTiles[i][ii] = fill;
                                }
                            }
                        }

                        foreach (var pair in this.Players)
                        {
                            pair.Value.resetCoins();
                        }
                    }

                    break;
                case "ts":
                case "lb":
                case "wp":
                    {
                        this.foregroundTiles[m.GetInt(0)][m.GetInt(1)] = m.GetInt(2);
                    }

                    break;
                case "init":
                    {
                        this.WorldWidth = m.GetInt(12);
                        this.WorldHeight = m.GetInt(13);

                        this.foregroundTiles = new int[this.WorldWidth][];
                        for (var i = 0; i < this.foregroundTiles.Length; i++)
                        {
                            this.foregroundTiles[i] = new int[this.WorldHeight];
                        }

                        this.tileData = new int[this.WorldWidth][][];
                        for (var i = 0; i < this.foregroundTiles.Length; i++)
                        {
                            this.tileData[i] = new int[this.WorldHeight][];
                        }

                        this.WorldKey = derot(m.GetString(5));
                        this.WorldGravity = m.GetDouble(15);

                        if (this.AddBotPlayer)
                        {
                            var p = new PhysicsPlayer(m.GetInt(6), m.GetString(9));
                            p.X = m.GetInt(7);
                            p.Y = m.GetInt(8);
                            p.HostWorld = this;
                            this.Players.TryAdd(p.ID, p);
                        }

                        this.desBlocks(m, 18);
                        this.inited = true;

                        foreach (var m2 in this.earlyMessages)
                        {
                            this.HandleMessage(m2);
                        }

                        this.earlyMessages.Clear();
                        this.earlyMessages = null;

                        if (this.AutoStart && (this.physicsThread == null || !this.physicsThread.IsAlive))
                        {
                            this.StartSimulation();
                        }
                    }

                    break;
            }
        }

        /// <summary>
        ///     Will run the physics simulation. Needs to be called only once.
        /// </summary>
        public void Run()
        {
            this.running = true;
            this.PhysicsRunning = true;

            this.sw.Start();
            long waitTime, frameStartTime, frameEndTime;
            while (this.running)
            {
                frameStartTime = this.sw.ElapsedMilliseconds;
                foreach (var pair in this.Players)
                {
                    pair.Value.tick();
                }

                this.OnTick(this, null);
                frameEndTime = this.sw.ElapsedMilliseconds;
                waitTime = 10 - (frameEndTime - frameStartTime);
                if (waitTime > 0)
                {
                    Thread.Sleep((int)waitTime);
                }
            }

            this.PhysicsRunning = false;
        }

        /// <summary>
        ///     Starts the physics simulation thread.
        /// </summary>
        public void StartSimulation()
        {
            if (!this.PhysicsRunning)
            {
                if (this.inited)
                {
                    this.physicsThread = new Thread(this.Run);
                    this.physicsThread.Start();
                }
                else
                {
                    throw new Exception("Cannot start before bot has received init message. ");
                }
            }
            else
            {
                throw new Exception("Simulation thread has already been started.");
            }
        }

        /// <summary>
        ///     Stops physics simulation thread.
        /// </summary>
        public void StopSimulation()
        {
            if (this.PhysicsRunning)
            {
                this.running = false;
            }
            else
            {
                throw new Exception("Simulation thread is not running.");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The derot.
        /// </summary>
        /// <param name="arg1">
        /// The arg 1.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        internal static string derot(string arg1)
        {
            // by Capasha (http://pastebin.com/Pj6tvNNx)
            var num = 0;
            var str = string.Empty;
            for (var i = 0; i < arg1.Length; i++)
            {
                num = arg1[i];
                if ((num >= 0x61) && (num <= 0x7a))
                {
                    if (num > 0x6d)
                    {
                        num -= 13;
                    }
                    else
                    {
                        num += 13;
                    }
                }
                else if ((num >= 0x41) && (num <= 90))
                {
                    if (num > 0x4d)
                    {
                        num -= 13;
                    }
                    else
                    {
                        num += 13;
                    }
                }

                str = str + ((char)num);
            }

            return str;
        }

        /// <summary>
        /// The get portal by id.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="Point"/>.
        /// </returns>
        internal Point GetPortalById(int id)
        {
            for (var i = 0; i < this.foregroundTiles.Length; i++)
            {
                for (var ii = 0; ii < this.foregroundTiles[i].Length; ii++)
                {
                    if (this.foregroundTiles[i][ii] == 242 || this.foregroundTiles[i][ii] == 381)
                    {
                        if (this.tileData[i][ii][1] == id)
                        {
                            return new Point(i, ii);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The des blocks.
        /// </summary>
        /// <param name="m">
        /// The m.
        /// </param>
        /// <param name="start">
        /// The start.
        /// </param>
        internal void desBlocks(Message m, uint start)
        {
            // Got and modified from Skylight by TakoMan02 (made originally in VB by Bass5098), credit to them
            // (http://seist.github.io/Skylight/)
            // > https://github.com/Seist/Skylight/blob/master/Skylight/Miscellaneous/Tools.cs, method ConvertMessageToBlockList
            try
            {
                var messageIndex = start;
                while (messageIndex < m.Count)
                {
                    if (m[messageIndex] is string)
                    {
                        break;
                    }

                    var blockId = m.GetInteger(messageIndex);
                    messageIndex++;

                    var z = m.GetInteger(messageIndex);
                    messageIndex++;

                    var xa = m.GetByteArray(messageIndex);
                    messageIndex++;

                    var ya = m.GetByteArray(messageIndex);
                    messageIndex++;

                    var data = new List<int>();
                    if (blockId == 242 || blockId == 381)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            data.Add(m.GetInteger(messageIndex));
                            messageIndex++;
                        }
                    }
                    else if (blockId == 374 || blockId == 43 || blockId == 165 || blockId == 83 || blockId == 77
                             || blockId == 361 || (blockId > 374 && blockId < 381) || blockId == 1000 || blockId == 385)
                    {
                        if (blockId != 1000 && blockId != 385)
                        {
                            data.Add(m.GetInteger(messageIndex));
                        }

                        messageIndex++;
                    }

                    int x = 0, y = 0;

                    for (var pos = 0; pos < ya.Length; pos += 2)
                    {
                        x = (xa[pos] * 256) + xa[pos + 1];
                        y = (ya[pos] * 256) + ya[pos + 1];

                        if (blockId < 500)
                        {
                            this.foregroundTiles[x][y] = blockId;
                            if (data.Count > 0)
                            {
                                this.tileData[x][y] = data.ToArray();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(" EEPhysics: Error loading existing blocks:\n" + e);
            }
        }

        /// <summary>
        /// The overlaps.
        /// </summary>
        /// <param name="p">
        /// The p.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        internal bool overlaps(PhysicsPlayer p)
        {
            if ((p.X < 0 || p.Y < 0) || ((p.X > this.WorldWidth * 16 - 16) || (p.Y > this.WorldHeight * 16 - 16)))
            {
                return true;
            }

            if (p.InGodMode)
            {
                return false;
            }

            int tileId;
            var firstX = (int)p.X >> 4;
            var firstY = (int)p.Y >> 4;
            var lastX = (p.X + PhysicsPlayer.Height) / Size;
            var lastY = (p.Y + PhysicsPlayer.Width) / Size;
            var _local7 = false;

            int x;
            var y = firstY;
            while (y < lastY)
            {
                x = firstX;
                for (; x < lastX; x++)
                {
                    tileId = this.foregroundTiles[x][y];
                    if (ItemId.isSolid(tileId))
                    {
                        switch (tileId)
                        {
                            case 23:
                                if (this.hideRed)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 24:
                                if (this.hideGreen)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 25:
                                if (this.hideBlue)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 26:
                                if (!this.hideRed)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 27:
                                if (!this.hideGreen)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 28:
                                if (!this.hideBlue)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 156:
                                if (this.hideTimedoor)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case 157:
                                if (!this.hideTimedoor)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.DOOR_PURPLE:
                                if (p.Purple)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.GATE_PURPLE:
                                if (!p.Purple)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.DOOR_CLUB:
                                if (p.IsClubMember)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.GATE_CLUB:
                                if (!p.IsClubMember)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.COINDOOR:
                                if (this.tileData[x][y][0] <= p.Coins)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.COINGATE:
                                if (this.tileData[x][y][0] > p.Coins)
                                {
                                    continue;
                                }

                                ;
                                break;
                            case ItemId.ZOMBIE_GATE:

                                /*if (p.Zombie) {
                                    continue;
                                };*/
                                break;
                            case ItemId.ZOMBIE_DOOR:

                                /*if (!p.Zombie) {
                                    continue;
                                };*/
                                continue;
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
                                if (p.SpeedY < 0 || y <= p.overlapy)
                                {
                                    if (y != firstY || p.overlapy == -1)
                                    {
                                        p.overlapy = y;
                                    }

                                    ;
                                    _local7 = true;
                                    continue;
                                }

                                ;
                                break;
                            case 83:
                            case 77:
                                continue;
                        }

                        ;
                        return true;
                    }
                }

                y++;
            }

            if (!_local7)
            {
                p.overlapy = -1;
            }

            return false;
        }

        #endregion
    }
}