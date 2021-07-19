using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// A voxelizer can take a 3d mesh and turn it into group of voxels
// to turn into group of voxels, we need to cut the 3d mesh into a 3d grid
// in that 3d grid each voxel have a certain size and position
public class Voxelizer : MonoBehaviour
{

  public GameObject ObjToVolexlize;
  public bool drawUsingCubes = false;
  List<Voxel> voxelsCreated = new List<Voxel>();
  float voxelSize = .1f;

  Texture3D volumeTex;

  Vector3 extents;

  GameObject voxelsToVisualize;

  [SerializeField] protected Shader shader;
  protected Material material;


  Mesh Build(Vector3 extents)
  {
    var vertices = new Vector3[] {
                Vector3.Scale(new Vector3 (-1f, -1f, -1), extents),
                Vector3.Scale(new Vector3 ( 1f, -1f, -1f), extents),
                Vector3.Scale(new Vector3 ( 1f,  1f, -1f), extents),
                Vector3.Scale(new Vector3 (-1f,  1f, -1f), extents),
                Vector3.Scale(new Vector3 (-1f,  1f,  1f), extents),
                Vector3.Scale(new Vector3 ( 1f,  1f,  1f), extents),
                Vector3.Scale(new Vector3 ( 1f, -1f,  1f), extents),
                Vector3.Scale(new Vector3 (-1f, -1f,  1f), extents),
            };
    var triangles = new int[] {
                0, 2, 1,
                0, 3, 2,
                2, 3, 4,
                2, 4, 5,
                1, 2, 5,
                1, 5, 6,
                0, 7, 4,
                0, 4, 3,
                5, 4, 7,
                5, 7, 6,
                0, 6, 7,
                0, 1, 6
            };

    var mesh = new Mesh();
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();
    mesh.hideFlags = HideFlags.HideAndDontSave;
    return mesh;
  }


  // Start is called before the first frame update
  void Start()
  {
    
  }

  // Update is called once per frame
  void Update()
  {
    if(voxelsCreated.Count == 0)
    {
      voxelsCreated = Voxelize();
    } 
    else if(drawUsingCubes)
    {
      // for debugging
      if(voxelsToVisualize == null || voxelsToVisualize.transform.childCount != voxelsCreated.Count)
      {
        if(voxelsToVisualize != null)
        {
          Destroy(voxelsToVisualize);
        }

        voxelsToVisualize = new GameObject("Voxels");

        foreach(Voxel v in voxelsCreated) 
        {
          GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
          cube.GetComponent<Renderer>().material.color = Color.grey;
          cube.transform.position = v.position;
          cube.transform.localScale *= voxelSize;
          cube.transform.parent = voxelsToVisualize.transform;
        }

      }
    }
    else
    {
      if (volumeTex != null)
      {
        if(material == null)
        {
          material = new Material(shader);
          GetComponent<MeshFilter>().sharedMesh = Build(extents);
          GetComponent<MeshRenderer>().sharedMaterial = material;
        }
        material.SetTexture("_Volume", volumeTex);
        material.SetVector("_Extents", extents);
        // update material correctly
      }
    }
  }

  public List<Voxel> Voxelize()
  {
    List<Voxel> voxels = new List<Voxel>();

    var mesh = ObjToVolexlize.GetComponent<MeshFilter>().sharedMesh;

    Vector3 meshMin = mesh.bounds.center - mesh.bounds.extents;
    Vector3 meshMax = mesh.bounds.center + mesh.bounds.extents;

    extents = mesh.bounds.extents;

    Vector3 gridSize = mesh.bounds.extents / (voxelSize / 2.0f);

    int gridSizeX = Mathf.RoundToInt(gridSize.x);
    int gridSizeY = Mathf.RoundToInt(gridSize.y);
    int gridSizeZ = Mathf.RoundToInt(gridSize.z);

    // create volume tex for ray marching in shader
    volumeTex = new Texture3D(gridSizeX, gridSizeY, gridSizeZ, TextureFormat.RGBA32, false);
    volumeTex.wrapMode = TextureWrapMode.Clamp;

    Color[] colors = new Color[gridSizeX * gridSizeY * gridSizeZ];

    int index = 0;

    for(int x = 0; x < gridSizeX; ++x)
    {
      for (int y = 0; y < gridSizeY; ++y)
      {
        for (int z = 0; z < gridSizeZ; ++z)
        {
          Voxel v = new Voxel();
          v.position = new Vector3(voxelSize * x + voxelSize / 2.0f, voxelSize * y + voxelSize / 2.0f, voxelSize * z + voxelSize / 2.0f);
          v.position += mesh.bounds.min;

          Bounds aabb = new Bounds();
          aabb.center = v.position;
          aabb.extents = new Vector3(voxelSize / 2.0f, voxelSize / 2.0f, voxelSize / 2.0f);

          // assert that mesh only have one submesh i.e. mesh.subMeshCount == 1

          for (int i = 0; i < mesh.GetIndexCount(0); i += 3)
          {
            Vector3 triangleV1 = mesh.vertices[mesh.triangles[i]];
            Vector3 triangleV2 = mesh.vertices[mesh.triangles[i + 1]];
            Vector3 triangleV3 = mesh.vertices[mesh.triangles[i + 2]];

            if (IntersectionUtils.InsersectTriangle(triangleV1, triangleV2, triangleV3, aabb))
            {
              colors[index] = Color.red;
              voxels.Add(v);
              break;
            }
          }

          index++;
        }
      }
    }

    volumeTex.SetPixels(colors);

    // Apply the changes to the texture and upload the updated texture to the GPU
    volumeTex.Apply();

    return voxels;
  }
}
