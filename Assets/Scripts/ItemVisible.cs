using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemVisible : MonoBehaviour
{
    int sizeCountBlock, sizeCountChunk_x, sizeCountChunk_y;
    public SettingsGeneration settingsGen;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Collect")
        {
            GetComponent<Rigidbody2D>().isKinematic = true;
            transform.position = Vector2.MoveTowards(transform.position, other.transform.position, Vector2.Distance(transform.position, other.transform.position) * Time.deltaTime);
        }
        if (other.tag == "Player")
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        GetComponent<Rigidbody2D>().isKinematic = false;
    }
    void Start()
    {
        sizeCountBlock = settingsGen.sizeCountBlock;
        sizeCountChunk_x = settingsGen.sizeCountChunk_x;
        sizeCountChunk_y = settingsGen.sizeCountChunk_y;
    }
    private void OnBecameVisible()
    {
        StopAllCoroutines();
        Debug.Log("Visible");
        Vector3 cam = transform.position;
        Vector3Int pos = new Vector3Int((int)Mathf.Round(cam.x), (int)Mathf.Round(cam.y), (int)Mathf.Round(cam.z));
        Vector3Int pos1 = new Vector3Int((int)Mathf.Round(cam.x / sizeCountBlock - ((int)sizeCountChunk_x)) * sizeCountBlock, (int)Mathf.Round(cam.y / sizeCountBlock - ((int)sizeCountChunk_y)) * sizeCountBlock, 0);
        Vector3Int chn = new Vector3Int(((int)pos.x / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), ((int)pos.y / sizeCountBlock) * sizeCountBlock + (sizeCountBlock), 0);
        GameObject[] gmjs = GameObject.FindGameObjectsWithTag("Chunk");
        for (int i = 0; i < gmjs.Length; i++)
        {
            if (gmjs[i].name == "Grid " + chn.x + chn.y)
            {
                GetComponent<BoxCollider2D>().enabled = true;
                GetComponent<Rigidbody2D>().simulated = true;
            }
        }
    }
    private void OnBecameInvisible()
    {
        Debug.Log("Invisible");
        if (this.gameObject.activeSelf)
        {
            StartCoroutine(DestroyObj());
        }

        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
    }
    IEnumerator DestroyObj()
    {
        yield return new WaitForSeconds(10f);
        Destroy(this.gameObject);
    }
}
