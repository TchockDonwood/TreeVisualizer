using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TreeVisualizer;

public class VisualNode : INotifyPropertyChanged
{
    public TreeNode Model { get; }

    public string Value => Model.Value.ToString();
    public double X => Model.X;
    public double Y => Model.Y;
    public bool IsLeaf => Model.Left == null && Model.Right == null; // <-- добавлено

    private string _leftText = "";
    public string LeftText
    {
        get => _leftText;
        set { _leftText = value; OnPropertyChanged(); }
    }

    private string _rightText = "";
    public string RightText
    {
        get => _rightText;
        set { _rightText = value; OnPropertyChanged(); }
    }

    public VisualNode(TreeNode model)
    {
        Model = model;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}