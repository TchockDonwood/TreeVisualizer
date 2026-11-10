using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using TreeVisualizer.Views;

namespace TreeVisualizer;

public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<VisualNode> Nodes { get; } = new();
    public ObservableCollection<VisualConnection> Connections { get; } = new();
    public ObservableCollection<string> Logs { get; } = new();

    private TreeNode? _root; // <-- храним корень

    private string _inputText = "ABD*G***CE**FH**J**";
    public string InputText
    {
        get => _inputText;
        set { _inputText = value; OnPropertyChanged(); }
    }

    private bool _isRunning;
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsNotRunning));
        }
    }
    public bool IsNotRunning => !IsRunning;

    private double _treeWidth = 800;
    public double TreeWidth
    {
        get => _treeWidth;
        set { _treeWidth = value; OnPropertyChanged(); }
    }

    private double _treeHeight = 600;
    public double TreeHeight
    {
        get => _treeHeight;
        set { _treeHeight = value; OnPropertyChanged(); }
    }

    private double _scale = 1.0;
    public double Scale
    {
        get => _scale;
        set { _scale = Math.Clamp(value, 0.2, 3.0); OnPropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }

    // Константы
    private const double NodeWidth = 60;
    private const double NodeHeight = 40;
    private const double HorizontalGap = 40;
    private const double VerticalGap = 100;

    public MainViewModel()
    {
        StartCommand = new RelayCommand(async _ => await RunAnimation(), _ => IsNotRunning);
        ResetCommand = new RelayCommand(_ => Reset(), _ => IsNotRunning);
        ZoomInCommand = new RelayCommand(_ => Scale += 0.1);
        ZoomOutCommand = new RelayCommand(_ => Scale -= 0.1);
    }

    private void Reset()
    {
        Nodes.Clear();
        Connections.Clear();
        Logs.Clear();
        _root = null;
    }

    private async Task RunAnimation()
    {
        Reset();
        IsRunning = true;
        Logs.Add("Начало построения...");

        int index = 0;
        var steps = new List<ActionStep>();

        try
        {
            _root = Parse(InputText, ref index, null, false, steps);

            if (index < InputText.Length)
                throw new Exception($"Лишние символы в конце строки: '{InputText.Substring(index)}'");
            if (_root == null)
                throw new Exception("Дерево пустое (корень равен NULL).");

            PerformLayout();

            foreach (var step in steps)
            {
                if (step.Node != null)
                {
                    var vNode = new VisualNode(step.Node);
                    Nodes.Add(vNode);

                    if (step.Parent != null)
                        Connections.Add(new VisualConnection(step.Parent, step.Node, step.IsLeft));
                }
                else if (step.Parent != null)
                {
                    var vParent = Nodes.FirstOrDefault(n => n.Model == step.Parent);
                    if (vParent != null)
                    {
                        if (step.IsLeft) vParent.LeftText = "NULL";
                        else vParent.RightText = "NULL";
                    }
                }

                Logs.Add(step.Log);
                await Task.Delay(500);
            }

            Logs.Add("Построение завершено успешно.");
        }
        catch (Exception ex)
        {
            Logs.Add($"ОШИБКА: {ex.Message}");
        }
        finally
        {
            IsRunning = false;
        }
    }

    // --- Парсинг ---
    private TreeNode? Parse(string s, ref int index, TreeNode? parent, bool isLeft, List<ActionStep> steps)
    {
        if (index >= s.Length)
            throw new Exception($"Неожиданный конец строки. Ожидался {(parent != null ? (isLeft ? "левый" : "правый") + " потомок для " + parent.Value : "корень")}.");

        char c = s[index++];
        string pos = parent != null ? (isLeft ? "левый" : "правый") : "корневой";

        if (c == '*')
        {
            steps.Add(new ActionStep
            {
                IsLeft = isLeft,
                Parent = parent,
                Node = null,
                Log = $"Шаг {index}: Считан '*'. {pos} потомок {(parent != null ? $"узла {parent.Value}" : "")} пуст (NULL)."
            });
            return null;
        }

        if (!char.IsLetter(c))
            throw new Exception($"Недопустимый символ '{c}'. Ожидалась буква или '*'.");

        TreeNode node = new TreeNode(c) { Parent = parent }; // <-- устанавливаем родителя
        steps.Add(new ActionStep
        {
            IsLeft = isLeft,
            Parent = parent,
            Node = node,
            Log = $"Шаг {index}: Считан '{c}'. Создан {pos} узел."
        });

        node.Left = Parse(s, ref index, node, true, steps);
        node.Right = Parse(s, ref index, node, false, steps);

        return node;
    }

    // --- Layout ---
    private void PerformLayout()
    {
        if (_root == null) return;

        double currentX = 0;
        CalculatePositions(_root, 0, ref currentX);

        double minX = double.MaxValue, maxX = double.MinValue;
        double minY = double.MaxValue, maxY = double.MinValue;
        GetBounds(_root, ref minX, ref maxX, ref minY, ref maxY);

        // Вычисляем отступы, чтобы дерево было по центру
        double padding = 50;
        ShiftTree(_root, -minX + padding, -minY + padding);

        // Задаем размеры холста, чтобы появились скроллбары при необходимости
        TreeWidth = (maxX - minX) + NodeWidth + padding * 2;
        TreeHeight = (maxY - minY) + NodeHeight + padding * 2;
    }

    private void CalculatePositions(TreeNode? node, int level, ref double currentX)
    {
        if (node == null) return;

        node.Y = level * VerticalGap;

        if (node.Left == null && node.Right == null)
        {
            // Лист
            node.X = currentX;
            currentX += NodeWidth + HorizontalGap;
        }
        else if (node.Left != null && node.Right == null)
        {
            // Только левый потомок (смещаем родителя вправо для скоса)
            CalculatePositions(node.Left, level + 1, ref currentX);
            node.X = node.Left.X + 25;
        }
        else if (node.Left == null && node.Right != null)
        {
            // Только правый потомок (смещаем родителя влево для скоса)
            CalculatePositions(node.Right, level + 1, ref currentX);
            node.X = node.Right.X - 25;
        }
        else
        {
            // Оба потомка
            CalculatePositions(node.Left, level + 1, ref currentX);
            CalculatePositions(node.Right, level + 1, ref currentX);
            node.X = (node.Left!.X + node.Right!.X) / 2;
        }
    }

    private void GetBounds(TreeNode? node, ref double minX, ref double maxX, ref double minY, ref double maxY)
    {
        if (node == null) return;
        minX = Math.Min(minX, node.X);
        maxX = Math.Max(maxX, node.X);
        minY = Math.Min(minY, node.Y);
        maxY = Math.Max(maxY, node.Y);

        GetBounds(node.Left, ref minX, ref maxX, ref minY, ref maxY);
        GetBounds(node.Right, ref minX, ref maxX, ref minY, ref maxY);
    }

    private void ShiftTree(TreeNode? node, double offsetX, double offsetY)
    {
        if (node == null) return;
        node.X += offsetX;
        node.Y += offsetY;
        ShiftTree(node.Left, offsetX, offsetY);
        ShiftTree(node.Right, offsetX, offsetY);
    }

    private void RebuildVisualTree()
    {
        Nodes.Clear();
        Connections.Clear();
        if (_root == null) return;

        PerformLayout();
        BuildVisualNodes(_root);
    }

    // --- Редактирование ---
    public void AddNode(TreeNode parent, bool isLeft)
    {
        var dialog = new InputDialog($"Введите значение для нового {(isLeft ? "левого" : "правого")} потомка узла '{parent.Value}':")
        {
            Owner = Application.Current.MainWindow
        };
        if (dialog.ShowDialog() != true) return;

        string input = dialog.Result.Trim();
        if (string.IsNullOrEmpty(input) || !char.IsLetter(input[0]))
        {
            Logs.Add("Ошибка: значение должно быть буквой.");
            return;
        }
        char value = input[0];

        var newNode = new TreeNode(value) { Parent = parent };
        if (isLeft) parent.Left = newNode;
        else parent.Right = newNode;

        Logs.Add($"Добавлен {(isLeft ? "левый" : "правый")} потомок '{value}' для узла '{parent.Value}'.");
        RebuildVisualTree();
        UpdateInputString();
    }

    public void DeleteNode(TreeNode node)
    {
        if (node.Parent == null)
        {
            Logs.Add("Нельзя удалить корневой узел.");
            return;
        }
        if (node.Left != null || node.Right != null)
        {
            Logs.Add("Можно удалять только листья.");
            return;
        }

        var parent = node.Parent;
        if (parent.Left == node) parent.Left = null;
        else if (parent.Right == node) parent.Right = null;

        Logs.Add($"Удален лист '{node.Value}'.");
        RebuildVisualTree();
        UpdateInputString();
    }

    private void BuildVisualNodes(TreeNode node)
    {
        var vNode = new VisualNode(node);
        if (node.Left == null) vNode.LeftText = "NULL";
        if (node.Right == null) vNode.RightText = "NULL";
        Nodes.Add(vNode);

        if (node.Left != null) Connections.Add(new VisualConnection(node, node.Left, true));
        if (node.Right != null) Connections.Add(new VisualConnection(node, node.Right, false));

        if (node.Left != null) BuildVisualNodes(node.Left);
        if (node.Right != null) BuildVisualNodes(node.Right);
    }

    private void UpdateInputString()
    {
        InputText = _root == null ? "*" : Serialize(_root);
    }

    private string Serialize(TreeNode? node)
    {
        if (node == null) return "*";
        return node.Value + Serialize(node.Left) + Serialize(node.Right);
    }

    // --- Вспомогательный класс ---
    private class ActionStep
    {
        public TreeNode? Node { get; set; }
        public TreeNode? Parent { get; set; }
        public bool IsLeft { get; set; }
        public string Log { get; set; } = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}