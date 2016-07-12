using System.Threading.Tasks;
using System.Threading;
using PlayerIOClient;
using EEPhysics;

namespace Sample
{
    class Program
    {
        private static Connection Connection { get; set; }
        private static PhysicsWorld World { get; set; }

        static void Main(string[] args)
        {
            Connection = PlayerIO.QuickConnect.SimpleConnect("everybody-edits-su9rn58o40itdbnw69plyw", "guest", "guest", null).Multiplayer.JoinRoom("PW01", null);
            World = new PhysicsWorld();

            Connection.OnMessage += (sender, m) =>
            {
                World.HandleMessage(m);
                if (m.Type == "init") Connection.Send("init2");
                else if (m.Type == "init2") StalkPlayers();
            };
            Connection.Send("init");

            Thread.Sleep(-1);
        }

        private static async void StalkPlayers()
        {
            while (true)
            {
                foreach (var player in World.Players.Values)
                {
                    Connection.Send("b", 0, (int)player.X / 16, (int)player.Y / 16, 9);
                    await Task.Delay(10);
                }
            }
        }
    }
}
