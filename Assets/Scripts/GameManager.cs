using System.Collections;
using System.Collections.Generic;
using System.Text; 
using UnityEngine;
using UnityEngine.UI;
using DeltaDNA;


public class GameManager : MonoBehaviour {

    private string sharedUserID;
    public PlayerManager player;
    public GameObject ballPrefab;
    private Ball ball; 
    private GameConsole console; 
    public Text txtStart;
    public Text txtGameOver;
    public Button bttnStart;
    public InputField infldCrossGameUserID; 


    public List<int?> bricksPerLevel = new List<int?>() { 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, 38, 40, 42, 44, 46, 48 };
    const int DEFAULT_BRICK_SPAWN = 6;
    public int brickSpawn ;
    public int brickLevelOveride = 0;

    // Start Button Size and Color
    private Color sourceColor;
    private Color targetColor;
    private Vector3 InitialScale;
    private Vector3 FinalScale;
    bool readyToStart = false; 




    private void Start()
    {
        // These are for pulsing the start button size and alpha 
        InitialScale = transform.localScale;
        FinalScale = new Vector3(InitialScale.x + 0.04f,
                                 InitialScale.y + 0.04f,
                                 InitialScale.z);
        sourceColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        targetColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        DdnaPlayerConfig();

        txtStart.gameObject.SetActive(true);
        bttnStart.gameObject.SetActive(true);
        readyToStart = true;

        console = GameObject.FindObjectOfType<GameConsole>();
    }

    private void Update()
    {
        if (readyToStart)
        {
            // Pulse the start button size and alpha
            bttnStart.image.color = Color.Lerp(sourceColor, targetColor, Mathf.PingPong(Time.time, 1.2f));
            bttnStart.transform.localScale = Vector3.Lerp(InitialScale, FinalScale, Mathf.PingPong(Time.time, 1.2f));

        }
    }
    private void DdnaPlayerConfig()
    {
        // Use deviceID in this simple cross promo example. 
        sharedUserID = SystemInfo.deviceUniqueIdentifier;
        
       
        if (sharedUserID == "n/a" || !string.IsNullOrEmpty(PlayerPrefs.GetString("SharedUserID")))
        {
            // WebGL and some other platforms don't support SystemInfo.deviceUniqueIdentifier;
            // Under normal circumstances you would use your own login system or a social identifier
            // but for demo simplicity we'll just generate a key that you can manually enter in to multiple games.
            // and store it in player prefs

            // check player prefs
            string s = PlayerPrefs.GetString("SharedUserID");
            if (string.IsNullOrEmpty(s))
            {
                // generate a new code                
                s = GenerateCode(5);
                // store new code
                PlayerPrefs.SetString("SharedUserID", s);                
            }
            sharedUserID = s; 
        }

        DDNA.Instance.CrossGameUserID = sharedUserID;
        Debug.Log("Cross Game userID (deviceID) = " + DDNA.Instance.CrossGameUserID);
        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);
        DDNA.Instance.ClientVersion = Application.version;
       
