namespace EEPhysics
{
    public class BlockData
    {
        public int Layer { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Id { get; set; }
        public object[] Args { get; set; }

        public BlockData(int layer, int x, int y, int id, params object[] args)
        {
            Layer = layer;
            X = x;
            Y = y;
            Id = id;
            Args = args;
        }
    }
}
