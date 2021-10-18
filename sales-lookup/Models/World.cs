using System;
using System.Collections.Generic;
using System.Text;

namespace sales_lookup.Models
{
    public class World
    {
        public Land[,] Lands = new Land[223, 222];

        public World()
        {
            int paviaX = -115;
            int paviaY = 84;

            for (int y = 0; y < Lands.GetLength(1); y++)
            {
                for (int x = 0; x < Lands.GetLength(0); x++)
                {
                    Lands[x, y] = new Land
                    {
                        X = paviaX,
                        Y = paviaY
                    };

                    paviaX++;
                }

                paviaY--;
                paviaX = -115;
            }

            // setup plaza
            // x: from -14 to 15
            // y: from -15 to 14
            for (int y = -15; y <= 14; y++)
            {
                for (int x = -14; x <= 15; x++)
                {
                    var coords = GetCoordinates(x, y);

                    Lands[coords.X, coords.Y].IsPlaza = true;
                }
            }
        }

        public (int X, int Y) GetCoordinates(int paviaX, int paviaY)
        {
            int x = paviaX + 115;
            int y = 84 - paviaY;

            return (x, y);
        }

        public int GetXCoordinate(int x)
        {
            return x - 115;
        }

        public int GetYCoordinate(int y)
        {
            return 84 - y;
        }
    }
}