        DDNA.Instance.StartSDK();


    }
    public void SetCustomCrossPormoUserID()
    {
        
        if (!string.IsNullOrEmpty(infldCrossGameUserID.text))
        {
            DDNA.Instance.CrossGameUserID = infldCrossGameUserID.text;
            PlayerPrefs.SetString("SharedUserID", DDNA.Instance.CrossGameUserID);
            console.UpdateConsole();
        }
    }

    private string GenerateCode(int n)
    {
        string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        StringBuilder result = new StringBuilder(n);
        for (int i = 0; i < n; i++)
        {
            result.Append(characters[Random.Range(0, characters.Length-1)]);
        }
        return result.ToString();
    }

    public void StartLevel(int levelNo)
    {
        // Player starts level
        player.SetLevel(1);
        brickSpawn = GetFoodSpawn(player.playerLevel);

        player.UpdatePlayerStatistics();

        txtGameOver.gameObject.SetActive(false);
        txtStart.gameObject.SetActive(false);
        bttnStart.gameObject.SetActive(false);
        readyToStart = false;

        // Spawn new Ball 
        Vector3 pos = new Vector3(0, 0, -1);
        ball = Instantiate(ballPrefab, pos, Quaternion.identity).GetComponent<Ball>();

        MissionStarted();
        
    }
    public void PlayerDied()
    {
        MissionFailed();

        txtGameOver.gameObject.SetActive(true);
        txtStart.gameObject.SetActive(true);
        bttnStart.gameObject.SetActive(true);
        readyToStart = true; 

    }
    public void LevelUp()
    {
        MissionCompleted();

        player.playerLevel++;
        
        Debug.Log("Level Up - playerLevel " + player.playerLevel);

        DDNA.Instance.RecordEvent(new GameEvent("levelUp")
            .AddParam("levelUpName",player.playerLevel.ToString())
            .AddParam("userLevel",player.playerLevel)
            .AddParam("coinBalance", player.playerCoins))
            .Add(new GameParametersHandler(gameParameters => {
                gameParametersHandler(gameParameters);
                }))
            .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
                imageMessageHandler(imageMessage);
            }))
            .Run();

        player.UpdatePlayerStatistics();

        brickSpawn = GetFoodSpawn(player.playerLevel);
        MissionStarted();
    }

    public void MissionStarted()
    {
        DDNA.Instance.RecordEvent(new GameEvent("missionStarted")
            .AddParam("missionName", "Mission " + player.playerLevel.ToString("D3"))
            .AddParam("missionID", player.playerLevel.ToString("D3"))
            .AddParam("userLevel", player.playerLevel)
            .AddParam("isTutorial", false)
            .AddParam("coinBalance", player.playerCoins)
            .AddParam("bricks",brickSpawn))
        .Add(new GameParametersHandler(gameParameters => {
            gameParametersHandler(gameParameters);
        }))
        .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
            imageMessageHandler(imageMessage);
        }))
        .Run();
    }

    public void MissionCompleted()
    {
        DDNA.Instance.RecordEvent(new GameEvent("missionCompleted")
            .AddParam("missionName", "Mission " + player.playerLevel.ToString("D3"))
            .AddParam("missionID", player.playerLevel.ToString("D3"))
            .AddParam("isTutorial", false)
            .AddParam("userLevel", player.playerLevel)
            .AddParam("coinBalance", player.playerCoins)
            .AddParam("bricks", brickSpawn))
        .Add(new GameParametersHandler(gameParameters => {
            gameParametersHandler(gameParameters);
        }))
        .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
            imageMessageHandler(imageMessage);
        }))
        .Run();
    }

    public void MissionFailed()
    {
        DDNA.Instance.RecordEvent(new GameEvent("missionFailed")
            .AddParam("missionName", "Mission " + player.playerLevel.ToString("D3"))
            .AddParam("missionID", player.playerLevel.ToString("D3"))
            .AddParam("userLevel", player.playerLevel)
            .AddParam("isTutorial", false)
            .AddParam("coinBalance", player.playerCoins)
            .AddParam("bricks", brickSpawn)
            .AddParam("bricksRemaining", player.bricksRemaining))
        .Add(new GameParametersHandler(gameParameters => {
            gameParametersHandler(gameParameters);
        }))
        .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
            imageMessageHandler(imageMessage);
        }))
        .Run();
    }

    public void ModifierApplied(string modifierType, int modifierAmount)
    {
        DDNA.Instance.RecordEvent(new GameEvent("modifierApplied")
            .AddParam("modifierType", modifierType)
            .AddParam("modifierAmount", modifierAmount)
            .AddParam("userLevel", player.playerLevel)
            .AddParam("coinBalance", player.playerCoins))
        .Add(new GameParametersHandler(gameParameters => {
            gameParametersHandler(gameParameters);
        }))
        .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
            imageMessageHandler(imageMessage);
        }))
        .Run();

    }

    public void RewardReceived(string rewardType, int rewardAmount)
    {
        DDNA.Instance.RecordEvent(new GameEvent("rewardReceived")
            .AddParam("rewardType", rewardType)
            .AddParam("rewardAmount", rewardAmount)
            .AddParam("userLevel", player.playerLevel)            
            .AddParam("coinBalance", player.playerCoins))
        .Add(new GameParametersHandler(gameParameters => {
            gameParametersHandler(gameParameters);
        }))
        .Add(new ImageMessageHandler(DDNA.Instance, imageMessage => {
            imageMessageHandler(imageMessage);
        }))
        .Run();
    }
    private void gameParametersHandler(Dictionary<string,object> gameParameters)
    {
        Debug.Log("Received GameParameters from event triggered campaign : " + DeltaDNA.MiniJSON.Json.Serialize(gameParameters));

        if(gameParameters.ContainsKey("coins"))
        {
            player.SetCoins(player.playerCoins + System.Convert.ToInt32(gameParameters["coins"]));
            RewardReceived("coins", System.Convert.ToInt32(gameParameters["coins"]));
        }
        
        if (gameParameters.ContainsKey("bricks"))
        {
            brickLevelOveride = System.Convert.ToInt32(gameParameters["bricks"]);
            ModifierApplied("bricks", System.Convert.ToInt32(gameParameters["bricks"]));
        }
    }
    private void imageMessageHandler(ImageMessage imageMessage)
    {
        Debug.Log("Received ImageMessage from event triggered campaign");
        imageMessage.OnDismiss += (ImageMessage.EventArgs obj) =>
        {
            Debug.Log("Image Message dismissed by " + obj.ID);
        };

        imageMessage.OnAction += (ImageMessage.EventArgs obj) =>
        {
            Debug.Log("Image Message Actioned by " + obj.ID);
            if (imageMessage.Parameters != null)
            {
                gameParametersHandler(imageMessage.Parameters);
            }
        };

        imageMessage.Show();
        
    }

    public void NewSession()
    {
        DDNA.Instance.NewSession();
        console.UpdateConsole();
    }

    public int GetFoodSpawn(int level)
    {
        int n = DEFAULT_BRICK_SPAWN;

        if (brickLevelOveride > 0)
        {
            n = brickLevelOveride;
        }
        else if (bricksPerLevel.Count > player.playerLevel && bricksPerLevel[player.playerLevel - 1] != null)
        {
            n = (int)bricksPerLevel[player.playerLevel - 1];
        }

        return n;
    }
}
