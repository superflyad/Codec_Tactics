using System;
using CodecTactics.Core.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CodecTactics.MonoGame;

public class Game1 : Game
{
    private const int NodeSize = 34;
    private const int BoardLeft = 120;
    private const int BoardTop = 110;
    private const int CellSize = 92;

    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = default!;
    private Texture2D _pixel = default!;
    private MouseState _previousMouse;
    private KeyboardState _previousKeyboard;
    private NetworkGame _game = NetworkGame.CreateVerticalSliceMission();
    private PlayerActionMode _selectedAction = PlayerActionMode.Claim;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 900;
        _graphics.PreferredBackBufferHeight = 680;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        UpdateWindowTitle("Mission ready.");
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();
        var mouse = Mouse.GetState();

        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape))
        {
            Exit();
        }

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
            UpdateWindowTitle(_game.EndPlayerTurnWithResult().Message);
        }
        else if (WasPressed(keyboard, Keys.R))
        {
            _game = NetworkGame.CreateVerticalSliceMission();
            UpdateWindowTitle("Mission restarted.");
        }

        if (mouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released)
        {
            TryApplyAction(mouse.Position);
        }

        _previousKeyboard = keyboard;
        _previousMouse = mouse;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(18, 21, 27));

        _spriteBatch.Begin();
        foreach (var connection in _game.Board.Connections)
        {
            var start = GetNodeCenter(connection.First);
            var end = GetNodeCenter(connection.Second);
            DrawLine(start, end, connection.IsActive ? new Color(80, 88, 102) : new Color(38, 42, 49), 4);
        }

        foreach (var node in _game.Board.Nodes)
        {
            DrawNode(node);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private bool WasPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key) && !_previousKeyboard.IsKeyDown(key);
    }

    private void SelectAction(PlayerActionMode action)
    {
        _selectedAction = action;
        UpdateWindowTitle($"Selected {_selectedAction}.");
    }

    private void TryApplyAction(Point mousePosition)
    {
        foreach (var node in _game.Board.Nodes)
        {
            var center = GetNodeCenter(node.Id);
            if (Vector2.Distance(center, mousePosition.ToVector2()) > NodeSize)
            {
                continue;
            }

            var result = _game.ExecutePlayerAction(_selectedAction, node.Id);
            UpdateWindowTitle(result.Message);
            return;
        }
    }

    private void DrawNode(NodeState node)
    {
        var center = GetNodeCenter(node.Id);
        var bounds = new Rectangle(
            (int)center.X - NodeSize / 2,
            (int)center.Y - NodeSize / 2,
            NodeSize,
            NodeSize);

        Fill(bounds, GetOwnerColor(node.Owner));

        if (node.Type != NodeType.Standard)
        {
            var marker = new Rectangle(bounds.X + 9, bounds.Y + 9, bounds.Width - 18, bounds.Height - 18);
            Fill(marker, GetTypeColor(node.Type));
        }

        if (_game.ObjectiveNode == node.Id)
        {
            DrawRectangle(bounds, new Color(242, 226, 80), 4);
        }
        else if (node.IsUnstable)
        {
            DrawRectangle(bounds, new Color(255, 144, 51), 4);
        }
        else
        {
            DrawRectangle(bounds, new Color(221, 226, 235), 2);
        }
    }

    private static Color GetOwnerColor(NodeOwner owner)
    {
        return owner switch
        {
            NodeOwner.Player => new Color(39, 181, 117),
            NodeOwner.Enemy => new Color(209, 57, 67),
            _ => new Color(132, 141, 154)
        };
    }

    private static Color GetTypeColor(NodeType type)
    {
        return type switch
        {
            NodeType.Resource => new Color(236, 194, 67),
            NodeType.Relay => new Color(78, 139, 229),
            NodeType.Firewall => new Color(173, 103, 221),
            _ => Color.Transparent
        };
    }

    private Vector2 GetNodeCenter(NodeId nodeId)
    {
        return new Vector2(BoardLeft + nodeId.X * CellSize, BoardTop + nodeId.Y * CellSize);
    }

    private void Fill(Rectangle rectangle, Color color)
    {
        _spriteBatch.Draw(_pixel, rectangle, color);
    }

    private void DrawRectangle(Rectangle rectangle, Color color, int thickness)
    {
        Fill(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Bottom - thickness, rectangle.Width, thickness), color);
        Fill(new Rectangle(rectangle.X, rectangle.Y, thickness, rectangle.Height), color);
        Fill(new Rectangle(rectangle.Right - thickness, rectangle.Y, thickness, rectangle.Height), color);
    }

    private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
    {
        var edge = end - start;
        var angle = MathF.Atan2(edge.Y, edge.X);

        _spriteBatch.Draw(_pixel, new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
    }

    private void UpdateWindowTitle(string status)
    {
        Window.Title = $"Codec_Tactics MonoGame | {_selectedAction} | Turn {_game.TurnNumber} | Energy {_game.PlayerEnergy} | {_game.Result} | {status}";
    }
}
