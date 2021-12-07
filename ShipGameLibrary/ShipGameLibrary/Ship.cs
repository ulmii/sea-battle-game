using System;
using System.Collections.Generic;
using System.Text;

namespace ShipGameLibrary
{
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
}
