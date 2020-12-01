using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Text;

[System.Serializable]
class dataChunkSave
{
    public BlockData[] blocks;
    public Vector3Int posChunk;
    public string nameChunk;
}
[System.Serializable]
public class ChunkData
{
    public BlockData[] blocks;
    public Vector3Int posChunk;
    public string nameChunk;

    /*public void setNameChunk(string set){
        nameChunk = set;
    }
    public string getNameChunk(){
        return nameChunk;
    }*/
    public void createChunk(int sizeBlocks)
    {
        string path = Application.persistentDataPath + "/Data";
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }
        blocks = new BlockData[sizeBlocks];
    }
    /*public void setPosition(Vector3Int set){
        posChunk = set;
    }
    public Vector3Int getPosition(){
        return posChunk;
    }
    public void addBlock(BlockData block){
        blocks.Add(block);
    }
    public void setBlock(int id,BlockData bl){
        blocks[id] = bl;
    }
    public BlockData getBlock(int id){
        return blocks[id];
    }*/
    public bool checkSavedChunk(string numChunk)
    {
        string destination = Application.persistentDataPath + "/Data/" + numChunk + ".data";
        return File.Exists(destination);
    }
    public IEnumerator saveChunk(string numChunk)
    {
        dataChunkSave obj = new dataChunkSave();
        obj.blocks = blocks;
        obj.posChunk = posChunk;
        obj.nameChunk = nameChunk;
        string json = JsonUtility.ToJson(obj);
        string destination = Application.persistentDataPath + "/Data/" + numChunk + ".data";
        File.WriteAllText(destination, json);
        yield return null;
    }
    public IEnumerator loadChunk(string numChunk)
    {
        string destination = Application.persistentDataPath + "/Data/" + numChunk + ".data";
        dataChunkSave data = JsonUtility.FromJson<dataChunkSave>(File.ReadAllText(destination));
        posChunk = data.posChunk;
        blocks = data.blocks;
        nameChunk = data.nameChunk;
        yield return null;
    }
}