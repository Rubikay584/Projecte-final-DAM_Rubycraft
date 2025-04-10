using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure {
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight) {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++) {
            Vector3 trunkPos = new Vector3(position.x, position.y + i, position.z);
            if (IsPositionInsideWorld(trunkPos))
                queue.Enqueue(new VoxelMod(trunkPos, 6));
        }


        for (int x = -3; x < 4; x++) {
            for (int y = 0; y < 7; y++) {
                for (int z = -3; z < 4; z++) {
                    Vector3 leafPos = new Vector3(position.x + x, position.y + height + y, position.z + z);
                    if (IsPositionInsideWorld(leafPos))
                        queue.Enqueue(new VoxelMod(leafPos, 5));
                }
            }
        }

        //for (int x = -2; x < 3; x++)
        //    for (int z = -2; z < 3; z++) {
        //        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height - 2, position.z + z), 5));
        //        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height - 3, position.z + z), 5));
        //    }

        //for (int x = -1; x < 2; x++)
        //    for (int z = -1; z < 2; z++)
        //        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height - 1, position.z + z), 5));

        //for (int x = -1; x < 2; x++)
        //    if (x == 0)
        //        for (int z = -1; z < 2; z++)
        //            queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height, position.z + z), 5));
        //    else
        //        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height, position.z), 5));


        return queue;
    }

    private static bool IsPositionInsideWorld(Vector3 pos) {
        return (
            pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
            pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
            pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels
        );
    }
}


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public static class Structure
//{
//    public static void MakeTree(Vector3 position, Queue<VoxelMod> queue, int minTrunkHeight, int maxTrunkHeight)
//    {
//        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
//        if (height < minTrunkHeight)
//            height = minTrunkHeight;

//        // Crear el tronco
//        for (int i = 1; i < height; i++)
//            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 6));


//        // Determinar aleatoriamente el tipo de árbol
//        bool isBalloon = Random.value > 0.5f;

//        if (isBalloon)
//            GenerateBalloonLeaves(position, queue, height);
//        else
//            GenerateNormalLeaves(position, queue, height);
//    }

//    private static void GenerateBalloonLeaves(Vector3 position, Queue<VoxelMod> queue, int height)
//    {
//        int[][] pattern = {
//            new int[] { 1 },
//            new int[] { 3 },
//            new int[] { 5 },
//            new int[] { 3, 5, 4 }
//        };

//        for (int y = 0; y < pattern.Length; y++)
//        {
//            int layerSize = pattern[y][0];
//            for (int x = -layerSize / 2; x <= layerSize / 2; x++)
//            {
//                for (int z = -layerSize / 2; z <= layerSize / 2; z++)
//                {
//                    if (x == 0 && z == 0 && y < pattern.Length - 1) continue; // Dejar el centro libre excepto la capa superior
//                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 5));
//                }
//            }
//        }
//    }

//    private static void GenerateNormalLeaves(Vector3 position, Queue<VoxelMod> queue, int height)
//    {
//        int[][] pattern = {
//            new int[] { 1 },
//            new int[] { 3 },
//            new int[] { 1, 3, 2 },
//            new int[] { 3, 5, 5, 5, 4 },
//            new int[] { 4, 5, 5, 5, 3 }
//        };

//        for (int y = 0; y < pattern.Length; y++)
//        {
//            int layerSize = pattern[y][0];
//            for (int x = -layerSize / 2; x <= layerSize / 2; x++)
//            {
//                for (int z = -layerSize / 2; z <= layerSize / 2; z++)
//                {
//                    if (y > 0 || (x != 0 || z != 0)) // Evita colocar hojas en el centro de la capa superior
//                        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 5));
//                }
//            }
//        }
//    }
//}