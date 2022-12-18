using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System;
using System.Collections;

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

    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            // Wait until a callback action is added to the queue
            yield return new WaitUntil(() => initClient._mainThreadhActions.Count > 0);
            // If this fails something is wrong ^^
            // simply get the first added callback
            if (!initClient._mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            // Execute the code of the added callback
            action?.Invoke();
        }
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
        characterButton.transform.localScale = new Vector3(1, 1, 1);

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
            characterPanel.transform.localScale = new Vector3(1,1,1);
            // set information
            characterPanel.GetComponent<CharacterPanelManager>().SetPanelInfo(character);
        }
    }

}

[Serializable]
public class UpdateLife
{
    public string id;
    public int life;
}