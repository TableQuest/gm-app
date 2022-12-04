using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;
public class CharacterListener : MonoBehaviour
{

    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    public GameObject characterPanelPrefab;

    // Start is called before the first frame update
    private IEnumerator Start()
    {
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        // Wait until a callback action is added to the queue
        yield return new WaitUntil(() => _mainThreadhActions.Count > 0);

        // If this fails something is wrong ^^
        // simply get the first added callback
        if (!_mainThreadhActions.TryDequeue(out var action))
        {
            Debug.LogError("Something Went Wrong ! ", this);
            yield break;
        }

        // Execute the code of the added callback
        action?.Invoke();
    }

    void SocketThread()
    {
        while (client == null)
        {

            Debug.Log("Client null");
            initialisationClient();
            Thread.Sleep(500);
        }

        client.On("characterSelection", (data) =>

        {
            System.Text.Json.JsonElement playerJson = data.GetValue(0);
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _mainThreadhActions.Enqueue(() =>
            {
                // This will be executed after the next Update call
                PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(data.GetValue(0).ToString());
                CharacterInfo characterInfo = playerInfo.character;

                addCharacterPanel(characterInfo);
            });
        });

        client.On("updateLifeCharacter", (data) =>
        {
            System.Text.Json.JsonElement playerJson = data.GetValue(0);
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _mainThreadhActions.Enqueue(() =>
            {
                // This will be executed after the next Update call
                UpdateLife updateLife = JsonUtility.FromJson<UpdateLife>(data.GetValue(0).ToString());

                updateLifeCharacter(updateLife);
            });
        });


    }

    private void initialisationClient()
    {
        client = initClient.client;
    }

    public void updateLifeCharacter(UpdateLife updateLife)
    {

        // recuperer panel du joueur
        // changer life dans panel 

    }

    public void addCharacterPanel(CharacterInfo characterInfo)
    {
        // Create the panel 
        GameObject characterPanel = Instantiate(characterPanelPrefab);
        characterPanel.transform.position = gameObject.transform.position;
        characterPanel.GetComponent<RectTransform>().SetParent(gameObject.transform);

        // Set the informations about the player 
        characterPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text=characterInfo.name;
        TextMeshProUGUI lifeText = characterPanel.transform.Find("LifeValue").GetComponent<TextMeshProUGUI>();
        lifeText.text =characterInfo.life.ToString();
        characterPanel.transform.Find("LifeMax").GetComponent<TextMeshProUGUI>().text =characterInfo.lifeMax.ToString();
        characterPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>().text =characterInfo.description;

        // Set the action of the button 
        var removeLifeButton = characterPanel.transform.Find("ButtonRemoveLife").GetComponent<Button>() as Button;
        removeLifeButton.onClick.AddListener(delegate { sendRemoveLife(lifeText, characterInfo.id, characterInfo.life); });
    }

    private async void sendRemoveLife(TextMeshProUGUI lifeText, int id, int life)
    {
        lifeText.text = (life -10).ToString();
        UpdateLife up = new UpdateLife() { id = id, life = life -10};
        string json = JsonUtility.ToJson(up);
        Debug.Log(json);
        await client.EmitAsync("updateLifeCharacter",json);
        Debug.Log("remove 10 point of life");
    }
}


[Serializable]
public class PlayerInfo
{
    public string player;
    public CharacterInfo character;
}

[Serializable]
public class CharacterInfo
{
    public int id;
    public string name;
    public int lifeMax;
    public int life;
    public string description;
}

[Serializable]
public class UpdateLife
{
    public int id;
    public int life;
}