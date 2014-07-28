﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using PlayerIOClient;

namespace EEPhysics
{

    public class PhysicsWorld
    {
        internal const int Size = 16;
        internal Stopwatch sw = new Stopwatch();
        private List<Message> earlyMessages = new List<Message>();
        private bool inited;
        private bool running;
        private Thread physicsThread;

        private int[][] foregroundTiles;
        private int[][][] tileData;
        private bool hideRed, hideBlue, hideGreen, hideTimedoor;
        internal double WorldGravity = 1;

        public int WorldWidth { get; private set; }
        public int WorldHeight { get; private set; }

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
        public ConcurrentDictionary<int, PhysicsPlayer> Players { get; private set; }
        public string WorldKey { get; private set; }
        public event EventHandler OnTick = delegate { };

        public PhysicsWorld()
        {
            AutoStart = true;
            AddBotPlayer = true;
            Players = new ConcurrentDictionary<int, PhysicsPlayer>();
        }

        /// <summary>
        /// Will run the physics simulation. Needs to be called only once.
        /// </summary>
        public void Run()
        {
            running = true;
            PhysicsRunning = true;

            sw.Start();
            long waitTime, frameStartTime, frameEndTime;
            while (running)
            {
                frameStartTime = sw.ElapsedMilliseconds;
                foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
                {
                    pair.Value.tick();
                }
                OnTick(this, null);
                frameEndTime = sw.ElapsedMilliseconds;
                waitTime = 10 - (frameEndTime - frameStartTime);
                if (waitTime > 0)
                    Thread.Sleep((int)waitTime);
            }

            PhysicsRunning = false;
        }

