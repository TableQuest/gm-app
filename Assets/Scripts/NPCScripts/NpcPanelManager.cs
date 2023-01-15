using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
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
        private GameObject _npcPanelPrefab;

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

            // client.On

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

            
        }
        
        private void SendModificationToServer(string variable, string value)
        {
            NpcUpdateInfo cui = new(_npcOfPanel.id, variable, value);
            var json = JsonUtility.ToJson(cui);
            _client.client.EmitAsync("updateInfoNpc", json);
        }
    }
}