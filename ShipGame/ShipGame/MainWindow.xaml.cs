using ShipGameLibrary;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace ShipGame
{
    class DataButton
    {
        public string Content { get; set; }
        public ValueCls Value { get; set; }


        public class ValueCls
        {
            public BoardType Type { get; set; }
            public Position Position { get; set; }
        }
    }

    public partial class MainWindow : Window
    {
        private readonly ShipGameEngine Engine = new ShipGameEngine(10, true);
        private int _addPlayerShipSize = 1;
        private bool _isShipAccepted = false;
        private List<Position> _playerShipPositions = new List<Position>();

        public MainWindow()
        {
            List<List<DataButton>> lsts = new List<List<DataButton>>();

            for (int i = 0; i < Engine.BoardSize; i++)
            {
                lsts.Add(new List<DataButton>());

                for (int j = 0; j < Engine.BoardSize; j++)
                {
                    var model = new DataButton
                    {
                        Content = Engine.EnemyShips.Arr[i, j] == 1 ? "*" : "",
                        Value = new DataButton.ValueCls
                        {
                            Type = BoardType.ENEMY_SHIPS,
                            Position = new Position(i, j)
                        }
                    };
                    lsts[i].Add(model);
                }
            }


            InitializeComponent();
            this.EnemyShipsIS.ItemsSource = lsts;
            this.PlayerHitsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_HITS);
            this.PlayerShipsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_SHIPS);
        }

        private List<List<DataButton>> LoadBasicBoard(BoardType type)
        {
            List<List<DataButton>> lsts = new List<List<DataButton>>();

            for (int i = 0; i < Engine.BoardSize; i++)
            {
                lsts.Add(new List<DataButton>());

                for (int j = 0; j < Engine.BoardSize; j++)
                {
                    var model = new DataButton
                    {
                        Content = "",
                        Value = new DataButton.ValueCls
                        {
                            Type = type,
                            Position = new Position(i, j)
                        }
                    };
                    lsts[i].Add(model);
                }
            }

            return lsts;
        }

        private void Turn(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var data = button.DataContext as DataButton;

            var board = Engine.GetBoard(data.Value.Type);

            if (this._addPlayerShipSize < 5)
            {
                this.InitPlayerShips(button);
            }
            else if (data.Value.Type == BoardType.PLAYER_HITS)
            {
                button.Content = Engine.AddPlayerHit(data.Value.Position);
            }
            if (this.Engine.GetGameStatus() == GameStatus.PLAYER_WIN)
            {
                throw new InvalidOperationException("REEEEEEEEE");
            }
        }

        private void InitPlayerShips(Button button)
        {
            var data = button.DataContext as DataButton;

            if (data.Value.Type == BoardType.PLAYER_SHIPS && button.Content.Equals("")
                && this.Engine.IsFreeSpaceForShip(this.Engine.PlayerShips, data.Value.Position)
                && IsCorrectShipPosition(data.Value.Position))
            {
                this._playerShipPositions.Add(data.Value.Position);
                button.Content = "*";

                if (this._addPlayerShipSize == this._playerShipPositions.Count)
                {
                    this.Engine.AddPlayerShip(new Ship(this._playerShipPositions.ToArray()));
                    this._addPlayerShipSize++;
                    this._playerShipPositions.Clear();
                }

                this.PlayerShipSize.Content = this._addPlayerShipSize;

                if (this._addPlayerShipSize > 4)
                {
                    this.PlayerShipSizeLabel.Visibility = Visibility.Hidden;
                    this.PlayerShipSize.Visibility = Visibility.Hidden;
                }
            }
        }

        private bool IsCorrectShipPosition(Position position)
        {
            if (this._playerShipPositions.Count == 0)
            {
                return true;
            }
            else if (this._playerShipPositions.Count == 1 && (
                (_playerShipPositions.Last().Y == position.Y && IsPositionOneUpOrDown(position))
                || (_playerShipPositions.Last().X == position.X && IsPositionOneLeftOrRight(position))))
            {
                return true;
            }
            else if ((_playerShipPositions.Last().Y == _playerShipPositions.First().Y && position.Y == _playerShipPositions.Last().Y 
                && IsPositionOneUpOrDown(position))
                || (_playerShipPositions.Last().X == _playerShipPositions.First().X && position.X == _playerShipPositions.Last().X 
                && IsPositionOneLeftOrRight(position)))
            {
                return true;
            }

            return false;
        }

        private bool IsPositionOneUpOrDown(Position position)
        {
            this._playerShipPositions.Sort((a, b) => a.X.CompareTo(b.X));

            return position.X == _playerShipPositions.Last().X - 1 || position.X == _playerShipPositions.Last().X + 1
                || position.X == _playerShipPositions.First().X - 1 || position.X == _playerShipPositions.First().X + 1;
        }
        private bool IsPositionOneLeftOrRight(Position position)
        {
            this._playerShipPositions.Sort((a, b) => a.Y.CompareTo(b.Y));

            return position.Y == _playerShipPositions.Last().Y - 1 || position.Y == _playerShipPositions.Last().Y + 1
                || position.Y == _playerShipPositions.First().Y - 1 || position.Y == _playerShipPositions.First().Y + 1;
        }
    }
}
