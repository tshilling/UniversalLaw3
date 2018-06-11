using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class subBlockClass
{
    public BlockClass Parent;
    private Vector3 controlPoint = new Vector3(.5f, .5f, .5f);
    private float density = 0;
    private float blockiness = 0;
    public byte Occlude { get; protected set; }
    public subBlockClass()
    {
        Occlude = 63;
    }
    public bool ValidCP = false;
    public bool ControlPointLocked = false;
    public Vector2[] Texture = { new Vector2(0, 0), new Vector2(3, 0), new Vector2(3, 0), new Vector2(3, 0), new Vector2(3, 0), new Vector2(2, 0) };
    public Vector3 ControlPoint
    {
        get
        {
            return controlPoint;
        }
        set
        {
            controlPoint = value;
        }
    }
    public float Density
    {
        get
        {
            return density;
        }
        set
        {
            density = value;
        }
    }
    public float Blockiness
    {
        get
        {
            return blockiness;
        }
        set
        {
            blockiness = value;
        }
    }
    public float TexUnit = 0.0625f;
} 
public class AirBlock : subBlockClass
{
    private float density = -1;
    private float blockiness = 0;
    public AirBlock()
    {
        Occlude = 0;
    }
}
public class BlockClass
{
    public enum StateEnum
    {
        Raw = 0,
        Seeded = 1,
        ValidCP = 2,
        Meshed = 3
    }
    private StateEnum state = StateEnum.Raw;
    public StateEnum State
    {
        get
        {
            return state;
        }
        set
        {
            state = value;
            if(state == StateEnum.Seeded)
            if (Chunk.State > ChunkClass.StateEnum.Seeded)
                Chunk.State = ChunkClass.StateEnum.Seeded;
            if (state == StateEnum.ValidCP)
                if (Chunk.State > ChunkClass.StateEnum.CPed)
                    Chunk.State = ChunkClass.StateEnum.CPed;
            /*
            if (value < state)
            {


                if (value == StateEnum.ValidCP)
                    if(Chunk.State > ChunkClass.StateEnum.CPed)
                    {
                        Chunk.State = ChunkClass.StateEnum.CPed;
                    }
                if (value == StateEnum.Raw)
                    if (Chunk.State > ChunkClass.StateEnum.Seeded)
                    {
                        Chunk.State = ChunkClass.StateEnum.Seeded;
                    }
            }
            */
            state = value;

        }
    }
    public Vector3Int Position { get; protected set; }
    public BlockClass[,,] Neighbor = new BlockClass[3,3,3];
    public ChunkClass Chunk;
    private subBlockClass subBlock = new subBlockClass();
    public subBlockClass SubBlock
    {
        get
        {
            return subBlock;
        }
        set
        {
            subBlock = value;
            State = StateEnum.Seeded;
            SetNeighborStates();
        }
    }
    public void SetDensity(float density)
    {
        SubBlock.Density = density;
        SetNeighborStates();
    }
    public void SetNeighborStates()
    {
        /*
        SubBlock.State = subBlockClass.StateEnum.Raw;
        if (Chunk.State > ChunkClass.StateEnum.Seeded)
            Chunk.State = ChunkClass.StateEnum.Seeded;
        for (int x = 0; x < 3; x++)
            for (int y = 0; y < 3; y++)
                for (int z = 0; z < 3; z++)
                {
                    if (Neighbor[x, y, z] != null)
                    {
                        if (x >= 1 && y >= 1 && z >= 1)
                        {
                            if (Neighbor[x, y, z].SubBlock.State == subBlockClass.StateEnum.Meshed)
                                Neighbor[x, y, z].SubBlock.State = subBlockClass.StateEnum.CPCalculated;
                        }
                        else if (x <= 1 && y <= 1 && z <= 1)
                        {
                            Neighbor[x, y, z].SubBlock.State = subBlockClass.StateEnum.Raw;
                        }

                    }
                }
                */
    }
    public BlockClass(Vector3Int _Position)
    {
        Position = _Position;
        Neighbor[1, 1, 1] = this;
        SubBlock.Parent = this;
    }
    Vector3Int I;
    public bool SetNeighbor(BlockClass B)
    {
        I = B.Position - this.Position;
        Neighbor[I.x + 1, I.y + 1, I.z + 1] = B;
        if(I.x>=0 && I.y>=0 && I.z>=0)  // Because only positive offsets are used for CP calculations.  Any change to negative neighbor doesn't effect CP
            State = StateEnum.Seeded;

        if (B.Neighbor[-I.x + 1, -I.y + 1, -I.z + 1] != this)
        {
            B.Neighbor[-I.x + 1, -I.y + 1, -I.z + 1] = this;
            if (I.x <= 0 && I.y <= 0 && I.z <= 0)
            {   // Neighbor would have to recalc CP because Parent is positve offset
                B.State = StateEnum.Seeded;
            }
            else if (B.state > StateEnum.ValidCP)
            {
                // If Neighbor had already generated its mesh, it will need to remesh.
                B.State = StateEnum.ValidCP;
            }
        }
        return true;
    }
    public bool SetNeighbor(BlockClass B, Vector3Int Offset)
    {
        Neighbor[Offset.x + 1, Offset.y + 1, Offset.z + 1] = B;
        if (Offset.x >= 0 && Offset.y >= 0 && Offset.z >= 0)  // Because only positive offsets are used for CP calculations.  Any change to negative neighbor doesn't effect CP
            State = StateEnum.Seeded;

        if (B.Neighbor[-Offset.x + 1, -Offset.y + 1, -Offset.z + 1] != this)
        {
            B.Neighbor[-Offset.x + 1, -Offset.y + 1, -Offset.z + 1] = this;
            if (Offset.x <= 0 && Offset.y <= 0 && Offset.z <= 0)
            {   // Neighbor would have to recalc CP because Parent is positve offset
                B.State = StateEnum.Seeded;
            }
            else if (B.state > StateEnum.ValidCP)
            {
                // If Neighbor had already generated its mesh, it will need to remesh.
                B.State = StateEnum.ValidCP;
            }
        }
        return true;
    }
    BlockClass[] adjacent = new BlockClass[CPPoints.Length];
    Vector3 NewCP;
    byte edgeCrossings;
   
