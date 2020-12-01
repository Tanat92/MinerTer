using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
public class FlyCamera2D : MonoBehaviour
{
    float x_speed, y_speed;
    public float speed;
    int sizeCountBlock, sizeCountChunk_x, sizeCountChunk_y;
    ChunkData getChunkP;
    public Generation gen;
    public SettingsGeneration settingsGen;
    // Start is called before the first frame update
    void Start()
    {
        sizeCountBlock = settingsGen.sizeCountBlock;
        sizeCountChunk_x = settingsGen.sizeCountChunk_x;
        sizeCountChunk_y = settingsGen.sizeCountChunk_y;
    }
    int getNameChunk(Vector3 cam)
    {
        Vector3Int pos = new Vector3Int((int)Mathf.Round(cam.x), (int)Mathf.Round(cam.y), (int)Mathf.Round(cam.z));
        Vector3Int pos1 = new Vector3Int((int)Mathf.Round(cam.x / sizeCountBlock - ((int)sizeCountChunk_x)) * sizeCountBlock, (int)Mathf.Round(cam.y / sizeCountBlock - ((int)sizeCountChunk_y)) * sizeCountBlock, 0);
        Vector3Int chn = new Vector3Int(((int)pos.x / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), ((int)pos.y / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), 0);
        ChunkData chunkPos = new ChunkData();
        for (int i = 0; i < gen.chunks.Count; i++)
        {
            if (gen.chunks[i].nameChunk == "Chunk" + chn.x + chn.y)
            {
                return i;
            }
        }
        return -1;
    }
    private void FixedUpdate()
    {
        transform.position = (Vector3)transform.position + new Vector3(x_speed, y_speed);
    }
    // Update is called once per frame
    void Update()
    {
        x_speed = Input.GetAxis("Horizontal") * speed;
        y_speed = Input.GetAxis("Vertical") * speed;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetKey(KeyCode.Mouse0))
        {
            int indx = getNameChunk(mousePos - new Vector3Int(sizeCountBlock / 2, sizeCountBlock / 2, 0) - new Vector3(0.5f, 0.5f));
            if (indx != -1)
            {
                Vector3Int pos = new Vector3Int(((int)(mousePos.x + (sizeCountBlock / 2)) / sizeCountBlock) * sizeCountBlock - (sizeCountBlock / 2), ((int)(mousePos.y + (sizeCountBlock / 2)) / sizeCountBlock) * sizeCountBlock - (sizeCountBlock / 2), 0);
                Vector3Int pos1 = new Vector3Int((int)mousePos.x, (int)mousePos.y, 0);
                Vector3Int posBlock = pos1 - pos - new Vector3Int(sizeCountBlock / 2, sizeCountBlock / 2, 0);
                Vector3Int posBlk = pos1 - pos;
                int blkIndx = posBlk.x * sizeCountBlock + posBlock.y + (sizeCountBlock / 2);
                if (gen.chunks[indx].blocks[blkIndx].enabledBlock)
                {
                    gen.tilemaps[indx].SetTile(posBlock, null);
                    gen.chunks[indx].blocks[blkIndx].enabledBlock = false;
                    gen.chunks[indx].blocks[blkIndx].tile = null;
                    GameObject item = new GameObject(gen.chunks[indx].blocks[blkIndx].dropBlock.nameBlock);
                    item.AddComponent<SpriteRenderer>();
                    item.GetComponent<SpriteRenderer>().sprite = gen.chunks[indx].blocks[blkIndx].dropBlock.drop.drop_spr;
                    item.AddComponent<Rigidbody2D>();
                    item.AddComponent<BoxCollider2D>();
                    item.AddComponent<ItemVisible>();
                    item.GetComponent<ItemVisible>().settingsGen = settingsGen;
                    item.transform.position = pos1 + new Vector3(0.5f, 0.5f);
                    item.transform.localScale = item.transform.localScale / 2;
                    item.layer = 8;
                    item.GetComponent<Rigidbody2D>().AddForce(Vector3.up * 3f, ForceMode2D.Impulse);
                    item.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
                    item.GetComponent<Rigidbody2D>().freezeRotation = true;
                    StartCoroutine(gen.chunks[indx].saveChunk(gen.chunks[indx].nameChunk));
                }
                gen.tilemapsCollider[indx].GenerateGeometry();
                //Debug.Log(posBlock);

            }
        }
    }
}
