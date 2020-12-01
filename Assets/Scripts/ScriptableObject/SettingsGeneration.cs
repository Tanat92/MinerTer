using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SettingsGeneration", menuName = "TileBlocks/SettingsGeneration", order = 0)]
public class SettingsGeneration : ScriptableObject
{
    [Range(3, 100)] public int sizeCountChunk_x;
    [Range(3, 100)] public int sizeCountChunk_y;
    [Range(5, 100)] public int sizeCountBlock;
    [Range(0, 200)] public int dist;
    public int heightCubeGen;
    public float heightMultip, multi, multiOre, multiUnder;
}