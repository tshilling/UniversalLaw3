using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;

public class WorldClass : MonoBehaviour {

    // Use this for initialization

    public GameObject _baseChunk;
    private void Awake()
    {
        Globals.ActiveWorld = this;
        var TempList = new List<Vector3Int>();
        for (var X = -Globals.chunkDistance.x; X <= Globals.chunkDistance.x; X += 1)
            for (var Y = Globals.chunkDistance.y; Y >= 0; Y -= 1)
                for (var Z = -Globals.chunkDistance.x; Z <= Globals.chunkDistance.x; Z += 1)
                    TempList.Add(new Vector3Int(X*Globals.ChunkSize, Y * Globals.ChunkSize, Z * Globals.ChunkSize));

        while (TempList.Count > 0)
        {
            float minV = 99999;
            var minI = 0;
            for (var i = 0; i < TempList.Count; i++)
            {
                float D = TempList[i].sqrMagnitude;
                if (D < minV)
                {
                    minV = D;
                    minI = i;
                }
            }

            Globals.RenderList.Add(TempList[minI]);
            TempList.RemoveAt(minI);
        }
        //_baseChunk = Instantiate(Resources.Load("Prefabs/Chunk") as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
    }
    
    public bool GetChunk(Vector3Int Key, out GameObject GO)
    {
        ChunkClass.KEYStruct In = new ChunkClass.KEYStruct();
        for(int i = 0; i < Globals.Chunks.Count; i++)
        {
            In = Globals.Chunks[i].GetComponent<ChunkClass>().KEY;
            if (In.X == Key.x)
                if (In.Y == Key.y)
                    if (In.Z == Key.z)
                    {
                        GO = Globals.Chunks[i];
                        return true;
                    }
        }
        GO = null;
        return false;
    }
    void Start ()
    {
        //StartCoroutine(InitialSceneLoad());
        InitialSceneLoadB();
        //StartCoroutine(SceneUpdate());
    }
    private bool _InitialSceneLoaded = false;
    private bool InitialSceneLoaded
    {
        get
        {
            return _InitialSceneLoaded;
        }
        set
        {
            if (value == true && _InitialSceneLoaded==false)
                Globals._player = Instantiate(Resources.Load("myFPS") as GameObject, new Vector3(0, 150, 0), Quaternion.identity);
            _InitialSceneLoaded = value;
        }
    }
	private IEnumerator InitialSceneLoad()
    {
        var SW = new Stopwatch();
        SW.Start();
        ChunkClass CO;
        for (int i = 0; i < Globals.RenderList.Count;i++)
        {
            Vector3Int V = Globals.RenderList[i];
            Vector3Int origin = new Vector3Int(V.x, V.y, V.z);
            CO = LoadNewChunk(origin);
            while (CO.State < ChunkClass.StateEnum.Seeded)
            {
                yield return null;
            }

            if (SW.ElapsedMilliseconds > 8)
            {
                SW.Stop();
                yield return null;
                SW.Reset();
                SW.Start();
            }

        }
        SW.Stop();
        
        InitialSceneLoaded = true;
    }
    private void InitialSceneLoadB()
    {
        var SW = new Stopwatch();
        SW.Start();
        ChunkClass CO;
        for (int i = 0; i < Globals.RenderList.Count; i++)
        {
            Vector3Int V = Globals.RenderList[i];
            Vector3Int origin = new Vector3Int(V.x, V.y, V.z);
            CO = LoadNewChunk(origin);
            while (CO.State < ChunkClass.StateEnum.Seeded)
            {
            }
        }
        InitialSceneLoaded = true;
    }
    List<GameObject> InActiveChunks = new List<GameObject>();
    WaitForSeconds shortWait = new WaitForSeconds(0.02f);
    WaitForSeconds longWait = new WaitForSeconds(0.02f);

    Stopwatch SW = new Stopwatch();
    Vector3 CameraPos;
    Vector3Int CameraPosI;
    private IEnumerator SceneUpdate()
    {
        while (!InitialSceneLoaded)
        {
            yield return longWait;
        }
        StopCoroutine(InitialSceneLoad());
        GameObject GO;
        ChunkClass CO;
        Vector3Int pos;
        Vector3Int WPt;
        ChunkClass.KEYStruct KEY;
        SW.Start();

        bool yielded = false;
        while (true)
        {
            yielded = false;
            // Check State of Chunks
            CameraPos = Globals._player.transform.position;
            CameraPos.y = 0;
            var CameraPosI = Vector3Int.FloorToInt(CameraPos / 16f) * 16;
            
            for(int i = 0; i < Globals.Chunks.Count; i++)
            {
                WPt = Globals.Chunks[i].GetComponent<ChunkClass>().Position;//.ElementAt(i).Key;
                pos = WPt - CameraPosI;
                if (!Globals.RenderList.Contains(pos))
                {
                    GO = Globals.Chunks[i];
                    Globals.Chunks.RemoveAt(i);
                    GO.GetComponent<ChunkClass>().SleepChunk();
                    InActiveChunks.Add(GO);
                }
            }
            for(int i = 0; i < Globals.RenderList.Count; i++) {
                pos = Globals.RenderList[i];
                WPt = pos + CameraPosI;
                if(!GetChunk(WPt,out GO))
                {
                    LoadNewChunk(WPt);
                    i = 0;
                    yielded = true;
                    yield return shortWait;
                }
            }
            if(!yielded)
                yield return shortWait;
        }
        
    }
    private ChunkClass LoadNewChunk(Vector3Int WPt)
    {
        GameObject GO;
        if (InActiveChunks.Count > 0)
        {
            GO = InActiveChunks[0];
            InActiveChunks.RemoveAt(0);
            GO.transform.position = WPt;
            GO.GetComponent<ChunkClass>().InitChunk();
            Globals.Chunks.Add(GO);
        }
        else
        {
            GO = Instantiate(_baseChunk, new Vector3(WPt.x, WPt.y, WPt.z), Quaternion.identity);
            Globals.Chunks.Add(GO);
        }
        return GO.GetComponent<ChunkClass>();
    }
    // Update is called once per frame
    Vector3Int Compare;

    GameObject tempGO;
    ChunkClass tempCO;
    Vector3Int tempV;
    bool tempBool;
    void LateUpdate () {
        if (!InitialSceneLoaded || Globals._player==null)
            return;
        //StopCoroutine(InitialSceneLoad());
        Vector3Int WPt;

            // Check State of Chunks
        CameraPosI = Vector3Int.FloorToInt(Globals._player.transform.position / 16f) * 16;
        CameraPosI.y = 0;
        for (int i = 0; i < Globals.Chunks.Count; i++)
        {
            tempCO = Globals.Chunks[i].GetComponent<ChunkClass>();
            tempV = tempCO.Position - CameraPosI;
            tempBool = false;
            for(int i2 = 0; i2 < Globals.RenderList.Count; i2++)
            {
                if(Globals.RenderList[i2].x == tempV.x)
                    if (Globals.RenderList[i2].y == tempV.y)
                        if (Globals.RenderList[i2].z == tempV.z)
                        {
                            tempBool = true;
                            break;
                        }

            }
            if (!tempBool)
            {
                Globals.Chunks[i].GetComponent<ChunkClass>().SleepChunk();
                InActiveChunks.Add(Globals.Chunks[i]);
                Globals.Chunks.RemoveAt(i);
            }
        }
        for (int i = 0; i < Globals.RenderList.Count; i++)
        {
            WPt = Globals.RenderList[i] + CameraPosI;
            if (!GetChunk(WPt, out tempGO))
            {
                LoadNewChunk(WPt);
                break;
            }
        }
        
    }
}
