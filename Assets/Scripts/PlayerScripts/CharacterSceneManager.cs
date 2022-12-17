using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System;
using System.Linq;

public class CharacterSceneManager : MonoBehaviour
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
    Datas datas;
    [SerializeField] GameObject dataObject;


    // Present character panel
    private int idCharacterOnPanel;

    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        datas = dataObject.GetComponent<Datas>();

        idCharacterOnPanel = -1;

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        AddAllCharacterToScrollView();
    }


    void SocketThread()
    {
        while (client == null)
        {
            client = initClient.client;
            Thread.Sleep(500);
        }

        // client.on
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

    public void AddAllCharacterToScrollView()
    {
        foreach (Character c in datas.charactersList)
        {
            AddCharacterToScroolView(c);
        }
    }
    public void AddCharacterToScroolView(Character character)
    {
        GameObject characterButton = Instantiate(characterButtonPrefab);
        characterButton.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;

        characterButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate { PrintCharacterPanel(character); });

        characterButton.transform.SetParent(scrollViewContentList);

    }

    public void PrintCharacterPanel(Character character)
    {
        // if scroll view not empty remove the panel 
        bool panelPresence = scrollViewContentPlayer.childCount != 0;
        if (panelPresence)
        {
            Destroy(scrollViewContentPlayer.GetChild(0).gameObject); 
        } 
        else
        {
            // Create the panel 
            GameObject characterPanel = Instantiate(characterPanelPrefab);
            characterPanel.transform.position = gameObject.transform.position;
            characterPanel.transform.SetParent(scrollViewContentPlayer);

            // set information
            characterPanel.GetComponent<CharacterPanelManager>().SetPanelInfo(character);
        }
    }


    // move to character panel manager
    public void updateLifeCharacter(UpdateLife updateLife)
    {
        // r�cuperer le character dans les data
        Character character = datas.charactersList.Find(c => c.playerId == updateLife.id);
        // lui enlever de la vie
        character.life = updateLife.life;
        Debug.Log("update function: " + character.life);

        // to remove because the panel is not always present 
        if (character.id == idCharacterOnPanel)
        {
            character.panel.transform.Find("LifeValue").GetComponent<TextMeshProUGUI>().text = character.life.ToString();
        }
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



}

[Serializable]
public class UpdateLife
{
    public string id;
    public int life;
}