using System;
using System.Collections.Generic;
using Debug;
using NUnit.Framework;
using UnityEngine;

namespace Gameplay
{
    public class BoardController : MonoBehaviour
    {
        [SerializeField] protected GameObject tilePrefab;

        //TODO: Move into some kind of level data class that gets broadcasted on level begin (unless this is consistent size)
        //A tile is 1m, so a board of size 1,1 can fit a single tile
        [SerializeField] protected int boardWidth;
        [SerializeField] protected int boardHeight;

        [SerializeField] protected float boardScale;

        [SerializeField] protected Transform mkrBoardStart;

        protected Tile[,] tiles;


        private void Start()
        {
            if (!tilePrefab)
            {
                DebugSystem.Error("Tile prefab undefined, cannot spawn new tiles on the board.");
                return;
            }

            //Create tiles array
            tiles = new Tile[boardWidth, boardHeight];

            //Iterate over and assign tiles to array
            int numberOfTiles = boardWidth * boardHeight;
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < numberOfTiles; i++)
            {
                //Spawn
                GameObject tileObject = Instantiate(tilePrefab,
                    GetTileLocalPositionOnGrid(new Vector2Int(currentX, currentY)),
                    transform.rotation, transform);
                //Get Tile comp and save it
                Tile tile = tileObject.GetComponentInChildren<Tile>();
                Assert.NotNull(tile);
                tile.SetGridLocation(new Vector2(currentX, currentY));
                //Add to data
                tiles[currentX, currentY] = tileObject.GetComponent<Tile>();
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