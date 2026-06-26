using CodecTactics.Core.Network;
using Godot;

namespace CodecTactics.Godot;

public partial class PrototypeScene : Control
{
    private const float NodeRadius = 24f;
    private const float BoardLeft = 96f;
    private const float BoardTop = 292f;
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
    private readonly Color _unstableColor = new(1.0f, 0.56f, 0.2f);
    private readonly Color _objectiveColor = new(0.98f, 0.94f, 0.36f);

    private NetworkGame _game = NetworkGame.CreateVerticalSliceMission();
    private PlayerActionMode _selectedAction = PlayerActionMode.Claim;
    private Label _turnLabel = default!;
    private Label _phaseLabel = default!;
    private Label _energyLabel = default!;
    private Label _actionLabel = default!;
    private Label _objectiveLabel = default!;
    private Label _hoverLabel = default!;
    private Label _statusLabel = default!;
    private Label _resultLabel = default!;
    private Button _claimButton = default!;
    private Button _reinforceButton = default!;
    private Button _weakenButton = default!;
    private Button _endTurnButton = default!;
    private Button _restartButton = default!;
    private string _status = string.Empty;
    private string _hoverStatus = "Hover a node to inspect it.";

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        _status = $"Mission ready: {_game.ObjectiveText}";

        _turnLabel = CreateHudLabel(new Vector2(32, 24), "Turn 1");
        _phaseLabel = CreateHudLabel(new Vector2(32, 52), "Phase: Player");
        _energyLabel = CreateHudLabel(new Vector2(32, 80), "Energy: 5");
        _actionLabel = CreateHudLabel(new Vector2(32, 108), "Action: Claim");
        _objectiveLabel = CreateHudLabel(new Vector2(32, 136), "Objective");
        _hoverLabel = CreateHudLabel(new Vector2(32, 164), _hoverStatus);
        _statusLabel = CreateHudLabel(new Vector2(32, 192), _status);
        _resultLabel = CreateHudLabel(new Vector2(32, 220), "Result: In progress");

        _claimButton = CreateActionButton(new Vector2(420, 24), "Claim", PlayerActionMode.Claim);
        _reinforceButton = CreateActionButton(new Vector2(520, 24), "Reinforce", PlayerActionMode.Reinforce);
        _weakenButton = CreateActionButton(new Vector2(650, 24), "Weaken", PlayerActionMode.Weaken);
        _endTurnButton = new Button
        {
            Text = "End Turn",
            Position = new Vector2(420, 76),
            Size = new Vector2(120, 40)
        };
        _endTurnButton.Pressed += OnEndTurnPressed;
        AddChild(_endTurnButton);

        _restartButton = new Button
        {
            Text = "Restart Mission",
            Position = new Vector2(560, 76),
            Size = new Vector2(160, 40)
        };
        _restartButton.Pressed += OnRestartPressed;
        AddChild(_restartButton);

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

            if (node.IsUnstable)
            {
                DrawArc(position, NodeRadius + 14f, 0f, Mathf.Tau, 48, _unstableColor, 5f);
            }

            if (_game.ObjectiveNode == node.Id)
            {
                DrawArc(position, NodeRadius + 20f, 0f, Mathf.Tau, 48, _objectiveColor, 5f);
                DrawString(ThemeDB.FallbackFont, position + new Vector2(-16f, -48f), "OBJ", HorizontalAlignment.Left, -1f, 14, _objectiveColor);
            }

            DrawString(ThemeDB.FallbackFont, position + new Vector2(-12f, 5f), $"{node.Id.X},{node.Id.Y}", HorizontalAlignment.Left, -1f, 14, Colors.Black);
            DrawString(ThemeDB.FallbackFont, position + new Vector2(-7f, -30f), GetNodeTypeLabel(node.Type), HorizontalAlignment.Left, -1f, 14, GetNodeTypeColor(node));
            DrawString(ThemeDB.FallbackFont, position + new Vector2(-18f, 44f), $"I{node.Integrity}/T{node.Threat}", HorizontalAlignment.Left, -1f, 12, node.IsUnstable ? _unstableColor : _outlineColor);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion motionEvent)
        {
            var hoveredNode = FindClickedNode(motionEvent.Position);
            _hoverStatus = hoveredNode.HasValue
                ? $"Hover: {FormatNodeStatus(_game.Board.GetNode(hoveredNode.Value))}"
                : "Hover a node to inspect it.";
            UpdateHud();
            return;
        }

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
            _status = "Game is complete. Use Restart Mission to play again.";
            UpdateHud();
            return;
        }

        var result = _game.ExecutePlayerAction(_selectedAction, clickedNode.Value);
        _status = result.Message;
        UpdateHud();
    }

    private void OnEndTurnPressed()
    {
        if (_game.Result != GameResult.InProgress)
        {
            _status = "Game is complete. Use Restart Mission to play again.";
            UpdateHud();
            return;
        }

        var result = _game.EndPlayerTurnWithResult();
        _status = result.Message;
        UpdateHud();
    }

    private void OnRestartPressed()
    {
        _game = NetworkGame.CreateVerticalSliceMission();
        _selectedAction = PlayerActionMode.Claim;
        _status = $"Mission restarted: {_game.ObjectiveText}";
        _hoverStatus = "Hover a node to inspect it.";
        UpdateHud();
    }

    private Button CreateActionButton(Vector2 position, string text, PlayerActionMode mode)
    {
        var button = new Button
        {
            Text = text,
            Position = position,
            Size = new Vector2(110, 40)
        };
        button.Pressed += () =>
        {
            _selectedAction = mode;
            _status = $"Selected action: {mode}.";
            UpdateHud();
        };
        AddChild(button);
        return button;
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
        _actionLabel.Text = $"Selected action: {_selectedAction}";
        _objectiveLabel.Text = $"Objective: secure {_game.ObjectiveNode} for {_game.ObjectiveHoldTurns}/{_game.RequiredObjectiveHoldTurns} turns";
        _hoverLabel.Text = _hoverStatus;
        _statusLabel.Text = $"Status: {_status}";
        _resultLabel.Text = $"Result: {FormatResult(_game.Result)}";
        var gameOver = _game.Result != GameResult.InProgress;
        _claimButton.Disabled = gameOver;
        _reinforceButton.Disabled = gameOver;
        _weakenButton.Disabled = gameOver;
        _endTurnButton.Disabled = gameOver;
        QueueRedraw();
    }

    private string FormatNodeStatus(NodeState node)
    {
        var unstable = node.IsUnstable
            ? $" Unstable {node.UnstableTurns}/{_game.Configuration.InstabilityTurnsBeforeCollapse}."
            : string.Empty;

        return $"{node.Id} {node.Owner} {node.Type}: integrity {node.Integrity}, threat {node.Threat}.{unstable} Reason: {node.DangerReason}";
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
