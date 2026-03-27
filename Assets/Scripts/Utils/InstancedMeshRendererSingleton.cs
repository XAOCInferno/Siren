using System;
using System.Collections.Generic;
using System.Linq;
using Debug;
using Unity.Burst;
using UnityEngine;

namespace Utils
{
    /// <summary>
    /// Instanced mesh renderer. Handles instances where we need to have a single mesh/material pair rendered on many objects with only 1 draw call.
    /// </summary>
    [BurstCompile]
    public class InstancedMeshRendererSingleton : MonoBehaviour
    {
        /// <summary>
        /// Struct containing data for a single render's location data.
        /// </summary>
        [BurstCompile]
        public struct MeshInstancingTransformDetails
        {
            public Vector3 location;
            public Quaternion rotation;
            public Vector3 scale;
        }

        /// <summary>
        /// Struct containing the mesh, material (RP) and matrix details required for Graphics.DrawInstancedMesh.
        /// </summary>
        [BurstCompile]
        private struct MeshInstancingDetails
        {
            public readonly Mesh mesh;
            public readonly RenderParams rp;
            public readonly Matrix4x4[] matrix;

            public MeshInstancingDetails(Mesh mesh, RenderParams rp, Matrix4x4[] matrix)
            {
                this.mesh = mesh;
                this.rp = rp;
                this.matrix = matrix;
            }
        }

        // Instance
        public static InstancedMeshRendererSingleton instance { get; private set; }

        // List of all our meshes material pairs using the batch IDX as a key. This will not grow too large so does not need to be dictionary
        private readonly List<(int, KeyValuePair<Mesh, Material>)> _batchIdxAndVisualsMap = new();

        // List containing dictionary of all the positional data for our renders using gameObjectInstanceID as the key 
        private readonly List<Dictionary<int, MeshInstancingTransformDetails>> _meshInstancingTransformDetails = new();

        // Map of all our instancing data. This is calculated over multiple frames.
        private readonly Dictionary<int, MeshInstancingDetails> _cachedInstancingDataMap = new();

        // Data for the current render we're calculating
        private Matrix4x4[] _currentMatrix = Array.Empty<Matrix4x4>();
        private Mesh _currentMesh;
        private RenderParams _currentRenderParams;
        private MeshInstancingTransformDetails[] _currentMeshInstancingTransformDetails;
        private int _currentBatchIdxAndVisualsMapIdx = 0;
        private int _lastMatrixCalculationProgress = 0;
        private int _currentMeshInstancingTransformDetailsIdx = 0;
        private bool _shouldFetchRenderDetails = true;

        private void Awake()
        {
            //Ensure singleton is unique
            if (instance != null && instance != this)
            {
                DebugSystem.Warn("Multiple InstancedMeshRendererSingleton in scene, this is invalid!");
                Destroy(gameObject);
                return;
            }

            //Set instance
            instance = this;
        }

        public void Update()
        {
            if (_meshInstancingTransformDetails.Count == 0) return;
            CalculateInstancedMeshBatches();
            RenderInstanceMeshBatches();
        }

        /// <summary>
        /// Iterates over all our data and calculates the matrix for it. Will limit how many calculations are done at once to improve performance.
        /// </summary>
        private void CalculateInstancedMeshBatches()
        {
            const int maxCalculationsPerFrame = 250;
            int thisFrameCalculationCount = 0;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse so that we continue calculating for move batches if we reach limit 
            while (thisFrameCalculationCount < maxCalculationsPerFrame)
            {
                // Otherwise we need 
                int numInstances = _meshInstancingTransformDetails[_currentMeshInstancingTransformDetailsIdx].Count;
                if (numInstances == 0)
                {
                    _cachedInstancingDataMap.Remove(_currentMeshInstancingTransformDetailsIdx);
                }
                else
                {
                    // Size changed, so need to reset the matrix
                    if (numInstances != _currentMatrix.Length)
                    {
                        _shouldFetchRenderDetails = true;
                    }

                    // New type of mesh, so fetch
                    if (_shouldFetchRenderDetails)
                    {
                        // Set we now don't need to fetch details
                        _shouldFetchRenderDetails = false;

                        // Mesh & mat
                        (Mesh key, Material value) = _batchIdxAndVisualsMap[_currentBatchIdxAndVisualsMapIdx].Item2;
                        _currentMesh = key;
                        _currentRenderParams = new RenderParams(value);

                        // Make new matrix
                        _currentMatrix = new Matrix4x4[numInstances];

                        // Cache our transform details (quicker than iterating over the dict
                        // TODO: is there a better way to do this instead of converting to an array?
                        _currentMeshInstancingTransformDetails =
                            _meshInstancingTransformDetails[_currentMeshInstancingTransformDetailsIdx].Values.ToArray();

                        // Clear progress
                        _lastMatrixCalculationProgress = 0;
                    }

                    // Calculate our matrix, continuing from where we last left off
                    for (int a = _lastMatrixCalculationProgress; a < numInstances; a++)
                    {
                        // Get transform details
                        MeshInstancingTransformDetails details = _currentMeshInstancingTransformDetails[a];

                        // Calc and set matrix
                        _currentMatrix[a] = Matrix4x4.TRS(details.location, details.rotation,
                            details.scale);

                        // Iterate our progress
                        _lastMatrixCalculationProgress++;

                        // Iterate our progress count for this frame, returning if we've exceeded the allowed limit per frame
                        thisFrameCalculationCount++;
                        if (thisFrameCalculationCount >= maxCalculationsPerFrame) return;
                    }

                    // Cache for later
                    _cachedInstancingDataMap.Remove(_currentMeshInstancingTransformDetailsIdx);
                    _cachedInstancingDataMap.Add(_currentMeshInstancingTransformDetailsIdx,
                        new MeshInstancingDetails(_currentMesh, _currentRenderParams, _currentMatrix));
                }

                // Finished a batch, increment idx
                _currentMeshInstancingTransformDetailsIdx = _currentMeshInstancingTransformDetailsIdx ==
                                                            _meshInstancingTransformDetails.Count - 1
                    ? 0
                    : _currentMeshInstancingTransformDetailsIdx + 1;
                _currentBatchIdxAndVisualsMapIdx = _currentBatchIdxAndVisualsMapIdx == _batchIdxAndVisualsMap.Count - 1
                    ? 0
                    : _currentBatchIdxAndVisualsMapIdx + 1;

                // Now we've finished a batch, we need to recalculate the render details for new batch
                _shouldFetchRenderDetails = true;
            }
        }

