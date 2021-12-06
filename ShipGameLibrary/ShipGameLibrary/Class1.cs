using System;
using System.Collections.Generic;

namespace ShipGameLibrary
{
    public enum Shot : int
    {
        MISSED = 1,
        HIT = 2
    }

    public class Board
    {
        int Size { get; set; }
        public int[,] Arr { get; set; }

        public Board(int size)
        {
            this.Size = size;
            this.Arr = new int[size, size];
        }

        public void SetPositions(int type, Position[] positions)
        {
            foreach (Position position in positions)
            {
                this.Arr[position.X, position.Y] = type;
            }
        }

        public void PrintBoard()
        {
            for (int i = 0; i < Arr.GetLength(0); i++)
            {
                for (int j = 0; j < Arr.GetLength(1); j++)
                {
                    Console.Write(Arr[i, j]);
                }
                Console.WriteLine();
            }
        }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    public class Ship
    {
        public Position[] Positions { get; }

        // random position
        public Ship(int size, int boardSize)
        {
            this.Positions = new Position[size];
            Random random = new Random();

            int orientation = random.Next(0, 2);

            if (orientation == 1)
            {
                int x = random.Next(0, boardSize - size);
                int y = random.Next(0, boardSize);
                for (int i = 0; i < size; i++)
                {
                    Position position = new Position(x + i, y);
                    this.Positions[i] = position;
                }
            }
            else
            {
                int x = random.Next(0, boardSize);
                int y = random.Next(0, boardSize - size);
                for (int i = 0; i < size; i++)
                {
                    Position position = new Position(x, y + i);
                    this.Positions[i] = position;
                }
            }

        }

        public Ship(Position[] positions)
        {
            this.Positions = positions;
        }
    }

    public enum BoardType
    {
        PLAYER_SHIPS,
        ENEMY_SHIPS,
        PLAYER_HITS,
        ENEMY_HITS,
    }

    public enum GameStatus
    {
        ONGOING,
        PLAYER_WIN,
        ENEMY_WIN
    }

    public class ShipGameEngine
    {
        public Board PlayerShips { get; }
        public Board EnemyShips { get; }
        public Board PlayerHits { get; }
        public Board EnemyHits { get; }
        public int BoardSize { get; }
        private bool AgainstComputer;
        private bool PlayersTurn;
        private int PlayerHitsCount = 0;
        private int EnemyHitsCount = 0;
        private int _playerShipCount = 0;
        private int _enemyShipCount = 0;
        private readonly Random _random = new Random();

        public ShipGameEngine(int boardSize, bool againstComputer)
        {
            this.BoardSize = boardSize;
            this.PlayerShips = new Board(boardSize);
            this.EnemyShips = new Board(boardSize);
            this.PlayerHits = new Board(boardSize);
            this.EnemyHits = new Board(boardSize);

            if (againstComputer)
            {
                int currentShipSize = 1;

                while (currentShipSize < 5)
                {
                    bool addShip = true;
                    Ship ship = new Ship(currentShipSize, boardSize);
                    for (int i = 0; i < this.EnemyShips.Arr.GetLength(0); i++)
                    {
                        for (int j = 0; j < this.EnemyShips.Arr.GetLength(1); j++)
                        {
                            for (int k = 0; k < ship.Positions.Length; k++)
                            {
                                Position shipPosition = ship.Positions[k];

                                if (!IsFreeSpaceForShip(this.EnemyShips, shipPosition))
                                {
                                    addShip = false;
                                }
                            }
                        }
                    }

                    if (addShip)
                    {
                        this.AddEnemyShip(ship);
                        currentShipSize += 1;
                    }
                }
            }

            this.PlayersTurn = _random.Next(0, 2) != 0;
            this.AgainstComputer = againstComputer;
        }

        public Board GetBoard(BoardType boardType)
        {
            if (boardType == BoardType.ENEMY_SHIPS)
            {
                return this.EnemyShips;
            }
            else if (boardType == BoardType.PLAYER_SHIPS)
            {
                return this.PlayerShips;
            }
            else if (boardType == BoardType.ENEMY_HITS)
            {
                return this.EnemyHits;
            }
            else if (boardType == BoardType.PLAYER_HITS)
            {
                return this.PlayerHits;
            }
            else
            {
                return null;
            }
        }

