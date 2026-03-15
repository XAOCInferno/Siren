using System;
using System.Collections.Generic;
using Debug;
using Global;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;
using Utils;

namespace Gameplay
{
    /// <summary>
    /// System handling a grid with items on it.
    /// T: Represents which layer we are occupying. EG: Tile layer, Piece layer. Items on different layers will not block each other and will need further checks if this is required. 
    /// </summary>
    public static class BoardSystem<T> where T : UnityEngine.Object
    {
        private static T[,] _items = new T[0, 0];

        /// <summary>
        /// Sets the size of the grid
        /// </summary>
        /// <param name="width">Number of items in x direction.</param>
        /// <param name="height">Number of items in y direction.</param>
        public static void SetGridSize(int width, int height)
        {
            _items = new T[width, height];
        }

        public static Vector2Int GetGridSize()
        {
            return new Vector2Int(_items.GetLength(0), _items.GetLength(1));
        }

        /// <summary>
        /// Gets the item at grid coordinates, or returns null if there is no item (or passed values are invalid)
        /// </summary>
        /// <param name="gridCoordinates">Coordinate of the item.</param>
        /// <returns>Returns T or Null.</returns>
        [CanBeNull]
        public static T GetItemOnGrid(Vector2Int gridCoordinates)
        {
            //Check we have valid array index
            if (gridCoordinates.x >= 0 && gridCoordinates.x < _items.GetLength(0)
                                       && gridCoordinates.y >= 0 && gridCoordinates.y < _items.GetLength(1))
            {
                //Valid index, return T or Null depending on if we have anything here
                return _items[gridCoordinates.x, gridCoordinates.y];
            }

            //Invalid index, return null
            return null;
        }


