using System;
using System.Collections.Generic;
using Debug;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;

namespace Gameplay
{
    public static class BoardSystem
    {
        private static Tile[,] _tiles;

        /// <summary>
        /// Sets the size of the grid
        /// </summary>
        /// <param name="width">Number of tiles in x direction.</param>
        /// <param name="height">Number of tiles in y direction.</param>
        public static void SetGridSize(int width, int height)
        {
            _tiles = new Tile[width, height];
        }

        /// <summary>
        /// Gets the tile at grid coordinates, or returns null if there is no tile (or passed values are invalid)
        /// </summary>
        /// <param name="gridCoordinates">Coordinate of the tile.</param>
        /// <returns>Returns Tile or Null.</returns>
        [CanBeNull]
        public static Tile GetTileOnGrid(Vector2Int gridCoordinates)
        {
            //Check we have valid array index
            if (gridCoordinates.x >= 0 && gridCoordinates.x < _tiles.GetLength(0)
                                       && gridCoordinates.y >= 0 && gridCoordinates.y < _tiles.GetLength(1))
            {
                //Valid index, return Tile or Null depending on if we have anything here
                return _tiles[gridCoordinates.x, gridCoordinates.y];
            }

            //Invalid index, return null
            return null;
        }


        /// <summary>
        /// Gets an array containing all the tiles that fit within a Square area
        /// </summary>
        /// <param name="center">Coordinate of central tile, middle of the square. For even numbers (eg 4 cell square) this will be weighted to the central bottom left.</param>
        /// <param name="size">How large the square area is. Indicates number of tiles in both x and y directions (eg: value of 2 is a 2x2 square). Value of 1 returns only central tile.</param>
        /// <returns>Returns Tiles.</returns>
        public static Tile[] GetTilesInSquare(Vector2Int center, int size)
        {
            //Cannot get tiles in square when size is <=0, smallest square is 1 (center tile only)
            Assert.Greater(size, 0);

            //If size 1, then just get the center tile
            if (size == 1)
            {
                return new Tile[1] { GetTileOnGrid(center) };
            }

            int halfSize = 1 + Mathf.CeilToInt(size / 2.0f);
            return GetTilesInRect(center, size, size);
        }

        /// <summary>
        /// Gets an array containing all the tiles that fit within a Rectangle area
        /// </summary>
        /// <param name="center">Coordinate of central tile, middle of the square. For even numbers (eg 4 cell square) this will be weighted to the central bottom left.</param>
        /// <param name="width">Number of tiles in x direction.</param>
        /// <param name="height">Number of tiles in y direction.</param>
        /// <returns>Returns Tiles.</returns>
        public static Tile[] GetTilesInRect(Vector2Int center, int width, int height)
        {
            //Ensure we have valid size, size of 0 will never provide any tiles
            Assert.Greater(height, 0);
            Assert.Greater(width, 0);

            //Tiles we will return
            List<Tile> listOfTiles = new();

            //Area, Half height
            int area = height * width;
            int halfHeight = height / 2;
            int halfWidth = width / 2;

            //Loop over the area and add tiles to our return list
            int xOffset = 0;
            int yOffset = 0;
            for (int i = 0; i < area; i++)
            {
                //Get grid coordinate and tile at that location
                Vector2Int gridLocation =
                    new Vector2Int(xOffset + center.x - halfWidth, yOffset + center.y - halfHeight);

                //Try get the tile, then add it if we succeeded. Failure suggests tile is outside the play area
                Tile nextTile = GetTileOnGrid(gridLocation);
                if (nextTile)
                {
                    listOfTiles.Add(nextTile);
                }

                //Increment, reset X if we've reached width
                xOffset++;
                if (xOffset == width)
                {
                    xOffset = 0;
                    yOffset++;
                }
            }

            //Return our finished array
            return listOfTiles.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the tiles that fit within a Circular area
        /// </summary>
        /// <param name="center">Coordinate of central tile, middle of the circle. Unlike square, this will ALWAYS be the central tile.</param>
        /// <param name="radius">How large should circle be. Value of 1 is the central tile + 1 in every direction, increasing by +1 tile in every direction.</param>
        /// <returns>Returns Tiles.</returns>
        public static Tile[] GetTilesInCircle(Vector2Int center, int radius)
        {
            //Tiles to return
            List<Tile> listOfTiles = new();

            //Y Bounds of circle
            int top = Mathf.CeilToInt(center.y - radius),
                bottom = Mathf.FloorToInt(center.y + radius);

            //Calculate all tiles in circle
            for (int y = top; y <= bottom; y++)
            {
                int dy = y - center.y;
                float dx = Mathf.Sqrt(radius * radius - dy * dy);
                int left = Mathf.CeilToInt(center.x - dx),
                    right = Mathf.FloorToInt(center.x + dx);
                for (int x = left; x <= right; x++)
                {
                    //Get our tile, add it to data if valid
                    Tile tile = GetTileOnGrid(new Vector2Int(x, y));
                    if (tile)
                    {
                        listOfTiles.Add(tile);
                    }
                }
            }

            return listOfTiles.ToArray();
        }

        /// <summary>
        /// Gets all tiles that lie inside an Arc (Curve)
        /// </summary>
        /// <param name="center">Coordinate of central tile, origin of the arc.</param>
        /// <param name="arcLength">Length to the end of the arc, length = 1 + n tiles in direction.</param>
        /// <param name="dirAngle">Which direction is angle pointing? 0-Left, 0.25-Up, 0.5-Right, 0.75-Down.</param>
        /// <param name="curveAngle">How large will the arc be? 1-HalfCircle, 0.5-QuarterCircle, etc.</param>
        /// <returns>Returns an array of tiles that lie inside the arc defined in params.</returns>
        public static Tile[] GetTilesInArc(Vector2Int center, int arcLength, float dirAngle, float curveAngle)
        {
            //Return list
            List<Tile> listOfTiles = new();

            //Convert to useable correctly formatted values for formula
            dirAngle = Math.Clamp(1 - dirAngle, 0, 1);
            curveAngle = Math.Clamp(1 - curveAngle, 0, 1);

            //Get the fwd direction, which is world left offset by the angle passed in to func 
            Vector2 fwd = (Quaternion.Euler(0f, 0f, 360 * dirAngle) * Vector2.left).normalized;

            //Y Bounds of circle
            int top = Mathf.CeilToInt(center.y - arcLength);
            int bottom = Mathf.FloorToInt(center.y + arcLength);

            //~TODO: Remove duplicated code?
            //Calculate all tiles in circle
            for (int y = top; y <= bottom; y++)
            {
                int dy = y - center.y;
                float dx = Mathf.Sqrt(arcLength * arcLength - dy * dy);
                int left = Mathf.CeilToInt(center.x - dx);
                int right = Mathf.FloorToInt(center.x + dx);
                for (int x = left; x <= right; x++)
                {
                    //Calculate if the point in the circle obeys the arc angle, using dot product 
                    Vector3 dirCenterToPoint = (center - new Vector2(x, y)).normalized;
                    float dot = Vector2.Dot(fwd, dirCenterToPoint);

                    //If angle is valid add the tile to our output list
                    if (dot >= curveAngle)
                    {
                        Tile tile = GetTileOnGrid(new Vector2Int(x, y));
                        if (tile)
                        {
                            listOfTiles.Add(tile);
                        }
                    }
                }
            }

            //Return, converted to array
            return listOfTiles.ToArray();
        }

        /// <summary>
        /// Sets the tile at a specific grid coordinate, performed during board setup
        /// </summary>
        /// <param name="gridCoordinates">Coordinate to set.</param>
        /// <param name="tile">The tile.</param>
        public static void SetTileOnGrid(Vector2Int gridCoordinates, Tile tile)
        {
            //Ensure we are not overwriting a tile, that is not supported. If we want to replace a tile, functionality should be added for that
            Assert.IsNull(_tiles[gridCoordinates.x, gridCoordinates.y]);
            _tiles[gridCoordinates.x, gridCoordinates.y] = tile;
        }
    }

