using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

public static class SaveSystem 
{
    //월드 데이터 저장
    public static void SaveWorld(WorldData world)
    {
        string savePath = MinecraftTerrain.Instance.appPath + "/saves/" + world.worldName + "/";
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        
        Debug.Log("Saving" + world.worldName + " to " + savePath);
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "world.world", FileMode.Create);
        
        formatter.Serialize(stream, world);
        stream.Close();
        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();
    }
    //변경 청크 데이터 저장
    public static void SaveChunks(WorldData world)
    {
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();
        
        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;
        }
        
    }
    //월드 데이터 로드
    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = MinecraftTerrain.Instance.appPath + "/saves/" + worldName + "/";

        if (File.Exists(loadPath + "world.world"))
        {
            Debug.Log(worldName + " loaded from " + loadPath);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "world.world", FileMode.Open);
            
            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();
            return new WorldData(world);
        }
        else
        {
            Debug.Log(worldName + " not loaded from " + loadPath);
            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);
            return world;
        }
    }
    //청크 저장
    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = chunk.Position.x + "-" + chunk.Position.y;
        string savePath = MinecraftTerrain.Instance.appPath + "/saves/" + worldName + "/chunks";
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + chunkName + ".chunk", FileMode.Create);
        
        formatter.Serialize(stream, chunk);
        stream.Close();
    }
    //청크 로드
    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = position.x + "-" + position.y;
        string loadPath = MinecraftTerrain.Instance.appPath + "/saves/" + worldName + "/chunks/" + chunkName + ".chunk";

        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);
            
            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();
            return chunkData;
        }

        return null;
    }
}
