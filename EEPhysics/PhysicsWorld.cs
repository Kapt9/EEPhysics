using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using PlayerIOClient;
using System;

namespace EEPhysics
{
    public class PhysicsWorld : IDisposable
    {
        private bool hideRed, hideBlue, hideGreen, hideCyan, hideMagenta, hideYellow, hideTimedoor;
        internal bool Connected => Connection != null && Connection.Connected;
        private readonly List<Message> earlyMessages = new List<Message>();
        public int WorldHeight { get; private set; }
        public int WorldWidth { get; private set; }
        internal Stopwatch Sw = new Stopwatch();
        internal Connection Connection { get; }
        internal double WorldGravity = 1;
        private Thread physicsThread;
        internal const int Size = 16;
        private int[][][] blockData;
        private int[][][] blocks;
        private bool running;
        private bool inited;

        /// <summary>
        /// Whether bot automatically starts the physics simulation when it gets init message. Defaults to true.
        /// </summary>
        public bool AutoStart { get; set; }
        /// <summary>
        /// Whether bot adds itself from init message. Defaults to true.
        /// </summary>
        public bool AddBotPlayer { get; set; }
        /// <summary>
        /// Whether physics simulation thread has been started.
        /// </summary>
        public bool PhysicsRunning { get; private set; }

        /// <summary>
        /// You shouldn't add or remove any items from this dictionary outside EEPhysics.
        /// </summary>
        public ConcurrentDictionary<int, PhysicsPlayer> Players { get; }
        public int BotId { get; private set; }

        /// <summary>
        /// Called upon every physics simulation tick. (every 10ms)
        /// </summary>
        public event EventHandler OnTick = delegate { };

        public PhysicsWorld()
        {
            AutoStart = true;
            AddBotPlayer = true;
            Players = new ConcurrentDictionary<int, PhysicsPlayer>();
        }
        public PhysicsWorld(Connection conn)
        {
            AutoStart = true;
            AddBotPlayer = true;
            Players = new ConcurrentDictionary<int, PhysicsPlayer>();
            Connection = conn;
        }

        /// <summary>
        /// Will run the physics simulation. Needs to be called only once. If you have AutoStart set to true or you started physics with StartSimulation, don't call this!
        /// </summary>
        public void Run()
        {
            running = true;
            PhysicsRunning = true;

            Sw.Start();
            while (running)
            {
                long frameStartTime = Sw.ElapsedMilliseconds;

                foreach (var pair in Players) pair.Value.Tick();

                OnTick(this, null);

                long frameEndTime = Sw.ElapsedMilliseconds;

                long waitTime = 10 - (frameEndTime - frameStartTime);
                if (waitTime > 0) Thread.Sleep((int)waitTime);
            }

            PhysicsRunning = false;
        }

