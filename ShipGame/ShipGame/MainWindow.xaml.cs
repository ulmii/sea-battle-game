using ShipGameLibrary;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.ObjectModel;

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
        private List<Position> _playerShipPositions = new List<Position>();

        public MainWindow()
        {
            InitializeComponent();
            this.PlayerHitsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_HITS);
            this.PlayerShipsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_SHIPS);

            this.PlayerHitsVB.Visibility = Visibility.Hidden;
            this.PlayerShipsVB.HorizontalAlignment = HorizontalAlignment.Center;
        }

        private ObservableCollection<ObservableCollection<DataButton>> LoadBasicBoard(BoardType type)
        {
            ObservableCollection<ObservableCollection<DataButton>> lsts = new ObservableCollection<ObservableCollection<DataButton>>();

            for (int i = 0; i < Engine.BoardSize; i++)
            {
                lsts.Add(new ObservableCollection<DataButton>());

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

            if (this._addPlayerShipSize <= 4)
            {
                this.InitPlayerShips(button);
                return;
            }

            if (data.Value.Type == BoardType.PLAYER_HITS && this.Engine.IsPlayersTurn() && data.Content.Count() == 0)
            {
                var playerHitBoard = this.PlayerHitsIS.ItemsSource as ObservableCollection<ObservableCollection<DataButton>>;

                playerHitBoard[data.Value.Position.X][data.Value.Position.Y] = new DataButton
                {
                    Content = Engine.AddPlayerHit(data.Value.Position) == Shot.HIT ? "X" : "O",
                    Value = new DataButton.ValueCls
                    {
                        Type = BoardType.PLAYER_SHIPS,
                        Position = new Position(data.Value.Position.X, data.Value.Position.Y)
                    }
                };
            }

            this.CheckEnemyShots();

            if (this.Engine.GetGameStatus() != GameStatus.ONGOING)
            {
                this.PlayerHitsVB.Visibility = Visibility.Hidden;
                this.PlayerShipsVB.Visibility = Visibility.Hidden;
                this.GameEndedLabel.Visibility = Visibility.Visible;
                this.GameEndedLabel.Content = this.Engine.GetGameStatus() == GameStatus.PLAYER_WIN ? "Player won" : "Computer won";
                this.PlayerShipsInfoLabel.Visibility = Visibility.Hidden;
                this.PlayerShotsInfoLabel.Visibility = Visibility.Hidden;
                this.QuitGameBt.Visibility = Visibility.Visible;
            }
        }

        private void ResetShipsBt_Click(object sender, RoutedEventArgs e)
        {
            this.Engine.ResetPlayerShips();
            this._playerShipPositions.Clear();
            this._addPlayerShipSize = 1;
            this.PlayerShipSizeCount.Content = 1;
            this.PlayerShipsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_SHIPS);
            this.StartGameBt.Visibility = Visibility.Hidden;
            this.PlayerShipSizeLabel.Visibility = Visibility.Visible;
            this.PlayerShipSizeCount.Visibility = Visibility.Visible;
        }
        private void StartGameBt_Click(object sender, RoutedEventArgs e)
        {
            this.Engine.StartGame();
            this.CheckEnemyShots();

            this.PlayerShipSizeLabel.Visibility = Visibility.Hidden;
            this.PlayerShipSizeCount.Visibility = Visibility.Hidden;
            this.PlayerHitsVB.Visibility = Visibility.Visible;
            this.PlayerShipsVB.HorizontalAlignment = HorizontalAlignment.Right;
            this.ResetShipsBt.Visibility = Visibility.Hidden;
            this.StartGameBt.Visibility = Visibility.Hidden;
            this.PlayerShotsInfoLabel.Visibility = Visibility.Visible;
            this.PlayerShipsInfoLabel.Visibility = Visibility.Visible;
        }

        private void CheckEnemyShots()
        {
            var playerBoard = this.PlayerShipsIS.ItemsSource as ObservableCollection<ObservableCollection<DataButton>>;
            var test = this.PlayerShipsIS.DataContext;

            foreach (var enemyShot in this.Engine.EnemyShots)
            {
                var pos = enemyShot.Key;

                playerBoard[pos.X][pos.Y] = new DataButton
                {
                    Content = enemyShot.Value == Shot.HIT ? "X" : "O",
                    Value = new DataButton.ValueCls
                    {
                        Type = BoardType.PLAYER_SHIPS,
                        Position = new Position(pos.X, pos.Y)
                    }
                };
            }
        }

        private void InitPlayerShips(Button button)
        {
            var playerBoard = this.PlayerShipsIS.ItemsSource as ObservableCollection<ObservableCollection<DataButton>>;
            var data = button.DataContext as DataButton;
            var check = this.Engine.IsFreeSpaceForShip(this.Engine.PlayerShips, data.Value.Position);

            if (data.Value.Type == BoardType.PLAYER_SHIPS && button.Content.Equals("")
                && this.Engine.IsFreeSpaceForShip(this.Engine.PlayerShips, data.Value.Position)
                && IsCorrectShipPosition(data.Value.Position))
            {
                this._playerShipPositions.Add(data.Value.Position);
                playerBoard[data.Value.Position.X][data.Value.Position.Y] = new DataButton
                {
                    Content = "*",
                    Value = new DataButton.ValueCls
                    {
                        Type = BoardType.PLAYER_SHIPS,
                        Position = new Position(data.Value.Position.X, data.Value.Position.Y)
                    }
                };

                if (this._addPlayerShipSize == this._playerShipPositions.Count)
                {
                    this.Engine.AddPlayerShip(new Ship(this._playerShipPositions.ToArray()));
                    this._addPlayerShipSize++;
                    this._playerShipPositions.Clear();
                }

                this.PlayerShipSizeCount.Content = this._addPlayerShipSize;

                if (this._addPlayerShipSize > 4)
                {
                    this.PlayerShipSizeLabel.Visibility = Visibility.Hidden;
                    this.PlayerShipSizeCount.Visibility = Visibility.Hidden;
                    this.StartGameBt.Visibility = Visibility.Visible;
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

        private void QuitGameBt_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
