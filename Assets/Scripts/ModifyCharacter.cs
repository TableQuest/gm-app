using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;

public class ModifyCharacter : MonoBehaviour
{
    // Socket and mainthread
    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    // Prefabs
    public GameObject characterPanelPrefab;
    public GameObject characterButtonPrefab;
    public Transform scrollViewContentList;
    public Transform scrollViewContentPlayer;

    // Datas
    Datas initDatas;
    [SerializeField] GameObject dataObject;
    List<Character> listCharacter;

    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        initDatas = dataObject.GetComponent<Datas>();
        listCharacter = initDatas.charactersList;


        Debug.Log(initDatas.charactersList.ToString());
        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        AddAllCharacterToScrollView();
    }


    private void initialisationClient()
    {
        client = initClient.client;
    }

    void SocketThread()
    {
        while (client == null)
        {
            initialisationClient();
            Thread.Sleep(500);
        }


        client.On("updateLifeCharacter", (data) =>
        {
            System.Text.Json.JsonElement playerJson = data.GetValue(0);
            initClient._mainThreadhActions.Enqueue(() =>
            {
                UpdateLife updateLife = JsonUtility.FromJson<UpdateLife>(data.GetValue(0).ToString());
                updateLifeCharacter(updateLife);
            });
        });
        
    }

    public void updateLifeCharacter(UpdateLife updateLife)
    {
        // r�cuperer le character dans les data
        Character character = initDatas.charactersList.Find(c => c.playerId == updateLife.id);
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
        characterPanel.transform.SetParent(scrollViewContentPlayer);

        // Set the informations about the player 

        characterPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text=character.name;
        TextMeshProUGUI lifeText = characterPanel.transform.Find("LifeValue").GetComponent<TextMeshProUGUI>();
        lifeText.text =character.life.ToString();
        characterPanel.transform.Find("LifeMax").GetComponent<TextMeshProUGUI>().text =character.lifeMax.ToString();
        characterPanel.transform.Find("Description").GetComponent<TextMeshProUGUI>().text =character.description;

        // Set the action of the button 
        var removeLifeButton = characterPanel.transform.Find("ButtonRemoveLife").GetComponent<UnityEngine.UI.Button>() as UnityEngine.UI.Button;
        removeLifeButton.onClick.AddListener(delegate { sendRemoveLife(character, lifeText); });

        character.LinkPanel(characterPanel);
    }

    private async void sendRemoveLife(Character character, TextMeshProUGUI lifeText)
    {
        character.life -= 10;
        lifeText.text = character.life.ToString(); ;
        UpdateLife up = new UpdateLife() { id = character.playerId, life = character.life };
        string json = JsonUtility.ToJson(up);
        await client.EmitAsync("updateLifeCharacter", json);
        Debug.Log("remove 10 point of life");
    }

    public void AddAllCharacterToScrollView()
    {
        foreach(Character c in initDatas.charactersList)
        {
            AddCharacterToScroolView(c);
        }
    }
    
    public void AddCharacterToScroolView(Character character)
    {
        GameObject characterButton = Instantiate(characterButtonPrefab);
        Debug.Log(character);
        Debug.Log(characterButton.transform.Find("TextOfButton").GetComponent<TextMeshProUGUI>().text);
        characterButton.transform.Find("TextOfButton").GetComponent<TextMeshProUGUI>().text = character.name;

        characterButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { printInfoCharacter(character); });

        characterButton.transform.SetParent(scrollViewContentList);

    }

    public void printInfoCharacter(Character character)
    {
        addCharacterPanel(character);
    }



}

[Serializable]
public class UpdateLife
{
    public string id;
    public int life;
}