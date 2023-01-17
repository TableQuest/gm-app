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
            Debug.Log("Open Add Npc Panel " +npc.id);

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
                    //
                    Destroy(addNpcPanel);
                    Debug.Log("remove window add npc");
                });

            // change name title
            addNpcPanel.transform.Find("TitlePanel").Find("NpcName").GetComponent<TextMeshProUGUI>().text = npc.name;
            
            // change name field
            /*
            addNpcPanel.transform.Find("NewNameField").GetComponent<TMP_InputField>().onEndEdit.AddListener(
                delegate(string arg0)
                {
                    Debug.Log(arg0);
                    Debug.Log(addNpcPanel.transform.Find("NewNameField").GetComponent<TMP_InputField>().text);
                });
            */
            // add npc button 
            addNpcPanel.transform.Find("AddButton").GetComponent<Button>().onClick.AddListener(
                delegate
                {
                    var newNpc = new Npc(npc.id, 
                        addNpcPanel.transform.Find("NewNameField").GetComponent<TMP_InputField>().text
                        , npc.lifeMax, npc.life, npc.description, npc.image);
                    _data.placedNpcList.Add(newNpc);
                    _client.client.EmitAsync("newNpc", newNpc.id);
                    Destroy(addNpcPanel);
                    
                    GameObject.Find("Canvas").GetComponent<NpcSceneManager>().AddNpcToScrollView(newNpc, 
                        GameObject.Find("Canvas").GetComponent<NpcSceneManager>().scrollViewContentListPlacedNpc);
                });
        }
        
        
        private void SendModificationToServer(string variable, string value)
        {
            NpcUpdateInfo cui = new(_npcOfPanel.id, variable, value);
            var json = JsonUtility.ToJson(cui);
            _client.client.EmitAsync("updateInfoNpc", json);
        }
    }
}