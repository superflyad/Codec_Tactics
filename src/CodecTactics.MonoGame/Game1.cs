using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using CodecTactics.Core.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaPoint = Microsoft.Xna.Framework.Point;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;

namespace CodecTactics.MonoGame;

public class Game1 : Game
{
    private const int WindowWidth = 1360;
    private const int WindowHeight = 860;
    private const int HudWidth = 286;
    private const int ButtonHeight = 38;
    private const int TextPadding = 12;
    private const int MaxLogEntries = 4;
    private const float NodeWorldRadius = 42f;
    private const float CameraMargin = 120f;

    private static readonly XnaColor BackgroundColor = new(7, 10, 15);
    private static readonly XnaColor NetworkSurfaceColor = new(9, 15, 24);
    private static readonly XnaColor PanelColor = new(20, 25, 33, 238);
    private static readonly XnaColor PanelBorderColor = new(66, 78, 96);
    private static readonly XnaColor TextColor = new(232, 238, 247);
    private static readonly XnaColor MutedTextColor = new(151, 164, 184);
    private static readonly XnaColor AccentColor = new(75, 190, 235);
    private static readonly XnaColor ValidMoveColor = new(92, 224, 145);
    private static readonly XnaColor ObjectiveColor = new(249, 219, 83);
    private static readonly XnaColor WarningColor = new(255, 154, 70);
    private static readonly XnaColor LossColor = new(225, 68, 87);
    private static readonly XnaColor WinColor = new(85, 210, 135);
    private static readonly XnaColor NeutralColor = new(70, 82, 101);
    private static readonly XnaColor DisabledOverlayColor = new(4, 7, 12, 150);

    private static readonly IReadOnlyDictionary<NodeId, Vector2> VerticalSliceTopology = new Dictionary<NodeId, Vector2>
    {
        [new NodeId(0, 0)] = new(-280f, -250f),
        [new NodeId(1, 0)] = new(-120f, -300f),
        [new NodeId(2, 0)] = new(60f, -260f),
        [new NodeId(3, 0)] = new(220f, -310f),
        [new NodeId(4, 0)] = new(370f, -220f),
        [new NodeId(0, 1)] = new(-350f, -90f),
        [new NodeId(1, 1)] = new(-170f, -115f),
        [new NodeId(2, 1)] = new(5f, -90f),
        [new NodeId(3, 1)] = new(170f, -130f),
        [new NodeId(4, 1)] = new(340f, -55f),
        [new NodeId(0, 2)] = new(-410f, 75f),
        [new NodeId(1, 2)] = new(-220f, 70f),
        [new NodeId(2, 2)] = new(-30f, 50f),
        [new NodeId(3, 2)] = new(170f, 55f),
        [new NodeId(4, 2)] = new(350f, 95f),
        [new NodeId(0, 3)] = new(-330f, 235f),
        [new NodeId(1, 3)] = new(-150f, 220f),
        [new NodeId(2, 3)] = new(35f, 245f),
        [new NodeId(3, 3)] = new(215f, 225f),
        [new NodeId(4, 3)] = new(390f, 270f),
        [new NodeId(0, 4)] = new(-240f, 380f),
        [new NodeId(1, 4)] = new(-45f, 355f),
        [new NodeId(2, 4)] = new(130f, 385f),
        [new NodeId(3, 4)] = new(300f, 360f),
        [new NodeId(4, 4)] = new(470f, 395f)
    };

    private readonly GraphicsDeviceManager _graphics;
    private readonly Dictionary<TextKey, Texture2D> _textCache = new();
    private readonly List<string> _actionLog = new();
    private readonly List<ButtonDefinition> _buttons = new();