        /// <summary>
        /// Renders all calculated data. Must be called each frame.
        /// </summary>
        private void RenderInstanceMeshBatches()
        {
            foreach (var cachedData in _cachedInstancingDataMap)
            {
                Graphics.RenderMeshInstanced(cachedData.Value.rp, cachedData.Value.mesh, 0, cachedData.Value.matrix);
            }
        }

        /// <summary>
        /// Add mesh instancing so that mesh is rendered each frame. Use this also to change any rendering info eg material or mesh
        /// </summary>
        /// <param name="oldBatchIdx">Optional. If we've been instanced before this will allow us to remove that instance.</param>
        /// <param name="gameObjectInstanceID">Unique ID from GameObject.GetInstanceID. Used to help us when removing object from instancing.</param>
        /// <param name="meshMaterialPair">The mesh and material this has.</param>
        /// <param name="transformDetails">Location data for the instancing.</param>
        /// <returns>The batch Idx that we can use when removing. Ensure you save this.</returns>
        public int AddMeshInstancing(int? oldBatchIdx, int gameObjectInstanceID,
            KeyValuePair<Mesh, Material> meshMaterialPair,
            MeshInstancingTransformDetails transformDetails)
        {
            // Have a valid idx, so try remove the old data before we add our new data
            if (oldBatchIdx is > -1)
            {
                TryRemoveMeshInstancing(oldBatchIdx.Value, gameObjectInstanceID);
            }

            // Work out where this is going to be batched
            int newBatchIdx = -1;
            for (int i = 0; i < _batchIdxAndVisualsMap.Count; i++)
            {
                // Skip if not the same mesh material pair
                if (_batchIdxAndVisualsMap[i].Item2.Key != meshMaterialPair.Key ||
                    _batchIdxAndVisualsMap[i].Item2.Value != meshMaterialPair.Value) continue;

                // Set batch idx and break out the loop
                newBatchIdx = _batchIdxAndVisualsMap[i].Item1;
                break;
            }

            // If this is a new batch we've not had before, add the new batch to our data
            if (newBatchIdx == -1)
            {
                newBatchIdx = _meshInstancingTransformDetails.Count;
                _batchIdxAndVisualsMap.Add((newBatchIdx, meshMaterialPair));
                _meshInstancingTransformDetails.Add(new Dictionary<int, MeshInstancingTransformDetails>());
            }

            // Add the transform details
            _meshInstancingTransformDetails[newBatchIdx].Add(gameObjectInstanceID, transformDetails);

            // Finally return the batch idx
            return newBatchIdx;
        }

        /// <summary>
        /// Removes from our instancing.
        /// </summary>
        /// <param name="batchIdx">The batch that this is instanced in, should match what was returned when adding mesh instancing.</param>
        /// <param name="gameObjectInstanceID">Unique ID from GameObject.GetInstanceID.</param>
        public void TryRemoveMeshInstancing(int batchIdx, int gameObjectInstanceID)
        {
            _meshInstancingTransformDetails[batchIdx].Remove(gameObjectInstanceID);
        }

        /// <summary>
        /// Checks through our instancing details to see if this is instanced. This can be slow due to no sorting, if you need to call this often try to use a different approach.
        /// </summary>
        /// <param name="batchIdx">The batch that this is instanced in.</param>
        /// <param name="gameObjectInstanceID">Unique ID from GameObject.GetInstanceID.</param>
        /// <returns></returns>
        private bool IsMeshInstanced(int batchIdx, int gameObjectInstanceID)
        {
            return _meshInstancingTransformDetails[batchIdx].ContainsKey(gameObjectInstanceID);
        }
    }
}