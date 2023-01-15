using System.Collections;
using System.Threading;
using NPCScripts;
using SocketIOClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NPCScripts
{
    public class NpcSceneManager : MonoBehaviour
    {

        // Socket and mainthread
        private InitiatlisationClient _initClient;
        private GameObject _clientObject;
        private SocketIO _client;

        // Prefabs
        public GameObject npcPanelPrefab;
        public GameObject npcButtonPrefab;
        
        public Transform scrollViewContentListNpc;
        public Transform scrollViewContentListPlacedNpc;
        public Transform scrollViewContentNpc;
        
        // Data
        private Datas _data;
        private GameObject _dataObject;

        private void Start()
        {
            _clientObject = GameObject.Find("SocketIOClient");
            _initClient = _clientObject.GetComponent<InitiatlisationClient>();

            _dataObject = GameObject.Find("DataContainer");
            _data = _dataObject.GetComponent<Datas>();

            //_idCharacterOnPanel = -1;

            // Create a new thread in order to run the InitSocketThread method
            var thread = new Thread(SocketThread);
            // start the thread
            thread.Start();
            
            AddAllNpcToScrollViewNpc();
            AddAllPlacedNpcToScrollViewPlacedNpc();
        }

        private void SocketThread()
        {
            while (_client == null)
            {
                _client = _initClient.client;
                Thread.Sleep(500);
            }

            // client.on

        }

        private IEnumerator MyUpdate()
        {
            while (true)
            {
                // Wait until a callback action is added to the queue
                yield return new WaitUntil(() => _initClient._mainThreadhActions.Count > 0);
                // If this fails something is wrong ^^
                // simply get the first added callback
                if (!_initClient._mainThreadhActions.TryDequeue(out var action))
                {
                    Debug.LogError("Something Went Wrong ! ", this);
                    yield break;
                }

                // Execute the code of the added callback
                action?.Invoke();
            }
        }

        private void AddAllNpcToScrollViewNpc()
        {
            foreach (var n in _data.npcList)
            {
                AddNpcToScrollView(n, scrollViewContentListNpc);
            }
        }

        private void AddAllPlacedNpcToScrollViewPlacedNpc()
        {
            foreach (var npc in _data.placedNpcList)
            {
                AddNpcToScrollView(npc, scrollViewContentListPlacedNpc) ;
            }
        }

        private void AddNpcToScrollView(Npc npc, Transform scrollViewContent)
        {
            var npcButton = Instantiate(npcButtonPrefab);
            npcButton.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = npc.name;
            npcButton.GetComponent<Button>().onClick.AddListener(delegate { PrintNpcPanel(npc); });

            // Image 
            
            var sprite = Resources.Load<Sprite>(npc.image);
            npcButton.transform.Find("Image").GetComponent<Image>().sprite = sprite;
            
            npcButton.transform.SetParent(scrollViewContent);
            npcButton.transform.localScale = new Vector3(1, 1, 1);
        }

        public void PrintNpcPanel(Npc npc)
        {
            // if scroll view not empty remove the panel 
            var panelPresence = scrollViewContentNpc.childCount != 0;
            if (panelPresence)
            {
                Destroy(scrollViewContentNpc.GetChild(0).gameObject);
                //idCharacterOnPanel = -1;
            }
            //idCharacterOnPanel = character.id;

            // Create the panel 
            var npcPanel = Instantiate(npcPanelPrefab);
            npcPanel.transform.position = gameObject.transform.position;
            npcPanel.transform.SetParent(scrollViewContentNpc);
            npcPanel.transform.localScale = new Vector3(1, 1, 1);
            // set information
            npcPanel.GetComponent<NpcPanelManager>().SetInfoPanel(npc);
        }

        public void TestFunction()
        {
            Debug.Log("Test funcction");
        }
    }
}