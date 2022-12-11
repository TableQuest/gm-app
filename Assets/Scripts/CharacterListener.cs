using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor;
public class CharacterListener : MonoBehaviour
{

    // Socket 
    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    // Main thread
    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    // Prefabs
    public GameObject characterButtonPrefab;
    public Transform scrollViewContent;

    // Datas
    Datas initDatas;
    [SerializeField] GameObject dataObject;
    List<Character> listCharacter;



    [MenuItem("MyAssets/Datas")]
    private void Start()
    {
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        initDatas = dataObject.GetComponent<Datas>();
        listCharacter = initDatas.charactersList;


        var thread = new Thread(SocketThread);
        thread.Start();

        StartCoroutine(myUpdate());

        GetAlreadyChosenCharacters();

    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            yield return new WaitUntil(() => _mainThreadhActions.Count > 0);

            if (!_mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            action?.Invoke();
        }
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
        while(initDatas == null)
        {
            initDatas = dataObject.GetComponent<Datas>();
            Thread.Sleep(300);
        }

        client.On("characterSelection", (data) =>

        {
            System.Text.Json.JsonElement playerJson = data.GetValue(0);
            _mainThreadhActions.Enqueue(() =>
            {

                PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(playerJson.ToString());
                CharacterInfo characterInfo = playerInfo.character;

                Character character = AddCharacterToData(characterInfo);
                if (character != null)
                {
                    AddCharacterToScroolView(character);
                }

            });
        });

    }



    private bool CharacterAlreadyChosen(string idCharacter)
    {
        foreach(Character c in initDatas.charactersList)
        {
            if(c.id.ToString() == idCharacter)
            {
                return true;
            } 
        }
        return false;
    }

    private void GetAlreadyChosenCharacters()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://localhost:3000/inGameCharacters");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string JsonResponse = reader.ReadToEnd();

        ListCharacter CharacterList = JsonUtility.FromJson<ListCharacter>(JsonResponse);

        foreach(CharacterInfo c in CharacterList.characterList)
        {
            Character character = new Character(c.id, c.name, c.life, c.lifeMax, c.description);
            initDatas.charactersList.Add(character);
   
            AddCharacterToScroolView(character);
            
        }
    }

    public Character AddCharacterToData(CharacterInfo characterInfo)
    {
        if (!CharacterAlreadyChosen(characterInfo.id.ToString()))
        {
            Character character = new Character(characterInfo.id, characterInfo.name, characterInfo.life, characterInfo.lifeMax, characterInfo.description);
            initDatas.charactersList.Add(character);

            //addCharacterToScroolView(character);
            return character;
        }
        return null;
    }

    private void AddCharacterToScroolView(Character character)
    {
        GameObject characterButton = Instantiate(characterButtonPrefab);
        characterButton.transform.Find("TextOfButton").GetComponent<TextMeshProUGUI>().text = character.name;
        characterButton.transform.SetParent(scrollViewContent);
    }

    public void StartGame()
    {
        
        // change state in the server
        client.EmitAsync("switchState", "PLAYING");

        // change the scene, here to player but to modify after
        SceneManager.LoadScene("Player");
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
public class ListCharacter
{
    public List<CharacterInfo> characterList;
}
