using CodecTactics.Core.Network;
using Godot;

namespace CodecTactics.Godot;

public partial class PrototypeScene : Control
{
    private const float NodeRadius = 24f;
    private const float BoardLeft = 96f;
    private const float BoardTop = 144f;
    private const float CellSize = 96f;

    private readonly Color _backgroundColor = new(0.06f, 0.07f, 0.09f);
    private readonly Color _connectionColor = new(0.42f, 0.47f, 0.55f);
    private readonly Color _inactiveConnectionColor = new(0.18f, 0.2f, 0.24f);
    private readonly Color _neutralColor = new(0.62f, 0.66f, 0.72f);
    private readonly Color _playerColor = new(0.18f, 0.73f, 0.48f);
    private readonly Color _enemyColor = new(0.86f, 0.24f, 0.28f);
    private readonly Color _outlineColor = new(0.88f, 0.9f, 0.94f);
    private readonly Color _reinforcedColor = new(0.42f, 0.86f, 1.0f);
    private readonly Color _resourceColor = new(0.95f, 0.78f, 0.24f);
    private readonly Color _relayColor = new(0.38f, 0.64f, 1.0f);
    private readonly Color _firewallColor = new(0.74f, 0.42f, 0.96f);

    private NetworkGame _game = NetworkGame.CreateDefault();
    private Label _turnLabel = default!;
    private Label _phaseLabel = default!;
    private Label _energyLabel = default!;
    private Label _statusLabel = default!;
    private Label _resultLabel = default!;
    private Button _endTurnButton = default!;
    private string _status = $"Click a reachable neutral node to claim it for {NetworkRules.ClaimEnergyCost} energy.";

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;

        _turnLabel = CreateHudLabel(new Vector2(32, 24), "Turn 1");
        _phaseLabel = CreateHudLabel(new Vector2(32, 52), "Phase: Player");
        _energyLabel = CreateHudLabel(new Vector2(32, 80), "Energy: 5");
        _statusLabel = CreateHudLabel(new Vector2(32, 108), _status);
        _resultLabel = CreateHudLabel(new Vector2(32, 136), "Result: In progress");

        _endTurnButton = new Button
        {
            Text = "End Turn",
            Position = new Vector2(420, 32),
            Size = new Vector2(140, 40)
        };
        _endTurnButton.Pressed += OnEndTurnPressed;
        AddChild(_endTurnButton);

        UpdateHud();
    }

    public override void _Draw()
    {
        DrawRect(GetViewportRect(), _backgroundColor);

        foreach (var connection in _game.Board.Connections)
        {
            var color = connection.IsActive ? _connectionColor : _inactiveConnectionColor;
            DrawLine(GetNodePosition(connection.First), GetNodePosition(connection.Second), color, 5f);
        }

        foreach (var node in _game.Board.Nodes.OrderBy(node => node.Id))
        {
            var position = GetNodePosition(node.Id);
            DrawCircle(position, NodeRadius, GetNodeColor(node));
            DrawArc(position, NodeRadius + 3f, 0f, Mathf.Tau, 48, _outlineColor, 2f);
            DrawArc(position, NodeRadius + 6f, 0f, Mathf.Tau, 48, GetNodeTypeColor(node), 3f);

            if (node.Integrity > 1)
            {
                DrawArc(position, NodeRadius + 9f, 0f, Mathf.Tau, 48, _reinforcedColor, 4f);
            }

            DrawString(ThemeDB.FallbackFont, position + new Vector2(-12f, 5f), $"{node.Id.X},{node.Id.Y}", HorizontalAlignment.Left, -1f, 14, Colors.Black);
            DrawString(ThemeDB.FallbackFont, position + new Vector2(-7f, -30f), GetNodeTypeLabel(node.Type), HorizontalAlignment.Left, -1f, 14, GetNodeTypeColor(node));
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } mouseEvent)
        {
            return;
        }

        var clickedNode = FindClickedNode(mouseEvent.Position);
        if (clickedNode is null)
        {
            return;
        }

        if (_game.Result != GameResult.InProgress)
        {
            _status = "Game is complete. Restart the scene to play again.";
            UpdateHud();
            return;
        }

        var node = _game.Board.GetNode(clickedNode.Value);
        if (node.Owner != NodeOwner.Neutral)
        {
            _status = $"{clickedNode} is already {node.Owner}.";
            UpdateHud();
            return;
        }

        var result = _game.ClaimNodeWithResult(clickedNode.Value);
        _status = result.Message;
        UpdateHud();
    }

    private void OnEndTurnPressed()
    {
        if (_game.Result != GameResult.InProgress)
        {
            _status = "Game is complete. Restart the scene to play again.";
            UpdateHud();
            return;
        }

        var result = _game.EndPlayerTurnWithResult();
        _status = result.Message;
        UpdateHud();
    }

    private Label CreateHudLabel(Vector2 position, string text)
    {
        var label = new Label
        {
            Text = text,
            Position = position,
            Size = new Vector2(760, 24)
        };
        AddChild(label);
        return label;
    }

    private void UpdateHud()
    {
        _turnLabel.Text = $"Turn: {_game.TurnNumber}";
        _phaseLabel.Text = $"Phase: {_game.Phase}";
        _energyLabel.Text = $"Energy: {_game.PlayerEnergy} | Corruption pressure: {_game.CorruptionPressure}";
        _statusLabel.Text = $"Status: {_status}";
        _resultLabel.Text = $"Result: {FormatResult(_game.Result)}";
        _endTurnButton.Disabled = _game.Result != GameResult.InProgress;
        QueueRedraw();
    }

    private NodeId? FindClickedNode(Vector2 position)
    {
        foreach (var node in _game.Board.Nodes)
        {
            if (position.DistanceTo(GetNodePosition(node.Id)) <= NodeRadius)
            {
                return node.Id;
            }
        }

        return null;
    }

    private Vector2 GetNodePosition(NodeId nodeId)
    {
        return new Vector2(BoardLeft + nodeId.X * CellSize, BoardTop + nodeId.Y * CellSize);
    }

    private Color GetNodeColor(NodeState node)
    {
        return node.Owner switch
        {
            NodeOwner.Player => _playerColor,
            NodeOwner.Enemy => _enemyColor,
            _ => _neutralColor
        };
    }

    private Color GetNodeTypeColor(NodeState node)
    {
        return node.Type switch
        {
            NodeType.Resource => _resourceColor,
            NodeType.Relay => _relayColor,
            NodeType.Firewall => _firewallColor,
            _ => _outlineColor
        };
    }

    private static string GetNodeTypeLabel(NodeType type)
    {
        return type switch
        {
            NodeType.Resource => "R",
            NodeType.Relay => "L",
            NodeType.Firewall => "F",
            _ => "S"
        };
    }

    private static string FormatResult(GameResult result)
    {
        return result switch
        {
            GameResult.PlayerWin => "Player win",
            GameResult.PlayerLoss => "Player loss",
            _ => "In progress"
        };
    }
}
