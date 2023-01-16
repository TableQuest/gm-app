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

namespace Initialisation
{
    public class WaitingPlayer : MonoBehaviour
    {

        // Socket 
        private InitiatlisationClient _initClient;
        private GameObject _clientObject;
        private SocketIO _client;

        // Prefabs
        public GameObject characterButtonPrefab;
        public Transform scrollViewContent;

        // Data
        private Datas _initData;

        [SerializeField] private GameObject dataObject;

        private void Start()
        {
            _clientObject = GameObject.Find("SocketIOClient");
            _initClient = _clientObject.GetComponent<InitiatlisationClient>();

            dataObject = GameObject.Find("DataContainer");
            _initData = dataObject.GetComponent<Datas>();

            var thread = new Thread(SocketThread);
            thread.Start();


            GetAlreadyChosenCharacters();
            AddAllNpcToData();
            //AddAllPlacedNpcToData();
            MyUpdate();
        }

        private IEnumerator MyUpdate()
        {
            while (true)
            {
                yield return new WaitUntil(() => _initClient._mainThreadhActions.Count > 0);

                if (!_initClient._mainThreadhActions.TryDequeue(out var action))
                {
                    Debug.LogError("Something Went Wrong ! ", this);
                    yield break;
                }

                action?.Invoke();
            }
        }

        private void SocketThread()
        {
            while (_client == null)
            {

                _client = _initClient.client;
                Thread.Sleep(500);
            }

            while (_initData == null)
            {
                _initData = dataObject.GetComponent<Datas>();
                Thread.Sleep(300);
            }

            _client.On("characterSelection", (data) =>
            {
                _initClient._mainThreadhActions.Enqueue(() =>
                {
                    System.Text.Json.JsonElement playerJson = data.GetValue(0);
                    _initClient._mainThreadhActions.Enqueue(() =>
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
            });   
        }

        private bool CharacterAlreadyChosen(string idCharacter)
        {
            foreach (var c in _initData.charactersList)
            {
                if (c.id.ToString() == idCharacter)
                {
                    return true;
                }
            }

            return false;
        }

        private void GetAlreadyChosenCharacters()
        {
            var request = (HttpWebRequest)WebRequest.Create(_initClient.requestURI + "/inGameCharacters");
            var response = (HttpWebResponse)request.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());

            var jsonResponse = reader.ReadToEnd();

            var characterList = JsonUtility.FromJson<ListCharacter>(jsonResponse);

            foreach (var chttp in characterList.characterList)
            {
                var c = chttp.characterInfo;

                List<Skill> skills = new List<Skill>();
                foreach (SkillInfo s in c.skills)
                {
                    skills.Add(new Skill(s.id, s.name, s.manaCost, s.range, s.maxTarget, s.statModifier));
                }

                var character = new Character(chttp.playerId, c.id, c.name,
                    c.life, c.lifeMax, c.mana, c.manaMax,
                    c.speed, c.description, skills, c.image);
                _initData.charactersList.Add(character);
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
                                                skills, characterInfo.image);
            _initData.charactersList.Add(character);
            return character;
        }
        return null;
    }

    private void AddAllNpcToData()
    {
        var request = (HttpWebRequest)WebRequest.Create(_initClient.requestURI + "/npcs");
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());

        var jsonResponse = reader.ReadToEnd();

        Debug.Log(jsonResponse);
        var npcList = JsonUtility.FromJson<ListNpc>(jsonResponse);

        foreach (var nhttp in npcList.npcList)
        {
            var npc = new Npc(nhttp.id, nhttp.name, nhttp.lifeMax, nhttp.life, nhttp.description, nhttp.image);
            _initData.npcList.Add(npc);
        }
    }

    private void AddAllPlacedNpcToData()
    {
        var request = (HttpWebRequest)WebRequest.Create(_initClient.requestURI + "/inGameNpcs");
        var response = (HttpWebResponse)request.GetResponse();
        var reader = new StreamReader(response.GetResponseStream());

        var jsonResponse = reader.ReadToEnd();

        var npcList = JsonUtility.FromJson<ListNpc>(jsonResponse);

        foreach (var nhttp in npcList.npcList)
        {
            var npc = new Npc(nhttp.id, nhttp.name, nhttp.lifeMax, nhttp.life, nhttp.description, nhttp.image);
            _initData.placedNpcList.Add(npc);
        }
    }

    private void AddCharacterToScroolView(Character character)
    {
        var characterButton = Instantiate(characterButtonPrefab);
        characterButton.transform.Find("TextOfButton").GetComponent<TextMeshProUGUI>().text = character.name;
        characterButton.transform.SetParent(scrollViewContent);
        characterButton.transform.localScale = new Vector3(1, 1, 1);
    }

    public void StartGame()
    {
        // change state in the server
        _initClient.client.EmitAsync("switchState", "FREE");

        // change the scene, here to player but to modify after
        SceneManager.LoadScene("Main");
    }
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
    public string image;
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

[Serializable]
public class ListNpc
{
    public List<NpcInfo> npcList;
}

[Serializable]
public class NpcInfo
{
    public string id;
    public string name;
    public int lifeMax;
    public int life;
    public string description;
    public string image;
}
