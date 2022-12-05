using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CharacterListener : MonoBehaviour
{

    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    public GameObject characterPanelPrefab;

    Datas initData;
    [SerializeField] GameObject dataObject;
    List<Character> datas;
    

    // Start is called before the first frame update
    private void Start()
    {
        initClient = clientObject.GetComponent<InitiatlisationClient>();
        initData = dataObject.GetComponent<Datas>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        StartCoroutine(myUpdate());

    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
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
    }

    private void Update()
    {

    }


    void SocketThread()
    {
        while (client == null)
        {

            Debug.Log("Client null");
            initialisationClient();
            datas = initData.characterslList;  
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

                PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(playerJson.ToString());
                CharacterInfo characterInfo = playerInfo.character;
                Character character = new(characterInfo.id, characterInfo.name, characterInfo.life, characterInfo.lifeMax, characterInfo.description);
                datas.Add(character);
                addCharacterPanel(character);
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
        // récuperer le character dans les data
        Character character = datas.Find(c => c.id == updateLife.id);
        // lui enlever de la vie
        character.life = updateLife.life;
        Debug.Log("update function: " + character.life);
        character.panel.transform.Find("LifeValue").GetComponent<TextMeshProUGUI>().text = character.life.ToString();
    }

    public void addCharacterPanel(Character character)
    {
        
        // Create the panel 
        GameObject characterPanel = Instantiate(characterPanelPrefab);
        characterPanel.transform.position = gameObject.transform.position;
        characterPanel.GetComponent<RectTransform>().SetParent(gameObject.transform);

        // Set the informations about the player 

        characterPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text=character.name;
        TextMeshProUGUI lifeText = characterPanel.transform.Find("LifeValue").GetComponent<TextMeshProUGUI>();
        lifeText.text =character.life.ToString();
        characterPanel.transform.Find("LifeMax").GetComponent<TextMeshProUGUI>().text =character.lifeMax.ToString();
        characterPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>().text =character.description;

        // Set the action of the button 
        var removeLifeButton = characterPanel.transform.Find("ButtonRemoveLife").GetComponent<Button>() as Button;
        removeLifeButton.onClick.AddListener(delegate { sendRemoveLife(character, lifeText); });

        character.LinkPanel(characterPanel);
    }

    private async void sendRemoveLife(Character character, TextMeshProUGUI lifeText)
    {
        character.life -= 10;
        //lifeText.text = character.life.ToString(); ;
        UpdateLife up = new UpdateLife() { id = character.id, life = character.life};
        string json = JsonUtility.ToJson(up);
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