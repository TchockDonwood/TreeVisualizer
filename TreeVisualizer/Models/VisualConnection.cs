namespace TreeVisualizer;

public class VisualConnection
{
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }

    public VisualConnection(TreeNode parent, TreeNode child, bool isLeft)
    {
        // Если потомок левый, линия выходит из центра левой нижней ячейки (X + 15)
        // Если правый - из центра правой (X + 45)
        StartX = parent.X + (isLeft ? 15 : 45);
        StartY = parent.Y + 40;

        // Входит в центр верхней грани потомка
        EndX = child.X + 30;
        EndY = child.Y;
    }
}