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
    private const int WindowWidth = 1280;
    private const int WindowHeight = 820;
    private const int HudWidth = 330;
    private const int NodeSize = 82;
    private const int CellSize = 118;
    private const int ButtonHeight = 42;
    private const int TextPadding = 12;
    private const int MaxLogEntries = 7;

    private static readonly XnaColor BackgroundColor = new(13, 17, 23);
    private static readonly XnaColor PanelColor = new(28, 34, 45);
    private static readonly XnaColor PanelBorderColor = new(84, 94, 112);
    private static readonly XnaColor TextColor = new(232, 238, 247);
    private static readonly XnaColor MutedTextColor = new(170, 181, 196);
    private static readonly XnaColor AccentColor = new(87, 187, 255);
    private static readonly XnaColor ValidMoveColor = new(93, 222, 137);
    private static readonly XnaColor ObjectiveColor = new(246, 219, 79);
    private static readonly XnaColor WarningColor = new(255, 154, 70);
    private static readonly XnaColor LossColor = new(218, 72, 85);
    private static readonly XnaColor WinColor = new(83, 202, 132);

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
    private string _status = "Mission ready. Select an action and claim toward the objective.";
    private string _invalidReason = string.Empty;
    private NodeState _hoveredNode;

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
        Window.Title = "Codec_Tactics MonoGame";
        Log("Mission ready. Reach and hold the Objective.");
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

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

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

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            HandleClick(mouse.Position);
        }

        Window.Title = $"Codec_Tactics MonoGame | {_selectedAction} | Turn {_game.TurnNumber} | Energy {_game.PlayerEnergy} | {_game.Result}";
        _previousKeyboard = keyboard;
        _previousMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(BackgroundColor);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        DrawBoard();
        DrawHud();
        DrawHoverTooltip();
        DrawResultBanner();
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawBoard()
    {
        var boardBounds = GetBoardBounds();
        DrawText("Secure the Uplink", boardBounds.X, 36, 28, TextColor);
        DrawText(_game.ObjectiveText, boardBounds.X, 72, 16, MutedTextColor, boardBounds.Width + 40);

        foreach (var connection in _game.Board.Connections)
        {
            var start = GetNodeCenter(connection.First);
            var end = GetNodeCenter(connection.Second);
            var color = connection.IsActive ? new XnaColor(72, 82, 99) : new XnaColor(38, 43, 52);
            DrawLine(start, end, color, connection.IsActive ? 5 : 3);
        }

        foreach (var node in _game.Board.Nodes.OrderBy(node => node.Id.Y).ThenBy(node => node.Id.X))
        {
            DrawNode(node);
        }
    }

    private void DrawNode(NodeState node)
    {
        var center = GetNodeCenter(node.Id);
        var bounds = new XnaRectangle(
            (int)center.X - NodeSize / 2,
            (int)center.Y - NodeSize / 2,
            NodeSize,
            NodeSize);
        var isHovered = _hoveredNode?.Id == node.Id;
        var isObjective = _game.ObjectiveNode == node.Id;
        var isValid = IsValidTargetForSelectedAction(node);

        Fill(bounds, GetOwnerColor(node.Owner));

        if (node.Type != NodeType.Standard)
        {
            Fill(new XnaRectangle(bounds.X + 6, bounds.Y + 6, bounds.Width - 12, 10), GetTypeColor(node.Type));
        }

        if (isValid)
        {
            DrawRectangle(Grow(bounds, 9), ValidMoveColor, 4);
        }

        if (isObjective)
        {
            DrawRectangle(Grow(bounds, 4), ObjectiveColor, 5);
        }
        else if (node.IsUnstable)
        {
            DrawRectangle(Grow(bounds, 4), WarningColor, 4);
        }
        else
        {
            DrawRectangle(bounds, new XnaColor(221, 226, 235), 2);
        }

        if (isHovered)
        {
            DrawRectangle(Grow(bounds, 13), AccentColor, 3);
        }

        var primaryLabel = GetPrimaryNodeLabel(node);
        var secondaryLabel = GetSecondaryNodeLabel(node);
        DrawCenteredText(primaryLabel, bounds.X + 5, bounds.Y + 22, bounds.Width - 10, 16, 13, TextColor);
        DrawCenteredText(secondaryLabel, bounds.X + 5, bounds.Y + 42, bounds.Width - 10, 16, 12, MutedTextColor);

        if (node.Owner == NodeOwner.Player)
        {
            DrawCenteredText($"I {node.Integrity}  T {node.Threat}", bounds.X + 4, bounds.Bottom - 20, bounds.Width - 8, 14, 11, TextColor);
        }
    }

    private void DrawHud()
    {
        var hud = GetHudBounds();
        Fill(hud, PanelColor);
        DrawRectangle(hud, PanelBorderColor, 2);

        var x = hud.X + TextPadding;
        var y = hud.Y + TextPadding;
        DrawText("Mission HUD", x, y, 24, TextColor);
        y += 38;
        DrawText($"Turn {_game.TurnNumber}   Energy {_game.PlayerEnergy}", x, y, 17, TextColor);
        y += 24;
        DrawText($"Objective {_game.ObjectiveHoldTurns}/{_game.RequiredObjectiveHoldTurns}", x, y, 17, ObjectiveColor);
        y += 24;
        DrawText($"Corruption pressure {_game.CorruptionPressure}", x, y, 16, WarningColor);
        y += 34;

        _buttons.Clear();
        y = DrawActionButton(x, y, "1", "Claim", PlayerActionMode.Claim);
        y = DrawActionButton(x, y + 8, "2", "Reinforce", PlayerActionMode.Reinforce);
        y = DrawActionButton(x, y + 8, "3", "Weaken", PlayerActionMode.Weaken);
        y = DrawCommandButton(x, y + 14, "Space", "End Turn", ButtonAction.EndTurn);
        y = DrawCommandButton(x, y + 8, "R", "Restart", ButtonAction.Restart);
        y += 22;

        DrawText("Status", x, y, 18, TextColor);
        y += 26;
        DrawText(_status, x, y, 15, TextColor, hud.Width - TextPadding * 2);
        y += EstimateWrappedHeight(_status, hud.Width - TextPadding * 2, 15) + 12;

        if (!string.IsNullOrWhiteSpace(_invalidReason))
        {
            DrawText("Invalid move", x, y, 16, WarningColor);
            y += 23;
            DrawText(_invalidReason, x, y, 14, WarningColor, hud.Width - TextPadding * 2);
            y += EstimateWrappedHeight(_invalidReason, hud.Width - TextPadding * 2, 14) + 12;
        }

        DrawText("Action Log", x, y, 18, TextColor);
        y += 26;
        foreach (var entry in _actionLog.TakeLast(MaxLogEntries))
        {
            DrawText("- " + entry, x, y, 13, MutedTextColor, hud.Width - TextPadding * 2);
            y += EstimateWrappedHeight("- " + entry, hud.Width - TextPadding * 2, 13) + 6;
        }
    }

    private int DrawActionButton(int x, int y, string shortcut, string label, PlayerActionMode action)
    {
        var selected = _selectedAction == action;
        var bounds = new XnaRectangle(x, y, HudWidth - TextPadding * 2, ButtonHeight);
        Fill(bounds, selected ? new XnaColor(44, 96, 133) : new XnaColor(44, 52, 66));
        DrawRectangle(bounds, selected ? AccentColor : PanelBorderColor, selected ? 3 : 2);
        DrawText(shortcut, bounds.X + 12, bounds.Y + 10, 15, AccentColor);
        DrawText(label, bounds.X + 58, bounds.Y + 9, 17, TextColor);
        _buttons.Add(new ButtonDefinition(bounds, ButtonAction.SelectAction, action));
        return y + ButtonHeight;
    }

    private int DrawCommandButton(int x, int y, string shortcut, string label, ButtonAction action)
    {
        var bounds = new XnaRectangle(x, y, HudWidth - TextPadding * 2, ButtonHeight);
        Fill(bounds, new XnaColor(48, 55, 68));
        DrawRectangle(bounds, PanelBorderColor, 2);
        DrawText(shortcut, bounds.X + 12, bounds.Y + 10, 14, AccentColor);
        DrawText(label, bounds.X + 82, bounds.Y + 9, 17, TextColor);
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
        var width = 280;
        var height = 156;
        var x = Math.Min(mouse.X + 18, WindowWidth - width - 12);
        var y = Math.Min(mouse.Y + 18, WindowHeight - height - 12);
        var bounds = new XnaRectangle(x, y, width, height);
        Fill(bounds, new XnaColor(21, 26, 35));
        DrawRectangle(bounds, AccentColor, 2);

        var node = _hoveredNode;
        DrawText($"{GetPrimaryNodeLabel(node)} {node.Id}", x + 12, y + 10, 17, TextColor);
        DrawText($"Owner: {GetOwnerLabel(node.Owner)}", x + 12, y + 36, 14, MutedTextColor);
        DrawText($"Type: {GetTypeLabel(node.Type)}", x + 12, y + 56, 14, MutedTextColor);
        DrawText($"Integrity {node.Integrity}   Threat {node.Threat}", x + 12, y + 76, 14, TextColor);
        DrawText($"Unstable turns: {node.UnstableTurns}", x + 12, y + 96, 14, node.IsUnstable ? WarningColor : MutedTextColor);
        DrawText(node.DangerReason, x + 12, y + 118, 13, node.IsUnstable ? WarningColor : MutedTextColor, width - 24);
    }

    private void DrawResultBanner()
    {
        if (_game.Result == GameResult.InProgress)
        {
            return;
        }

        var isWin = _game.Result == GameResult.PlayerWin;
        var bounds = new XnaRectangle(80, 330, WindowWidth - HudWidth - 170, 108);
        Fill(bounds, isWin ? new XnaColor(24, 86, 55) : new XnaColor(97, 31, 40));
        DrawRectangle(bounds, isWin ? WinColor : LossColor, 4);
        DrawCenteredText(isWin ? "MISSION COMPLETE" : "MISSION FAILED", bounds.X, bounds.Y + 22, bounds.Width, 32, 28, TextColor);
        DrawCenteredText("Press R or click Restart to run the mission again.", bounds.X, bounds.Y + 66, bounds.Width, 22, 16, TextColor);
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

        ApplyResult(_game.ExecutePlayerAction(_selectedAction, node.Id));
    }

    private void SelectAction(PlayerActionMode action)
    {
        _selectedAction = action;
        _invalidReason = string.Empty;
        _status = $"Selected {action}.";
        Log($"Selected {action}.");
    }

    private void RestartMission()
    {
        _game = NetworkGame.CreateVerticalSliceMission();
        _selectedAction = PlayerActionMode.Claim;
        _invalidReason = string.Empty;
        _status = "Mission restarted. Reach and hold the Objective.";
        _actionLog.Clear();
        Log("Mission restarted.");
    }

    private void ApplyResult(GameActionResult result)
    {
        _status = result.Message;
        _invalidReason = result.Succeeded ? string.Empty : result.Message;
        Log(result.Message);
    }

    private bool IsValidTargetForSelectedAction(NodeState node)
    {
        if (_game.Result != GameResult.InProgress)
        {
            return false;
        }

        return _selectedAction switch
        {
            PlayerActionMode.Claim => node.Owner == NodeOwner.Neutral
                && _game.PlayerEnergy >= _game.Configuration.ClaimEnergyCost
                && _game.Board.IsReachableForPlayerClaim(node.Id, _game.Configuration),
            PlayerActionMode.Reinforce => node.Owner == NodeOwner.Player
                && _game.PlayerEnergy >= _game.Configuration.ReinforceEnergyCost,
            PlayerActionMode.Weaken => node.Owner == NodeOwner.Enemy
                && _game.PlayerEnergy >= _game.Configuration.WeakenConnectionEnergyCost
                && _game.Board.GetAdjacentNodes(node.Id).Any(adjacent => adjacent.Owner == NodeOwner.Player),
            _ => false
        };
    }

    private NodeState GetNodeAt(XnaPoint mousePosition)
    {
        foreach (var node in _game.Board.Nodes)
        {
            var center = GetNodeCenter(node.Id);
            if (Vector2.Distance(center, mousePosition.ToVector2()) <= NodeSize / 2f + 8)
            {
                return node;
            }
        }

        return null;
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

    private XnaRectangle GetBoardBounds()
    {
        var boardWidth = (_game.Board.Width - 1) * CellSize + NodeSize;
        var boardHeight = (_game.Board.Height - 1) * CellSize + NodeSize;
        var availableWidth = WindowWidth - HudWidth - 60;
        var left = (availableWidth - boardWidth) / 2 + NodeSize / 2;
        var top = (WindowHeight - boardHeight) / 2 + NodeSize / 2 + 28;
        return new XnaRectangle(left - NodeSize / 2, top - NodeSize / 2, boardWidth, boardHeight);
    }

    private XnaRectangle GetHudBounds()
    {
        return new XnaRectangle(WindowWidth - HudWidth - 22, 22, HudWidth, WindowHeight - 44);
    }

    private Vector2 GetNodeCenter(NodeId nodeId)
    {
        var bounds = GetBoardBounds();
        return new Vector2(bounds.X + NodeSize / 2 + nodeId.X * CellSize, bounds.Y + NodeSize / 2 + nodeId.Y * CellSize);
    }

    private static string GetPrimaryNodeLabel(NodeState node)
    {
        if (node.Owner == NodeOwner.Enemy)
        {
            return "Corruption";
        }

        return node.Type == NodeType.Firewall ? "Firewall" : GetTypeLabel(node.Type);
    }

    private string GetSecondaryNodeLabel(NodeState node)
    {
        if (node.Id == _game.PlayerCore)
        {
            return "Core";
        }

        if (_game.ObjectiveNode == node.Id)
        {
            return "Objective";
        }

        return GetOwnerLabel(node.Owner);
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

    private static string GetTypeLabel(NodeType type)
    {
        return type switch
        {
            NodeType.Resource => "Resource",
            NodeType.Relay => "Relay",
            NodeType.Firewall => "Firewall",
            _ => "Node"
        };
    }

    private static XnaColor GetOwnerColor(NodeOwner owner)
    {
        return owner switch
        {
            NodeOwner.Player => new XnaColor(35, 126, 88),
            NodeOwner.Enemy => new XnaColor(137, 42, 54),
            _ => new XnaColor(64, 72, 86)
        };
    }

    private static XnaColor GetTypeColor(NodeType type)
    {
        return type switch
        {
            NodeType.Resource => new XnaColor(232, 190, 66),
            NodeType.Relay => new XnaColor(71, 146, 226),
            NodeType.Firewall => new XnaColor(178, 112, 229),
            _ => XnaColor.Transparent
        };
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
        foreach (var line in WrapText(text, maxWidth, size))
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

    private IEnumerable<string> WrapText(string text, int maxWidth, int size)
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
            if (GetTextTexture(candidate, size, TextColor).Width <= maxWidth)
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
        return WrapText(text, maxWidth, size).Sum(line => GetTextTexture(line, size, TextColor).Height + 3);
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

    private static XnaRectangle Grow(XnaRectangle rectangle, int amount)
    {
        return new XnaRectangle(rectangle.X - amount, rectangle.Y - amount, rectangle.Width + amount * 2, rectangle.Height + amount * 2);
    }

    private readonly record struct TextKey(string Text, int Size, uint PackedColor);

    private readonly record struct ButtonDefinition(XnaRectangle Bounds, ButtonAction Action, PlayerActionMode? PlayerAction);

    private enum ButtonAction
    {
        SelectAction,
        EndTurn,
        Restart
    }
}
