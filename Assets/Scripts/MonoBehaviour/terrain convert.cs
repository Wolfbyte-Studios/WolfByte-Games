using UnityEngine;
using System.IO;

public class TerrainToMeshExporter : MonoBehaviour
{
    public Terrain terrain;
    public string exportPath = "Assets/Terrain.obj";

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain not assigned.");
            return;
        }

        Mesh terrainMesh = TerrainToMesh(terrain);
        ExportMeshToOBJ(terrainMesh, exportPath);
        Debug.Log("Terrain exported to " + exportPath);
    }

    Mesh TerrainToMesh(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        Vector3 meshScale = terrainData.size;
        int tRes = terrainData.heightmapResolution;

        float[,] tData = terrainData.GetHeights(0, 0, tRes, tRes);
        tRes = tRes - 1;
        Vector3[] vertices = new Vector3[tRes * tRes];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[(tRes - 1) * (tRes - 1) * 6];
        int index = 0;
        int tIndex = 0;
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

                index++;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    void ExportMeshToOBJ(Mesh mesh, string path)
    {
        using (StreamWriter sw = new StreamWriter(path))
        {
            sw.Write(MeshToString(mesh));
        }
    }

    string MeshToString(Mesh mesh)
    {
        StringWriter sw = new StringWriter();

        sw.WriteLine("# Unity terrain to OBJ exporter");
        sw.WriteLine("g Terrain");

        foreach (Vector3 v in mesh.vertices)
        {
            sw.WriteLine(string.Format("v {0} {1} {2}", v.x, v.y, v.z));
        }

        sw.WriteLine();

        foreach (Vector3 v in mesh.normals)
        {
            sw.WriteLine(string.Format("vn {0} {1} {2}", v.x, v.y, v.z));
        }

        sw.WriteLine();

        foreach (Vector2 v in mesh.uv)
        {
            sw.WriteLine(string.Format("vt {0} {1}", v.x, v.y));
        }

        sw.WriteLine();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            sw.WriteLine(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}",
                mesh.triangles[i] + 1, mesh.triangles[i + 1] + 1, mesh.triangles[i + 2] + 1));
        }

        return sw.ToString();
    }
}
