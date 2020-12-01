using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParallaxBackground : MonoBehaviour
{
    public List<Transform> back = new List<Transform>();
    public float speed,v_speed;
    float h_inp, v_inp;
    public Transform hero;
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        for (int i = 0; i < back.Count; i++)
        {
            /*if (20 < back[i].transform.localPosition.x)
            {
                back[i].transform.localPosition = hero.transform.localPosition + new Vector3(-11f * 3, 0);
            }*/
            if (hero.transform.localPosition.x+Camera.main.pixelWidth > back[i].transform.localPosition.x)
            {
                back[i].transform.localPosition = hero.transform.localPosition + new Vector3(100*3, 0);
            }
            
            back[i].transform.localPosition = back[i].transform.localPosition + (transform.right * h_inp) + (transform.up * v_inp);
        }
    }
    // Update is called once per frame
    void Update()
    {
        InputManager();
    }
    void InputManager()
    {
        h_inp = Input.GetAxis("Horizontal") * speed;

        for (int i = 0; i < back.Count; i++)
        {
            /*if (back[i].transform.localPosition.y >= (300 / 2))
            {
                v_inp = Input.GetAxis("Vertical") * v_speed;
            }else{
                v_inp = 0;
                back[i].transform.localPosition = new Vector3(back[i].transform.localPosition.x, (300 / 2));
            }*/
        }
    }
}