        public void AddPlayerShip(Ship ship)
        {
            this.PlayerShips.SetPositions(1, ship.Positions);
            this._playerShipCount++;

            if(this.AgainstComputer && !this.PlayersTurn && this._playerShipCount >= 4)
            {
                this.AiEnemyShot();
            }
        }

        public bool IsFreeSpaceForShip(Board board, Position position)
        {
            return board.Arr[position.X, position.Y] == 0
                && board.Arr[position.X, Math.Min(this.BoardSize - 1, position.Y + 1)] == 0
                && board.Arr[position.X, Math.Max(0, position.Y - 1)] == 0
                && board.Arr[Math.Min(position.X + 1, this.BoardSize - 1), position.Y] == 0
                && board.Arr[Math.Max(position.X - 1, 0), position.Y] == 0
                && board.Arr[Math.Min(position.X + 1, this.BoardSize - 1), Math.Min(this.BoardSize - 1, position.Y + 1)] == 0
                && board.Arr[Math.Max(position.X - 1, 0), Math.Max(0, position.Y - 1)] == 0
                && board.Arr[Math.Min(position.X + 1, this.BoardSize - 1), Math.Max(0, position.Y - 1)] == 0
                && board.Arr[Math.Max(position.X - 1, 0), Math.Min(this.BoardSize - 1, position.Y + 1)] == 0;
        }

        public void AddEnemyShip(Ship ship)
        {
            if (this.AgainstComputer) throw new InvalidOperationException("Computer enabled");

            this.EnemyShips.SetPositions(1, ship.Positions);
            this._enemyShipCount++;
        }

        public Shot AddPlayerHit(Position position)
        {
            if (!this.PlayersTurn) throw new InvalidOperationException("Enemy turn");
            if (this._enemyShipCount < 4) throw new InvalidOperationException("Enemy ships not initialized");

            this.PlayersTurn = false;
            Shot shot;

            if (this.EnemyShips.Arr[position.X, position.Y] == 1)
            {
                this.PlayerHits.SetPositions((int)Shot.HIT, new Position[] { position });
                this.PlayerHitsCount++;
                shot = Shot.HIT;
            }
            else
            {
                this.PlayerHits.SetPositions((int)Shot.MISSED, new Position[] { position });
                shot = Shot.MISSED;
            }

            if (this.AgainstComputer)
            {
                this.AiEnemyShot();
            }

            return shot;
        }

        public void AiEnemyShot()
        {
            bool shot = false;

            while (!shot)
            {
                int x = this._random.Next(0, 10);
                int y = this._random.Next(0, 10);

                if (this.EnemyHits.Arr[x, y] == 0)
                {
                    this.AddEnemyHit(new Position(x, y));
                    shot = true;
                }

            }
        }

        public Dictionary<Position, Shot> GetEnemyShots()
        {
            var positions = new Dictionary<Position, Shot>();

            for (int i = 0; i < this.EnemyHits.Arr.GetLength(0); i++)
            {
                for (int j = 0; j < this.EnemyShips.Arr.GetLength(1); j++)
                {
                    if (this.EnemyHits.Arr[i, j] != 0)
                    {
                        positions.Add(new Position(i, j), (Shot) this.EnemyHits.Arr[i, j]);
                    }
                }
            }

            return positions;
        }

        public Shot AddEnemyHit(Position position)
        {
            if (this.PlayersTurn) throw new InvalidOperationException("Player turn");

            this.PlayersTurn = true;

            if (this.PlayerShips.Arr[position.X, position.Y] == 1)
            {
                this.EnemyHits.SetPositions((int)Shot.HIT, new Position[] { position });
                this.EnemyHitsCount++;
                return Shot.HIT;
            }
            else
            {

                this.EnemyHits.SetPositions((int)Shot.MISSED, new Position[] { position });
                return Shot.MISSED;
            }
        }

        public bool IsPlayersTurn()
        {
            return this.PlayersTurn;
        }

        public GameStatus GetGameStatus()
        {
            if (this.PlayerHitsCount == 10)
            {
                return GameStatus.PLAYER_WIN;
            }
            else if (this.EnemyHitsCount == 10)
            {
                return GameStatus.ENEMY_WIN;
            }
            else
            {
                return GameStatus.ONGOING;
            }
        }
    }
}
