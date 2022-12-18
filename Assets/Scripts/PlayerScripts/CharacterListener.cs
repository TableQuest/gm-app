using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterListener : MonoBehaviour
{

    // Socket 
    InitiatlisationClient initClient;
    GameObject clientObject;
    SocketIO client;


    // Datas
    Datas initDatas;

    [SerializeField] GameObject dataObject;
    List<Character> listCharacter;
    // Start is called before the first frame update
    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        initDatas = dataObject.GetComponent<Datas>();
        listCharacter = initDatas.charactersList;

        var thread = new Thread(SocketThread);
        thread.Start();

        myUpdate();
    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            yield return new WaitUntil(() => initClient._mainThreadhActions.Count > 0);

            if (!initClient._mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            action?.Invoke();
        }
    }

    void SocketThread()
    {
        while (client == null)
        {

            client = initClient.client;
            Thread.Sleep(500);
        }
        while (initDatas == null)
        {
            initDatas = dataObject.GetComponent<Datas>();
            Thread.Sleep(300);
        }

        client.On("updateInfoCharacter", (data) => {
            CharacterUpdateInfo cui = JsonUtility.FromJson<CharacterUpdateInfo>(data.ToString());
            updateInfoCharacter(cui.playerId, cui.variable, cui.value);
            Character character = initDatas.charactersList.Find(c => c.playerId == cui.playerId);
            Debug.Log(SceneManager.GetActiveScene().GetRootGameObjects());
            if (SceneManager.GetActiveScene().name == "Player")
            {
                Debug.Log("root game objects" + SceneManager.GetActiveScene().GetRootGameObjects());
                //int printedPanel = SceneManager.GetActiveScene().Get 
            }
        });

    }

    public void updateInfoCharacter(string playerId, string variable, string value)
    {
        Character character = initDatas.charactersList.Find(c => c.playerId == playerId);
        switch (variable)
        {
            case "life":
                try{
                    character.life = Int32.Parse(value);
                }
                catch (Exception e){
                    Debug.Log("Life value is not numerical: "+e);
                }
                break;
            case "lifeMax":
                try
                {
                    character.lifeMax = Int32.Parse(value);
                }
                catch (Exception e)
                {
                    Debug.Log("LifeMax value is not numerical: "+e);
                }
                break;
            case "mana":
                try
                {
                    character.mana = Int32.Parse(value);
                }
                catch (Exception e)
                {
                    Debug.Log("Mana value is not numerical: "+e);
                }
                break;
            case "manaMax":
                try
                {
                    character.manaMax = Int32.Parse(value);
                }
                catch (Exception e)
                {
                    Debug.Log("ManaMax value is not numerical: "+e);
                }
                break;
        }
    }
}

[Serializable]
public class CharacterUpdateInfo
{
    public CharacterUpdateInfo(string playerId, string variable, string value)
    {
        this.playerId = playerId;
        this.variable = variable;
        this.value = value;
    }

    public string playerId;
    public string variable;
    public string value;
}