    public class BoardController : MonoBehaviour
    {
        [SerializeField] protected GameObject tilePrefab;

        //TODO: Move into some kind of level data class that gets broadcasted on level begin (unless this is consistent size)
        //A tile is 1m, so a board of size 1,1 can fit a single tile
        [SerializeField] protected int boardWidth;
        [SerializeField] protected int boardHeight;

        [SerializeField] protected float boardScale;

        [SerializeField] protected Transform mkrBoardStart;
        
        private void Start()
        {
            //Ensure prefab is valid
            if (!tilePrefab)
            {
                DebugSystem.Error("Tile prefab undefined, cannot spawn new tiles on the board.");
                return;
            }

            //Create tiles array
            BoardSystem.SetGridSize(boardWidth, boardHeight);

            //Iterate over and assign tiles to array
            int numberOfTiles = boardWidth * boardHeight;
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < numberOfTiles; i++)
            {
                //Get coordinate
                Vector2Int gridCoordinates = new Vector2Int(currentX, currentY);

                //Spawn
                GameObject tileObject = Instantiate(tilePrefab,
                    GetTileLocalPositionOnGrid(gridCoordinates),
                    transform.rotation, transform);

                //Get Tile comp and save it, ensuring it is not null
                Tile tile = tileObject.GetComponentInChildren<Tile>();
                Assert.NotNull(tile);

                //Set grid location on tile
                tile.SetGridLocation(new Vector2Int(currentX, currentY));

                //Add to data
                BoardSystem.SetTileOnGrid(gridCoordinates, tile);

                //Set scale
                Transform tileTransform = tileObject.GetComponent<Transform>();
                tileTransform.localScale = new Vector3(boardScale, boardScale, boardScale);

                //Increment location for next tile, obeying grid layout
                currentX++;
                if (currentX == boardWidth)
                {
                    currentX = 0;
                    currentY++;
                }
            }
        }
        
        /// <summary>
        /// Gets the LocalPosition the tile should be placed in, based on its grid coordinates.
        /// </summary>
        /// <param name="gridCoordinates">Where does this tile lay on the grid.</param>
        /// <returns>Returns the local position for the tile.</returns>
        protected Vector3 GetTileLocalPositionOnGrid(Vector2Int gridCoordinates)
        {
            //Success
            Vector3 endPosition = mkrBoardStart ? mkrBoardStart.position : Vector3.zero;
            Vector3 xOffset = transform.right * gridCoordinates.x;
            Vector3 yOffset = transform.forward * gridCoordinates.y;
            Vector3 offset = (xOffset + yOffset) * boardScale;
            return endPosition + offset;
        }
    }
}