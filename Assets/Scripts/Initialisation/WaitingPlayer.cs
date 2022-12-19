using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;

public class WaitingPlayer : MonoBehaviour
{

    // Socket 
    InitiatlisationClient initClient;
    GameObject clientObject;
    SocketIO client;

    // Prefabs
    public GameObject characterButtonPrefab;
    public Transform scrollViewContent;

    // Datas
    Datas initDatas;

    [SerializeField] GameObject dataObject;
    List<Character> listCharacter;

    private void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        initDatas = dataObject.GetComponent<Datas>();
        listCharacter = initDatas.charactersList;

        var thread = new Thread(SocketThread);
        thread.Start();


        GetAlreadyChosenCharacters();

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
        while(initDatas == null)
        {
            initDatas = dataObject.GetComponent<Datas>();
            Thread.Sleep(300);
        }


        client.On("characterSelection", (data) =>
        {
            System.Text.Json.JsonElement playerJson = data.GetValue(0);
            initClient._mainThreadhActions.Enqueue(() =>
            {

                PlayerInfo playerInfo = JsonUtility.FromJson<PlayerInfo>(playerJson.ToString());
                CharacterInfo characterInfo = playerInfo.character;
                List<SkillInfo> skillInfos = characterInfo.skills;

                Character character = AddCharacterToData(playerInfo, characterInfo, skillInfos);
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
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(initClient.requestURI+"/inGameCharacters");
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string JsonResponse = reader.ReadToEnd();

        ListCharacter CharacterList = JsonUtility.FromJson<ListCharacter>(JsonResponse);

        foreach(CharacterForHttp chttp in CharacterList.characterList)
        {
            CharacterInfo c = chttp.characterInfo;

            List<Skill> skills = new List<Skill>();
            foreach (SkillInfo s in c.skills)
            {
                skills.Add(new Skill(s.id, s.name, s.manaCost, s.range, s.maxTarget, s.statModifier));
            }

            Character character = new Character(chttp.playerId, c.id, c.name,
                                                c.life, c.lifeMax, c.mana, c.manaMax,
                                                c.speed, c.description, skills);
            initDatas.charactersList.Add(character);
            AddCharacterToScroolView(character);   
        }
    }

    public Character AddCharacterToData(PlayerInfo playerInfo, CharacterInfo characterInfo, List<SkillInfo> skillsInfos)
    {
        if (!CharacterAlreadyChosen(characterInfo.id.ToString()))
        {
            List<Skill> skills = new List<Skill>();
            foreach (SkillInfo s in skillsInfos)
            {
                skills.Add(new Skill(s.id,s.name,s.manaCost,s.range,s.maxTarget, s.statModifier));
            }

            Character character = new Character(playerInfo.player, characterInfo.id, characterInfo.name,
                                                characterInfo.life, characterInfo.lifeMax, characterInfo.mana, 
                                                characterInfo.manaMax, characterInfo.speed, characterInfo.description, 
                                                skills);
            initDatas.charactersList.Add(character);
            return character;
        }
        return null;
    }

    private void AddCharacterToScroolView(Character character)
    {
        GameObject characterButton = Instantiate(characterButtonPrefab);
        characterButton.transform.Find("TextOfButton").GetComponent<TextMeshProUGUI>().text = character.name;
        characterButton.transform.SetParent(scrollViewContent);
        characterButton.transform.localScale = new Vector3(1,1,1);
    }

    public void StartGame()
    {
        // change state in the server
        initClient.client.EmitAsync("switchState", "FREE");

        // change the scene, here to player but to modify after
        SceneManager.LoadScene("Main");
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
    public int mana;
    public int manaMax;
    public int speed;
    public string description;
    public List<SkillInfo> skills;
}

[Serializable]
public class CharacterForHttp
{
    public string playerId;
    public CharacterInfo characterInfo;
}

[Serializable]
public class ListCharacter
{  
    public List<CharacterForHttp> characterList;
}

[Serializable]
public class SkillInfo
{
    public int id;
    public string name;
    public int manaCost;
    public int range;
    public int maxTarget;
    public int statModifier;
}
