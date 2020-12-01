using UnityEngine;
using UnityEngine.Tilemaps;
[System.Serializable]
public class BlockData {
    public Vector3Int pos;
    public bool enabledBlock;
    public bool enabledBackBlock;
    public bool enabledPlantBlock;
    public TileBase tileBackBlock;
    public TileBase tile;
    public SO_Block dropBlock;
    //public ChunkData chunk;
    public void setChunk(ChunkData set){
        //chunk = set;
    }
    /*public void setPosition(Vector3Int set){
        pos = set;
    }
    public Vector3Int getPosition(){
        return pos;
    }*/
    public ChunkData getChunk(){
        return null;
    }
    public void setEnabledBlock(bool set){
        enabledBlock = set;
    }
    public bool getEnabledBlock(){
        return enabledBlock;
    }
}