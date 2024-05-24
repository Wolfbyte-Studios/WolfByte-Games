using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class IncrementalTerrainToMeshExporter : MonoBehaviour
{
    public Terrain terrain;
    public string exportPath = "Assets/Terrain.obj";
    public int chunkSize = 1000; // Number of vertices to process per frame

    private TerrainData terrainData;
    private int width;
    private int height;
    private Vector3 meshScale;
    private int tRes;
    private float[,] tData;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;
    private int index;
    private int tIndex;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned.");
            return;
        }

        terrainData = terrain.terrainData;
        width = terrainData.heightmapResolution;
        height = terrainData.heightmapResolution;
        meshScale = terrainData.size;
        tRes = terrainData.heightmapResolution;

        tData = terrainData.GetHeights(0, 0, tRes, tRes);
        tRes = tRes - 1;
        vertices = new Vector3[tRes * tRes];
        uvs = new Vector2[vertices.Length];
        triangles = new int[(tRes - 1) * (tRes - 1) * 6];
        index = 0;
        tIndex = 0;

        StartCoroutine(ConvertTerrainToMesh());
    }

    IEnumerator ConvertTerrainToMesh()
    {
        using (StreamWriter sw = new StreamWriter(exportPath))
        {
            sw.WriteLine("# Unity terrain to OBJ exporter");
            sw.WriteLine("g Terrain");

            for (int y = 0; y < tRes; y++)
            {
                for (int x = 0; x < tRes; x++)
                {
                    vertices[index] = new Vector3(x * meshScale.x / tRes, tData[x, y] * meshScale.y, y * meshScale.z / tRes);
                    uvs[index] = new Vector2((float)x / tRes, (float)y / tRes);

                    if (x < tRes - 1 && y < tRes - 1)
                    {
                        int topLeft = index;
                        int bottomLeft = index + tRes;
                        int topRight = index + 1;
                        int bottomRight = index + tRes + 1;

                        triangles[tIndex++] = topLeft;
                        triangles[tIndex++] = bottomLeft;
                        triangles[tIndex++] = topRight;
                        triangles[tIndex++] = topRight;
                        triangles[tIndex++] = bottomLeft;
                        triangles[tIndex++] = bottomRight;
                    }

                    if (++index % chunkSize == 0)
                    {
                        yield return null; // Yield execution to spread the workload over multiple frames
                    }
                }
            }

            yield return StartCoroutine(WriteVertices(sw));
            yield return StartCoroutine(WriteNormals(sw));
            yield return StartCoroutine(WriteUVs(sw));
            yield return StartCoroutine(WriteTriangles(sw));
        }

        Debug.Log("Terrain exported to " + exportPath);
    }

    IEnumerator WriteVertices(StreamWriter sw)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            sw.WriteLine(string.Format("v {0} {1} {2}", vertices[i].x, vertices[i].y, vertices[i].z));
            if (i % chunkSize == 0)
            {
                yield return null; // Yield execution to spread the workload over multiple frames
            }
        }
        sw.WriteLine();
    }

    IEnumerator WriteNormals(StreamWriter sw)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            sw.WriteLine(string.Format("vn {0} {1} {2}", 0, 1, 0)); // Replace with actual normals if calculated
            if (i % chunkSize == 0)
            {
                yield return null; // Yield execution to spread the workload over multiple frames
            }
        }
        sw.WriteLine();
    }

    IEnumerator WriteUVs(StreamWriter sw)
    {
        for (int i = 0; i < uvs.Length; i++)
        {
            sw.WriteLine(string.Format("vt {0} {1}", uvs[i].x, uvs[i].y));
            if (i % chunkSize == 0)
            {
                yield return null; // Yield execution to spread the workload over multiple frames
            }
        }
        sw.WriteLine();
    }

    IEnumerator WriteTriangles(StreamWriter sw)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            sw.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            if (i % (chunkSize * 3) == 0)
            {
                yield return null; // Yield execution to spread the workload over multiple frames
            }
        }
    }
}
