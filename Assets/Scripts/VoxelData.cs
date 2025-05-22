using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VoxelData {

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 256;
    public static readonly int WorldSizeInChunks = 20;

    // Lightning Values
    public static float minLightLevel = 0.1f;
    public static float maxLightLevel = 0.9f;
    public static float lightFalloff = 0.08f;

    public static int seed;

    public static int WorldSizeInVoxels {
        get { return WorldSizeInChunks * ChunkWidth; }
    }

    // public static readonly int ViewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlocks = 16;
    public static float NormalizedBlockTextureSize {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }

    public static readonly Vector3[] voxelVerts = new Vector3[8] {
        new Vector3 (0.0f, 0.0f, 0.0f), // 0
        new Vector3 (1.0f, 0.0f, 0.0f), // 1
        new Vector3 (1.0f, 1.0f, 0.0f), // 2
        new Vector3 (0.0f, 1.0f, 0.0f), // 3
        new Vector3 (0.0f, 0.0f, 1.0f), // 4
        new Vector3 (1.0f, 0.0f, 1.0f), // 5
        new Vector3 (1.0f, 1.0f, 1.0f), // 6
        new Vector3 (0.0f, 1.0f, 1.0f), // 7
    };

    public static readonly Vector3[] faceChecks = new Vector3[6] {
        new Vector3(0.0f, 0.0f, -1.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, -1.0f, 0.0f),
        new Vector3(-1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f)
    };

    public static readonly int[,] voxelTris = new int[6, 4] {

        // Back, Front, Top, Bottom, Left, Right

        {0, 3, 1, 2}, // Back face
        {5, 6, 4, 7}, // Front face
        {3, 7, 2, 6}, // Top face
        {4, 0, 5, 1}, // Bottom face
        {4, 7, 0, 3}, // Left face
        {1, 2, 5, 6} // Right face
    };

    public static readonly Vector2[] voxelUvs = new Vector2[4] {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };

}