using System;

namespace ShipGameLibrary
{
    public enum ShipType : int
    {
        SMALL = 1,
        MEDIUM = 2,
        BIG = 3,
        LARGE = 4
    }

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
        ShipType Type { get; }
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

    public enum BoardType : int
    {
        PLAYER_SHIPS = 1,
        ENEMY_SHIPS = 2,
        PLAYER_HITS = 3,
        ENEMY_HITS = 4,
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

                                if (this.EnemyShips.Arr[shipPosition.X, shipPosition.Y] == 1
                                    || this.EnemyShips.Arr[Math.Min(shipPosition.X + 1, boardSize - 1), Math.Min(shipPosition.Y + 1, boardSize - 1)] == 1
                                    || this.EnemyShips.Arr[Math.Max(shipPosition.X - 1, 0), Math.Max(shipPosition.Y - 1, 0)] == 1)
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

            Random random = new Random();
            // this.PlayersTurn = random.Next(0, 2) == 0 ? false : true;
            this.PlayersTurn = true;
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
        }

        public void AddEnemyShip(Ship ship)
        {
            if (this.AgainstComputer) throw new InvalidOperationException("Computer enabled");

            this.EnemyShips.SetPositions(1, ship.Positions);
        }

        public void AddPlayerHit(Position position)
        {
            if (!this.PlayersTurn) throw new InvalidOperationException("Enemy turn");

            for (int i = 0; i < this.EnemyShips.Arr.GetLength(0); i++)
            {
                for (int j = 0; j < this.EnemyShips.Arr.GetLength(1); j++)
                {
                    if (this.EnemyShips.Arr[position.X, position.Y] == 1)
                    {
                        this.PlayerHits.SetPositions((int)Shot.HIT, new Position[] { position });
                        return;
                    }
                }
            }

            this.PlayerHits.SetPositions((int)Shot.MISSED, new Position[] { position });
        }

        public void AddEnemyHit(Position position)
        {
            if (this.AgainstComputer) throw new InvalidOperationException("Computer enabled");
            if (this.PlayersTurn) throw new InvalidOperationException("Player turn");


            for (int i = 0; i < this.PlayerShips.Arr.GetLength(0); i++)
            {
                for (int j = 0; j < this.PlayerShips.Arr.GetLength(1); j++)
                {
                    if (this.PlayerShips.Arr[position.X, position.Y] == 1)
                    {
                        this.EnemyHits.SetPositions((int)Shot.HIT, new Position[] { position });
                        return;
                    }
                }
            }

            this.EnemyHits.SetPositions((int)Shot.MISSED, new Position[] { position });
        }
    }
}
