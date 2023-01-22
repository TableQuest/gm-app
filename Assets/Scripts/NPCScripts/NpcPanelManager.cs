using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;

namespace NPCScripts
{
    public class NpcPanelManager : MonoBehaviour
    {
        private Npc _npcOfPanel;
        
        // Socket
        private InitiatlisationClient _client;
        private GameObject _clientObject;
        
        // Prefab
        [SerializeField] private GameObject addNpcPanelPrefab;
        [SerializeField] private GameObject scrollViewContentSkills;
        [SerializeField] private GameObject skillPanelPrefab;
        [SerializeField] private GameObject attackPanelPrefab;
        [SerializeField] private GameObject dicePanelPrefab;
        // Datas
        private Datas _data;
        private GameObject _dataObject;
        
        private void Start()
        {
            _clientObject = GameObject.Find("SocketIOClient");
            _client = _clientObject.GetComponent<InitiatlisationClient>();

            _dataObject = GameObject.Find("DataContainer");
            _data = _dataObject.GetComponent<Datas>();


            var thread = new Thread(SocketThread);
            thread.Start();

            StartCoroutine(MyUpdate());
        }
        private IEnumerator MyUpdate()
        {
            while (true)
            {
                yield return new WaitUntil(() => _client._mainThreadhActions.Count > 0);

                if (!_client._mainThreadhActions.TryDequeue(out var action))
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

                _clientObject = GameObject.Find("SocketIOClient");
                _client = _clientObject.GetComponent<InitiatlisationClient>();
                Thread.Sleep(500);
            }
            while (_data == null)
            {
                _dataObject = GameObject.Find("DataContainer");
                _data = _dataObject.GetComponent<Datas>();
                Thread.Sleep(300);
            }
        }

        public void SetInfoPanel(Npc npc, bool placed)
        {
            _npcOfPanel = npc;
            
            // Name
            gameObject.transform.Find("NamePanel").transform.Find("Name").GetComponent<TextMeshProUGUI>().text = npc.name;

            var basicInfoPanel = gameObject.transform.Find("BasicInfoPanel");
            
            // Life
            var inputFieldLife = basicInfoPanel.Find("Life").GetComponent<TMP_InputField>();
            inputFieldLife.text = npc.life.ToString();
            inputFieldLife.onEndEdit.AddListener(delegate
            {
                npc.life = Int32.Parse(inputFieldLife.text);
                SendModificationToServer("life", npc.life.ToString());
            });

            // LifeMax 
            var inputFieldLifeMax = basicInfoPanel.Find("LifeMax").GetComponent<TMP_InputField>();
            inputFieldLifeMax.text = npc.lifeMax.ToString();
            inputFieldLifeMax.onEndEdit.AddListener(delegate
            {
                npc.lifeMax = Int32.Parse(inputFieldLife.text);
                SendModificationToServer("lifeMax", npc.lifeMax.ToString());
            });
            
            // Image
            // Button remove body
            
            var removeButton = gameObject.transform.Find("DeadPanel").Find("RemoveButton").GetComponent<Button>();
            removeButton.onClick.AddListener(
                delegate
                {
                    _client.client.EmitAsync("removeNpc", npc.pawnCode);
                });
            if (npc.life <= 0)
            {
                removeButton.enabled = true;
                Debug.Log(npc.image+"_grey");
                var sprite = Resources.Load<Sprite>(npc.image+"_grey");
                Debug.Log(sprite);
                basicInfoPanel.Find("Image").GetComponent<Image>().sprite = sprite;
            }
            else
            {
                removeButton.enabled = false;
                var sprite = Resources.Load<Sprite>(npc.image);
                basicInfoPanel.Find("Image").GetComponent<Image>().sprite = sprite;
            }
            
            // Add npc button or dice if npc is placed
            var addNpcButton = gameObject.transform.Find("PlacedPanel").Find("PlacedButton").GetComponent<Button>();

            if (placed)
            {
                addNpcButton.GetComponentInChildren<TextMeshProUGUI>().text = "Roll dice";
                addNpcButton.onClick.AddListener(
                    delegate
                    {
                        printDicePanel(npc);
                    });
            }
            else
            {
                addNpcButton.onClick.AddListener(
                    delegate
                    {
                        PrintAddNpcPanel(npc);
                    }
                );
            }
            
            // Skills
            foreach (var s in npc.skills)
            {
                var skillPanel = Instantiate(skillPanelPrefab);
                skillPanel.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = s.name;
                skillPanel.transform.Find("Damage").GetComponent<TextMeshProUGUI>().text = s.statModifier.ToString();
                if (s.healing)
                {
                    skillPanel.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = "Heal";
                }
                else
                {
                    skillPanel.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = "Attack";

                }
                skillPanel.transform.SetParent(scrollViewContentSkills.transform);
                skillPanel.transform.localScale = new Vector3(1,1,1);
                skillPanel.transform.Find("Button").GetComponent<Button>().onClick.AddListener(
                    delegate
                    {
                        PrintAttackNpcPanel(npc,s, skillPanel);
                    });
            }
        }

