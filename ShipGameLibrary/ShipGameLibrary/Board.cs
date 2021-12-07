using System;
using System.Collections.Generic;
using System.Text;

namespace ShipGameLibrary
{
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
}
