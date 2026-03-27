using Debug;
using Gameplay.Tile;
using NUnit.Framework;
using UnityEngine;

namespace Gameplay.Piece
{
    public class PiecePreviewSingleton : MonoBehaviour
    {
        [SerializeField] private Transform connectionMkr;
        [SerializeField] private MeshFilter meshFilter;

        public static PiecePreviewSingleton instance { get; private set; }

        private TileObject lastPreviewedOnTile;

        private void Awake()
        {
            //Ensure singleton is unique
            if (instance != null && instance != this)
            {
                DebugSystem.Warn("Multiple PlayerPreviewCard in scene, this is invalid!");
                Destroy(gameObject);
            }

            //Ensure necessary is not null
            Assert.NotNull(meshFilter);
            Assert.NotNull(connectionMkr);

            //Set instance
            instance = this;

            //Hide mesh
            meshFilter.gameObject.SetActive(false);
        }

        public void PreviewPieceOnTile(PieceData data, TileObject tile)
        {
            //Update mesh
            if (data.GetMesh() != meshFilter.mesh)
            {
                meshFilter.mesh = data.GetMesh();
            }

            //Ensure we don't update if not necessary
            if (tile != lastPreviewedOnTile)
            {
                //Position
                lastPreviewedOnTile = tile;
                transform.parent = tile.GetMoveableObject().transform;
                transform.localPosition = tile.GetPieceConnectionMkr().transform.localPosition +
                                          (connectionMkr.transform.localPosition * -1);
            }

            //Scale
            transform.localScale = Vector3.one * data.GetMeshScale();

            //Visible
            meshFilter.gameObject.SetActive(true);
        }

        public void Hide()
        {
            transform.parent = null;
            meshFilter.gameObject.SetActive(false);
        }
    }
}