    public bool CalculateControlPoint()
    {
        if (State >= StateEnum.ValidCP) // Meaning, CP has already been successfully calculated
            return false;
        edgeCrossings = 0;
        for (int i = 0; i < CPPoints.Length; i++)
        {
            adjacent[i] = Neighbor[CPPoints[i].x + 1, CPPoints[i].y + 1, CPPoints[i].z + 1];
            if (adjacent[i] == null)
            {
                // If an adjacent block is null, then it hasn't been finalized and is invalid for calculations
                if(State > StateEnum.Seeded)
                    State = StateEnum.Seeded;
                return false;
            }
            else if(adjacent[i].state < StateEnum.Seeded)
            {
                if (State > StateEnum.Seeded)
                    State = StateEnum.Seeded;
                return false;
            }
            if (adjacent[i].State >= StateEnum.Meshed)
                adjacent[i].State = StateEnum.ValidCP;
        }
        NewCP = Vector3.zero;
        for (int i = 0; i < 12; i++)
        {

            if (((adjacent[BlockEdges[i, 0]].SubBlock.Density <= 0) &&
                             (adjacent[BlockEdges[i, 1]].SubBlock.Density > 0)) ||
                            ((adjacent[BlockEdges[i, 1]].SubBlock.Density < 0) &&
                             (adjacent[BlockEdges[i, 0]].SubBlock.Density >= 0)))
            {

                NewCP += Vector3.LerpUnclamped(
                    CPPoints[BlockEdges[i, 0]],
                    CPPoints[BlockEdges[i, 1]],
                    -(float)adjacent[BlockEdges[i, 0]].SubBlock.Density /
                    (adjacent[BlockEdges[i, 1]].SubBlock.Density -
                     (float)adjacent[BlockEdges[i, 0]].SubBlock.Density));
                edgeCrossings++;
            }

        }
        if (edgeCrossings != 0)
        {
            NewCP /= (float)edgeCrossings;
            var maxB = adjacent[0].SubBlock.Blockiness;
            for (var i = 1; i < adjacent.Length; i++)
            {
                if (adjacent[i].SubBlock.Blockiness > maxB)
                {
                    maxB = adjacent[i].SubBlock.Blockiness;
                }
            }
            NewCP = Vector3.Lerp(NewCP, new Vector3(0.5f, 0.5f, 0.5f), maxB);
        }
        else
        {
            NewCP = new Vector3(0.5f, 0.5f, 0.5f);
        }
        // Yeah, got to the end of calculation, this block is valid
        State = StateEnum.ValidCP;
        if (NewCP != SubBlock.ControlPoint)
        {
            SubBlock.ControlPoint = NewCP;
            return true;
        }
        else
            return false;
    }
    public static bool GenerateMesh(Globals.MeshData Mesh, BlockClass Block)
    {
        bool refreshRequired = false;
        // if (State < StateEnum.ValidCP) {
        //     CalculateControlPoint(this);
        // }
        Block.State = StateEnum.Meshed;
        for (byte Dir = 0; Dir < 6; Dir++)
        {
            var B0 = (byte)(Block.SubBlock.Occlude & (1 << Dir));
            if (B0 <= 0) continue;
            if (!(Block.Neighbor[Globals.DirectionVector[Dir].x + 1, Globals.DirectionVector[Dir].y + 1, Globals.DirectionVector[Dir].z + 1] == null))
            {
                var B1 = (byte)(Block.Neighbor[Globals.DirectionVector[Dir].x + 1, Globals.DirectionVector[Dir].y + 1, Globals.DirectionVector[Dir].z + 1].SubBlock.Occlude & (1 << (5 - Dir)));
                if (B1 == 0)
                {
                    if (Block.addFace(Mesh, Dir))
                    {
                        refreshRequired = true;
                    }
                    else
                    {
                        Block.State = StateEnum.ValidCP;
                        return false;
                    }
                }
            }
        }
        return refreshRequired;
    }
    public bool GenerateMesh(Globals.MeshData Mesh)
    {
        bool refreshRequired = false;
        State = StateEnum.Meshed;
        for (byte Dir = 0; Dir < 6; Dir++)
        {
            var B0 = (byte)(SubBlock.Occlude & (1 << Dir));
            if (B0 <= 0) continue;
            if (!(Neighbor[Globals.DirectionVector[Dir].x + 1, Globals.DirectionVector[Dir].y + 1, Globals.DirectionVector[Dir].z + 1] == null))
            {
                var B1 = (byte)(Neighbor[Globals.DirectionVector[Dir].x + 1, Globals.DirectionVector[Dir].y + 1, Globals.DirectionVector[Dir].z + 1].SubBlock.Occlude & (1 << (5 - Dir)));
                if (B1 == 0)
                {
                    if (addFace(Mesh, Dir))
                    {
                        refreshRequired = true;
                    }
                    else
                    {
                        State = StateEnum.ValidCP;
                        return false;
                    }
                }
            }
        }
        return refreshRequired;
    }
   
