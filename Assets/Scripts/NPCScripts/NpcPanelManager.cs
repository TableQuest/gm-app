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

namespace NPCScripts
{
    public class NpcPanelManager : MonoBehaviour
    {
        private Npc _npcOfPanel;
        
        // Socket
        private InitiatlisationClient _client;
        private GameObject _clientObject;
        
        // Prefab
        [SerializeField]
        private GameObject addNpcPanelPrefab;

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

        public void SetInfoPanel(Npc npc)
        {
            _npcOfPanel = npc;
            
            // TODO fill the panel with the npc infos
            
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
            var sprite = Resources.Load<Sprite>(npc.image);
            basicInfoPanel.Find("Image").GetComponent<Image>().sprite = sprite;

            // Add Npc Button 
            var addNpcButton = gameObject.transform.Find("PlacedPanel").Find("PlacedButton").GetComponent<Button>();
            addNpcButton.onClick.AddListener(
                delegate
                {
                    PrintAddNpcPanel(npc);
                }
            );
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
                    var newNpc = new Npc(npc.id, name, npc.lifeMax, npc.life, npc.description, npc.image);
                    _data.placedNpcList.Add(newNpc);
                    var objJson = new NewNpc(newNpc.id, newNpc.name);
                    _client.client.EmitAsync("newNpc", JsonUtility.ToJson(objJson));
                    Destroy(addNpcPanel);
                    GameObject.Find("Canvas").GetComponent<NpcSceneManager>().AddNpcToScrollView(newNpc, 
                        GameObject.Find("Canvas").GetComponent<NpcSceneManager>().scrollViewContentListPlacedNpc);
                    Debug.Log("Add Npc of id "+newNpc.id + " named "+newNpc.name);
                });
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