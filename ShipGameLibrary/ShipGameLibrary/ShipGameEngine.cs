using System;
using System.Collections.Generic;
using System.Text;

namespace ShipGameLibrary
{
    public class ShipGameEngine
    {
        public Board PlayerShips { get; set; }
        public Board EnemyShips { get; }
        public Board PlayerHits { get; }
        public Board EnemyHits { get; }
        public int BoardSize { get; }
        public Dictionary<Position, Shot> EnemyShots = new Dictionary<Position, Shot>();

        private readonly bool AgainstComputer;
        private readonly int AiDifficulty = 10;
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
        }

        public void ResetPlayerShips()
        {
            this.PlayerShips = new Board(this.BoardSize);
            this._playerShipCount = 0;
        }

        public void StartGame()
        {
            if (this.AgainstComputer && !this.PlayersTurn && this._playerShipCount >= 4)
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
                var hitProbability = this._random.Next(0, 100);

                if (hitProbability < this.AiDifficulty)
                {
                    var pos = FindPlayerShipPosition();

                    if (this.EnemyHits.Arr[pos.X, pos.Y] == 0)
                    {
                        this.AddEnemyHit(pos);
                        shot = true;
                    }
                }
                else
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
        }

        private Position FindPlayerShipPosition()
        {
            for (int i = 0; i < this.PlayerShips.Arr.GetLength(0); i++)
            {
                for (int j = 0; j < this.PlayerShips.Arr.GetLength(1); j++)
                {
                    var pos = new Position(i, j);
                    if (this.PlayerShips.Arr[i, j] == 1 && !this.EnemyShots.ContainsKey(pos))
                    {
                        return pos;
                    }
                }
            }

            return new Position(this._random.Next(0, 10), this._random.Next(0, 10));
        }

        public Shot AddEnemyHit(Position position)
        {
            if (this.PlayersTurn) throw new InvalidOperationException("Player turn");

            this.PlayersTurn = true;

            if (this.PlayerShips.Arr[position.X, position.Y] == 1)
            {
                this.EnemyHits.SetPositions((int)Shot.HIT, new Position[] { position });
                this.EnemyHitsCount++;
                this.EnemyShots.Add(position, Shot.HIT);
                return Shot.HIT;
            }
            else
            {
                this.EnemyHits.SetPositions((int)Shot.MISSED, new Position[] { position });
                this.EnemyShots.Add(position, Shot.MISSED);
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
