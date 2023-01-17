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
        [SerializeField] GameObject dataObject;

        private void Start()
        {
            _clientObject = GameObject.Find("SocketIOClient");
            _initClient = _clientObject.GetComponent<InitiatlisationClient>();

            _initData = dataObject.GetComponent<Datas>();

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
                _initData = dataObject.GetComponent<Datas>();
                Thread.Sleep(300);
            }

            _client.On("updateInfoNpc", (data) =>
            {
                var json = data.GetValue(0);
                _initClient._mainThreadhActions.Enqueue(() =>
                {
                    var nui = JsonUtility.FromJson<NpcUpdateInfo>(json.ToString());
                    UpdateInfoNpc(nui.pawnCode, nui.variable, nui.value);
                    var npc = _initData.placedNpcList.Find(c => c.pawnCode== nui.pawnCode);     
                    Debug.Log("Udpate inf npc " + npc.name);
                    //GameObject.Find("Canvas").GetComponent<NpcSceneManager>().PrintNpcPanel(npc);
                });
            });
            
            _client.On("newNpc", (data) =>
            {
                var pawnCode = data.GetValue().ToString();
                _initData.placedNpcList[^1].pawnCode = pawnCode;
                Debug.Log("Set pawncode "+pawnCode+" to last npc");
            });

        }

        private void UpdateInfoNpc(string pawnCode, string variable, string value)
        {
            var npc = _initData.placedNpcList.Find(c => c.pawnCode == pawnCode);

            switch (variable)
            {
                case "life":
                    try
                    {
                        npc.life = int.Parse(value);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Life value is not numerical: " + e);
                    }
                    break;
                case "lifeMax":
                    try
                    {
                        npc.lifeMax = Int32.Parse(value);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("LifeMax value is not numerical: " + e);
                    }
                    break;
            }
        }
    }
}

[Serializable]
public class NpcUpdateInfo
{
    public string pawnCode;
    public string variable;
    public string value;
    
    public NpcUpdateInfo(string pawnCode, string variable, string value)
    {
        this.pawnCode = pawnCode;
        this.value = value;
        this.variable = variable;
    }
    
}