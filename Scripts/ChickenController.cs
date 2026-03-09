using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChickenController : MonoBehaviour
{
    public float speed;
    [SerializeField] AudioSource yemeSes; [SerializeField] AudioSource tavukSes; 
    public Joystick MoveJoy;  
    float HorizontalX;
    public Animator anim;
    public SpriteRenderer tavukrend;
    public GameManager gm;
    [ HideInInspector]  public Vector3 anaBoyut;
   [HideInInspector] public float anaSpeed;
    public float buyumeOrani = 0.10f;
    public float speedOran = 0.10f;
    public float puanOran = 1; Rigidbody2D rb;
    public GameObject undeadEffect;
    public void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        string controller = PlayerPrefs.GetString("Controller");
        if (controller != "joystick")
        {
            Destroy(MoveJoy.gameObject);
        }

        anaSpeed = speed;
        anaBoyut = transform.localScale;
        StartCoroutine(ChickenSound());
    }


    IEnumerator ChickenSound()
    {

        while (true)
        {

            float rand = Random.Range(5, 20);
            yield return new WaitForSeconds(rand);
            tavukSes.Play();
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        string controller = PlayerPrefs.GetString("Controller");
        if (controller == "joystick")
        {
            HorizontalX = MoveJoy.Horizontal;

        }
        else
        {
            HorizontalX = Input.GetAxis("Horizontal");
        }
        if (HorizontalX >= 0.3f || HorizontalX <= -0.3f)
        {
            anim.SetBool("Walk", true);
        }
        else
        {
            anim.SetBool("Walk", false);
        }
        if (HorizontalX >= 0.15f)
        {
            tavukrend.flipX = false;
        }
        else if (HorizontalX <= -0.15f)
        {
            tavukrend.flipX = true;

        }
    }


    void FixedUpdate()
    {

        rb.velocity = new Vector2(HorizontalX * speed, rb.velocity.y);

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Yem"))
        {
            yemeSes.Play();
            DropObject Drop  = collision.collider.GetComponent<DropObject>();
            if (Drop.bozukyem)
            {
                gm.CanAzalt();
            }
            else
            {
                gm.PuanArt(collision.collider.GetComponent<DropObject>().kacpuan * puanOran,collision.collider.GetComponent<DropObject>().altinyem);

            }

            Destroy(collision.collider.gameObject);
        }
    }

}
