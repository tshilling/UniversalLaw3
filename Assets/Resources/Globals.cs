using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Globals
{
    public static GameObject _player;
    public static List<Vector3Int> RenderList = new List<Vector3Int>();
    public static List<GameObject> Chunks = new List<GameObject>();
    public static Stopwatch FrameStopWatch = new Stopwatch();
    public static WorldClass ActiveWorld;
    public static SeederClass Seeder = new SeederClass();
    public static int ChunkSize = 16;
    public static Vector3Int[] NeighborOffset =
    {
        new Vector3Int(0,-1,0),
        new Vector3Int(0,-1,1),
        new Vector3Int(1,-1,1),
        new Vector3Int(1,-1,0),
        new Vector3Int(1,-1,-1),
        new Vector3Int(0,-1,-1),
        new Vector3Int(-1,-1,-1),
        new Vector3Int(-1,-1,0),
        new Vector3Int(-1,-1,1),

        new Vector3Int(0,0,1),
        new Vector3Int(1,0,1),
        new Vector3Int(1,0,0),
        new Vector3Int(1,0,-1),
        new Vector3Int(0,0,-1),
        new Vector3Int(-1,0,-1),
        new Vector3Int(-1,0,0),
        new Vector3Int(-1,0,1),

        new Vector3Int(0,1,0),
        new Vector3Int(0,1,1),
        new Vector3Int(1,1,1),
        new Vector3Int(1,1,0),
        new Vector3Int(1,1,-1),
        new Vector3Int(0,1,-1),
        new Vector3Int(-1,1,-1),
        new Vector3Int(-1,1,0),
        new Vector3Int(-1,1,1)
    };
    public static Vector3Int[] DirectionVector =
    {
        new Vector3Int(0,1,0),  // UP
        new Vector3Int(0,0,1),  // North
        new Vector3Int(1,0,0),  // East
        new Vector3Int(-1,0,0), // West
        new Vector3Int(0,0,-1), // South
        new Vector3Int(0,-1,0)  // Down
    };
    public static Vector3Int chunkDistance = new Vector3Int(2, 8, 2);
    public enum Direction
    {
        Up = 0,
        North = 1,
        East = 2,
        West = 3,
        South = 4,
        Down = 5
    }
    public class MeshData
    {
        public List<Vector3> Vertices;
        public List<int> Triangles;
        public List<Vector3> Normals;
        public List<Vector2> UVs;
        public MeshData()
        {
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Normals = new List<Vector3>();
            UVs = new List<Vector2>();
        }
        public void Clear()
        {
            Vertices.Clear();
            Triangles.Clear();
            Normals.Clear();
            UVs.Clear();
        }
    }
}