        /// <summary>
        /// Gets an array containing all the items that fit within a Square area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the square. For even numbers (eg 4 cell square) this will be weighted to the central bottom left.</param>
        /// <param name="size">How large the square area is. Indicates number of items in both x and y directions (eg: value of 2 is a 2x2 square). Value of 1 returns only central item.</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInSquare(Vector2Int center, int size)
        {
            //Cannot get items in square when size is <=0, smallest square is 1 (center item only)
            Assert.Greater(size, 0);

            //If size 1, then just get the center item
            if (size == 1)
            {
                return new T[1] { GetItemOnGrid(center) };
            }

            int halfSize = 1 + Mathf.CeilToInt(size / 2.0f);
            return GetItemsInRect(center, size, size);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a Rectangle area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the square. For even numbers (eg 4 cell square) this will be weighted to the central bottom left.</param>
        /// <param name="width">Number of items in x direction.</param>
        /// <param name="height">Number of items in y direction.</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInRect(Vector2Int center, int width, int height)
        {
            //Ensure we have valid size, size of 0 will never provide any items
            Assert.Greater(height, 0);
            Assert.Greater(width, 0);

            //Items we will return
            List<T> listOfItems = new();

            //Area, Half height
            int area = height * width;
            int halfHeight = height / 2;
            int halfWidth = width / 2;

            //Loop over the area and add items to our return list
            int xOffset = 0;
            int yOffset = 0;
            for (int i = 0; i < area; i++)
            {
                //Get grid coordinate and item at that location
                Vector2Int gridLocation =
                    new Vector2Int(xOffset + center.x - halfWidth, yOffset + center.y - halfHeight);

                //Try get the item, then add it if we succeeded. Failure suggests item is outside the play area
                T nextItem = GetItemOnGrid(gridLocation);
                if (nextItem)
                {
                    listOfItems.Add(nextItem);
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
            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a Circular area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the circle. Unlike square, this will ALWAYS be the central item.</param>
        /// <param name="radius">How large should circle be. Value of 1 is the central item + 1 in every direction, increasing by +1 item in every direction.</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInCircle(Vector2Int center, int radius)
        {
            //Items to return
            List<T> listOfItems = new();

            //Y Bounds of circle
            int top = Mathf.CeilToInt(center.y - radius),
                bottom = Mathf.FloorToInt(center.y + radius);

            //Calculate all items in circle
            for (int y = top; y <= bottom; y++)
            {
                int dy = y - center.y;
                float dx = Mathf.Sqrt(radius * radius - dy * dy);
                int left = Mathf.CeilToInt(center.x - dx),
                    right = Mathf.FloorToInt(center.x + dx);
                for (int x = left; x <= right; x++)
                {
                    //Get our item, add it to data if valid
                    T item = GetItemOnGrid(new Vector2Int(x, y));
                    if (item)
                    {
                        listOfItems.Add(item);
                    }
                }
            }

            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets all items that lie inside an Arc (Curve)
        /// </summary>
        /// <param name="center">Coordinate of central item, origin of the arc.</param>
        /// <param name="arcLength">Length to the end of the arc, length = 1 + n items in direction.</param>
        /// <param name="dirAngle">Which direction is angle pointing? 0-Left, 0.25-Up, 0.5-Right, 0.75-Down.</param>
        /// <param name="curveAngle">How large will the arc be? 1-HalfCircle, 0.5-QuarterCircle, etc.</param>
        /// <returns>Returns an array of items that lie inside the arc defined in params.</returns>
        public static T[] GetItemsInArc(Vector2Int center, int arcLength, float dirAngle, float curveAngle)
        {
            //Return list
            List<T> listOfItems = new();

            //Convert to useable correctly formatted values for formula
            dirAngle = Math.Clamp(1 - dirAngle, 0, 1);
            curveAngle = Math.Clamp(1 - curveAngle, 0, 1);

            //Get the fwd direction, which is world left offset by the angle passed in to func 
            Vector2 fwd = (Quaternion.Euler(0f, 0f, 360 * dirAngle) * Vector2.left).normalized;

            //Y Bounds of circle
            int top = Mathf.CeilToInt(center.y - arcLength);
            int bottom = Mathf.FloorToInt(center.y + arcLength);

            //~TODO: Remove duplicated code?
            //Calculate all items in circle
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

                    //If angle is valid add the item to our output list
                    if (dot >= curveAngle)
                    {
                        T item = GetItemOnGrid(new Vector2Int(x, y));
                        if (item)
                        {
                            listOfItems.Add(item);
                        }
                    }
                }
            }

            //Return, converted to array
            return listOfItems.ToArray();
        }

        /// <summary>
        /// Sets the item at a specific grid coordinate, performed during board setup
        /// </summary>
        /// <param name="gridCoordinates">Coordinate to set.</param>
        /// <param name="item">The item.</param>
        public static void SetItemOnGrid(Vector2Int gridCoordinates, T item)
        {
            //Ensure we are not overwriting a item, that is not supported. If we want to replace a item, functionality should be added for that
            Assert.IsNull(_items[gridCoordinates.x, gridCoordinates.y]);
            _items[gridCoordinates.x, gridCoordinates.y] = item;
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

        private void Awake()
        {
            BoardEvents.OnOrderPlacePieceOnBoard += OnOrderPlacePieceOnBoard;
        }

        private void Start()
        {
            CreateBoardLayout();
        }

        /// <summary>
        /// Creates the board, filling it with tiles.
        /// </summary>
        protected void CreateBoardLayout()
        {
            //Ensure prefab is valid
            if (!tilePrefab)
            {
                DebugSystem.Error("Tile prefab undefined, cannot spawn new tiles on the board.");
                return;
            }

            //Create tiles array
            BoardSystem<Tile>.SetGridSize(boardWidth, boardHeight);
            BoardSystem<Piece>.SetGridSize(boardWidth, boardHeight);

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
                BoardSystem<Tile>.SetItemOnGrid(gridCoordinates, tile);

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
            Vector3 endPosition = mkrBoardStart ? mkrBoardStart.position : Vector3.zero;
            Vector3 xOffset = transform.right * gridCoordinates.x;
            Vector3 yOffset = transform.forward * gridCoordinates.y;
            Vector3 offset = (xOffset + yOffset) * boardScale;
            return endPosition + offset;
        }

        protected void OnOrderPlacePieceOnBoard(object sender, BoardEvents.OrderPlacePieceOnBoardPayload payload)
        {
            //Get tile we wish to place on
            Tile tile = BoardSystem<Tile>.GetItemOnGrid(payload.gridCoordinates);

            //Check tile is present
            if (!tile)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} as there is no tile there");
                return;
            }

            //Check location is free
            if (BoardSystem<Piece>.GetItemOnGrid(payload.gridCoordinates))
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} as this location is already occupied");
                return;
            }

            //Try get pooled object
            PooledObject piecePooledObject = PoolSystem<Piece>.GetPool().GetNextAvailable();
            if (!piecePooledObject)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to no pooled pieces being ready");
                return;
            }

            //Get piece
            Piece piece = piecePooledObject.GetComponent<Piece>();

            //Check connection pieces are valid
            if (!tile.GetPieceConnectionMkr() || !piece.GetTileConnectionMkr())
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to missing connection mkr on either the piece or the tile, check prefabs");
                return;
            }

            //Now everything has been validated, add it

            //Set scale
            Transform pieceTransform = piece.GetComponent<Transform>();
            pieceTransform.localScale = new Vector3(boardScale, boardScale, boardScale);

            //Parent & offset
            Tile tileParent = BoardSystem<Tile>.GetItemOnGrid(payload.gridCoordinates);
            if (!tileParent)
            {
                //Err: missing tile
                DebugSystem.Error(
                    $"No Tile at position {payload.gridCoordinates.ToString()} to place piece on! This shouldn't happen.");
                return;
            }

            pieceTransform.parent = tileParent.GetComponent<Transform>();
            pieceTransform.localPosition = tile.GetPieceConnectionMkr().transform.localPosition +
                                           (piece.GetTileConnectionMkr().transform.localPosition * -1);

            //Set State
            BoardSystem<Piece>.SetItemOnGrid(payload.gridCoordinates, piece);
            piece.SetGridLocation(payload.gridCoordinates);
            piece.SetState(EPieceState.OnBoard);
        }
    }
}