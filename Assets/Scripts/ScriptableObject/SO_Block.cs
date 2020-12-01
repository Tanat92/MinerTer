using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "SOBlock", menuName = "TileBlocks/Block", order = 0)]
public class SO_Block : ScriptableObject
{
    public string nameBlock;
    public float density;
    public Tile tileBlock;
    public RuleTile ruleTileBlock;
    public int y_DepthBlock_min, y_DepthBlock_max;
    public typeBlock type;
    public DropBlock drop;
    //public bool oreGen, tree, altStone;
    public enum typeBlock
    {
        def,
        ore,
        tree,
        altStone
    };
    public float multi;
    [Range(0,10)] public int treeHeightMax;
    [Range(0f,1f)] public float chanceBlock;
}