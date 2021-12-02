using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShipGameLibrary;

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
        private ShipGameEngine engine = new ShipGameEngine(10, true);
        public Board PlayerHits { get; set; }

        public MainWindow()
        {
            List<List<DataButton>> lsts = new List<List<DataButton>>();

            for (int i = 0; i < engine.BoardSize; i++)
            {
                lsts.Add(new List<DataButton>());

                for (int j = 0; j < engine.BoardSize; j++)
                {
                    var model = new DataButton
                    {
                        Content = engine.EnemyShips.Arr[i, j] == 1 ? "*" : "",
                        Value = new DataButton.ValueCls
                        {
                            Type = BoardType.ENEMY_SHIPS,
                            Position = new Position(i, j)
                        }
                    };
                    lsts[i].Add(model);
                }
            }

            this.PlayerHits = engine.PlayerHits;

            InitializeComponent();
            this.PlayerHitsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_HITS);
            this.PlayerShipsIS.ItemsSource = LoadBasicBoard(BoardType.PLAYER_SHIPS);
        }

        private List<List<DataButton>> LoadBasicBoard(BoardType type)
        {
            List<List<DataButton>> lsts = new List<List<DataButton>>();

            for (int i = 0; i < engine.BoardSize; i++)
            {
                lsts.Add(new List<DataButton>());

                for (int j = 0; j < engine.BoardSize; j++)
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

        private void ItemButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var data = button.DataContext as DataButton;

            var board = engine.GetBoard(data.Value.Type);

            if(data.Value.Type == BoardType.PLAYER_HITS)
            {
                if (engine.EnemyShips.Arr[data.Value.Position.X, data.Value.Position.Y] == 1)
                {
                    button.Content = Shot.HIT;
                }
                else
                {
                    button.Content = Shot.MISSED;
                }
                engine.AddPlayerHit(data.Value.Position);
            }
        }
    }
}
