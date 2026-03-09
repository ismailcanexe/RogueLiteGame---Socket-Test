using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropObject : MonoBehaviour
{
    public Sprite[] sprites;
    GameManager manager;
    SpriteRenderer sr;
    public float kacpuan = 1;
    public bool altinyem = false;
    public bool bozukyem = false;
    private void Start()
    {

        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        sr = GetComponent<SpriteRenderer>();
        int rand = Random.Range(0, sprites.Length);
        sr.sprite = sprites[rand];

        int yellowrand = Random.Range(0, 10);
        if (yellowrand==3)
        {
            kacpuan = 2;
            altinyem = true;
            sr.color = Color.yellow;
        }
        if (yellowrand == 4 )
        {
            sr.color = Color.black;
            bozukyem = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("yer"))
        {
            if (bozukyem)
            {
                Destroy(gameObject, 0.2f);
            }

            else
            StartCoroutine(YereDegdi());
        }   
    }
    IEnumerator YereDegdi()
    {

        yield return new WaitForSeconds(1f);
        manager.CanAzalt();
        Destroy(gameObject);
        
    }
}
