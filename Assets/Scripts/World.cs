using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class World : MonoBehaviour {

    public Settings settings;

    [Header("World Generation Values")]
    public BiomeAttributes biome;

    [Range(0f, 1f)] public float globalLightLevel;
    public Color day;
    public Color night;

    public Transform player;
    public Vector3 spawnPosition;
    
    public Material material;
    public Material transparentMaterial;

    public BlockType[] blockTypes;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerCurrentChunkCoord;
    ChunkCoord playerLastChunkCoord;

    List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    public bool _inUI = false;

    public GameObject debugScreen;

    public GameObject creativeInventoryWindow;
    public GameObject cursorSlot;

    Thread ChunkObjectThread;
    public object ChunkUpdateThreadLock = new object();

    private void Start() {
        string path = Application.dataPath + "/settings.json";
        
        if (!File.Exists(path)) {
            string jsonExport = JsonUtility.ToJson(settings);
            File.WriteAllText(path, jsonExport);
        }
        string jsonImport = File.ReadAllText(path);
        settings = JsonUtility.FromJson<Settings>(jsonImport);
        
        Random.InitState(settings.seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (settings.enableThreading) {
            ChunkObjectThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkObjectThread.Start();
        }

        spawnPosition = new Vector3(((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f) , VoxelData.ChunkHeight - 125f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    private void Update() {
        playerCurrentChunkCoord = GetChunkCoordFromVector3(player.position);

        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
        Camera.main.backgroundColor = Color.Lerp(night, day, globalLightLevel);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerCurrentChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToDraw.Count > 0)
            if (chunksToDraw.Peek().isEditable)
                chunksToDraw.Dequeue().CreateMesh();

        if (!settings.enableThreading) {
            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }

    void GenerateWorld() {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; x < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; x++) {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - settings.viewDistance; z < (VoxelData.WorldSizeInChunks / 2) + settings.viewDistance; z++) {

                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk, this);
                chunksToCreate.Add(newChunk);

            }
        }

        player.position = spawnPosition;
        CheckViewDistance();
    }

    void CreateChunk() {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks() {
        bool updated = false;
        int index = 0;

        lock (ChunkUpdateThreadLock) {
            while (!updated && index < chunksToUpdate.Count - 1) {
                if (chunksToUpdate[index].isEditable) {
                    chunksToUpdate[index].UpdateChunk();
                    activeChunks.Add(chunksToUpdate[index].coord);
                    chunksToUpdate.RemoveAt(index);
                    updated = true;
                } else
                    index++;
            }
        }
    }

    void ThreadedUpdate() {
        while (true) {
            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    private void OnDisable() {
        if (settings.enableThreading) {
            ChunkObjectThread.Abort();
        }
    }

    void ApplyModifications() {
        applyingModifications = true;

        while (modifications.Count > 0) {
            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0) {
                VoxelMod v = queue.Dequeue();
                ChunkCoord c = GetChunkCoordFromVector3(v.position);

                if (chunks[c.x, c.z] == null) {
                    chunks[c.x, c.z] = new Chunk(c, this);
                    chunksToCreate.Add(c);
                }

                chunks[c.x, c.z].modifications.Enqueue(v);
            }
        }

        applyingModifications = false;
    }

    //void ApplyModifications() {
    //    applyingModifications = true;

    //    try {
    //        while (modifications.Count > 0) {
    //            Queue<VoxelMod> queue;

    //            // Manejo seguro de la cola
    //            lock (modifications) {
    //                if (modifications.Count == 0)
    //                    break;

    //                queue = modifications.Dequeue();
    //            }

    //            while (queue?.Count > 0) {
    //                VoxelMod v = queue.Dequeue();
    //                ChunkCoord c = GetChunkCoordFromVector3(v.position);

    //                // Validaci�n de coordenadas
    //                if (c.x < 0 || c.x >= chunks.GetLength(0) ||
    //                    c.z < 0 || c.z >= chunks.GetLength(1)) {
    //                    //Debug.LogError($"Coordenadas de chunk inv�lidas: ({c.x}, {c.z})");
    //                    continue;
    //                }

    //                // Manejo seguro del chunk
    //                if (chunks[c.x, c.z] == null) {
    //                    chunks[c.x, c.z] = new Chunk(c, this);
    //                }

    //                // Validaci�n del chunk
    //                if (chunks[c.x, c.z] != null) {
    //                    chunks[c.x, c.z].modifications.Enqueue(v);

    //                    if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
    //                        chunksToUpdate.Add(chunks[c.x, c.z]);
    //                } else
    //                    Debug.LogError($"No se pudo crear el chunk en ({c.x}, {c.z})");
    //            }
    //        }
    //    } catch (System.Exception e) {
    //        Debug.LogError($"Error en ApplyModifications: {e.Message}");
    //    } finally {
    //        applyingModifications = false;
    //    }
    //}

    ChunkCoord GetChunkCoordFromVector3 (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3 (Vector3 pos) {
        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }

    void CheckViewDistance() {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerCurrentChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);
        activeChunks.Clear();

        for (int x = coord.x - settings.viewDistance; x < coord.x + settings.viewDistance; x++) {
            for (int z = coord.z - settings.viewDistance; z < coord.z + settings.viewDistance; z++) {

                if (IsChunkInWorld(new ChunkCoord(x, z))) {

                    if (chunks[x, z] == null) {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this);
                        chunksToCreate.Add(new ChunkCoord(x, z));
                    } else if (!chunks[x, z].isActive)
                        chunks[x, z].isActive = true;

                    activeChunks.Add(new ChunkCoord(x, z));

                }

                for (int i = 0; i < previouslyActiveChunks.Count; i++) {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }

            }
        }

        // previouslyActiveChunks list are no longer in the player's view distance, so loop and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;
    }

    public bool CheckForVoxel(Vector3 pos) {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos).id].isSolid;

        return blockTypes[GetVoxel(pos)].isSolid;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {

        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return null;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isEditable)
            return chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos);

        return new VoxelState(GetVoxel(pos));

    }

    public bool inUI {
        get { return _inUI; }
        set {
            _inUI = value;
            if (_inUI) {
                Cursor.lockState = CursorLockMode.None;
                creativeInventoryWindow.SetActive(true);
                cursorSlot.SetActive(true);
            } else {
                Cursor.lockState = CursorLockMode.Locked;
                creativeInventoryWindow.SetActive(false);
                cursorSlot.SetActive(false);
            }
        }
    }

    // IMPORTANT
    public byte GetVoxel(Vector3 pos) {
        int yPos = Mathf.FloorToInt(pos.y);

        /* IMMUTABLE PASS */

        /*
            0 Air
            1 Stone
            2 Cobblestone
            3 Grass block
            4 Dirt
            5 Oak leaves
            6 Oak log
            7 Oak planks
            8 Bricks
            9 Sand
            10 Glass
            11 Bedrock
        */
        // Outside world, return air.
        if (!IsVoxelInWorld(pos))
            return 0;

        // Bottom block of chunk, return bedrock.
        if (yPos == 0)
            return 11;

        /* BASIC TERRAIN PASS */
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.terrainScale)) + biome.solidGroundHeight;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = 3; // Grass Block
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = 4; // Dirt
        else if (yPos > terrainHeight)
            return 0; // Air
        else
            voxelValue = 1; // Stone

        /* SECOND PASS */
        if (voxelValue == 1) {

            foreach (Lode lode in biome.lodes) {

                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;

            }

        }

        /* TREE PASS */
        if (yPos == terrainHeight) {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold) {
                //voxelValue = 11;
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.treePlacementScale) > biome.treePlacementThreshold) {
                    //voxelValue = 8;
                    modifications.Enqueue(Structure.MakeTree(pos, biome.minTreeHeight, biome.maxTreeHeight));
                    //Structure.MakeTree(new Vector3(71, 79, 68), modifications, biome.minTreeHeight, biome.maxTreeHeight);
                    //Structure.MakeTree(new Vector3(pos.x, pos.y, pos.z), modifications, biome.minTreeHeight, biome.maxTreeHeight);
                }
            }
        }

        return voxelValue;
    }

    bool IsChunkInWorld(ChunkCoord coord) {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return false;
    }

    bool IsVoxelInWorld(Vector3 pos) {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;
    }

}

[System.Serializable]
public class BlockType {

    public string blockName;
    public bool isSolid;
    public bool renderNeighborFaces;
    public float transparency;
    public Sprite icon;
    public int maxStackSize;

    [Header("Textures values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID (int faceIndex) {

        switch (faceIndex) {
            case 0: return backFaceTexture;
            case 1: return frontFaceTexture;
            case 2: return topFaceTexture;
            case 3: return bottomFaceTexture;
            case 4: return leftFaceTexture;
            case 5: return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;
        }

    }

}

public class VoxelMod {
    public Vector3 position;
    public byte id;

    public VoxelMod() {
        position = new Vector3();
        id = 0;
    }

    public VoxelMod(Vector3 _position, byte _id) {
        position = _position;
        id = _id;
    }
}

[System.Serializable]
public class Settings {
    [Header("Game Data")]
    public string version;
    
    [Header("Performance")]
    public int viewDistance;
    public bool enableThreading;
    
    [Header("Controls")]
    [Range(0.1f, 10f)] public float mouseSensitivity;

    [Header("World Generation")]
    public int seed;
}