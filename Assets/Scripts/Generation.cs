using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Jobs;
using Unity.Collections;

public struct NoiseGen : IJob
{
    public NativeArray<float> nm;
    public float xWorld,multi,seed,heightMultip,seed2,seed3;
    public void Execute()
    {
        nm[0] = Mathf.RoundToInt((Mathf.PerlinNoise(1f, xWorld * multi + seed)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 4) + seed2)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 8) + seed3)) * heightMultip);
    }
}
public class Generation : MonoBehaviour
{
    public List<ChunkData> chunks = new List<ChunkData>();
    int sizeCountChunk_x, sizeCountChunk_y;
    int sizeCountBlock;
    int dist;
    public GameObject player;
    RuleTile treeTile;
    public List<SO_Block> tilesSO = new List<SO_Block>();
    public List<Tilemap> tilemaps = new List<Tilemap>();
    public List<Tilemap> tilemapsBack = new List<Tilemap>();
    public List<CompositeCollider2D> tilemapsCollider = new List<CompositeCollider2D>();
    List<GameObject> grids = new List<GameObject>();
    Tile tile;
    float heightMultip, multi, multiOre, multiUnder;
    int heightCubeGen;
    bool changePosChunks = false;
    float seed, seed2, seed3;
    List<TreeDataGen> treeDataGen = new List<TreeDataGen>();
    List<BushDataGen> bushDataGen = new List<BushDataGen>();
    bool enb = false;
    List<Vector3Int> lChunk = new List<Vector3Int>();
    List<Vector3Int> lChunkNext = new List<Vector3Int>();
    List<bool> bChunk = new List<bool>();
    List<GameObject> changeChunks = new List<GameObject>();
    List<GameObject> chunksMove = new List<GameObject>();
    ChunkData getChunkP;
    public Material mat;
    public RuleTile shd;
    public Tilemap shdTilemap;
    List<SO_Block> groundTiles = new List<SO_Block>();
    List<SO_Block> groundAltTiles = new List<SO_Block>();
    List<SO_Block> treeGen = new List<SO_Block>();
    List<SO_Block> oreTiles = new List<SO_Block>();
    List<float> oreGen = new List<float>();
    List<float> stoneAltGen = new List<float>();
    public SettingsGeneration settingsGen;

