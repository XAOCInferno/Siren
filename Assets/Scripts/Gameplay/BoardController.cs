using System;
using System.Collections.Generic;
using Debug;
using Gameplay.Piece;
using Gameplay.Tile;
using Global;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEngine;
using Utils;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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

        public static T[,] GetAllItems() => _items;

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
        /// <param name="center">Coordinate of central item, middle of the square.</param>
        /// <param name="size">How large the square area is. Indicates size from center in all directions eg 1 = 3x3, 2 = 5x5.</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInSquare(Vector2Int center, int size)
        {
            //Cannot get items in square when size is <=0, smallest square is 1 (center item only)
            Assert.Greater(size, 0);
            return GetItemsInRect(center, size, size);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a Rectangle area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the square.</param>
        /// <param name="width">Number of items in x direction away from center.</param>
        /// <param name="height">Number of items in y direction away from center.</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInRect(Vector2Int center, int width, int height)
        {
            //Ensure we have valid size, size of 0 will never provide any items
            Assert.Greater(height, 0);
            Assert.Greater(width, 0);

            //Items we will return
            List<T> listOfItems = new();
            Vector2Int origin = center - new Vector2Int(0, height); //Start at top
            Vector2Int xDir = Vector2Int.right;
            Vector2Int[] directions = { xDir, xDir * -1 };

            //height *= 2;

            //Double to get omni-directional rect, centered on Center
            //height *= 2;
            //width *= 2;

            for (int i = 0; i < (height * 2) + 1; i++)
            {
                listOfItems.AddRange(GetItemsInLines(origin, directions, width + 1, false));
                origin += Vector2Int.up;
            }

            //Remove duplicates
            listOfItems.RemoveDuplicates();

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
        /// Gets an array containing all the items that fit within a line
        /// </summary>
        /// <param name="origin">Coordinate of origin of start of the line.</param>
        /// <param name="dir">Which direction is line pointing in?</param>
        /// <param name="distance">How long is the line?</param>
        /// <param name="ignoreOrigin">Should we ignore the origin itself?</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInLine(Vector2Int origin, Vector2Int dir, int distance, bool ignoreOrigin = false)
        {
            //Items to return
            List<T> listOfItems = new();

            //Ensure direction is clamped correctly
            dir.Clamp(new Vector2Int(-1, -1), new Vector2Int(1, 1));

            //Calculate all items in circle
            for (int i = ignoreOrigin ? 1 : 0; i < distance; i++)
            {
                T item = GetItemOnGrid(origin + (dir * i));
                if (item)
                {
                    listOfItems.Add(item);
                }
            }

            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a line
        /// </summary>
        /// <param name="origin">Coordinate of origin of start of the line.</param>
        /// <param name="dirs">Which directions is line pointing in?</param>
        /// <param name="distance">How long is the line?</param>
        /// <param name="ignoreOrigin">Should we ignore the origin itself?</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInLines(Vector2Int origin, Vector2Int[] dirs, int distance, bool ignoreOrigin = false)
        {
            //Items to return
            List<T> listOfItems = new();

            //Add for all directions
            for (int i = 0; i < dirs.Length; i++)
            {
                listOfItems.AddRange(GetItemsInLine(origin, dirs[i], distance, ignoreOrigin));
            }

            //Remove any duplicates which can happen if lines cross
            listOfItems.RemoveDuplicates();

            //Return
            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a + shaped cross
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInCross(Vector2Int center, int distance)
        {
            //Items to return
            List<T> listOfItems = new();

            //XY Directions
            Vector2Int xDir = Vector2Int.right;
            Vector2Int yDir = Vector2Int.up;
            Vector2Int[] directions = { xDir, yDir, xDir * -1, yDir * -1 };

            //Get origin point
            T item = GetItemOnGrid(center);
            if (item) listOfItems.Add(item);

            //Get all pieces in a line in X and Y dir
            listOfItems.AddRange(GetItemsInLines(center, directions, distance + 1, true));

            //Remove any duplicates, can happen when comboing checks
            listOfItems.RemoveDuplicates();

            //Return
            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the items that fit within an x shaped cross
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInDiagonalCross(Vector2Int center, int distance)
        {
            //Items to return
            List<T> listOfItems = new();

            //XY Directions
            Vector2Int xDir = new Vector2Int(1, 1);
            Vector2Int yDir = new Vector2Int(-1, 1);
            Vector2Int[] directions = { xDir, yDir, xDir * -1, yDir * -1 };

            //Get origin point
            T item = GetItemOnGrid(center);
            if (item) listOfItems.Add(item);

            //Get all pieces in a line in X and Y dir
            listOfItems.AddRange(GetItemsInLines(center, directions, distance + 1, true));

            //Remove any duplicates, can happen when comboing checks
            listOfItems.RemoveDuplicates();

            //Return
            return listOfItems.ToArray();
        }

        /// <summary>
        /// Gets an array containing all the items that fit within an combined cross and diamond shaped star
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static T[] GetItemsInStar(Vector2Int center, int distance)
        {
            List<T> returnList = new();
            returnList.AddRange(GetItemsInCross(center, distance));
            returnList.AddRange(GetItemsInDiagonalCross(center, distance));
            return returnList.ToArray();
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
        protected async void CreateBoardLayout()
        {
            //Ensure prefab is valid
            if (!tilePrefab)
            {
                DebugSystem.Error("Tile prefab undefined, cannot spawn new tiles on the board.");
                return;
            }

            //Create tiles array
            BoardSystem<TileObject>.SetGridSize(boardWidth, boardHeight);
            BoardSystem<PieceObject>.SetGridSize(boardWidth, boardHeight);

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
                TileObject tile = tileObject.GetComponentInChildren<TileObject>();
                Assert.NotNull(tile);
                await tile.Init();
                tile.GetState().GetLogicStateMachine().SetState(ETileLogicState.Idle);
                tile.GetState().GetViewStateMachine().SetState(ETileViewState.Idle);

                //Set grid location on tile
                tile.GetState().SetGridLocation(new Vector2Int(currentX, currentY));

                //Add to data
                BoardSystem<TileObject>.SetItemOnGrid(gridCoordinates, tile);

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
            TileObject tile = BoardSystem<TileObject>.GetItemOnGrid(payload.gridCoordinates);

            //Check tile is present
            if (!tile)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} as there is no tile there");
                return;
            }

            //Check location is free
            if (BoardSystem<PieceObject>.GetItemOnGrid(payload.gridCoordinates))
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} as this location is already occupied");
                return;
            }

            //Try get pooled object
            PooledObject piecePooledObject = PoolSystem<PieceLogic>.GetPool().GetNextAvailable();
            if (!piecePooledObject)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to no pooled pieces being ready");
                return;
            }

            //Get piece  mkr
            PieceObject pieceObject = piecePooledObject.GetComponent<PieceObject>();
            Transform connectionMkr = pieceObject.GetTileConnectionMkr();
            //Check connection pieces are valid
            if (!tile.GetPieceConnectionMkr() || !connectionMkr)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to missing connection mkr on either the piece or the tile, check prefabs");
                return;
            }

            //Now everything has been validated, add it

            //Logic
            PieceLogic pieceLogic = piecePooledObject.GetComponent<PieceLogic>();

            //Set scale
            Transform pieceTransform = pieceLogic.GetComponent<Transform>();
            pieceTransform.localScale = new Vector3(boardScale, boardScale, boardScale);

            //Parent & offset
            TileObject tileParent = BoardSystem<TileObject>.GetItemOnGrid(payload.gridCoordinates);
            if (!tileParent)
            {
                //Err: missing tile
                DebugSystem.Error(
                    $"No Tile at position {payload.gridCoordinates.ToString()} to place piece on! This shouldn't happen.");
                return;
            }

            //Set occupied
            tileParent.GetState().SetOccupier(pieceLogic);

            pieceTransform.parent = tileParent.GetComponent<Transform>();
            pieceTransform.localPosition = tile.GetPieceConnectionMkr().transform.localPosition +
                                           (connectionMkr.transform.localPosition * -1);

            //Set State
            pieceObject.GetLogic().SetCardData(payload.pieceData);
            pieceObject.GetState().SetOwnerPlayer(payload.spawnedByPlayer);
            BoardSystem<PieceObject>.SetItemOnGrid(payload.gridCoordinates, pieceObject);
            pieceObject.GetState().SetGridLocation(payload.gridCoordinates);
            pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }
    }
}