    private SpriteBatch _spriteBatch = default!;
    private Texture2D _pixel = default!;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private NetworkGame _game = NetworkGame.CreateVerticalSliceMission();
    private PlayerActionMode _selectedAction = PlayerActionMode.Claim;
    private string _status = "Mission ready. Expand toward the uplink.";
    private string _invalidReason = string.Empty;
    private NodeState _hoveredNode;
    private NodeId? _selectedNodeId;
    private Vector2 _cameraCenter;
    private Vector2 _targetCameraCenter;
    private float _zoom = 1f;
    private float _targetZoom = 1f;
    private double _totalSeconds;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = WindowWidth;
        _graphics.PreferredBackBufferHeight = WindowHeight;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Window.Title = "Codec_Tactics";
        RecenterCamera(immediate: true);
        Log("Reach and hold the uplink.");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { XnaColor.White });
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void UnloadContent()
    {
        foreach (var texture in _textCache.Values)
        {
            texture.Dispose();
        }

        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();
        _totalSeconds = gameTime.TotalGameTime.TotalSeconds;

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        HandleCameraInput(mouse, keyboard);
        _cameraCenter = Vector2.Lerp(_cameraCenter, _targetCameraCenter, 0.18f);
        _zoom = MathHelper.Lerp(_zoom, _targetZoom, 0.18f);
        _hoveredNode = GetNodeAt(mouse.Position);

        if (WasPressed(keyboard, Keys.D1))
        {
            SelectAction(PlayerActionMode.Claim);
        }
        else if (WasPressed(keyboard, Keys.D2))
        {
            SelectAction(PlayerActionMode.Reinforce);
        }
        else if (WasPressed(keyboard, Keys.D3))
        {
            SelectAction(PlayerActionMode.Weaken);
        }
        else if (WasPressed(keyboard, Keys.Space))
        {
            ApplyResult(_game.EndPlayerTurnWithResult());
        }
        else if (WasPressed(keyboard, Keys.R))
        {
            RestartMission();
        }
        else if (WasPressed(keyboard, Keys.C))
        {
            RecenterCamera(immediate: false);
        }

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            HandleClick(mouse.Position);
        }

        Window.Title = $"Codec_Tactics | {_selectedAction} | Turn {_game.TurnNumber} | Energy {_game.PlayerEnergy} | {_game.Result}";
        _previousKeyboard = keyboard;
        _previousMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawNetwork();
        DrawHud();
        DrawHoverTooltip();
        DrawResultBanner();
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawNetwork()
    {
        var viewport = GetBoardViewport();
        Fill(viewport, NetworkSurfaceColor);
        DrawNetworkBackdrop(viewport);

        foreach (var connection in _game.Board.Connections)
        {
            DrawConnection(connection);
        }

        foreach (var node in _game.Board.Nodes.OrderBy(node => GetNodeWorldPosition(node.Id).Y))
        {
            DrawNode(node);
        }

        DrawText("CODEC_TACTICS", viewport.X + 18, viewport.Y + 16, 22, TextColor);
        DrawText(_game.ObjectiveText, viewport.X + 20, viewport.Y + 48, 14, MutedTextColor, viewport.Width - 42);
    }

    private void DrawNetworkBackdrop(XnaRectangle viewport)
    {
        var drift = (float)(_totalSeconds * 18d % 42d);
        for (var x = viewport.X - 60; x < viewport.Right + 60; x += 42)
        {
            DrawLine(new Vector2(x + drift, viewport.Y), new Vector2(x - 140 + drift, viewport.Bottom), new XnaColor(23, 39, 55, 76), 1);
        }

        for (var y = viewport.Y + 78; y < viewport.Bottom; y += 78)
        {
            DrawLine(new Vector2(viewport.X, y), new Vector2(viewport.Right, y), new XnaColor(20, 36, 52, 52), 1);
        }

        DrawRectangle(viewport, new XnaColor(36, 67, 91), 2);
    }

    private void DrawConnection(ConnectionState connection)
    {
        var startNode = _game.Board.GetNode(connection.First);
        var endNode = _game.Board.GetNode(connection.Second);
        var start = WorldToScreen(GetNodeWorldPosition(connection.First));
        var end = WorldToScreen(GetNodeWorldPosition(connection.Second));
        var active = connection.IsActive;
        var baseColor = GetConnectionColor(startNode, endNode, active);
        var thickness = Math.Max(2, (int)(active ? 6 * _zoom : 3 * _zoom));

        DrawLine(start, end, new XnaColor(7, 12, 19, 210), thickness + 5);
        DrawLine(start, end, baseColor, thickness);

        if (!active)
        {
            DrawLine(start, end, new XnaColor(95, 103, 116, 95), 1);
            return;
        }

        var flowColor = GetFlowColor(startNode, endNode);
        var edge = end - start;
        var length = edge.Length();
        if (length <= 0f)
        {
            return;
        }

        var direction = Vector2.Normalize(edge);
        for (var i = 0; i < 3; i++)
        {
            var t = (float)((_totalSeconds * 0.34d + i * 0.31d) % 1d);
            var position = start + direction * (length * t);
            var particle = new XnaRectangle((int)position.X - 4, (int)position.Y - 4, 8, 8);
            Fill(particle, flowColor);
        }
    }

    private void DrawNode(NodeState node)
    {
        var center = WorldToScreen(GetNodeWorldPosition(node.Id));
        var radius = MathHelper.Clamp(NodeWorldRadius * _zoom, 30f, 54f);
        var isHovered = _hoveredNode?.Id == node.Id;
        var isSelected = _selectedNodeId == node.Id;
        var isObjective = _game.ObjectiveNode == node.Id;
        var preview = GetActionPreview(node);
        var canActOnAnyNode = _game.Result == GameResult.InProgress && HasAnyValidTargetForSelectedAction();
        var ownerColor = GetOwnerColor(node.Owner);
        var typeColor = GetTypeColor(node, isObjective);
        var pulse = GetPulse(3.1f, node.Id.X * 0.21f + node.Id.Y * 0.13f);

        DrawCircleOutline(center, radius + 15f + pulse * 5f, new XnaColor(typeColor.R, typeColor.G, typeColor.B, node.Owner == NodeOwner.Neutral ? (byte)42 : (byte)95), 2);
        DrawCircleOutline(center, radius + 8f, new XnaColor(ownerColor.R, ownerColor.G, ownerColor.B, (byte)105), 3);

        if (preview.IsValid)
        {
            DrawCircleOutline(center, radius + 20f + pulse * 3f, ValidMoveColor, 3);
        }
        else if (canActOnAnyNode)
        {
            DrawCircle(center, radius + 4f, DisabledOverlayColor);
        }

        if (isObjective)
        {
            DrawCircleOutline(center, radius + 26f + pulse * 8f, ObjectiveColor, 4);
        }
        else if (node.IsUnstable)
        {
            DrawCircleOutline(center, radius + 23f + pulse * 8f, WarningColor, 4);
        }

        DrawNodeSilhouette(node, center, radius, ownerColor, typeColor, isObjective);

        if (node.Owner == NodeOwner.Enemy)
        {
            DrawCorruptionOverlay(center, radius, pulse);
        }

        if (node.Owner == NodeOwner.Player)
        {
            DrawIntegrityBar(node, center, radius);
        }

        if (isSelected)
        {
            DrawCircleOutline(center, radius + 31f, XnaColor.White, 3);
        }

        if (isHovered)
        {
            DrawCircleOutline(center, radius + 37f, AccentColor, 3);
        }
    }

    private void DrawNodeSilhouette(NodeState node, Vector2 center, float radius, XnaColor ownerColor, XnaColor typeColor, bool isObjective)
    {
        var fill = Blend(ownerColor, typeColor, node.Type == NodeType.Standard && !isObjective ? 0.14f : 0.32f);
        var outline = node.Owner == NodeOwner.Enemy ? LossColor : typeColor;
        var iconColor = node.Owner == NodeOwner.Enemy ? new XnaColor(255, 189, 191) : TextColor;

        if (node.Id == _game.PlayerCore)
        {
            DrawCircle(center, radius, fill);
            DrawCircleOutline(center, radius, AccentColor, 4);
            DrawCircleOutline(center, radius * 0.62f, TextColor, 3);
            DrawLine(center + new Vector2(-radius * 0.36f, 0), center + new Vector2(radius * 0.36f, 0), iconColor, 4);
            DrawLine(center + new Vector2(0, -radius * 0.36f), center + new Vector2(0, radius * 0.36f), iconColor, 4);
            return;
        }

        if (isObjective)
        {
            DrawShield(center, radius, fill, ObjectiveColor);
            DrawCircleOutline(center, radius * 0.48f, ObjectiveColor, 3);
            DrawLine(center + new Vector2(-radius * 0.35f, 0), center + new Vector2(radius * 0.35f, 0), ObjectiveColor, 3);
            DrawLine(center + new Vector2(0, -radius * 0.35f), center + new Vector2(0, radius * 0.35f), ObjectiveColor, 3);
            return;
        }

        switch (node.Type)
        {
            case NodeType.Resource:
                DrawDiamond(center, radius, fill, outline);
                DrawLine(center + new Vector2(-radius * 0.34f, radius * 0.08f), center + new Vector2(radius * 0.34f, radius * 0.08f), iconColor, 4);
                DrawLine(center + new Vector2(-radius * 0.2f, -radius * 0.12f), center + new Vector2(radius * 0.2f, -radius * 0.12f), iconColor, 4);
                break;
            case NodeType.Relay:
                DrawHexagon(center, radius, fill, outline);
                DrawCircle(center, radius * 0.12f, iconColor);
                DrawLine(center, center + new Vector2(0, -radius * 0.45f), iconColor, 4);
                DrawLine(center, center + new Vector2(-radius * 0.42f, radius * 0.25f), iconColor, 4);
                DrawLine(center, center + new Vector2(radius * 0.42f, radius * 0.25f), iconColor, 4);
                break;
            case NodeType.Firewall:
                DrawShield(center, radius, fill, outline);
                DrawLine(center + new Vector2(-radius * 0.34f, -radius * 0.16f), center + new Vector2(radius * 0.34f, -radius * 0.16f), iconColor, 4);
                DrawLine(center + new Vector2(-radius * 0.42f, radius * 0.05f), center + new Vector2(radius * 0.42f, radius * 0.05f), iconColor, 4);
                DrawLine(center + new Vector2(-radius * 0.22f, radius * 0.26f), center + new Vector2(radius * 0.22f, radius * 0.26f), iconColor, 4);
                break;
            default:
                DrawCircle(center, radius, fill);
                DrawCircleOutline(center, radius, outline, 3);
                DrawCircleOutline(center, radius * 0.42f, iconColor, 3);
                break;
        }
    }

    private void DrawIntegrityBar(NodeState node, Vector2 center, float radius)
    {
        var width = (int)(radius * 1.42f);
        var x = (int)(center.X - width / 2f);
        var y = (int)(center.Y + radius * 0.66f);
        var integrityRatio = Math.Clamp(node.Integrity / 12f, 0f, 1f);
        var threatRatio = Math.Clamp(node.Threat / 12f, 0f, 1f);

        Fill(new XnaRectangle(x, y, width, 5), new XnaColor(22, 30, 39, 220));
        Fill(new XnaRectangle(x, y, (int)(width * integrityRatio), 5), ValidMoveColor);
        Fill(new XnaRectangle(x, y + 7, width, 4), new XnaColor(22, 30, 39, 220));
        Fill(new XnaRectangle(x, y + 7, (int)(width * threatRatio), 4), WarningColor);
    }

    private void DrawCorruptionOverlay(Vector2 center, float radius, float pulse)
    {
        var color = new XnaColor((byte)255, (byte)116, (byte)128, (byte)(170 + pulse * 55));
        DrawLine(center + new Vector2(-radius * 0.42f, -radius * 0.42f), center + new Vector2(radius * 0.42f, radius * 0.42f), color, 5);
        DrawLine(center + new Vector2(radius * 0.42f, -radius * 0.42f), center + new Vector2(-radius * 0.42f, radius * 0.42f), color, 5);
    }

    private void DrawHud()
    {
        var hud = GetHudBounds();
        Fill(hud, PanelColor);
        DrawRectangle(hud, PanelBorderColor, 2);

        var x = hud.X + TextPadding;
        var y = hud.Y + TextPadding;
        DrawText("Uplink", x, y, 22, TextColor);
        DrawText($"Turn {_game.TurnNumber}", hud.Right - 84, y + 4, 15, MutedTextColor);
        y += 36;

        DrawResourceStrip(x, y, hud.Width - TextPadding * 2);
        y += 54;
        DrawObjectiveProgress(x, y, hud.Width - TextPadding * 2);
        y += 45;

        _buttons.Clear();
        y = DrawActionButton(x, y, "1", "Claim", PlayerActionMode.Claim, ValidMoveColor);
        y = DrawActionButton(x, y + 7, "2", "Reinforce", PlayerActionMode.Reinforce, AccentColor);
        y = DrawActionButton(x, y + 7, "3", "Weaken", PlayerActionMode.Weaken, WarningColor);
        y = DrawCommandButton(x, y + 12, "Space", "End", ButtonAction.EndTurn);
        y = DrawCommandButton(x, y + 7, "R", "Reset", ButtonAction.Restart);
        y += 18;

        DrawText("Signal", x, y, 16, TextColor);
        y += 24;
        DrawText(_status, x, y, 13, TextColor, hud.Width - TextPadding * 2);
        y += EstimateWrappedHeight(_status, hud.Width - TextPadding * 2, 13) + 10;

        if (!string.IsNullOrWhiteSpace(_invalidReason))
        {
            DrawText(_invalidReason, x, y, 13, WarningColor, hud.Width - TextPadding * 2);
            y += EstimateWrappedHeight(_invalidReason, hud.Width - TextPadding * 2, 13) + 10;
        }

        DrawText("Trace", x, y, 16, TextColor);
        y += 23;
        foreach (var entry in _actionLog.TakeLast(MaxLogEntries))
        {
            DrawText(entry, x, y, 12, MutedTextColor, hud.Width - TextPadding * 2);
            y += EstimateWrappedHeight(entry, hud.Width - TextPadding * 2, 12) + 5;
        }
    }

    private void DrawResourceStrip(int x, int y, int width)
    {
        DrawText("Energy", x, y, 14, MutedTextColor);
        DrawText(_game.PlayerEnergy.ToString(), x + 72, y - 4, 22, AccentColor);
        var bar = new XnaRectangle(x, y + 28, width, 10);
        Fill(bar, new XnaColor(48, 58, 72));
        Fill(new XnaRectangle(bar.X, bar.Y, (int)(bar.Width * Math.Clamp(_game.PlayerEnergy / 10f, 0f, 1f)), bar.Height), AccentColor);
        DrawRectangle(bar, PanelBorderColor, 1);

        DrawText("Corruption", x + 140, y, 14, MutedTextColor);
        DrawText(_game.CorruptionPressure.ToString(), x + 236, y - 4, 22, WarningColor);
    }

    private int DrawActionButton(int x, int y, string shortcut, string label, PlayerActionMode action, XnaColor iconColor)
    {
        var selected = _selectedAction == action;
        var bounds = new XnaRectangle(x, y, HudWidth - TextPadding * 2, ButtonHeight);
        Fill(bounds, selected ? new XnaColor(35, 72, 91) : new XnaColor(34, 41, 53));
        DrawRectangle(bounds, selected ? iconColor : PanelBorderColor, selected ? 3 : 2);
        DrawCircle(new Vector2(bounds.X + 23, bounds.Y + 19), 9, iconColor);
        DrawText(shortcut, bounds.X + 44, bounds.Y + 10, 13, MutedTextColor);
        DrawText(label, bounds.X + 78, bounds.Y + 8, 16, TextColor);
        _buttons.Add(new ButtonDefinition(bounds, ButtonAction.SelectAction, action));
        return y + ButtonHeight;
    }

    private int DrawCommandButton(int x, int y, string shortcut, string label, ButtonAction action)
    {
        var bounds = new XnaRectangle(x, y, HudWidth - TextPadding * 2, ButtonHeight);
        Fill(bounds, new XnaColor(35, 41, 52));
        DrawRectangle(bounds, PanelBorderColor, 2);
        DrawText(shortcut, bounds.X + 14, bounds.Y + 10, 12, AccentColor);
        DrawText(label, bounds.X + 82, bounds.Y + 8, 16, TextColor);
        _buttons.Add(new ButtonDefinition(bounds, action, null));
        return y + ButtonHeight;
    }

    private void DrawHoverTooltip()
    {
        if (_hoveredNode is null)
        {
            return;
        }

        var mouse = Mouse.GetState().Position;
        var width = 300;
        var node = _hoveredNode;
        var preview = GetActionPreview(node);
        var height = node.IsUnstable || !string.IsNullOrWhiteSpace(node.DangerReason) ? 188 : 166;
        var x = Math.Min(mouse.X + 18, WindowWidth - HudWidth - width - 42);
        var y = Math.Min(mouse.Y + 18, WindowHeight - height - 18);
        var bounds = new XnaRectangle(Math.Max(18, x), Math.Max(18, y), width, height);
        Fill(bounds, new XnaColor(16, 22, 31, 242));
        DrawRectangle(bounds, preview.IsValid ? ValidMoveColor : AccentColor, 2);

        DrawText($"{GetOwnerLabel(node.Owner)} {GetTypeLabel(node)} {node.Id}", bounds.X + 12, bounds.Y + 10, 16, TextColor);
        DrawText($"Integrity {node.Integrity}   Threat {node.Threat}   Cost {preview.Cost}", bounds.X + 12, bounds.Y + 38, 13, MutedTextColor);
        DrawText(preview.IsValid ? preview.SuccessText : preview.Reason, bounds.X + 12, bounds.Y + 62, 13, preview.IsValid ? ValidMoveColor : WarningColor, width - 24);
        DrawText($"Selected: {_selectedAction}", bounds.X + 12, bounds.Y + 108, 12, MutedTextColor);

        if (node.IsUnstable || !string.IsNullOrWhiteSpace(node.DangerReason))
        {
            DrawText(node.DangerReason, bounds.X + 12, bounds.Y + 132, 12, node.IsUnstable ? WarningColor : MutedTextColor, width - 24);
        }
    }

    private void DrawResultBanner()
    {
        if (_game.Result == GameResult.InProgress)
        {
            return;
        }

        var isWin = _game.Result == GameResult.PlayerWin;
        var viewport = GetBoardViewport();
        var bounds = new XnaRectangle(viewport.X + viewport.Width / 2 - 220, viewport.Y + 28, 440, 86);
        Fill(bounds, isWin ? new XnaColor(20, 86, 54, 235) : new XnaColor(98, 28, 42, 235));
        DrawRectangle(bounds, isWin ? WinColor : LossColor, 4);
        DrawCenteredText(isWin ? "MISSION COMPLETE" : "MISSION FAILED", bounds.X, bounds.Y + 16, bounds.Width, 28, 24, TextColor);
        DrawCenteredText("R resets the network.", bounds.X, bounds.Y + 54, bounds.Width, 18, 14, TextColor);
    }

    private void HandleClick(XnaPoint mousePosition)
    {
        foreach (var button in _buttons)
        {
            if (!button.Bounds.Contains(mousePosition))
            {
                continue;
            }

            if (button.Action == ButtonAction.SelectAction && button.PlayerAction.HasValue)
            {
                SelectAction(button.PlayerAction.Value);
            }
            else if (button.Action == ButtonAction.EndTurn)
            {
                ApplyResult(_game.EndPlayerTurnWithResult());
            }
            else if (button.Action == ButtonAction.Restart)
            {
                RestartMission();
            }

            return;
        }

        var node = GetNodeAt(mousePosition);
        if (node is null)
        {
            return;
        }

        _selectedNodeId = node.Id;
        ApplyResult(_game.ExecutePlayerAction(_selectedAction, node.Id));
    }

    private void HandleCameraInput(MouseState mouse, KeyboardState keyboard)
    {
        var viewport = GetBoardViewport();
        var mouseInsideNetwork = viewport.Contains(mouse.Position);

        if (mouseInsideNetwork)
        {
            var wheelDelta = mouse.ScrollWheelValue - _previousMouse.ScrollWheelValue;
            if (wheelDelta != 0)
            {
                _targetZoom = MathHelper.Clamp(_targetZoom + wheelDelta / 1200f, 0.62f, 1.62f);
            }
        }

        if ((mouse.RightButton == ButtonState.Pressed || mouse.MiddleButton == ButtonState.Pressed) && mouseInsideNetwork)
        {
            var delta = mouse.Position.ToVector2() - _previousMouse.Position.ToVector2();
            _targetCameraCenter -= delta / Math.Max(0.1f, _zoom);
        }

        var keyboardPan = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
        {
            keyboardPan.X -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
        {
            keyboardPan.X += 1f;
        }

        if (keyboard.IsKeyDown(Keys.Up) || keyboard.IsKeyDown(Keys.W))
        {
            keyboardPan.Y -= 1f;
        }

        if (keyboard.IsKeyDown(Keys.Down) || keyboard.IsKeyDown(Keys.S))
        {
            keyboardPan.Y += 1f;
        }

        if (keyboardPan != Vector2.Zero)
        {
            _targetCameraCenter += Vector2.Normalize(keyboardPan) * 10f / Math.Max(0.1f, _zoom);
        }
    }

    private void SelectAction(PlayerActionMode action)
    {
        _selectedAction = action;
        _invalidReason = string.Empty;
        _status = $"{action}: {CountValidTargetsForSelectedAction()} target(s) available.";
        Log(_status);
    }

    private void RestartMission()
    {
        _game = NetworkGame.CreateVerticalSliceMission();
        _selectedAction = PlayerActionMode.Claim;
        _invalidReason = string.Empty;
        _status = "Mission reset. Expand toward the uplink.";
        _selectedNodeId = null;
        _actionLog.Clear();
        RecenterCamera(immediate: false);
        Log("Network reset.");
    }

    private void ApplyResult(GameActionResult result)
    {
        _status = FormatStatusMessage(result);
        _invalidReason = result.Succeeded ? string.Empty : result.Message;
        Log(FormatLogMessage(result));
    }

    private ActionPreview GetActionPreview(NodeState node)
    {
        if (_game.Result != GameResult.InProgress)
        {
            return new ActionPreview(false, "Mission already ended.", "None", "Restart", 0);
        }

        return _selectedAction switch
        {
            PlayerActionMode.Claim => PreviewClaim(node),
            PlayerActionMode.Reinforce => PreviewReinforce(node),
            PlayerActionMode.Weaken => PreviewWeaken(node),
            _ => new ActionPreview(false, "Unknown action.", "None", "Select action", 0)
        };
    }

    private ActionPreview PreviewClaim(NodeState node)
    {
        var cost = _game.Configuration.ClaimEnergyCost;
        if (node.Owner != NodeOwner.Neutral)
        {
            return new ActionPreview(false, "Only neutral nodes can be claimed.", cost.ToString(), "Claim node", cost);
        }

        if (_game.PlayerEnergy < cost)
        {
            return new ActionPreview(false, $"Need {cost} energy.", cost.ToString(), "Claim node", cost);
        }

        if (!_game.Board.IsReachableForPlayerClaim(node.Id, _game.Configuration))
        {
            return new ActionPreview(false, "Outside player claim range.", cost.ToString(), "Claim node", cost);
        }

        return new ActionPreview(true, string.Empty, cost.ToString(), $"Claim {node.Id}; corruption resolves.", cost);
    }

    private ActionPreview PreviewReinforce(NodeState node)
    {
        var cost = _game.Configuration.ReinforceEnergyCost;
        if (node.Owner != NodeOwner.Player)
        {
            return new ActionPreview(false, "Only player nodes can be reinforced.", cost.ToString(), "Reinforce node", cost);
        }

        if (_game.PlayerEnergy < cost)
        {
            return new ActionPreview(false, $"Need {cost} energy.", cost.ToString(), "Reinforce node", cost);
        }

        return new ActionPreview(true, string.Empty, cost.ToString(), $"Reinforce {node.Id}; improves stability.", cost);
    }

    private ActionPreview PreviewWeaken(NodeState node)
    {
        var cost = _game.Configuration.WeakenConnectionEnergyCost;
        if (node.Owner != NodeOwner.Enemy)
        {
            return new ActionPreview(false, "Only corrupted nodes can be weakened.", cost.ToString(), "Weaken link", cost);
        }

        if (_game.PlayerEnergy < cost)
        {
            return new ActionPreview(false, $"Need {cost} energy.", cost.ToString(), "Weaken link", cost);
        }

        if (!_game.Board.GetAdjacentNodes(node.Id).Any(adjacent => adjacent.Owner == NodeOwner.Player))
        {
            return new ActionPreview(false, "No adjacent player link.", cost.ToString(), "Weaken link", cost);
        }

        return new ActionPreview(true, string.Empty, cost.ToString(), $"Weaken link to {node.Id}.", cost);
    }

    private bool HasAnyValidTargetForSelectedAction()
    {
        return _game.Board.Nodes.Any(node => GetActionPreview(node).IsValid);
    }

    private int CountValidTargetsForSelectedAction()
    {
        return _game.Board.Nodes.Count(node => GetActionPreview(node).IsValid);
    }

    private NodeState GetNodeAt(XnaPoint mousePosition)
    {
        if (!GetBoardViewport().Contains(mousePosition))
        {
            return null;
        }

        return _game.Board.Nodes
            .Select(node => new { Node = node, Distance = Vector2.Distance(WorldToScreen(GetNodeWorldPosition(node.Id)), mousePosition.ToVector2()) })
            .Where(candidate => candidate.Distance <= NodeWorldRadius * _zoom + 16f)
            .OrderBy(candidate => candidate.Distance)
            .Select(candidate => candidate.Node)
            .FirstOrDefault();
    }

    private bool WasPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void Log(string message)
    {
        _actionLog.Add(message);
        if (_actionLog.Count > 18)
        {
            _actionLog.RemoveAt(0);
        }
    }

    private void RecenterCamera(bool immediate)
    {
        var positions = _game.Board.Nodes.Select(node => GetNodeWorldPosition(node.Id)).ToArray();
        var minX = positions.Min(position => position.X);
        var maxX = positions.Max(position => position.X);
        var minY = positions.Min(position => position.Y);
        var maxY = positions.Max(position => position.Y);
        var viewport = GetBoardViewport();
        var networkWidth = Math.Max(1f, maxX - minX);
        var networkHeight = Math.Max(1f, maxY - minY);
        var fitZoom = Math.Min(viewport.Width / (networkWidth + CameraMargin * 2f), viewport.Height / (networkHeight + CameraMargin * 2f));

        _targetCameraCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        _targetZoom = MathHelper.Clamp(fitZoom, 0.65f, 1.28f);

        if (immediate)
        {
            _cameraCenter = _targetCameraCenter;
            _zoom = _targetZoom;
        }
    }

    private XnaRectangle GetBoardViewport()
    {
        return new XnaRectangle(22, 22, WindowWidth - HudWidth - 58, WindowHeight - 44);
    }

    private XnaRectangle GetHudBounds()
    {
        return new XnaRectangle(WindowWidth - HudWidth - 22, 22, HudWidth, WindowHeight - 44);
    }

    private Vector2 GetNodeWorldPosition(NodeId nodeId)
    {
        if (VerticalSliceTopology.TryGetValue(nodeId, out var position))
        {
            return position;
        }

        var xOffset = nodeId.Y % 2 == 0 ? 0f : 58f;
        return new Vector2(nodeId.X * 170f + xOffset, nodeId.Y * 148f);
    }

    private Vector2 WorldToScreen(Vector2 world)
    {
        var viewport = GetBoardViewport();
        return new Vector2(viewport.X + viewport.Width / 2f, viewport.Y + viewport.Height / 2f) + (world - _cameraCenter) * _zoom;
    }

    private string GetTypeLabel(NodeState node)
    {
        if (node.Id == _game.PlayerCore)
        {
            return "Core";
        }

        if (node.Id == _game.ObjectiveNode)
        {
            return "Objective";
        }

        return node.Type switch
        {
            NodeType.Resource => "Resource",
            NodeType.Relay => "Relay",
            NodeType.Firewall => "Firewall",
            _ => "Node"
        };
    }

    private static string GetOwnerLabel(NodeOwner owner)
    {
        return owner switch
        {
            NodeOwner.Player => "Player",
            NodeOwner.Enemy => "Corruption",
            _ => "Neutral"
        };
    }

    private static XnaColor GetOwnerColor(NodeOwner owner)
    {
        return owner switch
        {
            NodeOwner.Player => new XnaColor(35, 145, 101),
            NodeOwner.Enemy => new XnaColor(153, 38, 55),
            _ => NeutralColor
        };
    }

    private XnaColor GetTypeColor(NodeState node, bool isObjective)
    {
        if (node.Id == _game.PlayerCore)
        {
            return AccentColor;
        }

        if (isObjective)
        {
            return ObjectiveColor;
        }

        return node.Type switch
        {
            NodeType.Resource => new XnaColor(232, 190, 66),
            NodeType.Relay => new XnaColor(66, 170, 230),
            NodeType.Firewall => new XnaColor(186, 117, 230),
            _ => new XnaColor(150, 165, 184)
        };
    }

    private static XnaColor GetConnectionColor(NodeState start, NodeState end, bool active)
    {
        if (!active)
        {
            return new XnaColor(47, 55, 66, 130);
        }

        if (start.Owner == NodeOwner.Enemy || end.Owner == NodeOwner.Enemy)
        {
            return start.Owner == NodeOwner.Enemy && end.Owner == NodeOwner.Enemy
                ? new XnaColor(143, 41, 59, 210)
                : new XnaColor(204, 100, 71, 220);
        }

        if (start.Owner == NodeOwner.Player && end.Owner == NodeOwner.Player)
        {
            return new XnaColor(62, 188, 139, 230);
        }

        return new XnaColor(70, 116, 143, 190);
    }

    private static XnaColor GetFlowColor(NodeState start, NodeState end)
    {
        if (start.Owner == NodeOwner.Enemy || end.Owner == NodeOwner.Enemy)
        {
            return new XnaColor(255, 110, 126);
        }

        if (start.Owner == NodeOwner.Player || end.Owner == NodeOwner.Player)
        {
            return new XnaColor(104, 229, 184);
        }

        return new XnaColor(108, 196, 238);
    }

    private void DrawObjectiveProgress(int x, int y, int width)
    {
        DrawText("Objective", x, y, 14, ObjectiveColor);
        var bar = new XnaRectangle(x, y + 24, width, 12);
        Fill(bar, new XnaColor(48, 58, 72));
        var progress = _game.RequiredObjectiveHoldTurns == 0
            ? 0f
            : Math.Clamp(_game.ObjectiveHoldTurns / (float)_game.RequiredObjectiveHoldTurns, 0f, 1f);
        Fill(new XnaRectangle(bar.X, bar.Y, (int)(bar.Width * progress), bar.Height), ObjectiveColor);
        DrawRectangle(bar, PanelBorderColor, 1);
        DrawText($"{_game.ObjectiveHoldTurns}/{_game.RequiredObjectiveHoldTurns}", x + width - 42, y - 2, 16, TextColor);
    }

    private string FormatStatusMessage(GameActionResult result)
    {
        if (!result.Succeeded)
        {
            return result.Message;
        }

        var parts = new List<string> { result.Message.Split('.')[0] + "." };

        if (result.CorruptionTarget.HasValue)
        {
            parts.Add($"Corruption captured {result.CorruptionTarget.Value}.");
        }
        else if (result.CorruptionFocusTarget.HasValue)
        {
            parts.Add($"Corruption pressed {result.CorruptionFocusTarget.Value}; node held.");
        }
        else
        {
            parts.Add("Corruption pressure rose.");
        }

        if (result.CollapsedNodes is { Count: > 0 })
        {
            parts.Add($"Collapse: {string.Join(", ", result.CollapsedNodes)}.");
        }

        parts.Add($"Objective {_game.ObjectiveHoldTurns}/{_game.RequiredObjectiveHoldTurns}.");

        if (result.EnergyGenerated > 0)
        {
            parts.Add($"+{result.EnergyGenerated} energy.");
        }

        if (result.Result == GameResult.PlayerWin)
        {
            parts.Add("Mission complete.");
        }
        else if (result.Result == GameResult.PlayerLoss)
        {
            parts.Add("Mission failed.");
        }

        return string.Join(" ", parts);
    }

    private static string FormatLogMessage(GameActionResult result)
    {
        if (!result.Succeeded)
        {
            return "Blocked: " + result.Message;
        }

        var firstSentence = result.Message.Split('.')[0] + ".";
        if (result.Result == GameResult.PlayerWin)
        {
            return firstSentence + " Mission complete.";
        }

        if (result.Result == GameResult.PlayerLoss)
        {
            return firstSentence + " Mission failed.";
        }

        if (result.CollapsedNodes is { Count: > 0 })
        {
            return firstSentence + $" Collapse: {string.Join(", ", result.CollapsedNodes)}.";
        }

        if (result.CorruptionTarget.HasValue)
        {
            return firstSentence + $" Corruption -> {result.CorruptionTarget.Value}.";
        }

        return firstSentence;
    }

    private float GetPulse(float speed, float offset)
    {
        return (MathF.Sin((float)_totalSeconds * speed + offset) + 1f) / 2f;
    }

    private void DrawCenteredText(string text, int x, int y, int width, int height, int size, XnaColor color)
    {
        var texture = GetTextTexture(text, size, color);
        var target = new XnaRectangle(x + (width - texture.Width) / 2, y + (height - texture.Height) / 2, texture.Width, texture.Height);
        _spriteBatch.Draw(texture, target, XnaColor.White);
    }

    private void DrawText(string text, int x, int y, int size, XnaColor color, int maxWidth = 0)
    {
        if (maxWidth <= 0)
        {
            var texture = GetTextTexture(text, size, color);
            _spriteBatch.Draw(texture, new Vector2(x, y), XnaColor.White);
            return;
        }

        var lineY = y;
        foreach (var line in WrapText(text, maxWidth, size, color))
        {
            var texture = GetTextTexture(line, size, color);
            _spriteBatch.Draw(texture, new Vector2(x, lineY), XnaColor.White);
            lineY += texture.Height + 3;
        }
    }

    private Texture2D GetTextTexture(string text, int size, XnaColor color)
    {
        var key = new TextKey(text, size, color.PackedValue);
        if (_textCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        using var font = new Font("Segoe UI", size, FontStyle.Bold, GraphicsUnit.Pixel);
        var measured = WinForms.TextRenderer.MeasureText(text, font, new Size(int.MaxValue, int.MaxValue), WinForms.TextFormatFlags.NoPadding);
        var width = Math.Max(1, measured.Width + 2);
        var height = Math.Max(1, measured.Height + 2);
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(System.Drawing.Color.Transparent);
            WinForms.TextRenderer.DrawText(
                graphics,
                text,
                font,
                new System.Drawing.Point(1, 1),
                System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B),
                WinForms.TextFormatFlags.NoPadding);
        }

        var data = new XnaColor[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                data[y * width + x] = new XnaColor(pixel.R, pixel.G, pixel.B, pixel.A);
            }
        }

        var texture = new Texture2D(GraphicsDevice, width, height);
        texture.SetData(data);
        _textCache[key] = texture;
        return texture;
    }

    private IEnumerable<string> WrapText(string text, int maxWidth, int size, XnaColor color)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            yield break;
        }

        var line = words[0];
        for (var i = 1; i < words.Length; i++)
        {
            var candidate = line + " " + words[i];
            if (GetTextTexture(candidate, size, color).Width <= maxWidth)
            {
                line = candidate;
            }
            else
            {
                yield return line;
                line = words[i];
            }
        }

        yield return line;
    }

    private int EstimateWrappedHeight(string text, int maxWidth, int size)
    {
        return WrapText(text, maxWidth, size, TextColor).Sum(line => GetTextTexture(line, size, TextColor).Height + 3);
    }

    private void Fill(XnaRectangle rectangle, XnaColor color)
    {
        _spriteBatch.Draw(_pixel, rectangle, color);
    }

    private void DrawRectangle(XnaRectangle rectangle, XnaColor color, int thickness)
    {
        Fill(new XnaRectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        Fill(new XnaRectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        Fill(new XnaRectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        Fill(new XnaRectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private void DrawLine(Vector2 start, Vector2 end, XnaColor color, int thickness)
    {
        var edge = end - start;
        var angle = MathF.Atan2(edge.Y, edge.X);
        _spriteBatch.Draw(_pixel, new XnaRectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
    }

    private void DrawCircle(Vector2 center, float radius, XnaColor color)
    {
        var top = (int)MathF.Floor(center.Y - radius);
        var bottom = (int)MathF.Ceiling(center.Y + radius);
        for (var y = top; y <= bottom; y += 2)
        {
            var dy = y - center.Y;
            var halfWidth = MathF.Sqrt(MathF.Max(0f, radius * radius - dy * dy));
            Fill(new XnaRectangle((int)(center.X - halfWidth), y, (int)(halfWidth * 2f), 2), color);
        }
    }

    private void DrawCircleOutline(Vector2 center, float radius, XnaColor color, int thickness)
    {
        var previous = center + new Vector2(radius, 0);
        for (var i = 1; i <= 48; i++)
        {
            var angle = MathHelper.TwoPi * i / 48f;
            var next = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            DrawLine(previous, next, color, thickness);
            previous = next;
        }
    }

    private void DrawDiamond(Vector2 center, float radius, XnaColor fill, XnaColor outline)
    {
        var points = new[]
        {
            center + new Vector2(0, -radius),
            center + new Vector2(radius, 0),
            center + new Vector2(0, radius),
            center + new Vector2(-radius, 0)
        };
        FillPolygonApprox(points, fill);
        DrawPolygonOutline(points, outline, 4);
    }

    private void DrawHexagon(Vector2 center, float radius, XnaColor fill, XnaColor outline)
    {
        var points = Enumerable.Range(0, 6)
            .Select(i =>
            {
                var angle = MathHelper.TwoPi * i / 6f + MathHelper.PiOver2;
                return center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            })
            .ToArray();
        FillPolygonApprox(points, fill);
        DrawPolygonOutline(points, outline, 4);
    }

    private void DrawShield(Vector2 center, float radius, XnaColor fill, XnaColor outline)
    {
        var points = new[]
        {
            center + new Vector2(-radius * 0.74f, -radius * 0.7f),
            center + new Vector2(radius * 0.74f, -radius * 0.7f),
            center + new Vector2(radius * 0.62f, radius * 0.18f),
            center + new Vector2(0, radius),
            center + new Vector2(-radius * 0.62f, radius * 0.18f)
        };
        FillPolygonApprox(points, fill);
        DrawPolygonOutline(points, outline, 4);
    }

    private void FillPolygonApprox(IReadOnlyList<Vector2> points, XnaColor color)
    {
        var center = points.Aggregate(Vector2.Zero, (sum, point) => sum + point) / points.Count;
        var maxRadius = points.Max(point => Vector2.Distance(point, center));
        DrawCircle(center, maxRadius * 0.82f, color);
    }

    private void DrawPolygonOutline(IReadOnlyList<Vector2> points, XnaColor color, int thickness)
    {
        for (var i = 0; i < points.Count; i++)
        {
            DrawLine(points[i], points[(i + 1) % points.Count], color, thickness);
        }
    }

    private static XnaColor Blend(XnaColor first, XnaColor second, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return new XnaColor(
            (byte)(first.R + (second.R - first.R) * amount),
            (byte)(first.G + (second.G - first.G) * amount),
            (byte)(first.B + (second.B - first.B) * amount),
            (byte)(first.A + (second.A - first.A) * amount));
    }

    private readonly record struct TextKey(string Text, int Size, uint PackedColor);

    private readonly record struct ButtonDefinition(XnaRectangle Bounds, ButtonAction Action, PlayerActionMode? PlayerAction);

    private readonly record struct ActionPreview(bool IsValid, string Reason, string Cost, string SuccessText, int EnergyCost);

    private enum ButtonAction
    {
        SelectAction,
        EndTurn,
        Restart
    }
}