    private void Awake()
    {
        heightCubeGen = settingsGen.heightCubeGen;
        heightMultip = settingsGen.heightMultip;
        multi = settingsGen.multi;
        multiOre = settingsGen.multiOre;
        multiUnder = settingsGen.multiUnder;
        sizeCountBlock = settingsGen.sizeCountBlock;
        sizeCountChunk_x = settingsGen.sizeCountChunk_x;
        sizeCountChunk_y = settingsGen.sizeCountChunk_y;
        dist = settingsGen.dist;
    }
    // Start is called before the first frame update
    IEnumerator Start()
    {
        
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        
        string path = Application.persistentDataPath + "/Data";
        DirectoryInfo di = new DirectoryInfo(path);
        if (di.Exists)
        {
            DirectoryInfo[] diA = di.GetDirectories();
            FileInfo[] fi = di.GetFiles();
            foreach (FileInfo f in fi)
            {
                f.Delete();
            }
        }

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        if (!dirInfo.Exists)
        {
            dirInfo.Create();
        }

        seed = Random.Range(-99999f, 99999f);
        seed2 = Random.Range(-99999f, 99999f);
        seed3 = Random.Range(-99999f, 99999f);

        for (int i = 0; i < tilesSO.Count; i++)
        {
            if (tilesSO[i].type == SO_Block.typeBlock.ore)
            {
                oreGen.Add(Random.Range(-99999f, 99999f));
            }
            if (tilesSO[i].type == SO_Block.typeBlock.altStone)
            {
                stoneAltGen.Add(Random.Range(-99999f, 99999f));
            }
        }

        int countChk = 0;
        for (int i = 0; i < sizeCountChunk_x; i++)
        {
            for (int j = 0; j < sizeCountChunk_y; j++)
            {
                GameObject gmj = new GameObject("Grid");
                gmj.AddComponent<Grid>();
                gmj.tag = "Chunk";
                grids.Add(gmj);
                gmj.transform.SetParent(this.gameObject.transform);
                GameObject gmj1 = new GameObject("Tilemap");
                gmj1.AddComponent<Tilemap>();
                gmj1.AddComponent<TilemapRenderer>();
                gmj1.GetComponent<TilemapRenderer>().material = mat;
                gmj1.AddComponent<TilemapCollider2D>();
                gmj1.GetComponent<TilemapCollider2D>().usedByComposite = true;
                gmj1.AddComponent<Rigidbody2D>();
                gmj1.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                gmj1.AddComponent<CompositeCollider2D>();
                gmj1.GetComponent<CompositeCollider2D>().generationType = CompositeCollider2D.GenerationType.Manual;
                gmj1.transform.SetParent(gmj.transform);
                tilemapsCollider.Add(gmj1.GetComponent<CompositeCollider2D>());
                tilemaps.Add(gmj1.GetComponent<Tilemap>());
                GameObject gmj2 = new GameObject("Background");
                gmj2.AddComponent<Tilemap>();
                gmj2.AddComponent<TilemapRenderer>();
                gmj2.GetComponent<TilemapRenderer>().material = mat;
                gmj2.GetComponent<Tilemap>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
                gmj2.GetComponent<TilemapRenderer>().sortingOrder = -2;
                gmj2.transform.SetParent(gmj.transform);
                tilemapsBack.Add(gmj2.GetComponent<Tilemap>());

                Vector3 pl = player.transform.position;
                Vector3Int pos = new Vector3Int((int)Mathf.Round(pl.x / sizeCountBlock - ((int)sizeCountChunk_x / 2)) * sizeCountBlock, (int)Mathf.Round(pl.y / sizeCountBlock - ((int)sizeCountChunk_y / 2)) * sizeCountBlock, 0);
                ChunkData chunk = new ChunkData();
                chunk.createChunk(sizeCountBlock * sizeCountBlock);
                int count = 0;
                for (int x = 0; x < sizeCountBlock; x++)
                {
                    for (int y = 0; y < sizeCountBlock; y++)
                    {
                        BlockData block = new BlockData();
                        block.pos = new Vector3Int(x, y, 0);
                        block.setChunk(chunk);
                        block.setEnabledBlock(false);
                        chunk.blocks[count] = block;
                        count++;
                    }
                }
                gmj.transform.position = (new Vector3Int((i * sizeCountBlock), (j * sizeCountBlock), 0) + pos);
                chunk.posChunk = (new Vector3Int((i * sizeCountBlock), (j * sizeCountBlock), 0) + pos);
                chunk.nameChunk = "Chunk" + chunk.posChunk.x.ToString() + chunk.posChunk.y.ToString();
                gmj.name = "Grid " + chunk.posChunk.x.ToString() + chunk.posChunk.y.ToString();
                chunks.Add(chunk);

                int countBlock = 0;
                float xWorld, yWorld;
                for (int x = 0; x < sizeCountBlock; x++)
                {
                    xWorld = (i * sizeCountBlock) + sizeCountBlock / 2 + pos.x + x;
                    float g = 0;// = Mathf.RoundToInt((Mathf.PerlinNoise(1f, xWorld * multi + seed)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 4) + seed2)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 8) + seed3)) * heightMultip);
                    NativeArray<float> array = new NativeArray<float>(10, Allocator.TempJob);
                    NoiseGen ng = new NoiseGen();
                    ng.nm = array;
                    ng.heightMultip = heightMultip;
                    ng.multi = multi;
                    ng.seed = seed;
                    ng.seed2 = seed2;
                    ng.seed3 = seed3;
                    ng.xWorld = xWorld;
                    JobHandle jb = ng.Schedule();
                    jb.Complete();
                    if (jb.IsCompleted) g = ng.nm[0];
                    array.Dispose();
                    for (int y = 0; y < sizeCountBlock; y++)
                    {
                        yWorld = (j * sizeCountBlock) + sizeCountBlock / 2 + pos.y + y;
                        BlockData block = chunk.blocks[countBlock];
                        Vector3Int posBlock = new Vector3Int((i * sizeCountBlock) + sizeCountBlock / 2, (j * sizeCountBlock) + sizeCountBlock / 2, 0) + pos - new Vector3Int(sizeCountBlock, sizeCountBlock, 0) + new Vector3Int(x, y, 0);

                        float DungeonGen = (Mathf.PerlinNoise(xWorld * multiUnder + seed, yWorld * multiUnder)) + (Mathf.PerlinNoise(xWorld * (multiUnder * 3) + seed2, yWorld * (multiUnder * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (multiUnder * 6) + seed3, yWorld * (multiUnder * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (multiUnder / 2) + seed, yWorld * (multiUnder / 3)) / 1.5f);


                        for (int f = 0; f < tilesSO.Count; f++)
                        {
                            if (tilesSO[f].y_DepthBlock_min < 0)
                            {
                                groundTiles.Add(tilesSO[f]);
                            }
                            if (tilesSO[f].type == SO_Block.typeBlock.altStone)
                            {
                                groundAltTiles.Add(tilesSO[f]);
                            }
                            if (tilesSO[f].type == SO_Block.typeBlock.tree)
                            {
                                treeGen.Add(tilesSO[f]);
                            }
                            if (tilesSO[f].type == SO_Block.typeBlock.ore)
                            {
                                oreTiles.Add(tilesSO[f]);
                            }
                        }
                        for (int f = 0; f < tilesSO.Count; f++)
                        {
                            if (posBlock.y > (heightCubeGen / 2) - g)
                            {
                                block.pos = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0);
                                block.setEnabledBlock(false);
                                block.enabledBackBlock = false;
                            }
                            if (tilesSO[f].y_DepthBlock_min >= 0)
                            {
                                if (posBlock.y <= (heightCubeGen / 2) - g)
                                {
                                    if (posBlock.y <= (heightCubeGen / 2) - g - tilesSO[f].y_DepthBlock_min)
                                    {
                                        if (posBlock.y >= (heightCubeGen / 2) - g - tilesSO[f].y_DepthBlock_max)
                                        {
                                            if (tilesSO[f].type == SO_Block.typeBlock.def)
                                            {
                                                tile = tilesSO[f].tileBlock;
                                            }
                                        }
                                    }
                                    block.tile = tile;
                                    block.pos = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0);
                                    if (DungeonGen < 0.6f)
                                    {
                                        for (int o = 0; o < stoneAltGen.Count; o++)
                                        {
                                            float groundAltGen = (Mathf.PerlinNoise(xWorld * groundAltTiles[o].multi + stoneAltGen[o] + 1, yWorld * groundAltTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 3) + seed2 + 2, yWorld * (groundAltTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 6) + seed3 + 3, yWorld * (groundAltTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi / 2) + seed + 4, yWorld * (groundAltTiles[o].multi / 3)) / 1.5f);
                                            if (groundAltGen > groundAltTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                            {
                                                if (posBlock.y <= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_min)
                                                {
                                                    if (posBlock.y >= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_max)
                                                    {
                                                        tile = groundAltTiles[o].tileBlock;
                                                        block.tile = groundAltTiles[o].tileBlock;
                                                    }
                                                }
                                            }
                                        }
                                        for (int o = 0; o < oreGen.Count; o++)
                                        {
                                            float OreGen = (Mathf.PerlinNoise(xWorld * oreTiles[o].multi + oreGen[o] + 1, yWorld * oreTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi * 3) + seed2 + 2, yWorld * (oreTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi * 6) + seed3 + 3, yWorld * (oreTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi / 2) + seed + 4, yWorld * (oreTiles[o].multi / 3)) / 1.5f);
                                            if (OreGen > oreTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                            {
                                                if (posBlock.y <= (heightCubeGen / 2) - g - oreTiles[o].y_DepthBlock_min)
                                                {
                                                    if (posBlock.y >= (heightCubeGen / 2) - g - oreTiles[o].y_DepthBlock_max)
                                                    {
                                                        tile = oreTiles[o].tileBlock;
                                                        block.tile = oreTiles[o].tileBlock;
                                                    }
                                                }
                                            }
                                        }
                                        block.setEnabledBlock(true);
                                        block.enabledBackBlock = true;
                                        block.tileBackBlock = tile;
                                        block.dropBlock = tilesSO[f];
                                        gmj1.GetComponent<Tilemap>().SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), tile);
                                    }
                                    else
                                    {
                                        block.tileBackBlock = tile;
                                        block.setEnabledBlock(false);
                                    }
                                    block.enabledBackBlock = true;
                                    for (int o = 0; o < stoneAltGen.Count; o++)
                                    {
                                        float groundAltGen = (Mathf.PerlinNoise(xWorld * groundAltTiles[o].multi + stoneAltGen[o] + 1, yWorld * groundAltTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 3) + seed2 + 2, yWorld * (groundAltTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 6) + seed3 + 3, yWorld * (groundAltTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi / 2) + seed + 4, yWorld * (groundAltTiles[o].multi / 3)) / 1.5f);
                                        if (groundAltGen > groundAltTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                        {
                                            if (posBlock.y <= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_min)
                                            {
                                                if (posBlock.y >= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_max)
                                                {
                                                    tile = groundAltTiles[o].tileBlock;
                                                    block.tileBackBlock = groundAltTiles[o].tileBlock;
                                                }
                                            }
                                        }
                                    }
                                    gmj2.GetComponent<Tilemap>().SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), tile);
                                    shdTilemap.SetTile(new Vector3Int((sizeCountBlock * i) - (sizeCountBlock / 2) + x, (sizeCountBlock * j) - (sizeCountBlock / 2) + y, 0) + pos, shd);
                                }
                            }
                            else
                            {
                                if (posBlock.y == (heightCubeGen / 2) - g)
                                {
                                    float rand = Random.Range(0f, 1f);
                                    int rnd = Random.Range(0, treeGen.Count);
                                    int randG = Random.Range(0, groundTiles.Count);
                                    if (rand > treeGen[rnd].chanceBlock && DungeonGen < 0.6f)
                                    {
                                        Vector3Int posY = new Vector3Int(posBlock.x, posBlock.y + 1, posBlock.z);
                                        TreeDataGen tree = new TreeDataGen();
                                        tree.chunkIndex = countChk;
                                        tree.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1, 0);
                                        tree.posChunk = posY;
                                        tree.tile = treeGen[rnd].ruleTileBlock;
                                        tree.block = tilesSO[f];
                                        treeDataGen.Add(tree);
                                        for (int k = 1; k < 2 + Random.Range(0, 4); k++)
                                        {
                                            posY = new Vector3Int(posBlock.x, posBlock.y + 1 + k, posBlock.z);
                                            tree = new TreeDataGen();
                                            tree.chunkIndex = countChk;
                                            tree.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1 + k, 0);
                                            tree.posChunk = posY;
                                            tree.tile = treeGen[rnd].ruleTileBlock;
                                            tree.block = tilesSO[f];
                                            treeDataGen.Add(tree);
                                        }
                                    }
                                    else if (rand > groundTiles[randG].chanceBlock && DungeonGen < 0.6f)
                                    {
                                        Vector3Int posY = new Vector3Int(posBlock.x, posBlock.y + 1, posBlock.z);
                                        BushDataGen bushStone = new BushDataGen();
                                        bushStone.chunkIndex = countChk;
                                        bushStone.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1, 0);
                                        if (groundTiles[randG].ruleTileBlock != null)
                                        {
                                            bushStone.tileRule = groundTiles[randG].ruleTileBlock;
                                        }
                                        else
                                        {
                                            bushStone.tile = groundTiles[randG].tileBlock;
                                        }
                                        bushStone.block = tilesSO[f];
                                        bushStone.posChunk = posY;
                                        bushDataGen.Add(bushStone);
                                    }
                                }
                            }
                        }
                        chunk.blocks[countBlock] = block;
                        countBlock++;
                    }
                }
                yield return StartCoroutine(chunk.saveChunk("Chunk" + gmj.transform.position.x.ToString() + gmj.transform.position.y.ToString()));
                gmj1.GetComponent<CompositeCollider2D>().GenerateGeometry();
                countChk++;
            }
        }
        yield return StartCoroutine(generationBushStone());
        yield return StartCoroutine(generationTrees());

        Vector3 plh = player.transform.position;
        Vector3Int pos1 = new Vector3Int((int)(plh.x / sizeCountBlock - ((int)sizeCountChunk_x / 2)) * sizeCountBlock, (int)(plh.y / sizeCountBlock - ((int)sizeCountChunk_y / 2)) * sizeCountBlock, 0);
        for (int l = 0; l < sizeCountChunk_x; l++)
        {
            for (int h = 0; h < sizeCountChunk_y; h++)
            {
                lChunk.Add(Vector3Int.zero);
                lChunkNext.Add(Vector3Int.zero);
            }
        }
        for (int i = 0; i < sizeCountChunk_x * sizeCountChunk_y; i++)
        {
            bChunk.Add(false);
        }
        enb = true;
    }

    private void FixedUpdate()
    {

    }
    // Update is called once per frame
    void Update()
    {
        if (enb)
        {
            StopCoroutine("generationNewGenWorld");
            StartCoroutine(generationNewGenWorld());
        }
    }


    IEnumerator getNameChunk(Vector3 cam)
    {
        Vector3Int pos = new Vector3Int((int)Mathf.Round(cam.x), (int)Mathf.Round(cam.y), (int)Mathf.Round(cam.z));
        Vector3Int pos1 = new Vector3Int((int)Mathf.Round(cam.x / sizeCountBlock - ((int)sizeCountChunk_x)) * sizeCountBlock, (int)Mathf.Round(cam.y / sizeCountBlock - ((int)sizeCountChunk_y)) * sizeCountBlock, 0);
        Vector3Int chn = new Vector3Int(((int)pos.x / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), ((int)pos.y / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), 0);
        ChunkData chunkPos = new ChunkData();
        string destination = Application.persistentDataPath + "/Data/" + "Chunk" + chn.x + chn.y + ".data";
        if (File.Exists(destination))
        {
            yield return StartCoroutine(chunkPos.loadChunk("Chunk" + chn.x + chn.y));
            getChunkP = chunkPos;
        }
        yield return null;
    }


    private IEnumerator generationNewGenWorld()
    {
        enb = false;
        if (!changePosChunks)
        {
            int count = 0;
            for (int i = 0; i < sizeCountChunk_x; i++)
            {
                for (int j = 0; j < sizeCountChunk_y; j++)
                {
                    float posChunk = Vector3.Distance(grids[count].transform.position - new Vector3(0.5f, 0.5f), player.transform.position);
                    if (posChunk > dist)
                    {
                        changePosChunks = true;
                    }
                    count++;
                }
            }
        }
        else
        {
            Vector3 pl = player.transform.position;
            yield return StartCoroutine(lChunkSet());
            int count = 0;

            for (int i = 0; i < sizeCountChunk_x; i++)
            {
                for (int j = 0; j < sizeCountChunk_y; j++)
                {
                    for (int l = 0; l < sizeCountChunk_x * sizeCountChunk_y; l++)
                    {
                        if (lChunk[l] == grids[count].transform.position)
                        {
                            bChunk[count] = true;
                            break;
                        }
                    }
                    count++;
                }
            }
            chunksMove.Clear();
            changeChunks.Clear();
            for (int l = 0; l < sizeCountChunk_x * sizeCountChunk_y; l++)
            {
                if (bChunk[l])
                {
                    changeChunks.Add(grids[l]);
                }
                else
                {
                    chunksMove.Add(grids[l]);
                }
            }
            List<Vector3> posChunks = new List<Vector3>();
            for (int i = 0; i < changeChunks.Count; i++)
            {
                posChunks.Add(changeChunks[i].transform.position);
            }
            int changeChunkCount = 0;
            count = 0;
            for (int i = 0; i < sizeCountChunk_x; i++)
            {
                for (int j = 0; j < sizeCountChunk_y; j++)
                {
                    Vector3Int pos = new Vector3Int((int)Mathf.Round(pl.x / sizeCountBlock - ((int)sizeCountChunk_x / 2)) * sizeCountBlock, (int)Mathf.Round(pl.y / sizeCountBlock - ((int)sizeCountChunk_y / 2)) * sizeCountBlock, 0);
                    bool enb = false;
                    for (int l = 0; l < posChunks.Count; l++)
                    {
                        if ((new Vector3Int((i * sizeCountBlock), (j * sizeCountBlock), 0) + pos) == posChunks[l])
                        {
                            enb = true;
                            break;
                        }
                    }
                    if (enb == false)
                    {
                        for (int l = 0; l < posChunks.Count; l++)
                        {
                            if (changeChunkCount < chunksMove.Count)
                            {
                                for (int h = 0; h < grids.Count; h++)
                                {
                                    if (grids[h].transform.position == chunksMove[changeChunkCount].transform.position)
                                    {
                                        //yield return StartCoroutine(ShadowChunkDel(i, j, h));
                                        grids[h].transform.position = (new Vector3Int((i * sizeCountBlock), (j * sizeCountBlock), 0) + pos);
                                        count = h;
                                    }
                                }
                                changeChunkCount++;
                                grids[count].name = "Grid " + grids[count].transform.position.x.ToString() + grids[count].transform.position.y.ToString();
                                chunks[count].nameChunk = ("Chunk" + grids[count].transform.position.x.ToString() + grids[count].transform.position.y.ToString());

                                tilemaps[count].ClearAllTiles();
                                tilemapsBack[count].ClearAllTiles();
                                if (chunks[count].checkSavedChunk(chunks[count].nameChunk))
                                {
                                    int countBlock = 0;
                                    yield return StartCoroutine(chunks[count].loadChunk(chunks[count].nameChunk));
                                    for (int x = 0; x < sizeCountBlock; x++)
                                    {
                                        for (int y = 0; y < sizeCountBlock; y++)
                                        {
                                            BlockData block = chunks[count].blocks[countBlock];
                                            if (block.enabledBackBlock)
                                            {
                                                tilemapsBack[count].SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), block.tileBackBlock);
                                            }
                                            if (block.getEnabledBlock())
                                            {
                                                tilemaps[count].SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), block.tile);
                                                if (block.enabledBackBlock)
                                                {
                                                    tilemapsBack[count].SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), block.tileBackBlock);
                                                }
                                            }
                                            countBlock++;
                                        }
                                    }
                                }
                                else
                                {
                                    int countBlock = 0;
                                    float xWorld, yWorld;
                                    for (int x = 0; x < sizeCountBlock; x++)
                                    {
                                        xWorld = (i * sizeCountBlock) + sizeCountBlock / 2 + pos.x + x;
                                        float g = Mathf.RoundToInt((Mathf.PerlinNoise(1f, xWorld * multi + seed)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 4) + seed2)) * heightMultip) + Mathf.RoundToInt((Mathf.PerlinNoise(1 / 3, xWorld * (multi * 8) + seed3)) * heightMultip);
                                        for (int y = 0; y < sizeCountBlock; y++)
                                        {
                                            yWorld = (j * sizeCountBlock) + sizeCountBlock / 2 + pos.y + y;
                                            BlockData block = chunks[count].blocks[countBlock];
                                            Vector3Int posBlock = new Vector3Int((i * sizeCountBlock) + sizeCountBlock / 2, (j * sizeCountBlock) + sizeCountBlock / 2, 0) + pos - new Vector3Int(sizeCountBlock, sizeCountBlock, 0) + new Vector3Int(x, y, 0);
                                            float DungeonGen = (Mathf.PerlinNoise(xWorld * multiUnder + seed, yWorld * multiUnder)) + (Mathf.PerlinNoise(xWorld * (multiUnder * 3) + seed2, yWorld * (multiUnder * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (multiUnder * 6) + seed3, yWorld * (multiUnder * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (multiUnder / 2) + seed, yWorld * (multiUnder / 3)) / 1.5f);
                                            for (int f = 0; f < tilesSO.Count; f++)
                                            {
                                                if (posBlock.y > (heightCubeGen / 2) - g)
                                                {
                                                    block.pos = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0);
                                                    block.setEnabledBlock(false);
                                                    block.enabledBackBlock = false;
                                                }
                                                if (tilesSO[f].y_DepthBlock_min >= 0)
                                                {
                                                    if (posBlock.y <= (heightCubeGen / 2) - g)
                                                    {
                                                        if (posBlock.y <= (heightCubeGen / 2) - g - tilesSO[f].y_DepthBlock_min)
                                                        {
                                                            if (posBlock.y >= (heightCubeGen / 2) - g - tilesSO[f].y_DepthBlock_max)
                                                            {
                                                                if (tilesSO[f].type == SO_Block.typeBlock.def)
                                                                {
                                                                    tile = tilesSO[f].tileBlock;
                                                                }
                                                            }
                                                        }
                                                        block.tile = tile;
                                                        block.pos = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0);
                                                        if (DungeonGen < 0.6f)
                                                        {
                                                            for (int o = 0; o < stoneAltGen.Count; o++)
                                                            {
                                                                float groundAltGen = (Mathf.PerlinNoise(xWorld * groundAltTiles[o].multi + stoneAltGen[o] + 1, yWorld * groundAltTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 3) + seed2 + 2, yWorld * (groundAltTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 6) + seed3 + 3, yWorld * (groundAltTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi / 2) + seed + 4, yWorld * (groundAltTiles[o].multi / 3)) / 1.5f);
                                                                if (groundAltGen > groundAltTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                                                {
                                                                    if (posBlock.y <= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_min)
                                                                    {
                                                                        if (posBlock.y >= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_max)
                                                                        {
                                                                            tile = groundAltTiles[o].tileBlock;
                                                                            block.tile = groundAltTiles[o].tileBlock;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            for (int o = 0; o < oreGen.Count; o++)
                                                            {
                                                                float OreGen = (Mathf.PerlinNoise(xWorld * oreTiles[o].multi + oreGen[o] + 1, yWorld * oreTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi * 3) + seed2 + 2, yWorld * (oreTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi * 6) + seed3 + 3, yWorld * (oreTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (oreTiles[o].multi / 2) + seed + 4, yWorld * (oreTiles[o].multi / 3)) / 1.5f);
                                                                if (OreGen > oreTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                                                {
                                                                    if (posBlock.y <= (heightCubeGen / 2) - g - oreTiles[o].y_DepthBlock_min)
                                                                    {
                                                                        if (posBlock.y >= (heightCubeGen / 2) - g - oreTiles[o].y_DepthBlock_max)
                                                                        {
                                                                            tile = oreTiles[o].tileBlock;
                                                                            block.tile = oreTiles[o].tileBlock;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            block.setEnabledBlock(true);
                                                            block.enabledBackBlock = true;
                                                            block.tileBackBlock = tile;
                                                            block.dropBlock = tilesSO[f];
                                                            tilemaps[count].GetComponent<Tilemap>().SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), tile);
                                                        }
                                                        else
                                                        {
                                                            block.tileBackBlock = tile;
                                                            block.setEnabledBlock(false);
                                                        }
                                                        block.enabledBackBlock = true;
                                                        for (int o = 0; o < stoneAltGen.Count; o++)
                                                        {
                                                            float groundAltGen = (Mathf.PerlinNoise(xWorld * groundAltTiles[o].multi + stoneAltGen[o] + 1, yWorld * groundAltTiles[o].multi)) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 3) + seed2 + 2, yWorld * (groundAltTiles[o].multi * 3)) / 3) + (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi * 6) + seed3 + 3, yWorld * (groundAltTiles[o].multi * 3)) / 6) - (Mathf.PerlinNoise(xWorld * (groundAltTiles[o].multi / 2) + seed + 4, yWorld * (groundAltTiles[o].multi / 3)) / 1.5f);
                                                            if (groundAltGen > groundAltTiles[o].chanceBlock && posBlock.y < (heightCubeGen / 2) - g - 10)
                                                            {
                                                                if (posBlock.y <= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_min)
                                                                {
                                                                    if (posBlock.y >= (heightCubeGen / 2) - g - groundAltTiles[o].y_DepthBlock_max)
                                                                    {
                                                                        tile = groundAltTiles[o].tileBlock;
                                                                        block.tileBackBlock = groundAltTiles[o].tileBlock;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        tilemapsBack[count].GetComponent<Tilemap>().SetTile(new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2), 0), tile);
                                                    }
                                                }
                                                else
                                                {
                                                    if (posBlock.y == (heightCubeGen / 2) - g)
                                                    {
                                                        float rand = Random.Range(0f, 1f);
                                                        int rnd = Random.Range(0, treeGen.Count);
                                                        int randG = Random.Range(0, groundTiles.Count);
                                                        if (rand > treeGen[rnd].chanceBlock && DungeonGen < 0.6f)
                                                        {
                                                            Vector3Int posY = new Vector3Int(posBlock.x, posBlock.y + 1, posBlock.z);
                                                            TreeDataGen tree = new TreeDataGen();
                                                            tree.chunkIndex = count;
                                                            tree.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1, 0);
                                                            tree.posChunk = posY;
                                                            tree.tile = treeGen[rnd].ruleTileBlock;
                                                            treeDataGen.Add(tree);
                                                            for (int k = 1; k < 2 + Random.Range(0, treeGen[rnd].treeHeightMax); k++)
                                                            {
                                                                posY = new Vector3Int(posBlock.x, posBlock.y + 1 + k, posBlock.z);
                                                                tree = new TreeDataGen();
                                                                tree.chunkIndex = count;
                                                                tree.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1 + k, 0);
                                                                tree.posChunk = posY;
                                                                tree.tile = treeGen[rnd].ruleTileBlock;
                                                                treeDataGen.Add(tree);
                                                            }
                                                        }
                                                        else if (rand > groundTiles[randG].chanceBlock && DungeonGen < 0.6f)
                                                        {
                                                            Vector3Int posY = new Vector3Int(posBlock.x, posBlock.y + 1, posBlock.z);
                                                            BushDataGen bushStone = new BushDataGen();
                                                            bushStone.chunkIndex = count;
                                                            bushStone.posBlock = new Vector3Int(x - (sizeCountBlock / 2), y - (sizeCountBlock / 2) + 1, 0);
                                                            if (groundTiles[randG].ruleTileBlock != null)
                                                            {
                                                                bushStone.tileRule = groundTiles[randG].ruleTileBlock;
                                                            }
                                                            else
                                                            {
                                                                bushStone.tile = groundTiles[randG].tileBlock;
                                                            }

                                                            bushStone.posChunk = posY;
                                                            bushDataGen.Add(bushStone);
                                                        }
                                                    }
                                                }

                                            }
                                            chunks[count].blocks[countBlock] = block;
                                            countBlock++;
                                        }
                                    }
                                    yield return StartCoroutine(chunks[count].saveChunk("Chunk" + grids[count].transform.position.x.ToString() + grids[count].transform.position.y.ToString()));
                                    tilemapsCollider[count].GenerateGeometry();
                                }
                                //yield return StartCoroutine(ShadowChunkGen(i, j, count));

                                break;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < sizeCountChunk_x * sizeCountChunk_y; i++)
            {
                bChunk[i] = false;
            }
            changePosChunks = false;
            yield return StartCoroutine(generationBushStone());
            yield return StartCoroutine(generationTrees());
        }

        enb = true;
    }
    IEnumerator ShadowChunkGen(int i, int j, int countl)
    {
        Vector3Int pos = new Vector3Int((int)Mathf.Round(grids[countl].transform.position.x - (sizeCountBlock / 2)), (int)Mathf.Round(grids[countl].transform.position.y - (sizeCountBlock / 2)), 0);
        for (int x = -(sizeCountBlock / 2); x < sizeCountBlock / 2; x++)
        {
            for (int y = -(sizeCountBlock / 2); y < sizeCountBlock / 2; y++)
            {
                if (tilemapsBack[countl].GetTile(new Vector3Int(x, y, 0)))
                {
                    shdTilemap.SetTile(new Vector3Int(x - (sizeCountBlock / 2) + sizeCountBlock, y - (sizeCountBlock / 2) + sizeCountBlock, 0) + pos, shd);
                }
            }
        }
        yield return null;
    }
    IEnumerator ShadowChunkDel(int i, int j, int countl)
    {
        Vector3Int pos = new Vector3Int((int)Mathf.Round(grids[countl].transform.position.x - (sizeCountBlock / 2)), (int)Mathf.Round(grids[countl].transform.position.y - (sizeCountBlock / 2)), 0);
        for (int x = -(sizeCountBlock / 2); x < sizeCountBlock / 2; x++)
        {
            for (int y = -(sizeCountBlock / 2); y < sizeCountBlock / 2; y++)
            {
                shdTilemap.SetTile(new Vector3Int(x - (sizeCountBlock / 2) + sizeCountBlock, y - (sizeCountBlock / 2) + sizeCountBlock, 0) + pos, null);
            }
        }
        yield return null;
    }
    IEnumerator lChunkSet()
    {
        int countLChunk = 0;
        Vector3 pl = player.transform.position;
        Vector3Int pos1 = new Vector3Int((int)Mathf.Round(pl.x / sizeCountBlock - ((int)sizeCountChunk_x / 2)) * sizeCountBlock, (int)Mathf.Round(pl.y / sizeCountBlock - ((int)sizeCountChunk_y / 2)) * sizeCountBlock, 0);
        for (int l = 0; l < sizeCountChunk_x; l++)
        {
            for (int h = 0; h < sizeCountChunk_y; h++)
            {
                lChunk[countLChunk] = (new Vector3Int((l * sizeCountBlock), (h * sizeCountBlock), 0) + pos1);
                countLChunk++;
            }
        }
        yield return null;
    }
    IEnumerator generationBushStone()
    {
        if (bushDataGen.Count > 0)
        {
            ChunkData chn = null;
            for (int i = 0; i < bushDataGen.Count; i++)
            {
                chn = chunks[bushDataGen[i].chunkIndex];
                for (int l = 0; l < chn.blocks.Length; l++)
                {
                    if (chn.blocks[l].pos == bushDataGen[i].posBlock)
                    {
                        if (bushDataGen[i].tileRule != null)
                        {
                            chn.blocks[l].tile = bushDataGen[i].tileRule;
                        }
                        else
                        {
                            chn.blocks[l].tile = bushDataGen[i].tile;
                        }
                        chn.blocks[l].setEnabledBlock(true);
                        chn.blocks[l].enabledBackBlock = false;
                        chn.blocks[l].dropBlock = bushDataGen[i].block;
                        tilemaps[bushDataGen[i].chunkIndex].SetTile(chn.blocks[l].pos, chn.blocks[l].tile);
                    }
                }
                yield return StartCoroutine(chn.saveChunk(chn.nameChunk));
            }
            bushDataGen.Clear();
        }
        yield return null;
    }
    IEnumerator generationTrees()
    {
        if (treeDataGen.Count > 0)
        {
            ChunkData chn = null;
            for (int i = 0; i < treeDataGen.Count; i++)
            {
                chn = chunks[treeDataGen[i].chunkIndex];
                for (int l = 0; l < chn.blocks.Length; l++)
                {
                    if (chn.blocks[l].pos == treeDataGen[i].posBlock)
                    {
                        chn.blocks[l].tile = treeDataGen[i].tile;
                        chn.blocks[l].enabledBlock = true;
                        chn.blocks[l].enabledBackBlock = false;
                        chn.blocks[l].dropBlock = treeDataGen[i].block;
                        tilemaps[treeDataGen[i].chunkIndex].SetTile(chn.blocks[l].pos, treeDataGen[i].tile);
                    }
                }
                yield return StartCoroutine(chn.saveChunk(chn.nameChunk));
            }
            treeDataGen.Clear();
        }
        yield return null;
    }

