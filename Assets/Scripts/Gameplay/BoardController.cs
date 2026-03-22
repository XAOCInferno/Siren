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
    //TODO: It seems inefficient converting from list to array here. Should refactor to return lists instead of arrays

    /// <summary>
    /// System handling a grid with items on it.
    /// T: Represents which layer we are occupying. EG: Tile layer, Piece layer. Items on different layers will not block each other and will need further checks if this is required. 
    /// </summary>
    public static class BoardSystem<T> where T : UnityEngine.Object
    {
        public struct GetItemsInAreaResponse
        {
            public T[] foundItems;

            public bool
                isMissingExpectedItems; //Any items we were expecing are missing? eg if the grid locations were outside the grid

            public GetItemsInAreaResponse(T[] foundItems, bool isMissingExpectedItems)
            {
                this.foundItems = foundItems;
                this.isMissingExpectedItems = isMissingExpectedItems;
            }
        }

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
        /// Gets the item at an offset position
        /// </summary>
        /// <param name="origin">Where to start moving from</param>
        /// <param name="dir">Direction to offset in, consider this a rotation</param>
        /// <param name="distanceX">How far to offset in X direction</param>
        /// <param name="distanceY">How far to offset in Y direction</param>
        /// <returns>Returns T or Null.</returns>
        public static T GetItemAtOffset(Vector2Int origin, Vector2Int dir, int distanceX, int distanceY)
        {
            int xOffset = distanceX * dir.x;
            int yOffset = distanceY * dir.y;

            return GetItemOnGrid(new Vector2Int(origin.x + xOffset, origin.y + yOffset));
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a Square area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the square.</param>
        /// <param name="size">How large the square area is. Indicates size from center in all directions eg 1 = 3x3, 2 = 5x5.</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInSquare(Vector2Int center, int size)
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
        public static GetItemsInAreaResponse GetItemsInRect(Vector2Int center, int width, int height)
        {
            //Ensure we have valid size, size of 0 will never provide any items
            Assert.Greater(height, 0);
            Assert.Greater(width, 0);

            //Items we will return
            List<T> listOfItems = new();
            Vector2Int origin = center - new Vector2Int(0, height); //Start at top
            Vector2Int xDir = Vector2Int.right;
            Vector2Int[] directions = { xDir, xDir * -1 };
            bool isMissingExpectedItems = false;

            for (int i = 0; i < (height * 2) + 1; i++)
            {
                GetItemsInAreaResponse response = GetItemsInLines(origin, directions, width + 1, false);
                if (response.isMissingExpectedItems)
                {
                    isMissingExpectedItems = true;
                }

                listOfItems.AddRange(response.foundItems);
                origin += Vector2Int.up;
            }

            //Remove duplicates
            listOfItems.RemoveDuplicates();

            //Return our finished array
            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a Circular area
        /// </summary>
        /// <param name="center">Coordinate of central item, middle of the circle. Unlike square, this will ALWAYS be the central item.</param>
        /// <param name="radius">How large should circle be. Value of 1 is the central item + 1 in every direction, increasing by +1 item in every direction.</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInCircle(Vector2Int center, int radius)
        {
            //Items to return
            List<T> listOfItems = new();

            bool isMissingExpectedItems = false;

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
                    else
                    {
                        isMissingExpectedItems = true;
                    }
                }
            }

            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
        }

        /// <summary>
        /// Gets all items that lie inside an Arc (Curve)
        /// </summary>
        /// <param name="center">Coordinate of central item, origin of the arc.</param>
        /// <param name="arcLength">Length to the end of the arc, length = 1 + n items in direction.</param>
        /// <param name="dirAngle">Which direction is angle pointing? 0-Left, 0.25-Up, 0.5-Right, 0.75-Down.</param>
        /// <param name="curveAngle">How large will the arc be? 1-HalfCircle, 0.5-QuarterCircle, etc.</param>
        /// <returns>Returns an array of items that lie inside the arc defined in params.</returns>
        public static GetItemsInAreaResponse GetItemsInArc(Vector2Int center, int arcLength, float dirAngle,
            float curveAngle)
        {
            //Return list
            List<T> listOfItems = new();

            bool isMissingExpectedItems = false;

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
                        else
                        {
                            isMissingExpectedItems = true;
                        }
                    }
                }
            }

            //Return, converted to array
            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a line
        /// </summary>
        /// <param name="origin">Coordinate of origin of start of the line.</param>
        /// <param name="dir">Which direction is line pointing in?</param>
        /// <param name="distance">How long is the line?</param>
        /// <param name="ignoreOrigin">Should we ignore the origin itself?</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInLine(Vector2Int origin, Vector2Int dir, int distance,
            bool ignoreOrigin = false)
        {
            //Items to return
            List<T> listOfItems = new();

            bool isMissingExpectedItems = false;

            //Ensure direction is clamped correctly
            dir.Clamp(new Vector2Int(-1, -1), new Vector2Int(1, 1));
            Assert.AreNotEqual(Vector2.zero, dir);

            //Calculate all items in circle
            for (int i = ignoreOrigin ? 1 : 0; i < distance; i++)
            {
                T item = GetItemOnGrid(origin + (dir * i));
                if (item)
                {
                    listOfItems.Add(item);
                }
                else
                {
                    isMissingExpectedItems = true;
                }
            }

            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a line
        /// </summary>
        /// <param name="origin">Coordinate of origin of start of the line.</param>
        /// <param name="dirs">Which directions is line pointing in?</param>
        /// <param name="distance">How long is the line?</param>
        /// <param name="ignoreOrigin">Should we ignore the origin itself?</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInLines(Vector2Int origin, Vector2Int[] dirs, int distance,
            bool ignoreOrigin = false)
        {
            //Items to return
            List<T> listOfItems = new();
            bool isMissingExpectedItems = false;

            //Add for all directions
            for (int i = 0; i < dirs.Length; i++)
            {
                GetItemsInAreaResponse response = GetItemsInLine(origin, dirs[i], distance, ignoreOrigin);
                if (response.isMissingExpectedItems)
                {
                    isMissingExpectedItems = true;
                }

                listOfItems.AddRange(response.foundItems);
            }

            //Remove any duplicates which can happen if lines cross
            listOfItems.RemoveDuplicates();

            //Return
            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within a + shaped cross
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInCross(Vector2Int center, int distance)
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
            GetItemsInAreaResponse response = GetItemsInLines(center, directions, distance + 1, true);
            listOfItems.AddRange(response.foundItems);

            //Remove any duplicates, can happen when comboing checks
            listOfItems.RemoveDuplicates();

            //Return
            return new GetItemsInAreaResponse(listOfItems.ToArray(), response.isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within an x shaped cross
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInDiagonalCross(Vector2Int center, int distance)
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
            GetItemsInAreaResponse response = GetItemsInLines(center, directions, distance + 1, true);
            listOfItems.AddRange(response.foundItems);

            //Remove any duplicates, can happen when comboing checks
            listOfItems.RemoveDuplicates();

            //Return
            return new GetItemsInAreaResponse(listOfItems.ToArray(), response.isMissingExpectedItems);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within an combined cross and diamond shaped star
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInStar(Vector2Int center, int distance)
        {
            List<T> returnList = new();

            //Get items
            GetItemsInAreaResponse responseCross = GetItemsInCross(center, distance);
            GetItemsInAreaResponse responseDiagonalCross = GetItemsInDiagonalCross(center, distance);

            //Write to our list
            returnList.AddRange(responseCross.foundItems);
            returnList.AddRange(responseDiagonalCross.foundItems);

            //Return
            return new GetItemsInAreaResponse(returnList.ToArray(),
                responseDiagonalCross.isMissingExpectedItems && responseCross.isMissingExpectedItems);
        }

        /// <summary>
        /// Returns the item at the end of standard L shape.
        /// </summary>
        /// <param name="origin">Coordinate of central item.</param>
        /// <param name="dir">Direction of the L, note this needs to be not zero.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static T GetItemAtEndOfStandingLShape(Vector2Int origin, Vector2Int dir, int distance)
        {
            return GetItemAtEndOfLShape(origin, dir, distance, false);
        }

        /// <summary>
        /// Returns the item at the end of a resting L shape. Resting means L rotated 90 degrees.
        /// </summary>
        /// <param name="origin">Coordinate of central item.</param>
        /// <param name="dir">Direction of the L, note this needs to be not zero.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <returns>Returns Array of T.</returns>
        public static T GetItemAtEndOfRestingLShape(Vector2Int origin, Vector2Int dir, int distance)
        {
            return GetItemAtEndOfLShape(origin, dir, distance, true);
        }

        /// <summary>
        /// Returns the item at the end of an L shape
        /// </summary>
        /// <param name="origin">Coordinate of central item.</param>
        /// <param name="dir">Direction of the L, note this needs to be not zero.</param>
        /// <param name="distance">How far from center can it move?</param>
        /// <param name="isResting">Is the L on its side (rotated 90 degrees)</param>
        /// <returns>Returns Array of T.</returns>
        public static T GetItemAtEndOfLShape(Vector2Int origin, Vector2Int dir, int distance, bool isResting)
        {
            dir.Clamp(new Vector2Int(-1, -1), new Vector2Int(1, 1));
            Assert.NotZero(dir.x);
            Assert.NotZero(dir.y);

            int x = (isResting ? 1 : 2) * distance;
            int y = (isResting ? -2 : -1) * distance;
            return GetItemAtOffset(origin, dir, x, y);
        }

        /// <summary>
        /// Gets an array containing all the items that fit within in LShapes in all directions
        /// </summary>
        /// <param name="center">Coordinate of central item.</param>
        /// <param name="distance">Distance. This is inclusive, so distance > 1 will show results for 1 and the difference</param>
        /// <returns>Returns Array of T.</returns>
        public static GetItemsInAreaResponse GetItemsInLShapeCross(Vector2Int center, int distance)
        {
            List<T> listOfItems = new();
            Vector2Int topRight = new Vector2Int(1, 1);
            Vector2Int topLeft = new Vector2Int(-1, 1);
            Vector2Int bottomRight = new Vector2Int(1, -1);
            Vector2Int bottomLeft = new Vector2Int(-1, -1);

            bool isMissingExpectedItems = false;
            for (int i = 0; i < distance; i++)
            {
                int dist = i + 1;
                T itemStdTR = GetItemAtEndOfStandingLShape(center, topRight, dist);
                T itemStdTL = GetItemAtEndOfStandingLShape(center, topLeft, dist);
                T itemStdBR = GetItemAtEndOfStandingLShape(center, bottomRight, dist);
                T itemStdBL = GetItemAtEndOfStandingLShape(center, bottomLeft, dist);

                T itemRestingTR = GetItemAtEndOfRestingLShape(center, topRight, dist);
                T itemRestingTL = GetItemAtEndOfRestingLShape(center, topLeft, dist);
                T itemRestingBR = GetItemAtEndOfRestingLShape(center, bottomRight, dist);
                T itemRestingBL = GetItemAtEndOfRestingLShape(center, bottomLeft, dist);

                //Iterate over all items, if any are null then set isMissingExpectedItems to true. Only return non-null
                foreach (T item in new T[8]
                         {
                             itemStdTR, itemStdTL, itemStdBR, itemStdBL, itemRestingTR, itemRestingTL, itemRestingBR,
                             itemRestingBL
                         })
                {
                    if (item)
                    {
                        listOfItems.Add(item);
                    }
                    else
                    {
                        isMissingExpectedItems = true;
                    }
                }
            }

            return new GetItemsInAreaResponse(listOfItems.ToArray(), isMissingExpectedItems);
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
            PieceLogic piecePooledItem = PoolSystem<PieceLogic>.GetPool().GetNextAvailable();
            if (!piecePooledItem)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to no pooled pieces being ready");
                return;
            }

            //Get piece  mkr
            PieceObject pieceObject = piecePooledItem.GetComponent<PieceObject>();
            Transform connectionMkr = pieceObject.GetTileConnectionMkr();
            //Check connection pieces are valid
            if (!tile.GetPieceConnectionMkr() || !connectionMkr)
            {
                DebugSystem.Error(
                    $"Cannot place piece on location {payload.gridCoordinates.ToString()} due to missing connection mkr on either the piece or the tile, check prefabs");
                return;
            }

            //Now everything has been validated, add it

            //Set scale
            Transform pieceTransform = piecePooledItem.GetComponent<Transform>();
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
            tileParent.GetState().SetOccupier(piecePooledItem);

            pieceTransform.parent = tileParent.GetComponent<Transform>();
            pieceTransform.localPosition = tile.GetPieceConnectionMkr().transform.localPosition +
                                           (connectionMkr.transform.localPosition * -1);

            //Set State
            pieceObject.GetLogic().SetPieceData(payload.pieceData);
            pieceObject.GetState().SetOwnerPlayer(payload.spawnedByPlayer);
            BoardSystem<PieceObject>.SetItemOnGrid(payload.gridCoordinates, pieceObject);
            pieceObject.GetState().SetGridLocation(payload.gridCoordinates);
            pieceObject.GetState().GetLogicStateMachine().SetState(EPieceLogicState.IdleOnBoard);
        }
    }
}