using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : MonoBehaviour {

    public GameObject tail;
    public GameObject brick;
    private Transform rBorder;
    private Transform lBorder;
    private Transform tBorder;
    private Transform bBorder;
    public GameObject tailburst;
    private GameManager gameManager;
    private PlayerManager playerManager; 



    

    private List<GameObject> tailSections = new List<GameObject>();
    private List<GameObject> tailBurstParticles = new List<GameObject>();
    private List<GameObject> brickList = new List<GameObject>();
    private bool vertical = false;
    private bool horizontal = true;
    private bool eat = false;
    private bool dead = false; 
    private float speed = 0.020f;
	Vector2 vector = Vector2.up;
	Vector2 moveVector;

    // Use this for initialization
    void Start () {

        playerManager = GameObject.FindObjectOfType<PlayerManager>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        rBorder = GameObject.Find("border-right").transform;
        lBorder = GameObject.Find("border-left").transform;
        tBorder = GameObject.Find("border-top").transform;
        bBorder = GameObject.Find("border-bottom").transform;
        
        InvokeRepeating("Movement", 0.1f, speed);
        SpawnBricks();

    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.RightArrow) && horizontal) {
            horizontal = false;
            vertical = true;
            vector = Vector2.right;
        } else if (Input.GetKey(KeyCode.UpArrow) &&  vertical) {
            horizontal = true;
            vertical = false;
            vector = Vector2.up;
        } else if (Input.GetKey(KeyCode.DownArrow) && vertical) {
            horizontal = true;
            vertical = false;
            vector = -Vector2.up;
        } else if (Input.GetKey(KeyCode.LeftArrow) && horizontal) {
            horizontal = false;
            vertical = true;
            vector = -Vector2.right;
        }
        moveVector = vector / 20f;

    }

    void Movement()
    {
        if (!dead)
        {
            Vector3 ta = transform.position;
            if (eat)
            {
                if (speed > 0.002)
                {
                    speed = speed - 0.002f;
                }


                GameObject g = (GameObject)Instantiate(tail, ta, Quaternion.identity);
                tailSections.Insert(0, g);
                //Debug.Log(speed);
                eat = false;
            }
            else if (tailSections.Count > 0)
            {
                tailSections.Last().transform.position = ta;
                tailSections.Insert(0, tailSections.Last());
                tailSections.RemoveAt(tailSections.Count - 1);
            }

            transform.Translate(moveVector);//* Time.deltaTime);
        }
    }
    void OnTriggerEnter(Collider c)
    {

        if (c.name.StartsWith("brick"))
        {
            eat = true;
            EatFood(c.gameObject);
        }
        else if(c.name.StartsWith("border"))
        {
            dead = true;
            Debug.Log("Boom");
            foreach(GameObject t in tailSections)
            {
                GameObject p = (GameObject)Instantiate(tailburst, t.transform.position, Quaternion.identity);
                tailBurstParticles.Add(p);
                Destroy(t); // Destroy tail sections
            }

            StartCoroutine(DeathOnTimer());
            
        }
    }
    private IEnumerator DeathOnTimer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            
            CleanUpFood();
            gameManager.PlayerDied();
            Destroy(gameObject);
        }
    }

    private void EatFood(GameObject f)
    {
        foreach (GameObject food in brickList)
        {
            if (food == f)
            {
                brickList.Remove(food);
                Destroy(f);
                Debug.Log("Munch");
                break;
            }
        }

        if (brickList.Count == 0)
        {
            gameManager.LevelUp();
            SpawnBricks();

        }
        playerManager.SetBricksRemaining(brickList.Count);


    }
    public void SpawnBricks()
    {
        int n = gameManager.brickSpawn;

        for (int i = 0; i < n; i++)
        {
            float x = (float)Random.Range(lBorder.position.x+1, rBorder.position.x-1);
            float y = (float)Random.Range(bBorder.position.y+1, tBorder.position.y-1);
            GameObject f = Instantiate(brick, new Vector3(x, y, -1), Quaternion.identity);
            brickList.Add(f);
        }
        gameManager.brickLevelOveride = 0; 
        playerManager.SetBricksRemaining(brickList.Count);
    }

    public void CleanUpFood()
    {
        foreach(GameObject food in brickList)
        {
            Destroy(food);
            playerManager.SetBricksRemaining(brickList.Count);
        }
    }

}
