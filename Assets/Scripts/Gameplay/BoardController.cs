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

        protected List<GameObject> spawnedTiles = new();


        private void Start()
        {
            if (!tilePrefab)
            {
                DebugSystem.Error("Tile prefab undefined, cannot spawn new tiles on the board.");
                return;
            }

            int numberOfTiles = boardWidth * boardHeight;
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < numberOfTiles; i++)
            {
                //Spawn
                GameObject tile = Instantiate(tilePrefab, GetTileLocalPosition(new Vector2Int(currentX, currentY)),
                    transform.rotation, transform);
                //Set scale
                Transform tileTransform = tile.GetComponent<Transform>();
                tileTransform.localScale = new Vector3(boardScale, boardScale, boardScale);
                spawnedTiles.Add(tile);
                //TODO: Tell tile its grid position (or perhaps we track that here. Probably here?)
                //Ensure grid layout
                currentX++;
                if (currentX == boardWidth)
                {
                    currentX = 0;
                    currentY++;
                }
            }
        }

        protected Vector3 GetTileLocalPosition(Vector2Int position)
        {
            //Success
            Vector3 endPosition = mkrBoardStart ? mkrBoardStart.position : Vector3.zero;
            Vector3 xOffset = transform.right *  position.x;
            Vector3 yOffset = transform.forward * position.y;
            Vector3 offset = (xOffset + yOffset) * boardScale;
            return endPosition + offset;
        }
    }
}