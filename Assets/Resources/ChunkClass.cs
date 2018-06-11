using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ChunkClass : MonoBehaviour {
    public struct KEYStruct
    {
        public int X;
        public int Y;
        public int Z;
    }
    public KEYStruct KEY = new KEYStruct();
    public BlockClass[][][] Blocks;// = new BlockClass[Globals.ChunkSize, Globals.ChunkSize, Globals.ChunkSize];
    public ChunkClass[,,] Neighbor = new ChunkClass[3, 3, 3];
    public Vector3Int Position
    {
        get
        {
            return Vector3Int.FloorToInt(gameObject.transform.position);
        }
    }
    public int ID = 0;
    public enum FillStateEnum
    {
        Solid = 0,
        Mixed = 1,
        Empty = 2
    }
    public FillStateEnum FillState = FillStateEnum.Empty;
    public enum StateEnum
    {
        Raw = 0,
        Initialized = 1,
        Seeded = 2,
        CPed = 3,
        Meshed = 4,
        Faced = 5
    }
    public StateEnum State = StateEnum.Raw;

    public Globals.MeshData Mesh = new Globals.MeshData();
    public ChunkClass()
    {
    }
    void Awake()
    {
        meshData = gameObject.GetComponent<MeshFilter>().mesh;
        meshCollider = gameObject.GetComponent<MeshCollider>();
        KEY.X = (int)transform.position.x;
        KEY.Y = (int)transform.position.y;
        KEY.Z = (int)transform.position.z;
        InitChunk();
    }
    void Start()
    {
    }
    Mesh meshData;
    MeshCollider meshCollider;
    Stopwatch SW;
    public void InitChunk()
    {
       // SW.Reset();
       // SW.Start();
        Mesh.Clear();
        //Mesh meshData = gameObject.GetComponent<MeshFilter>().mesh;
        meshData.Clear();

        State = StateEnum.Raw;
        this.gameObject.SetActive(true);
        // Initialize Neighbor Block
        Neighbor = new ChunkClass[3, 3, 3];
        // Set Center Block to own Block
        Neighbor[1, 1, 1] = this;

        Blocks = new BlockClass[Globals.ChunkSize][][];
        for (int x = 0; x < Globals.ChunkSize; x++)
        {
            Blocks[x] = new BlockClass[Globals.ChunkSize][];
            for (int y = 0; y < Globals.ChunkSize; y++)
            {
                Blocks[x][y] = new BlockClass[Globals.ChunkSize];
                for (int z = 0; z < Globals.ChunkSize; z++)
                {
                    Blocks[x][y][z] = new BlockClass(Position + new Vector3Int(x, y, z));
                    Blocks[x][y][z].Chunk = this;
                }
            }
        }
        LinkChunks();
        LinkBlocks();
        State = StateEnum.Initialized;
        Seed();
        StartCoroutine(UpdateItAllCR());
        //SW.Stop();
        //UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
    }
    public void SleepChunk()
    {
        StopAllCoroutines();
        State = StateEnum.Raw;
        this.gameObject.SetActive(false);
    }
    public void Seed()
    {
        SeederClass.SeedChunk(this);
        State = StateEnum.Seeded;
    }
    private void LinkChunks()
    {
        Vector3Int Pos = Position;
        for (int i = 0; i < Globals.NeighborOffset.Length; i++)
        {
            Vector3Int V = Globals.NeighborOffset[i];
            Vector3Int VN = new Vector3Int(-V.x, -V.y, -V.z);
            Vector3Int NeighborChunkPosition = Pos + V * Globals.ChunkSize;
            GameObject NeighborGO;
            if (Globals.ActiveWorld.GetChunk(NeighborChunkPosition, out NeighborGO))
            {
                ChunkClass NeighborChunk = NeighborGO.GetComponent<ChunkClass>();
                Neighbor[V.x + 1, V.y + 1, V.z + 1] = NeighborChunk;
                NeighborChunk.Neighbor[-V.x + 1, -V.y + 1, -V.z + 1] = this;
                State = StateEnum.Seeded;
            }
        }
    }
    Vector3Int Pos;
    Vector3Int P;
    Vector3Int R;
    Vector3Int V;
    Vector3Int N;
    Vector3Int ChunkOffset;
    BlockClass B0;
    private void LinkBlocks()
    {
        Pos = Position;
        P = new Vector3Int();
        for (int y = 0; y < Globals.ChunkSize; y++)
            for (int z = 0; z < Globals.ChunkSize; z++)
                for (int x = 0; x < Globals.ChunkSize; x++)
                {
                    P.x = x;
                    P.y = y;
                    P.z = z;
                    for(int i = 0; i < Globals.NeighborOffset.Length;i++)
                    { 
                        V = Globals.NeighborOffset[i];
                        B0 = Blocks[x][y][z];
                        R = P + V;
                        if (R.x >= 0 && R.y >= 0 && R.z >= 0 && R.x < Globals.ChunkSize && R.y < Globals.ChunkSize && R.z < Globals.ChunkSize)
                        {
                            B0.Neighbor[V.x + 1, V.y + 1, V.z + 1] = Blocks[V.x + x][V.y + y][V.z + z];
                            //B0.SetNeighbor(Blocks[R.x][R.y][R.z]);
                        }
                        else
                        {
                            ChunkOffset = new Vector3Int(1, 1, 1);
                            if (R.x < 0)
                                ChunkOffset.x = 0;
                            if (R.y < 0)
                                ChunkOffset.y = 0;
                            if (R.z < 0)
                                ChunkOffset.z = 0;
                            if (R.x >= Globals.ChunkSize)
                                ChunkOffset.x = 2;
                            if (R.y >= Globals.ChunkSize)
                                ChunkOffset.y = 2;
                            if (R.z >= Globals.ChunkSize)
                                ChunkOffset.z = 2;
                            if (Neighbor[ChunkOffset.x, ChunkOffset.y, ChunkOffset.z] != null)
                            {
                                N = new Vector3Int(R.x, R.y, R.z);
                                while (N.x < 0)
                                    N.x += Globals.ChunkSize;
                                while (N.y < 0)
                                    N.y += Globals.ChunkSize;
                                while (N.z < 0)
                                    N.z += Globals.ChunkSize;
                                while (N.x >= Globals.ChunkSize)
                                    N.x -= Globals.ChunkSize;
                                while (N.y >= Globals.ChunkSize)
                                    N.y -= Globals.ChunkSize;
                                while (N.z >= Globals.ChunkSize)
                                    N.z -= Globals.ChunkSize;
                                B0.SetNeighbor(Neighbor[ChunkOffset.x, ChunkOffset.y, ChunkOffset.z].Blocks[N.x][N.y][N.z],V);
                            }
                        }
                    }
                }
    }
    // Update is called once per frame
    void LateUpdate()
    {
        if (State >= StateEnum.Seeded && State < StateEnum.Faced)
        {
            //if(Running == false)
            //{
            //UpdateItAllCR();
            //}
        }
        //UpdateItAll();
        /*
        Vector3 CameraPos = Globals._player.transform.position;
        CameraPos.y = 0;
        var CameraPosI = Vector3Int.FloorToInt(CameraPos / 16f) * 16;
        Vector3Int Key = Position - CameraPosI;
        if (!Globals.RenderList.Contains(Key))
        {
            if (Globals.Chunks.ContainsValue(gameObject))
            {
                Globals.Chunks.Remove(Vector3Int.FloorToInt(gameObject.transform.position));
                Destroy(this.gameObject);
                return;

            }
        }
        */
        //if (Globals.FrameStopWatch.ElapsedMilliseconds > 16)
        //    yield return null;

        //UpdateState();
    }
    public bool RefaceRequired = false;
    bool Running = false;
    WaitForSeconds shortWait = new WaitForSeconds(0.02f);
    WaitForSeconds longWait = new WaitForSeconds(0.02f);
    public IEnumerator UpdateItAllCR()
    {
        while (true)
        {
            if (State < StateEnum.Seeded)
            {
                yield return shortWait;
            }
            else if (State < StateEnum.CPed)
            {
                if (FillState == FillStateEnum.Mixed)
                {
                    for (int y = 0; y < Globals.ChunkSize; y++)
                    {
                        for (int z = 0; z < Globals.ChunkSize; z++)
                        {
                            for (int x = 0; x < Globals.ChunkSize; x++)
                            {
                                if (Blocks[x][y][z].CalculateControlPoint())
                                {
                                    RefaceRequired = true;
                                }
                            }
                        }
                        if (Globals.FrameStopWatch.ElapsedMilliseconds > 8)
                        {
                            yield return shortWait;
                        }
                    }
                }
                State = StateEnum.CPed;
            }
            else if (State < StateEnum.Meshed)
            {
                if (FillState != FillStateEnum.Empty)
                {
                    Mesh.Clear();
                    for (int y = 0; y < Globals.ChunkSize; y++)
                    { 
                        for (int z = 0; z < Globals.ChunkSize; z++)
                            for (int x = 0; x < Globals.ChunkSize; x++)
                            {
                                //if(BlockClass.GenerateMesh(Mesh, Blocks[x][y][z]))
                                //{
                                 //   RefaceRequired = true;

//                                }
                                if (Blocks[x][y][z].GenerateMesh(Mesh))
                                {
                                    RefaceRequired = true;
                                }
                            }


                        if (Globals.FrameStopWatch.ElapsedMilliseconds > 8)
                        {
                            yield return shortWait;
                        }
                    }
                }
                State = StateEnum.Meshed;
            }
            else if (State < StateEnum.Faced)
            {
                if (RefaceRequired)
                {
                    if (FillState != FillStateEnum.Empty)
                    {
                        FaceChunk();
                        RefaceRequired = false;
                    }
                }
                State = StateEnum.Faced;
            }
            yield return longWait;
        }
    }
    public void FaceChunk()
    {
        /*
        Mesh.Clear();
        for (int y = 0; y < Globals.ChunkSize; y++)
            for (int z = 0; z < Globals.ChunkSize; z++)
                for (int x = 0; x < Globals.ChunkSize; x++)
                {
                    if (Blocks[x][y][z].Mesh.Vertices.Count > 0)
                    Mesh.Normals.AddRange(Blocks[x][y][z].Mesh.Normals);
                    Mesh.UVs.AddRange(Blocks[x][y][z].Mesh.UVs);
                    Vector3 Pos = new Vector3Int(x, y, z);
                    for (int i = 0; i < Blocks[x][y][z].Mesh.Triangles.Count; i++)
                    {
                        Mesh.Triangles.Add(Mesh.Vertices.Count + Blocks[x][y][z].Mesh.Triangles[i]);
                    }
                    for (int i = 0; i < Blocks[x][y][z].Mesh.Vertices.Count; i++)
                    {
                        Mesh.Vertices.Add(Blocks[x][y][z].Mesh.Vertices[i] + Pos);
                    }
                    Blocks[x][y][z].Mesh.Clear();
                }
*/
        //Mesh meshData = gameObject.GetComponent<MeshFilter>().mesh;
        meshData.Clear();
        meshData.vertices = Mesh.Vertices.ToArray();
        meshData.triangles = Mesh.Triangles.ToArray();
        meshData.uv = Mesh.UVs.ToArray();
        meshData.RecalculateNormals();
        if (gameObject.GetComponent<MeshCollider>())
        {
            meshCollider.sharedMesh = meshData;
        }
        State = StateEnum.Meshed;
        Mesh.Normals.Clear();
        Mesh.Triangles.Clear();
        Mesh.UVs.Clear();
        Mesh.Vertices.Clear();
    }
}