        private void printDicePanel(Npc npc)
        {

            var dicePanel = Instantiate(dicePanelPrefab);

            var canvas = GameObject.Find("Canvas").GetComponent <Canvas>();
            dicePanel.transform.SetParent(canvas.transform);
            dicePanel.transform.localScale = new Vector3(1, 1, 1);
            dicePanel.transform.position = new Vector3(1200, 500, 0);
            
            var random = new Random();

            // exit button 
            dicePanel.transform.Find("TitlePanel").Find("ExitButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    Destroy(dicePanel);
                });
            
            // npc name
            dicePanel.transform.Find("TitlePanel").Find("NpcName").GetComponent<TextMeshProUGUI>().text = npc.name;

            // roll button 
            dicePanel.transform.Find("RollButton").GetComponent<Button>().onClick.AddListener(delegate
            {
                var result = random.Next(1, 21);
                dicePanel.transform.Find("ResultPanel").Find("Result").GetComponent<TextMeshProUGUI>().text =
                    result.ToString();
                Debug.Log("Dice "+result+"/20 for npc"+npc.pawnCode);
            });
            
            // send button 
            dicePanel.transform.Find("ResultPanel").Find("SendButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    var result = dicePanel.transform.Find("ResultPanel").Find("Result").GetComponent<TextMeshProUGUI>()
                        .text;
                    var message = new DiceMessage(npc.pawnCode, 4, int.Parse(result));
                    _client.client.EmitAsync("dice", JsonUtility.ToJson(message));
                    Debug.Log("Send dice "+result+"/20 for npc"+npc.pawnCode);
                    Destroy(dicePanel);
                });

        }
        
        private void PrintAddNpcPanel(Npc npc)
        {
            var addNpcPanel = Instantiate(addNpcPanelPrefab);
            // set value of the panel 
            
            // add the panel on the canvas
            var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            addNpcPanel.transform.SetParent(canvas.transform);
            addNpcPanel.transform.localScale = new Vector3(1, 1, 1);
            addNpcPanel.transform.position = new Vector3(1200, 550, 0);
            
            // exit button
            addNpcPanel.transform.Find("TitlePanel").Find("ExitButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    Destroy(addNpcPanel);
                });

            // change name title
            addNpcPanel.transform.Find("TitlePanel").Find("NpcName").GetComponent<TextMeshProUGUI>().text = npc.name;
            
            // add npc button 
            addNpcPanel.transform.Find("AddButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    var name = addNpcPanel.transform.Find("NewNameField").GetComponent<TMP_InputField>().text;
                    var newNpc = new Npc(npc.id, name, npc.lifeMax, npc.life, npc.description, npc.image, npc.skills);
                    _data.placedNpcList.Add(newNpc);
                    var objJson = new NewNpc(newNpc.id, newNpc.name);
                    _client.client.EmitAsync("newNpc", JsonUtility.ToJson(objJson));
                    Destroy(addNpcPanel);
                    GameObject.Find("Canvas").GetComponent<NpcSceneManager>().AddNpcToScrollView(newNpc, 
                        GameObject.Find("Canvas").GetComponent<NpcSceneManager>().scrollViewContentListPlacedNpc, true);
                    Debug.Log("Add Npc of id "+newNpc.id + " named "+newNpc.name);
                });
        }

        private void PrintAttackNpcPanel(Npc npc, Skill skill, GameObject skillPanel)
        {
            var panelAttack = Instantiate(attackPanelPrefab);
                        
            panelAttack.transform.SetParent(GameObject.Find("Canvas").transform);
            panelAttack.transform.localScale = new Vector3(1, 1, 1);
            panelAttack.transform.position = new Vector3(1200, 550, 0);
            
            // Exit Button
            panelAttack.transform.Find("TitlePanel").transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    Destroy(panelAttack);
                });
            
            // Attack Button 
            panelAttack.transform.Find("AttackButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    var dropdown = panelAttack.transform.Find("DropdownTarget").GetComponent<TMP_Dropdown>();
                    var targetIsNpc = (dropdown.options[dropdown.value].text.Split(" : ")[0] == "N");
 
                    var targetId = dropdown.options[dropdown.value].text.Split(" : ")[1];
                    
                    var newDamage = int.Parse(panelAttack.transform.Find("DamageField").GetComponent<TMP_InputField>().text);
                    var newHealing = (panelAttack.transform.Find("DropdownType").GetComponent<TMP_Dropdown>().value == 0);
                    
                    var iSkill = new SkillInfo(skill.id, skill.name, skill.manaCost, skill.range, skill.maxTarget, skill.type, newDamage, newHealing, skill.image);
                    var jsonSkill = JsonUtility.ToJson(iSkill);
                    
                    var attackInfo = new AttackMessage(npc.pawnCode,targetId,targetIsNpc, iSkill);
                    Debug.Log(JsonUtility.ToJson(attackInfo));
                    _client.client.EmitAsync("attackNpc", JsonUtility.ToJson(attackInfo));
                    Debug.Log("Launch attack "+skill.name+" with healing: "+skill.healing+" and damage "+skill.statModifier+" and target "+targetId);
                    
                    Destroy(panelAttack);
                });
            
            // Save Button 
            panelAttack.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    skill.healing = (panelAttack.transform.Find("DropdownType").GetComponent<TMP_Dropdown>().value == 0);
                    skill.statModifier = int.Parse(panelAttack.transform.Find("DamageField").GetComponent<TMP_InputField>().text);

                    
                    //_data.placedNpcList.Find(n => ( n.pawnCode == npc.pawnCode)).skills.Find(s => ( s.id == skill.id))
                     //   .statModifier = skill.statModifier;
                    
                    //_data.placedNpcList.Find(n => ( n.pawnCode == npc.pawnCode)).skills.Find(s => ( s.id == skill.id))
                     //   .healing = skill.healing;

                     // Set new values in skills panel 
                     skillPanel.transform.Find("Damage").GetComponent<TextMeshProUGUI>().text = skill.statModifier.ToString();
                     if (skill.healing) { skillPanel.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = "Heal"; }
                     else { skillPanel.transform.Find("Type").GetComponent<TextMeshProUGUI>().text = "Attack"; }

                    // close the window
                    Debug.Log("Save attack "+skill.name+" with healing: "+skill.healing+" and damage "+skill.statModifier);
                    Destroy(panelAttack);
                });

            // Target
            var targetDropdown = panelAttack.transform.Find("DropdownTarget").GetComponent<TMP_Dropdown>();
            var dropdownOptions = new List<TMP_Dropdown.OptionData>();
            foreach (var oNpc in _data.placedNpcList)
            {
                var option = new TMP_Dropdown.OptionData();
                option.text = "N : "+oNpc.pawnCode+" : "+oNpc.name;
                //option.image = Resources.Load(oNpc.image);
                dropdownOptions.Add(option);
            }
            foreach (var character in _data.charactersList)
            {
                var option = new TMP_Dropdown.OptionData();
                option.text = "P : "+character.playerId+" : "+character.name;
                //option.image = Resources.Load(character.image);
                dropdownOptions.Add(option);
            }
            targetDropdown.AddOptions(dropdownOptions);
            
            // add all the player and npc as toogle on the panel 
            
            // Npc Name
            panelAttack.transform.Find("TitlePanel").transform.Find("NpcName").GetComponent<TextMeshProUGUI>().text =
                npc.name;

            // Attack Name 
            panelAttack.transform.Find("TitlePanel").transform.Find("AttackName").GetComponent<TextMeshProUGUI>().text =
                skill.name;
            
            // Set corresponding values
            panelAttack.transform.Find("DamageField").GetComponent<TMP_InputField>().text = skill.statModifier.ToString();
            if (skill.healing) { panelAttack.transform.Find("DropdownType").GetComponent<TMP_Dropdown>().value = 0; }
            else { panelAttack.transform.Find("DropdownType").GetComponent<TMP_Dropdown>().value = 1; }
        }
        private void SendModificationToServer(string variable, string value)
        {
            NpcUpdateInfo cui = new(_npcOfPanel.pawnCode, variable, value);
            var json = JsonUtility.ToJson(cui);
            _client.client.EmitAsync("updateInfoNpc", json);
        }
    }
}

[Serializable]
public class NewNpc
{
    public string id;
    public string name;

    public NewNpc(string id, string name)
    {
        this.id = id;
        this.name = name;
    }
}

[Serializable]
public class AttackMessage
{
    public string launchId;
    public string targetId;
    public bool targetIsNpc;
    public SkillInfo skill;

    public AttackMessage(string launchId, string targetId, bool targetIsNpc, SkillInfo skill)
    {
        this.launchId = launchId;
        this.targetId = targetId;
        this.targetIsNpc = targetIsNpc;
        this.skill = skill;
    }
}

[Serializable]

public class DiceMessage
{
    public string playerId;
    public int diceId;
    public int value;

    public DiceMessage(string playerId, int diceId, int value)
    {
        this.playerId = playerId;
        this.diceId = diceId;
        this.value = value;
    }
}