#if UNITY_EDITOR

    public bool blockGizmoz = false;
    public bool lineGizmoz = false;
    public bool infoGizmoz = false;
    public bool chunkGizmoz = false;
    public bool chunkWorldGizmoz = false;
    public bool mouseChunk = false;



    private void OnDrawGizmos()
    {
        if (chunkGizmoz)
        {
            for (int i = 0; i < treeDataGen.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(new Vector3((int)treeDataGen[i].posChunk.x + 0.5f, (int)treeDataGen[i].posChunk.y + 0.5f, 0), Vector3Int.one);
                Gizmos.color = Color.white;
            }
            for (int i = 0; i < lChunk.Count; i++)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(lChunk[i], Vector3.one * (sizeCountBlock / 2));
                Gizmos.color = Color.white;
            }
            for (int i = 0; i < lChunkNext.Count; i++)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(lChunkNext[i], Vector3.one * (sizeCountBlock / 3));
                Gizmos.color = Color.white;
            }
        }
        if (mouseChunk)
        {
            Vector3 cam = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int pos = new Vector3Int((int)cam.x, (int)cam.y, (int)cam.z);
            Gizmos.DrawIcon(new Vector3((int)pos.x, (int)pos.y, 0)+new Vector3(0.5f,0.5f), "Chunk", false);
            /*Vector3Int chn = new Vector3Int(((int)pos.x / sizeCountBlock) * sizeCountBlock + (sizeCountBlock / 2), ((int)pos.y / sizeCountBlock) * sizeCountBlock + (sizeCountBlock / 2), 0);
            Gizmos.DrawIcon(new Vector3((int)chn.x, (int)chn.y, 0) - new Vector3(0.5f, 0.5f), "Chunk", false);*/
            UnityEditor.Handles.Label(pos + new Vector3(0, -1), pos.ToString());
            //UnityEditor.Handles.Label(pos + new Vector3(0, -2), "Chunk:" + chn.ToString());
        }
        for (int i = 0; i < grids.Count; i++)
        {
            if (chunkGizmoz)
            {
                Gizmos.DrawWireCube(grids[i].transform.position, new Vector3(sizeCountBlock, sizeCountBlock, sizeCountBlock));
                Gizmos.DrawIcon(grids[i].transform.position, "Chunk", false);
            }
            if (infoGizmoz)
            {
                UnityEditor.Handles.Label(grids[i].transform.position - transform.right, chunks[i].posChunk.ToString());
            }
            if (lineGizmoz)
            {
                Gizmos.DrawLine(player.transform.position, grids[i].transform.position);
            }
            if (infoGizmoz)
            {
                UnityEditor.Handles.Label(grids[i].transform.position + transform.up * 2 - transform.right, chunks[i].nameChunk);
                UnityEditor.Handles.Label(grids[i].transform.position + transform.up - transform.right, "Distance:" + (int)Vector3.Distance(grids[i].transform.position, player.transform.position));
            }
            if (blockGizmoz)
            {
                for (int x = 0; x < sizeCountBlock * sizeCountBlock; x++)
                {
                    if (infoGizmoz)
                    {
                        GUIStyle st = new GUIStyle();
                        st.fontSize = 8;
                    }
                }
            }
        }
        if (chunkWorldGizmoz)
        {
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    Gizmos.DrawWireCube(new Vector3(i * sizeCountBlock + sizeCountBlock, j * sizeCountBlock + sizeCountBlock), new Vector3(sizeCountBlock, sizeCountBlock, sizeCountBlock));
                }
            }
        }
    }
#endif

}
