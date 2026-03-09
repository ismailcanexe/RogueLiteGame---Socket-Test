using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EasyTextEffects;

public class GameManager : MonoBehaviour
{
    [Header("Stats")]
    public float puan;
    public float can;
    public float UpgradePuan;
    public float UpgradePuanArtis;
    public int secimsayisi = 0;

    public float dropTime;
    public float difficulty = 1;

    int scaleLevel = 0;
    int speedLevel = 0;

    [SerializeField]int[] potID;
    bool Olumsuz = false;


    [Header("Game")]
    public GameObject Yem;
    public Transform Pos1, Pos2;
    public ChickenController player;

    public GameObject[] SkillGameobjects;
    public GameObject[] Potions;
    [HideInInspector] public string playerName;


    int combo = 0;
    [Header("UI")]
    public TMP_Text puanTxt;

    public GameObject killButton;
    public Slider healthBar;
    public Slider extraHealthBar;
    public Slider DiffBar;

    public GameObject StatsCanvas;
    public TMP_Text StatsTXT;

    public GameObject endCanvas;
    public TMP_Text endtxt;
    public GameObject upgradeCanvas;
    Coroutine undeadCoroutine;
    [SerializeField] TMP_Text[] PotNumbers;
    WebSocketManager Socket;
    TextEffect puantexteffect;
    private void Awake()
    {
        puantexteffect = puanTxt.GetComponent<TextEffect>();
       
        playerName = PlayerPrefs.GetString("Username");
        Socket = GetComponent<WebSocketManager>(); 

        Time.timeScale = 1f;
    }



    public void kill()
    {
        can = 0;
        Setuper();
    }
        
