using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SocketIOClient;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace NPCScripts
{

    public class NpcAttackManager : MonoBehaviour
    {
        // Socket and mainthread
        private InitiatlisationClient _client;
        private GameObject clientObject;
        
        // Datas
        Datas _data;
        private GameObject dataObject;
        
        // Prefab
        private void Start()
        {
            clientObject = GameObject.Find("SocketIOClient");
            _client = clientObject.GetComponent<InitiatlisationClient>();

            dataObject = GameObject.Find("DataContainer");
            _data = dataObject.GetComponent<Datas>();
            Debug.Log("start attack manager "+_data.placedNpcList);
            var thread = new Thread(SocketThread);
            thread.Start();

            StartCoroutine(myUpdate());
        }
        
        private IEnumerator myUpdate()
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
        
        void SocketThread()
        {
            while (_client == null)
            {

                clientObject = GameObject.Find("SocketIOClient");
                _client = clientObject.GetComponent<InitiatlisationClient>();
                Thread.Sleep(500);
            }
            while (_data == null)
            {
                Debug.Log("Data null attack manager");
                dataObject = GameObject.Find("DataContainer");
                _data = dataObject.GetComponent<Datas>();
                Thread.Sleep(300);
            }

            // client.On

        }

        public void setInfoPanel(Npc npc, Skill skill)
        {
            // Exit Button
            gameObject.transform.Find("TitlePanel").transform.Find("ExitButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    Destroy(gameObject);
                });
            
            // Attack Button 
            gameObject.transform.Find("AttackButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    var dropdown = gameObject.transform.Find("DropdownTarget").GetComponent<Dropdown>();
                    var targetId = dropdown.options[dropdown.value].text.Split(":")[0];
                    var iSkill = new SkillInfo(skill.id, skill.name, skill.manaCost, skill.range, skill.maxTarget, skill.type, skill.statModifier, skill.healing, skill.image);
                    
                    var attackInfo = new AttackMessage(npc.pawnCode,targetId, iSkill);
                    
                    Debug.Log(npc.pawnCode);
                    Debug.Log(targetId);
                    Debug.Log("Attack info json");
                    Debug.Log(JsonUtility.ToJson(attackInfo));
                    _client.client.EmitAsync("attackNpc", JsonUtility.ToJson(attackInfo));
                    Debug.Log("Launch attack "+skill.name+" with healing: "+skill.healing+" and damage "+skill.statModifier+" and target "+targetId);
                    Destroy(gameObject);
                });
            
            // Save Button 
            gameObject.transform.Find("SaveButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    skill.healing = (gameObject.transform.Find("DropDownType").GetComponent<Dropdown>().value == 0);
                    skill.statModifier = int.Parse(gameObject.transform.Find("DamageField").GetComponent<TMP_InputField>().text);

                    
                    //_data.placedNpcList.Find(n => ( n.pawnCode == npc.pawnCode)).skills.Find(s => ( s.id == skill.id))
                     //   .statModifier = skill.statModifier;
                    
                    //_data.placedNpcList.Find(n => ( n.pawnCode == npc.pawnCode)).skills.Find(s => ( s.id == skill.id))
                     //   .healing = skill.healing;

                    // close the window
                    Debug.Log("Save attack "+skill.name+" with healing: "+skill.healing+" and damage "+skill.statModifier);
                    Destroy(gameObject);
                });
            
            // Attack type
            // see how work the dropdown
            
            // Target
            var targetDropdown = gameObject.transform.Find("DropdownTarget").GetComponent<Dropdown>();
            var dropdownOptions = new List<Dropdown.OptionData>();
            Debug.Log(_data);
            Debug.Log(_data.placedNpcList);
            foreach (var oNpc in _data.placedNpcList)
            {
                var option = new Dropdown.OptionData();
                option.text = oNpc.pawnCode+" : "+oNpc.name;
                //option.image = Resources.Load(oNpc.image);
                dropdownOptions.Add(option);
            }
            foreach (var character in _data.charactersList)
            {
                var option = new Dropdown.OptionData();
                option.text = character.id+" : "+character.name;
                //option.image = Resources.Load(character.image);
                dropdownOptions.Add(option);
            }
            targetDropdown.AddOptions(dropdownOptions);
            
            // add all the player and npc as toogle on the panel 
            
            // Npc Name
            gameObject.transform.Find("TitlePanel").transform.Find("NpcName").GetComponent<TextMeshProUGUI>().text =
                npc.name;

            // Attack Name 
            gameObject.transform.Find("TitlePanel").transform.Find("AttackName").GetComponent<TextMeshProUGUI>().text =
                skill.name;
            

        }
    }
}