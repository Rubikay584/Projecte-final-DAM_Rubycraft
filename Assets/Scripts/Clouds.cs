using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour {
    public int cloudHeight = 200;
    public int cloudDepth = 4;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;
    private bool[,] cloudData; // Array of bools representing where a cloud is.
    
    private int cloudTextureWidth;

    private int cloudTileSize;
    private Vector3Int offset;
    
    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start() {
        cloudTextureWidth = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkWidth; // Now it's 1. VoxelData.ChunkWidth is 16
        offset = new Vector3Int(-(cloudTextureWidth / 2), 0, -(cloudTextureWidth / 2));
        
        transform.position = new Vector3(VoxelData.WorldCenter, cloudHeight, VoxelData.WorldCenter);
        
        LoadCloudData();
        CreateClouds();
    }

    private void LoadCloudData() {
        cloudData = new bool[cloudTextureWidth, cloudTextureWidth];
        Color[] cloudTexture = cloudPattern.GetPixels();

        for (int x = 0; x < cloudTextureWidth; x++) {
            for (int y = 0; y < cloudTextureWidth; y++) {
                cloudData[x, y] = (cloudTexture[y * cloudTextureWidth + x].a > 0);
            }
        }
    }

    private void CreateClouds() {
        if (world.settings.clouds == CloudStyle.Off) return;
        
        for (int x = 0; x < cloudTextureWidth; x += cloudTileSize) {
            for (int y = 0; y < cloudTextureWidth; y += cloudTileSize) {
                Mesh cloudMesh;

                if (world.settings.clouds == CloudStyle.Fast)
                    cloudMesh = CreateFastCloudMesh(x, y);
                else
                    cloudMesh = CreateFancyCloudMesh(x, y);

                Vector3 position = new Vector3(x, cloudHeight, y);
                position += transform.position - new Vector3 (cloudTextureWidth / 2f, 0f, cloudTextureWidth / 2f);
                position.y = cloudHeight;
                clouds.Add(
                    CloudTilePosFromVector3(position),
                    CreateCloudTile(cloudMesh, position)
                );

            }
        }
    }

    public void UpdateClouds() {
        if (world.settings.clouds == CloudStyle.Off) return;
        
        for (int x = 0; x < cloudTextureWidth; x += cloudTileSize) {
            for (int y = 0; y < cloudTextureWidth; y += cloudTileSize) {

                Vector3 position = world.player.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                Vector2Int cloudPosition = CloudTilePosFromVector3(position);
                
                clouds[cloudPosition].transform.position = position;

            }
        }
    }

    private int RoundToCloud(float value) {
        return Mathf.RoundToInt(value / cloudTileSize) * cloudTileSize;
    }

    private Mesh CreateFastCloudMesh(int x, int z /* 'z' represents 'y' */) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;
        
        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++) {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++) {
                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal]) {
                    // Add four vertices for cloud face.
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));
        
                    // We know what direction our faces are... facing, so we just add them directly.
                    for (int i = 0; i < 4; i++)
                        normals.Add(Vector3.down);
        
                    // Add first triangle
                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);
                    // Add second triangle
                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);
                    // Increment vertCount
                    vertCount += 4;
                }
                
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private Mesh CreateFancyCloudMesh(int x, int z /* 'z' represents 'y' */) {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;
        
        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++) {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++) {
                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal]) {
                    // Loop through neighbour points using faceCheck array.
                    for (int p = 0; p < 6; p++) {
                        // If the current neighbour has no cloud, draw this face.
                        if (!CheckCloudData(new Vector3Int(xVal, 0, zVal) + VoxelData.faceChecks[p])) {
                            
                            // Add our four vertices for this face.
                            for (int i = 0; i < 4; i++) {
                                Vector3 vert = new Vector3Int(xIncrement, 0, zIncrement);
                                vert += VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                                vert.y *= cloudDepth;
                                vertices.Add(vert);
                            }

                            for (int i = 0; i < 4; i++) {
                                normals.Add(VoxelData.faceChecks[p]);
                            }
                            
                            triangles.Add(vertCount);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 3);
                            
                            vertCount += 4;
                        }
                    }
                }
            }
        }
        
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }
    
    // Returns true or false depending on there is clouds at the given point.
    private bool CheckCloudData(Vector3Int point) {
        // Because clouds are 2D, if "y" is above or below 0, return false.
        if (point.y != 0) return false;
        
        int x = point.x;
        int z = point.z;
        
        // If the "x" or "z" value is outside of the cloudData range, wrap it around.
        if (point.x < 0) x = cloudTextureWidth - 1;
        if (point.x > cloudTextureWidth - 1) x = 0;
        if (point.z < 0) z = cloudTextureWidth - 1;
        if (point.z > cloudTextureWidth - 1) z = 0;

        return cloudData[x, z];
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position) {
        GameObject newCloudTile = new GameObject();
        newCloudTile.transform.position = position;
        newCloudTile.transform.parent = transform;
        newCloudTile.name = "Cloud " + position.x + ", " + position.z;
        MeshFilter meshFilter = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = newCloudTile.AddComponent<MeshRenderer>();
        
        meshRenderer.material = cloudMaterial;
        meshFilter.mesh = mesh;
        
        return newCloudTile;
    }

    private Vector2Int CloudTilePosFromVector3(Vector3 position) {
        return new Vector2Int(CloudTileCoordFromFloat(position.x), CloudTileCoordFromFloat(position.z));
    }

    private int CloudTileCoordFromFloat(float value) {
        float a = value / (float)cloudTextureWidth; // Gets the position using cloudtexture width as units.
        a -= Mathf.FloorToInt(a); // Substract whole numbers to get a 0-1 value representing position in cloud texture.
        int b = Mathf.FloorToInt((float)cloudTextureWidth * a); // Multiply cloud texture width by "a" to get position in texture globally.

        return b;
    }
}

public enum CloudStyle {
    Off, Fast, Fancy
}