    void Start()
    {

        StartCoroutine(YemDongu());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StatsCanvas.SetActive(!StatsCanvas.activeSelf);
        }
    }
    IEnumerator YemDongu()
    {
        while (true)
        {
            float YemSpawnX = Random.Range(Pos1.position.x, Pos2.position.x);
            float YemSpawnY = Random.Range(Pos1.position.y, Pos2.position.y);

            Instantiate(Yem, new Vector3(YemSpawnX, YemSpawnY, 0), Quaternion.identity);

            Setuper();         
            float diffedDropTime = dropTime / difficulty;

            yield return new WaitForSeconds(diffedDropTime);

            difficulty += 0.04f;
            Setuper();
            DiffBar.value = difficulty;
        }
    }
    [System.Serializable]
    public class RekorVerisi
    {
        public string oyuncu_adi; 
        public int skor;
    }

    
    IEnumerator ServeraKaydet()
    {
        string adres = "http://localhost:3000/rekor-kaydet";

        WWWForm form = new WWWForm();
        form.AddField("oyuncu_adi", playerName);
        form.AddField("skor", (int)puan);

        using (UnityWebRequest www = UnityWebRequest.Post(adres, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Hata: " + www.error);
            }
            else
            {
                Debug.Log("Sunucudan gelen cevap: " + www.downloadHandler.text);
            }
        }
    }

    void CheckUpgradeEarn()
    {
        while (puan >= UpgradePuan)
        {
            secimsayisi++;

            UpgradePuan += UpgradePuanArtis;

            UpgradePuanArtis = UpgradePuanArtis * 1.05f;
        }
    }

    void TryOpenUpgrade()
    {
        if (secimsayisi > 0 && !upgradeCanvas.activeSelf)
        {
            secimsayisi--;

            foreach (GameObject item in SkillGameobjects)
                item.SetActive(false);

            List<int> numbers = new List<int>();
            for (int i = 0; i < SkillGameobjects.Length; i++)
                numbers.Add(i);

            for (int i = 0; i < numbers.Count; i++)
            {
                int randomIndex = Random.Range(i, numbers.Count);
                int temp = numbers[i];
                numbers[i] = numbers[randomIndex];
                numbers[randomIndex] = temp;
            }

            List<GameObject> activeSkills = new List<GameObject>();
            for (int i = 0; i < 3; i++)
            {
                GameObject skill = SkillGameobjects[numbers[i]];
                skill.SetActive(true);
                activeSkills.Add(skill);
            }

            for (int i = 0; i < activeSkills.Count; i++)
            {
                int randIndex = Random.Range(i, activeSkills.Count);
                activeSkills[randIndex].transform.SetSiblingIndex(i);
            }

            upgradeCanvas.SetActive(true);
            Time.timeScale = 0;
        }
    }

    void Setuper()
    {
        difficulty = Mathf.Clamp(difficulty, 0.5f, 5f);
        DiffBar.value = difficulty;
        healthBar.value = can;
        extraHealthBar.value = can - 100;
        puanTxt.text = ((int)puan).ToString();   
        StatsTXT.text =
        $"Can:{can:G}\r\n" +
        $"Zorluk:{difficulty:F2}\r\n" +
        $"Hız:{player.speed:F2}\r\n" +
        $"Hız Çarpan:{player.speedOran:F2}\r\n" +
        $"Boyut:{player.gameObject.transform.localScale.x:F2}\r\n" +
        $"Boyut Çarpan:{player.buyumeOrani:F2}\r\n" +
        $"Sonraki Geliştirme Puanı:{UpgradePuan:F2}\r\n" +
        $"Puan Artış Çarpanı:{player.puanOran:G}";

        for (int i = 0; i < potID.Length; i++)
        {
            PotNumbers[i].text = potID[i]+"";

        }
        if (puan > 10000) { killButton.SetActive(true); }

        if (combo >= 5)
        {
            puantexteffect.Refresh();
            puantexteffect.enabled = true;
        }
        else
        {
            puantexteffect.enabled = false;

        }
        if (can <= 0)
        {
            Socket.AktifiSil(playerName);
            StartCoroutine(ServeraKaydet());
            killButton.SetActive(false);
            upgradeCanvas.SetActive(false);
            endCanvas.SetActive(true);
            player.gameObject.SetActive(false);
            string endText = "";

            if (puan < 30)
                endText = "Gıdak! İlk yemlerini toplamayı başardın. Daha yolun başındasın ama kanat çırpmayı öğrendin!";
            else if (puan < 100)
                endText = "Harika! Karnın biraz daha doldu. Reflekslerin gelişiyor, kümeste adın duyulmaya başladı...";
            else if (puan < 500)
                endText = "Süper performans! Yağmur gibi yağan yemleri ustalıkla topladın. Artık bu işin hakkını veriyorsun!";
            else if (puan < 800)
                endText = "Muhteşem! Hız, dikkat ve strateji bir arada! Rakiplerine tüy bile bırakmadın!";
            else
                endText = "EFSANE! Gökyüzünden ne yağarsa yağsın, hepsini topladın! Kümeste bir efsane doğdu";
            endtxt.text = endText;
            Time.timeScale = 0;
        }
    
}

    private void FixedUpdate()
    {
        float diffX = (2.5f * difficulty);
        diffX = Mathf.Clamp(diffX, 5f, 8.5f);

        Pos1.transform.position = new Vector3(diffX, 5, 0);
        Pos2.transform.position = new Vector3(-diffX, 5, 0);
    }
    public void PuanArt(float artis, bool altinyem)
    {
        if (altinyem)
        {
            difficulty -= 0.1f;
            Setuper();
            can += 5;
            healthBar.value = can;
            extraHealthBar.value = can - 100;
        }
        combo++;
        puan += artis;
        Setuper() ;
        CheckUpgradeEarn();
        TryOpenUpgrade();
        Socket.PuanGonder(playerName,(int)puan);
    }

    public void MakeUpgrade(int upgradeid)
    {
        Time.timeScale = 1;
        switch (upgradeid)
        {
            case 0:
                scaleLevel++;
                player.transform.localScale =
                    player.anaBoyut + (player.anaBoyut * player.buyumeOrani * scaleLevel);
                break;

            case 1:
                speedLevel++;
                player.speed =
                    player.anaSpeed + (player.anaSpeed * player.speedOran * speedLevel);
                break;

            case 2:
                difficulty -= 0.75f;
                Setuper();
                break;

            case 3:
                can += (20 * (player.buyumeOrani + 1));
                Setuper();   
                break;

            case 4:
                int rand = Random.Range(0, 4);
                if (rand != 1)
                {         
                    
                    difficulty += 1f;
                    CanAzalt();
                }
                player.puanOran+= 0.5f;
                player.puanOran *= 1.1f;
                

                Setuper();
                break;

            case 5:
                PuanArt(5*player.puanOran, false);
                Setuper();                CheckUpgradeEarn();
                break;

            case 6:
                player.buyumeOrani += 0.05f;
                player.transform.localScale =
                    player.anaBoyut + (player.anaBoyut * player.buyumeOrani * scaleLevel);
                break;

            case 7:
                player.speedOran += 0.05f;
                player.speed =
                    player.anaSpeed + (player.anaSpeed * player.speedOran * speedLevel);
                break;
            case 8:
                potID[0]++; PotNumbers[0].text = potID[0].ToString(); Potions[0].SetActive(true);
            break;
                case 9:
                potID[1]++; PotNumbers[1].text = potID[1].ToString(); Potions[1].SetActive(true);
                break;
                case 10:
                potID[2]++; PotNumbers[2].text = potID[2].ToString(); Potions[2].SetActive(true);
                break;
                case 11:
                potID[3]++; PotNumbers[3].text = potID[3].ToString(); Potions[3].SetActive(true);
                break;
        }

        upgradeCanvas.SetActive(false);


        TryOpenUpgrade(); 
    }
    float phCarpan = 1;

    private void OnApplicationQuit()
    {
        Socket.AktifiSil(playerName);
    }

    public void CanAzalt()
    {
        if (Olumsuz) { }
        else
        {



            if (puan < 1000)
                phCarpan = 1;
            else if (puan < 10000) phCarpan = 1.5f;
            else if (puan < 50000) phCarpan = 2f;
            else phCarpan = 3;


            difficulty += 0.05f;
            combo = 0;
            float hasar = 10 * ((difficulty/1.5f) * phCarpan);
            can -= hasar;


            Setuper();

        }
    }



    public void YenidenBasla(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void iksir(int potnumber)
    {
        if (potID[potnumber] > 0)
        {
        switch (potnumber)
        {
            case 0:
                    can += (20 * (player.buyumeOrani + 1)); 
                    potID[0]--; 
                    Setuper();
                    if (potID[0] <= 0) Potions[0].SetActive(false); 

                    break;
            case 1:
                difficulty -= 0.30f;
                    potID[1]--; Setuper();
                    if (potID[1] <= 0) 
                        Potions[1].SetActive(false);

                    break;
                case 2:
                    potID[2]--;

                    if (undeadCoroutine != null)
                        StopCoroutine(undeadCoroutine);

                    undeadCoroutine = StartCoroutine(Sureliiksir(2, 5f));
                        
                    Setuper();
                    if (potID[2] <= 0) Potions[2].SetActive(false);
                    break;
                case 3:
                    difficulty += 0.25f; 
                    potID[3]--; 
                    StartCoroutine(Sureliiksir(3, 15f));
                    Setuper();
                    if (potID[3] <= 0) Potions[3].SetActive(false);

                    break;

            default:
                break;
        }
        }
    }

    IEnumerator Sureliiksir(int potnumber,float sure)
    {

        switch (potnumber)
        {
            case 2: Olumsuz = true; player.undeadEffect.SetActive(true);
                break;
            case 3:
            player.puanOran++;
            break;
        default:
            break;
        }

        yield return new WaitForSeconds(sure);
        switch (potnumber)
        {
            case 2:
                Olumsuz = false; player.undeadEffect.SetActive(false); undeadCoroutine = null;

                break;
            case 3:
                player.puanOran--;
                break;
            default:
                break;
        }
    }
}