    const float pixelOffset = (1.0f / 256.0f);
    const float pixelOffset2 = (2.0f / 256.0f);
    BlockClass B0;
    private bool addFace(Globals.MeshData Mesh, byte dir)
    {
        for (var i = 0; i < 4; i++)
        {
            var blk = FacePoints[BlockFaces[dir, i]];
            if (Neighbor[blk.x + 1, blk.y + 1, blk.z + 1] == null)
            {
                Mesh.Vertices.RemoveRange(Mesh.Vertices.Count - i, i);
                Mesh.Normals.RemoveRange(Mesh.Normals.Count - i, i);
                return false;
            }
            if (Neighbor[blk.x + 1, blk.y + 1, blk.z + 1].State < StateEnum.ValidCP)
            {
                Mesh.Vertices.RemoveRange(Mesh.Vertices.Count - i, i);
                Mesh.Normals.RemoveRange(Mesh.Normals.Count - i, i);
                return false;
            }
            Mesh.Vertices.Add(Position + blk + Neighbor[blk.x + 1, blk.y + 1, blk.z + 1].SubBlock.ControlPoint - Chunk.Position);
            Mesh.Normals.Add(Globals.DirectionVector[dir]);
        }

        var v1 = Mesh.Vertices[Mesh.Vertices.Count - 1] -
                 Mesh.Vertices[Mesh.Vertices.Count - 2];
        var v2 = Mesh.Vertices[Mesh.Vertices.Count - 3] -
                 Mesh.Vertices[Mesh.Vertices.Count - 2];

        var N = Vector3.Cross(v1, v2).normalized;
        if (N.y > .3) dir = (byte)Globals.Direction.Up; //Up

        var sc = Mesh.Vertices.Count - 4; // squareCount << 2;//Multiply by 4
        if ((Mesh.Vertices[Mesh.Vertices.Count - 3] - Mesh.Vertices[Mesh.Vertices.Count - 1]).sqrMagnitude < (Mesh.Vertices[Mesh.Vertices.Count - 4] - Mesh.Vertices[Mesh.Vertices.Count - 2]).sqrMagnitude)
        {
            Mesh.Triangles.Add(sc);
            Mesh.Triangles.Add(sc + 1);
            Mesh.Triangles.Add(sc + 3);
            Mesh.Triangles.Add(sc + 1);
            Mesh.Triangles.Add(sc + 2);
            Mesh.Triangles.Add(sc + 3);
        }
        else
        {
            Mesh.Triangles.Add(sc);
            Mesh.Triangles.Add(sc + 1);
            Mesh.Triangles.Add(sc + 2);
            Mesh.Triangles.Add(sc);
            Mesh.Triangles.Add(sc + 2);
            Mesh.Triangles.Add(sc + 3);

        }

        //var v = blocks[center.x + 1][center.y + 1][center.z + 1].GetTex();
        var v = SubBlock.Texture;
        
        var uv = new Vector2(v[dir].x / 16f + pixelOffset, (15 - v[dir].y) / 16f + pixelOffset);
        Mesh.UVs.Add(uv);
        Mesh.UVs.Add(new Vector2(uv.x + SubBlock.TexUnit - pixelOffset2, uv.y));
        Mesh.UVs.Add(new Vector2(uv.x + SubBlock.TexUnit - pixelOffset2, uv.y + SubBlock.TexUnit - pixelOffset2));
        Mesh.UVs.Add(new Vector2(uv.x, uv.y + SubBlock.TexUnit - pixelOffset2));
        return true;
        //squareCount++;
    }

    //######################################## Static Variables ##########################################
    public static readonly Vector3Int[] FacePoints =
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, -1),
        new Vector3Int(-1, 0, -1),
        new Vector3Int(-1, -1, -1),
        new Vector3Int(0, -1, -1)
    };
    public static readonly Vector3Int[] CPPoints =
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1)
    };
    public static readonly byte[,] BlockFaces =
    {
        {0, 4, 5, 1}, //UP
        {2, 3, 0, 1}, //North
        {3, 7, 4, 0}, //East
        {6, 2, 1, 5}, //West
        {7, 6, 5, 4}, //South
        {2, 6, 7, 3} //Down 
    };
    public static readonly byte[,] BlockEdges =
    {
        {0, 1},
        {1, 2},
        {2, 3},
        {3, 0},
        {4, 5},
        {5, 6},
        {6, 7},
        {7, 4},
        {0, 4},
        {1, 5},
        {2, 6},
        {3, 7}
    };
}
