using System.Collections;
using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;
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

            StartCoroutine(GetAllPlacedNpc());
            StartCoroutine(GetAllNpc());
            StartCoroutine(GetTestAlreadyChosenCharacters());
            StartCoroutine(MyUpdate());

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
                            AddCharacterToScrollView(character);
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

        IEnumerator GetTestAlreadyChosenCharacters() {
            var www = UnityWebRequest.Get(_initClient.requestURI+"/inGameCharacters");
        
            yield return www.SendWebRequest();
 
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                // Show results as text
                var jsonResponse = www.downloadHandler.text;
                var characterList = JsonUtility.FromJson<ListCharacter>(jsonResponse);
                Debug.Log(jsonResponse);
                
                foreach (var chttp in characterList.characterList)
                {
                    var c = chttp.characterInfo;

                    List<Skill> skills = new List<Skill>();
                    foreach (SkillInfo s in c.skills)
                    {
                        skills.Add(new Skill(s.id, s.name, s.manaCost, s.range, s.maxTarget, s.type, s.statModifier, s.healing, s.image));
                    }

                    var character = new Character(chttp.playerId, c.id, c.name,
                        c.life, c.lifeMax, c.mana, c.manaMax,
                        c.speed, c.description, skills, c.image);
                    _initData.charactersList.Add(character);
                    AddCharacterToScrollView(character);
                }
            }
        }
        public Character AddCharacterToData(PlayerInfo playerInfo, CharacterInfo characterInfo, List<SkillInfo> skillsInfos)
    {
        if (!CharacterAlreadyChosen(characterInfo.id.ToString()))
        {
            var skills = new List<Skill>();
            foreach (var s in skillsInfos)
            {
                skills.Add(new Skill(s.id,s.name,s.manaCost,s.range,s.maxTarget,s.type, s.statModifier, s.healing, s.image));
            }

            var character = new Character(playerInfo.player, characterInfo.id, characterInfo.name,
                                                characterInfo.life, characterInfo.lifeMax, characterInfo.mana, 
                                                characterInfo.manaMax, characterInfo.speed, characterInfo.description, 
                                                skills, characterInfo.image);
            _initData.charactersList.Add(character);
            return character;
        }
        return null;
    }
        IEnumerator GetAllNpc() {
            var www = UnityWebRequest.Get(_initClient.requestURI+"/npcs");
        
            yield return www.SendWebRequest();
 
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                // Show results as text
                var jsonResponse = www.downloadHandler.text;
                var npcList = JsonUtility.FromJson<ListNpc>(jsonResponse);

                foreach (var nhttp in npcList.npcList)
                {
                    var skills = new List<Skill>();
                    
                    foreach (SkillInfo s in nhttp.skills)
                    {
                        skills.Add(new Skill(s.id, s.name, s.manaCost, s.range, s.maxTarget, s.type, s.statModifier, s.healing, s.image));
                    }
                    
                    var npc = new Npc(nhttp.id, nhttp.name, nhttp.lifeMax, nhttp.life, nhttp.description, nhttp.image, skills);
                    _initData.npcList.Add(npc);
                }
            }
        }
    
        IEnumerator GetAllPlacedNpc() {
            var www = UnityWebRequest.Get(_initClient.requestURI+"/inGameNpcs");
            
            yield return www.SendWebRequest();
     
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
            }
            else {
                // Show results as text
                var jsonResponse = www.downloadHandler.text;
                var npcList = JsonUtility.FromJson<ListNpc>(jsonResponse);

                foreach (var nhttp in npcList.npcList)
                {
                    var skills = new List<Skill>();
                    
                    foreach (SkillInfo s in nhttp.skills)
                    {
                        skills.Add(new Skill(s.id, s.name, s.manaCost, s.range, s.maxTarget, s.type, s.statModifier, s.healing, s.image));
                    }
                    
                    var npc = new Npc(nhttp.id, nhttp.name, nhttp.lifeMax, nhttp.life, nhttp.description, nhttp.image, skills);
                    npc.pawnCode = nhttp.pawnCode;
                    _initData.placedNpcList.Add(npc);
                }
            }
        }

        private void AddCharacterToScrollView(Character character)
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
    public string type;
    public int statModifier;
    public bool healing;
    public string image;

    public SkillInfo(int id, string name, int manaCost, int range, int maxTarget, string type, int statModifier, bool healing, string image)
    {
        this.id = id;
        this.name = name;
        this.manaCost = manaCost;
        this.range = range;
        this.maxTarget = maxTarget;
        this.type = type;
        this.statModifier = statModifier;
        this.healing = healing;
        this.image = image;
    }
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
    public string pawnCode;
    public List<SkillInfo> skills;
}
