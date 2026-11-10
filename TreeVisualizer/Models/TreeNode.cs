namespace TreeVisualizer;

public class TreeNode
{
    public char Value { get; set; }
    public TreeNode? Left { get; set; }
    public TreeNode? Right { get; set; }
    public TreeNode? Parent { get; set; }

    // Координаты для отрисовки
    public double X { get; set; }
    public double Y { get; set; }
    public double SubtreeWidth { get; set; }

    public TreeNode(char value)
    {
        Value = value;
    }
}