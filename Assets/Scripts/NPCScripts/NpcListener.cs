using System;
using UnityEngine;
using SocketIOClient;
using System.Threading;
using System.Collections;

namespace NPCScripts
{
    public class NpcListener : MonoBehaviour
    {
        private InitiatlisationClient _initClient;
        private GameObject _clientObject;
        private SocketIO _client;

        private Datas _initData;
        private GameObject _dataObject;

        private void Start()
        {
            _clientObject = GameObject.Find("SocketIOClient");
            _initClient = _clientObject.GetComponent<InitiatlisationClient>();

            _dataObject = GameObject.Find("DataContainer");
            _initData = _dataObject.GetComponent<Datas>();

            var thread = new Thread(SocketThread);
            thread.Start();

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
                _initData = _dataObject.GetComponent<Datas>();
                Thread.Sleep(300);
            }

            _client.On("updateInfoNPC", (data) =>
            {
                var json = data.GetValue(0);
                var nui = JsonUtility.FromJson<NpcUpdateInfo>(json.ToString());
                UpdateInfoNpc(nui.npcId, nui.variable, nui.value);
                var npc = _initData.npcList.Find(c => c.id== nui.npcId);
                
                // TODO maj when the scene is the npc scene
                /*
                Debug.Log(SceneManager.GetActiveScene().GetRootGameObjects());
                if (SceneManager.GetActiveScene().name == "Player")
                {
                    
                    Debug.Log("root game objects" + SceneManager.GetActiveScene().GetRootGameObjects());
                    //int printedPanel = SceneManager.GetActiveScene().Get 
                }*/
            });

        }

        private void UpdateInfoNpc(string npcId, string variable, string value)
        {
            var character = _initData.npcList.Find(c => c.id == npcId);
            /*
            switch (variable)
            {
                
                case "life":
                    try
                    {
                        character.life = Int32.Parse(value);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Life value is not numerical: " + e);
                    }

                    break;
            }*/
        }
    }
}

[Serializable]
public class NpcUpdateInfo
{
    public string npcId;
    public string variable;
    public string value;
    
    public NpcUpdateInfo(string npcId, string variable, string value)
    {
        this.npcId = npcId;
        this.value = value;
        this.variable = variable;
    }
    
}