        /// <summary>
        /// Call this for every PlayerIO Message you receive.
        /// </summary>
        public void HandleMessage(Message m)
        {
            if (!inited)
            {
                if (m.Type == "init")
                {
                    WorldWidth = m.GetInt(18);
                    WorldHeight = m.GetInt(19);

                    blocks = new int[2][][];
                    for (int i = 0; i < blocks.Length; i++)
                    {
                        blocks[i] = new int[WorldWidth][];
                        for (int ii = 0; ii < WorldWidth; ii++) blocks[i][ii] = new int[WorldHeight];
                    }

                    blockData = new int[WorldWidth][][];
                    for (int i = 0; i < WorldWidth; i++) blockData[i] = new int[WorldHeight][];

                    WorldGravity = m.GetDouble(20);

                    if (AddBotPlayer)
                    {
                        PhysicsPlayer p = new PhysicsPlayer(m.GetInt(5), m.GetString(13))
                        {
                            X = m.GetInt(10),
                            Y = m.GetInt(11),
                            HostWorld = this
                        };
                        BotId = p.Id;
                        p.IsMe = true;
                        Players.TryAdd(p.Id, p);
                    }

                    DeserializeBlocks(m);
                    inited = true;

                    foreach (var m2 in earlyMessages) HandleMessage(m2);

                    earlyMessages.Clear();

                    if (AutoStart && (physicsThread == null || !physicsThread.IsAlive)) StartSimulation();
                }
                else if (m.Type != "add" && m.Type != "left")
                {
                    earlyMessages.Add(m);
                    return;
                }
            }
            switch (m.Type)
            {
                case "m":
                    {
                        int id = m.GetInt(0);
                        PhysicsPlayer p;
                        if (id != BotId && Players.TryGetValue(id, out p))
                        {
                            p.X = m.GetDouble(1);
                            p.Y = m.GetDouble(2);
                            p.SpeedX = m.GetDouble(3);
                            p.SpeedY = m.GetDouble(4);
                            p.ModifierX = m.GetDouble(5);
                            p.ModifierY = m.GetDouble(6);
                            p.Horizontal = m.GetInt(7);
                            p.Vertical = m.GetInt(8);
                            p.SpaceDown = m.GetBoolean(9);
                            p.IsDead = false;
                            if (p.HasLevitation)
                            {
                                if (p.SpaceDown)
                                {
                                    p.ApplyThrust();
                                    p.IsThrusting = true;
                                }
                                else p.IsThrusting = false;
                            }
                        }
                    }
                    break;
                case "b":
                    {
                        int zz = m.GetInt(0);
                        int xx = m.GetInt(1);
                        int yy = m.GetInt(2);
                        int blockId = m.GetInt(3);
                        if (zz == 0)
                        {
                            switch (blocks[zz][xx][yy])
                            {
                                case 100: foreach (var pair in Players) pair.Value.RemoveCoin(xx, yy); break;
                                case 101: foreach (var pair in Players) pair.Value.RemoveBlueCoin(xx, yy); break;
                            }
                        }
                        blocks[zz][xx][yy] = blockId;
                    }
                    break;
                case "add":
                    {
                        var p = new PhysicsPlayer(m.GetInt(0), m.GetString(1))
                        {
                            HostWorld = this,
                            X = m.GetDouble(4),
                            Y = m.GetDouble(5),
                            InGodMode = m.GetBoolean(6) || m.GetBoolean(7),
                            HasChat = m.GetBoolean(8),
                            Coins = m.GetInt(9),
                            BlueCoins = m.GetInt(10),
                            IsClubMember = m.GetBoolean(12),
                            Team = m.GetInt(16)
                        };

                        Players.TryAdd(p.Id, p);
                    }
                    break;
                case "left":
                    {
                        PhysicsPlayer p;
                        Players.TryRemove(m.GetInt(0), out p);
                    }
                    break;
                case "show":
                case "hide":
                    {
                        bool b = (m.Type == "hide");
                        switch (m.GetString(0))
                        {
                            case "timedoor": hideTimedoor = b; break;
                            case "blue": hideBlue = b; break;
                            case "red": hideRed = b; break;
                            case "green": hideGreen = b; break;
                            case "cyan": hideCyan = b; break;
                            case "magenta": hideMagenta = b; break;
                            case "yellow": hideYellow = b; break;
                        }
                    }
                    break;
                case "ps":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p)) p.Switches[m.GetInt(1)] = m.GetInt(2) == 1;
                    }
                    break;
                case "psi":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.Switches = new BitArray(100);
                            byte[] bytes = m.GetByteArray(1);
                            if (bytes.Length > 100) p.Switches.Length = bytes.Length;
                            for (int i = 0; i < bytes.Length; i++) p.Switches[i] = (bytes[i] == 1);
                        }
                    }
                    break;
                case "c":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.Coins = m.GetInt(1);
                            p.BlueCoins = m.GetInt(2);
                        }
                    }
                    break;
                case "bc":
                case "br":
                case "bs":
                    {
                        int xx = m.GetInt(0);
                        int yy = m.GetInt(1);
                        blocks[0][xx][yy] = m.GetInt(2);
                        blockData[xx][yy] = new int[1];
                        for (uint i = 3; i < 4; i++) blockData[xx][yy][i - 3] = m.GetInt(i);
                    }
                    break;
                case "pt":
                    {
                        int xx = m.GetInt(0);
                        int yy = m.GetInt(1);
                        blocks[0][xx][yy] = m.GetInt(2);
                        blockData[xx][yy] = new int[3];
                        for (uint i = 3; i < 6; i++) blockData[xx][yy][i - 3] = m.GetInt(i);
                    }
                    break;
                /*case "fill":
                    {
                        int blockId = m.GetInt(0);
                        int z = m.GetInt(1);
                        int startX = m.GetInt(2);
                        int startY = m.GetInt(3);
                        int endX = startX + m.GetInt(4);
                        int endY = startY + m.GetInt(5);
                        for (int x = startX; x < endX; x++)
                            for (int y = startY; y < endY; y++)
                                blocks[z][x][y] = blockId;
                    }
                    break;*/  //Fill got removed a long long time ago..
                case "god":
                case "mod":
                case "admin":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p)) p.InGodMode = m.GetBoolean(1);
                    }
                    break;
                case "effect":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p)) p.SetEffect(m.GetInt(1), m.GetBoolean(2));
                    }
                    break;
                case "team":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p)) p.Team = m.GetInt(1);
                    }
                    break;
                case "tele":
                    {
                        try
                        {
                            var resetCoins = m.GetBoolean(0);
                            for (uint a = 2; a < m.Count; a++)
                            {
                                var id = m.GetInt(a);
                                var nx = m.GetInt(a + 1);
                                var ny = m.GetInt(a + 2);
                                var deaths = m.GetInt(a + 3);

                                var p = Players[id];
                                if (p != null)
                                {
                                    p.X = nx;
                                    p.Y = ny;
                                    p.Respawn();
                                    p.Deaths = deaths;
                                    if (resetCoins) p.Reset();
                                }
                                if (id == BotId && Connected)
                                {
                                    var player = Players[BotId];
                                    player.X = nx;
                                    player.Y = ny;
                                    player.Respawn();
                                    if (resetCoins) player.Reset();
                                }
                                a = a + 3;
                            }
                        }
                        catch { }
                    }
                    break;
                case "teleport":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.X = m.GetInt(1);
                            p.Y = m.GetInt(2);
                        }
                    }
                    break;
                case "reset":
                    {
                        DeserializeBlocks(m);
                        foreach (var pair in Players) pair.Value.Reset();
                    }
                    break;
                case "clear":
                    {
                        int border = m.GetInt(2);
                        int fill = m.GetInt(3);
                        for (int i = 0; i < WorldWidth; i++)
                            for (int ii = 0; ii < WorldHeight; ii++)
                            {
                                if (i == 0 || ii == 0 || i == WorldWidth - 1 || ii == WorldHeight - 1) blocks[0][i][ii] = border;
                                else blocks[0][i][ii] = fill;

                                blocks[1][i][ii] = 0;
                            }

                        foreach (var pair in Players) pair.Value.Reset();
                    }
                    break;
                case "ts":
                case "lb":
                case "wp":
                    {
                        blocks[0][m.GetInt(0)][m.GetInt(1)] = m.GetInt(2);
                    }
                    break;
                case "kill":
                    {
                        int userId = m.GetInt(0u);

                        if (userId == BotId && Connected) Players[BotId].KillPlayer();

                        PhysicsPlayer p;
                        if (Players.TryGetValue(userId, out p)) p.Deaths++;
                    }
                    break;
            }
        }

        /// <returns>Foreground block ID</returns>
        public int GetBlock(int x, int y) { return GetBlock(0, x, y); }

        /// <param name="z">Block layer: 0 = foreground, 1 = background</param>
        /// <param name="x">Block X</param>
        /// <param name="y">Block Y</param>
        /// <returns>Block ID</returns>
        public int GetBlock(int z, int x, int y)
        {
            if (z < 0 || z > 1) throw new ArgumentOutOfRangeException("z", "Layer must be 0 (foreground) or 1 (background).");
            if (x < 0 || x >= WorldWidth || y < 0 || y >= WorldHeight) return -1;

            return blocks[z][x][y];
        }

        /// <returns>Extra block data, eg. rotation, id and target id from portals. Doesn't support signs.</returns>
        public int[] GetBlockData(int x, int y)
        {
            if (x < 0 || x >= WorldWidth || y < 0 || y >= WorldHeight) return null;
            return blockData[x][y];
        }

        internal bool TryGetPortalById(int id, out Point p)
        {
            for (int i = 0; i < WorldWidth; i++)
            {
                for (int ii = 0; ii < WorldHeight; ii++)
                {
                    if (blocks[0][i][ii] != 242 && blocks[0][i][ii] != 381) continue;
                    if (blockData[i][ii][1] != id) continue;

                    p = new Point(i, ii);
                    return true;
                }
            }
            p = default(Point);
            return false;
        }

        internal bool GetOnStatus(int x, int y)
        {
            // TODO: Does effect disable or enable?
            return true;
        }

        /// <summary>
        /// Starts the physics simulation in another thread.
        /// </summary>
        public void StartSimulation()
        {
            if (!PhysicsRunning)
            {
                if (inited)
                {
                    physicsThread = new Thread(Run) { IsBackground = true };
                    physicsThread.Start();
                }
                else throw new Exception("Cannot start before bot has received init message.");
            }
            else throw new Exception("Simulation thread has already been started.");

        }

        /// <summary>
        /// Stops the physics simulation thread.
        /// </summary>
        public void StopSimulation() { if (PhysicsRunning) running = false; }

        internal bool Overlaps(PhysicsPlayer p)
        {
            if ((p.X < 0 || p.Y < 0) || ((p.X > WorldWidth * 16 - 16) || (p.Y > WorldHeight * 16 - 16))) return true;
            if (p.InGodMode) return false;

            var firstX = ((int)p.X >> 4);
            var firstY = ((int)p.Y >> 4);
            double lastX = ((p.X + PhysicsPlayer.Height) / Size);
            double lastY = ((p.Y + PhysicsPlayer.Width) / Size);
            bool skip = false;
            var playerRectangle = new Rectangle((int)p.X, (int)p.Y, 16, 16);

            int y = firstY;

            int a = firstY;
            while (y < lastY)
            {
                var x = firstX;
                var b = firstX;
                for (; x < lastX; x++)
                {
                    var tileId = blocks[0][x][y];

                    if (!ItemId.IsSolid(tileId)) continue;
                    if (!playerRectangle.IntersectsWith(new Rectangle(x * 16, y * 16, 16, 16))) continue;
                    var rot = 1;
                    try
                    {
                        //TODO: Figure out this.
                        rot = blockData[x][y] == null ? 1 : blockData[x][y][0];
                    }
                    catch { }

                    if (tileId == ItemId.OnewayCyan || tileId == ItemId.OnewayPink || tileId == ItemId.OnewayOrange ||
                        tileId == ItemId.OnewayYellow || tileId == ItemId.OnewayGray || tileId == ItemId.OnewayBlue ||
                        tileId == ItemId.OnewayRed || tileId == ItemId.OnewayGreen || tileId == ItemId.OnewayBlack)
                    {
                        if (ItemId.CanJumpThroughFromBelow(tileId))
                        {
                            if ((p.SpeedY < 0 || a <= p.Overlapy) && rot == 1)
                            {
                                if (a != firstY || p.Overlapy == -1)
                                {
                                    p.Overlapy = a;
                                }

                                skip = true;
                                continue;
                            }

                            if ((p.SpeedX > 0 || b <= p.Overlapy) && rot == 2)
                            {
                                if (b == firstX || p.Overlapy == -1)
                                {
                                    p.Overlapy = b;
                                }

                                skip = true;
                                continue;
                            }

                            if ((p.SpeedY > 0 || a <= p.Overlapy) && rot == 3)
                            {
                                if (a == firstY || p.Overlapy == -1)
                                {
                                    p.Overlapy = a;
                                }

                                skip = true;
                                continue;
                            }
                            if ((p.SpeedX < 0 || b <= p.Overlapy) && rot == 0)
                            {
                                if (b != firstX || p.Overlapy == -1)
                                {
                                    p.Overlapy = b;
                                }

                                skip = true;
                                continue;
                            }
                        }
                    }
                    else if (ItemId.IsHalfBlock(tileId))
                    {
                        if (rot == 1)
                        {
                            if (!playerRectangle.IntersectsWith(new Rectangle(b * 16, a * 16 + 8, 16, 8))) continue;
                        }
                        else if (rot == 2)
                        {
                            if (!playerRectangle.IntersectsWith(new Rectangle(b * 16, a * 16, 8, 16))) continue;
                        }
                        else if (rot == 3)
                        {
                            if (!playerRectangle.IntersectsWith(new Rectangle(b * 16, a * 16, 16, 8))) continue;
                        }
                        else if (rot == 0)
                        {
                            if (!playerRectangle.IntersectsWith(new Rectangle(b * 16 + 8, a * 16, 8, 16))) continue;
                        }
                    }
                    else if (ItemId.CanJumpThroughFromBelow(tileId))
                    {
                        if (p.SpeedY < 0 || a <= p.Overlapy)
                        {
                            if (a != y || p.Overlapy == -1) p.Overlapy = a;

                            skip = true;
                            continue;
                        }
                    }

                    switch (tileId)
                    {
                        case 23:
                            if (hideRed)
                            {
                                continue;
                            }
                            break;
                        case 24:
                            if (hideGreen)
                            {
                                continue;
                            }
                            break;
                        case 25:
                            if (hideBlue)
                            {
                                continue;
                            }
                            break;
                        case 26:
                            if (!hideRed)
                            {
                                continue;
                            }
                            break;
                        case 27:
                            if (!hideGreen)
                            {
                                continue;
                            }
                            break;
                        case 28:
                            if (!hideBlue)
                            {
                                continue;
                            }
                            break;
                        case 156:
                            if (hideTimedoor)
                            {
                                continue;
                            }
                            break;
                        case 157:
                            if (!hideTimedoor)
                            {
                                continue;
                            }
                            break;
                        case ItemId.CyanDoor:
                            if (hideCyan)
                            {
                                continue;
                            }
                            break;
                        case ItemId.MagentaDoor:
                            if (hideMagenta)
                            {
                                continue;
                            }
                            break;
                        case ItemId.YellowDoor:
                            if (hideYellow)
                            {
                                continue;
                            }
                            break;
                        case ItemId.CyanGate:
                            if (!hideCyan)
                            {
                                continue;
                            }
                            break;
                        case ItemId.MagentaGate:
                            if (!hideMagenta)
                            {
                                continue;
                            }
                            break;
                        case ItemId.YellowGate:
                            if (!hideYellow)
                            {
                                continue;
                            }
                            break;
                        case ItemId.DoorPurple:
                            {
                                int pid = blockData[x][y][0];
                                if (p.Switches[pid])
                                {
                                    continue;
                                }
                            }
                            break;
                        case ItemId.GatePurple:
                            {
                                int pid = blockData[x][y][0];
                                if (!p.Switches[pid])
                                {
                                    continue;
                                }
                            }
                            break;
                        case ItemId.DeathDoor:
                            if (p.Deaths >= blockData[x][y][0])
                            {
                                continue;
                            }
                            break;
                        case ItemId.DeathGate:
                            if (p.Deaths < blockData[x][y][0])
                            {
                                continue;
                            }
                            break;
                        case ItemId.TeamDoor:
                            if (p.Team == GetBlockData(x, y)[0])
                            {
                                continue;
                            }
                            break;
                        case ItemId.TeamGate:
                            if (p.Team != GetBlockData(x, y)[0])
                            {
                                continue;
                            }
                            break;
                        case ItemId.DoorClub:
                            if (p.IsClubMember)
                            {
                                continue;
                            }
                            break;
                        case ItemId.GateClub:
                            if (!p.IsClubMember)
                            {
                                continue;
                            }
                            break;
                        case ItemId.Coindoor:
                        case ItemId.BlueCoindoor:
                            if (blockData[x][y][0] <= p.Coins)
                            {
                                continue;
                            }
                            break;
                        case ItemId.Coingate:
                        case ItemId.BlueCoingate:
                            if (blockData[x][y][0] > p.Coins)
                            {
                                continue;
                            }
                            break;
                        case ItemId.ZombieGate:
                            /*if (p.Zombie) {
                                        continue;
                                    };*/
                            break;
                        case ItemId.ZombieDoor:
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
                            if (p.SpeedY < 0 || y <= p.Overlapy)
                            {
                                if (y != firstY || p.Overlapy == -1)
                                {
                                    p.Overlapy = y;
                                }
                                skip = true;
                                continue;
                            }
                            break;
                        case 83:
                        case 77:
                            continue;
                    }

                    return true;
                }
                y++;
            }
            if (!skip)
            {
                p.Overlapy = -1;
            }
            return false;
        }

        internal void DeserializeBlocks(Message m)
        {
            var data = InitParse.Parse(m);
            foreach (var d in data)
            {
                foreach (var p in d.Locations)
                {
                    blocks[d.Layer][p.x][p.y] = (int)d.Type;
                    if (d.Layer == 0 && d.Args.Length > 0)
                    {
                        List<int> bdata = new List<int>();
                        foreach (var o in d.Args)
                        {
                            if (o is int) bdata.Add((int)o);
                            else if (o is uint) bdata.Add((int)((uint)o));
                        }
                        blockData[p.x][p.y] = bdata.ToArray();
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (PhysicsRunning)
                StopSimulation();
        }
    }

    public class Rectangle
    {
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }

        public Rectangle(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool IntersectsWith(Rectangle r)
        {
            return (X + Width >= r.X && X < r.X + r.Width) && (Y + Height >= r.Y && Y < r.Y + r.Height);
        }
    }
}
