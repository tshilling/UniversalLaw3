using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeederClass
{
    public static FastNoiseSIMD Noise = new FastNoiseSIMD(0);
    public static bool SeedChunk(ChunkClass Chunk)
    {
        int AirBlocks = 0;
        int NonAir = 0;
        Vector3Int _position = Chunk.Position;
        Noise.SetPerturbType(FastNoiseSIMD.PerturbType.None);
        Noise.SetAxisScales(1, 1, 1);
        Noise.SetNoiseType(FastNoiseSIMD.NoiseType.Cubic);
        Noise.SetFrequency(0.01f);
        var LongPeriod = Noise.GetSampledNoiseSet(_position.x, 0, _position.z, Globals.ChunkSize, 1, Globals.ChunkSize, 1);
        Noise.SetNoiseType(FastNoiseSIMD.NoiseType.SimplexFractal);
        Noise.SetFractalType(FastNoiseSIMD.FractalType.FBM);
        Noise.SetFrequency(0.01f);
        var ShortPeriod = Noise.GetSampledNoiseSet(_position.x, 0, _position.z, Globals.ChunkSize, 1, Globals.ChunkSize, 1);

        int index = 0;
        for (int y = 0; y < Globals.ChunkSize; y++)
        {
            index = 0;
            for (int x = 0; x < Globals.ChunkSize; x++)
                for (int z = 0; z < Globals.ChunkSize; z++)
                {
                    float v = LongPeriod[index] * 32 + 16 + ShortPeriod[index++]*16;// * (float)(Biom[x, z] * 8);
                    if (v >=  Chunk.Position.y + y)
                    {
                        Chunk.Blocks[x][y][z].SubBlock = new subBlockClass();
                        Chunk.Blocks[x][y][z].SetDensity(v - (Chunk.Position.y + y));
                        NonAir++;

                    }
                    else
                    {

                        Chunk.Blocks[x][y][z].SubBlock = new AirBlock();
                        Chunk.Blocks[x][y][z].SetDensity(v - (Chunk.Position.y + y));
                        AirBlocks++;
                    }
                }
        }
        if (NonAir == 0)
        {
            Chunk.FillState = ChunkClass.FillStateEnum.Empty;
        }
        else if (AirBlocks == 0)
        {
            Chunk.FillState = ChunkClass.FillStateEnum.Solid;
        }
        else
        {
            Chunk.FillState = ChunkClass.FillStateEnum.Mixed;
        }
        return true;
    }
}
