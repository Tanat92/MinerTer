using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "DropBlock", menuName = "TileBlocks/DropBlock", order = 0)]
public class DropBlock : ScriptableObject
{
    public string nameBlock;
    public Sprite inventory_spr;
    public Sprite drop_spr;
    public typeBlock type;
    public enum typeBlock{
        ore,
        tree,
        block
    }
}
