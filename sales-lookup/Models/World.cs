using System;
using System.Collections.Generic;
using System.Text;

namespace sales_lookup.Models
{
    public class World
    {
        public Land[,] Lands = new Land[500, 327];

        public World()
        {
            int paviaX = -259; // left most x on map
            int paviaY = 138; // upper y on map

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
                paviaX = -259;
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
            int x = paviaX + 259;
            int y = 138 - paviaY;

            return (x, y);
        }

        public int GetXCoordinate(int x)
        {
            return x - 259;
        }

        public int GetYCoordinate(int y)
        {
            return 138 - y;
        }
    }
}