        public void HandleMessage(Message m)
        {
            if (!inited && m.Type != "init")
            {
                earlyMessages.Add(m);
                return;
            }
            switch (m.Type)
            {
                case "m":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p))
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
                            int xx = m.GetInt(1);
                            int yy = m.GetInt(2);
                            int blockId = m.GetInt(3);
                            foregroundTiles[xx][yy] = blockId;
                            if (blockId == 100)
                            {
                                foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
                                    pair.Value.removeCoin(xx, yy);
                            }
                            else if (blockId == 101)
                            {
                                foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
                                    pair.Value.removeBlueCoin(xx, yy);
                            }
                        }
                    }
                    break;
                case "add":
                    {
                        PhysicsPlayer p = new PhysicsPlayer(m.GetInt(0), m.GetString(1));
                        p.HostWorld = this;
                        p.X = m.GetDouble(3);
                        p.Y = m.GetDouble(4);
                        p.Coins = m.GetInt(8);
                        p.Purple = m.GetBoolean(9);
                        p.InGodMode = m.GetBoolean(5);
                        p.IsClubMember = m.GetBoolean(12);

                        Players.TryAdd(p.ID, p);
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
                            case "timedoor":
                                hideTimedoor = b;
                                break;
                            case "blue":
                                hideBlue = b;
                                break;
                            case "red":
                                hideRed = b;
                                break;
                            case "green":
                                hideGreen = b;
                                break;
                        }
                    }
                    break;
                case "bc":
                case "br":
                case "bs":
                    {
                        int xx = m.GetInt(0);
                        int yy = m.GetInt(1);
                        foregroundTiles[xx][yy] = m.GetInt(2);
                        tileData[xx][yy] = new int[m.Count - 4];
                        for (uint i = 3; i < 4; i++)
                        {
                            tileData[xx][yy][i - 3] = m.GetInt(i);
                        }
                    }
                    break;
                case "pt":
                    {
                        int xx = m.GetInt(0);
                        int yy = m.GetInt(1);
                        foregroundTiles[xx][yy] = m.GetInt(2);
                        tileData[xx][yy] = new int[m.Count - 3];
                        for (uint i = 3; i < 6; i++)
                        {
                            tileData[xx][yy][i - 3] = m.GetInt(i);
                        }
                    }
                    break;
                case "god":
                case "mod":
                    {
                        PhysicsPlayer p;
                        if (Players.TryGetValue(m.GetInt(0), out p))
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
                        bool b = m.GetBoolean(0);
                        uint i = 1;
                        while (i + 2 < m.Count)
                        {
                            PhysicsPlayer p;
                            if (Players.TryGetValue(m.GetInt(i), out p))
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
                        if (Players.TryGetValue(m.GetInt(0), out p))
                        {
                            p.X = m.GetInt(1);
                            p.Y = m.GetInt(2);
                        }
                    }
                    break;
                case "reset":
                    {
                        desBlocks(m, 0);
                        foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
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
                        int border = m.GetInt(2);
                        int fill = m.GetInt(3);
                        for (int i = 0; i < foregroundTiles.Length; i++)
                        {
                            for (int ii = 0; ii < foregroundTiles[i].Length; ii++)
                            {
                                if (i == 0 || ii == 0 || i == WorldWidth - 1 || ii == WorldHeight - 1)
                                {
                                    foregroundTiles[i][ii] = border;
                                }
                                else
                                {
                                    foregroundTiles[i][ii] = fill;
                                }
                            }
                        }
                        foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
                            pair.Value.resetCoins();
                    }
                    break;
                case "ts":
                case "lb":
                case "wp":
                    {
                        foregroundTiles[m.GetInt(0)][m.GetInt(1)] = m.GetInt(2);
                    }
                    break;
                case "init":
                    {
                        WorldWidth = m.GetInt(12);
                        WorldHeight = m.GetInt(13);

                        foregroundTiles = new int[WorldWidth][];
                        for (int i = 0; i < foregroundTiles.Length; i++)
                            foregroundTiles[i] = new int[WorldHeight];

                        tileData = new int[WorldWidth][][];
                        for (int i = 0; i < foregroundTiles.Length; i++)
                            tileData[i] = new int[WorldHeight][];

                        WorldKey = derot(m.GetString(5));
                        WorldGravity = m.GetDouble(15);

                        if (AddBotPlayer)
                        {
                            PhysicsPlayer p = new PhysicsPlayer(m.GetInt(6), m.GetString(9));
                            p.X = m.GetInt(7) * 16; p.Y = m.GetInt(8) * 16;
                            p.HostWorld = this;
                            Players.TryAdd(p.ID, p);
                        }

                        desBlocks(m, 18);
                        inited = true;

                        foreach (Message m2 in earlyMessages)
                        {
                            HandleMessage(m2);
                        }
                        earlyMessages.Clear();
                        earlyMessages = null;

                        if (AutoStart && (physicsThread == null || !physicsThread.IsAlive))
                        {
                            StartSimulation();
                        }
                    }
                    break;
            }
        }

        public PhysicsPlayer GetPlayer(int id)
        {
            PhysicsPlayer p;
            if (Players.TryGetValue(id, out p))
            {
                return p;
            }
            else
            {
                return null;
            }
        }
        public PhysicsPlayer GetPlayer(string name)
        {
            foreach (KeyValuePair<int, PhysicsPlayer> pair in Players)
            {
                if (pair.Value.Name == name)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        /// <returns>Foreground block ID</returns>
        public int GetBlock(int xx, int yy)
        {
            if (xx < 0 || xx >= foregroundTiles.Length || yy < 0 || yy >= foregroundTiles[0].Length)
            {
                return 0;
            }
            return foregroundTiles[xx][yy];
        }
        /// <returns>Extra block data, eg. rotation, id and target id from portals.</returns>
        public int[] GetBlockData(int xx, int yy)
        {
            if (xx < 0 || xx >= foregroundTiles.Length || yy < 0 || yy >= foregroundTiles[0].Length)
            {
                return null;
            }
            return tileData[xx][yy];
        }
        internal Point GetPortalById(int id)
        {
            for (int i = 0; i < foregroundTiles.Length; i++)
            {
                for (int ii = 0; ii < foregroundTiles[i].Length; ii++)
                {
                    if (foregroundTiles[i][ii] == 242 || foregroundTiles[i][ii] == 381)
                    {
                        if (tileData[i][ii][1] == id)
                        {
                            return new Point(i, ii);
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Starts the physics simulation thread.
        /// </summary>
        public void StartSimulation()
        {
            if (!PhysicsRunning)
            {
                if (inited)
                {
                    physicsThread = new Thread(new ThreadStart(Run));
                    physicsThread.Start();
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
        /// Stops physics simulation thread.
        /// </summary>
        public void StopSimulation()
        {
            if (PhysicsRunning)
            {
                running = false;
            }
            else
            {
                throw new Exception("Simulation thread is not running.");
            }
        }

        internal bool overlaps(PhysicsPlayer p)
        {
            if ((p.X < 0 || p.Y < 0) || ((p.X > WorldWidth * 16 - 16) || (p.Y > WorldHeight * 16 - 16)))
            {
                return true;
            }
            if (p.InGodMode)
            {
                return false;
            }
            int tileId;
            var firstX = ((int)p.X >> 4);
            var firstY = ((int)p.Y >> 4);
            double lastX = ((p.X + PhysicsPlayer.Height) / Size);
            double lastY = ((p.Y + PhysicsPlayer.Width) / Size);
            bool _local7 = false;

            int x;
            int y = firstY;
            while (y < lastY)
            {
                x = firstX;
                for (; x < lastX; x++)
                {
                    tileId = foregroundTiles[x][y];
                    if (ItemId.isSolid(tileId))
                    {
                        switch (tileId)
                        {
                            case 23:
                                if (hideRed)
                                {
                                    continue;
                                };
                                break;
                            case 24:
                                if (hideGreen)
                                {
                                    continue;
                                };
                                break;
                            case 25:
                                if (hideBlue)
                                {
                                    continue;
                                };
                                break;
                            case 26:
                                if (!hideRed)
                                {
                                    continue;
                                };
                                break;
                            case 27:
                                if (!hideGreen)
                                {
                                    continue;
                                };
                                break;
                            case 28:
                                if (!hideBlue)
                                {
                                    continue;
                                };
                                break;
                            case 156:
                                if (hideTimedoor)
                                {
                                    continue;
                                };
                                break;
                            case 157:
                                if (!hideTimedoor)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.DOOR_PURPLE:
                                if (p.Purple)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.GATE_PURPLE:
                                if (!p.Purple)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.DOOR_CLUB:
                                if (p.IsClubMember)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.GATE_CLUB:
                                if (!p.IsClubMember)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.COINDOOR:
                                if (tileData[x][y][0] <= p.Coins)
                                {
                                    continue;
                                };
                                break;
                            case ItemId.COINGATE:
                                if (tileData[x][y][0] > p.Coins)
                                {
                                    continue;
                                };
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
                                    };
                                    _local7 = true;
                                    continue;
                                };
                                break;
                            case 83:
                            case 77:
                                continue;
                        };
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


        internal static string derot(string arg1)
        {
            // by Capasha (http://pastebin.com/Pj6tvNNx)
            int num = 0;
            string str = "";
            for (int i = 0; i < arg1.Length; i++)
            {
                num = arg1[i];
                if ((num >= 0x61) && (num <= 0x7a))
                {
                    if (num > 0x6d) num -= 13;
                    else num += 13;
                }
                else if ((num >= 0x41) && (num <= 90))
                {
                    if (num > 0x4d) num -= 13;
                    else num += 13;
                }
                str = str + ((char)num);
            }
            return str;
        }

        internal void desBlocks(Message m, uint start)
        {
            // Got and modified from Skylight by TakoMan02 (made originally in VB by Bass5098), credit to them
            // (http://seist.github.io/Skylight/)
            // > https://github.com/Seist/Skylight/blob/master/Skylight/Miscellaneous/Tools.cs, method ConvertMessageToBlockList
            try
            {
                uint messageIndex = start;
                while (messageIndex < m.Count)
                {
                    if (m[messageIndex] is string)
                    {
                        break;
                    }

                    int blockId = m.GetInteger(messageIndex);
                    messageIndex++;

                    int z = m.GetInteger(messageIndex);
                    messageIndex++;

                    byte[] xa = m.GetByteArray(messageIndex);
                    messageIndex++;

                    byte[] ya = m.GetByteArray(messageIndex);
                    messageIndex++;

                    List<int> data = new List<int>();
                    if (blockId == 242 || blockId == 381)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            data.Add(m.GetInteger(messageIndex));
                            messageIndex++;
                        }
                    }
                    else if (blockId == 374 || blockId == 43 ||
                             blockId == 165 || blockId == 83 ||
                             blockId == 77 || blockId == 361 ||
                             (blockId > 374 && blockId < 381) ||
                             blockId == 1000 || blockId == 385)
                    {
                        if (blockId != 1000 && blockId != 385)
                        {
                            data.Add(m.GetInteger(messageIndex));
                        }
                        messageIndex++;
                    }
                    int x = 0, y = 0;

                    for (int pos = 0; pos < ya.Length; pos += 2)
                    {
                        x = (xa[pos] * 256) + xa[pos + 1];
                        y = (ya[pos] * 256) + ya[pos + 1];

                        if (blockId < 500)
                        {
                            foregroundTiles[x][y] = blockId;
                            if (data.Count > 0)
                            {
                                tileData[x][y] = data.ToArray();
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
    }
}