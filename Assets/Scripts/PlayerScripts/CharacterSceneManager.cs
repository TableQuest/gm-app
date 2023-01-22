using UnityEngine;
using TMPro;
using SocketIOClient;
using System.Threading;
using System;
using System.Collections;
using UnityEngine.UI;

namespace PlayerScripts
{
    public class CharacterSceneManager : MonoBehaviour
    {
        // Socket and mainthread
        private InitiatlisationClient _initClient;
        [SerializeField] private GameObject clientObject;
        private SocketIO _client;

        // Prefabs
        public GameObject characterPanelPrefab;
        public GameObject characterButtonPrefab;
        public Transform scrollViewContentList;
        public Transform scrollViewContentPlayer;

        // Datas
        Datas _data;
        [SerializeField] private GameObject dataObject;


        // Present character panel
        //private int idCharacterOnPanel;

        void Start()
        {
            clientObject = GameObject.Find("SocketIOClient");
            _initClient = clientObject.GetComponent<InitiatlisationClient>();

            dataObject = GameObject.Find("DataContainer");
            _data = dataObject.GetComponent<Datas>();

            //idCharacterOnPanel = -1;

            // Create a new thread in order to run the InitSocketThread method
            var thread = new Thread(SocketThread);
            // start the thread
            thread.Start();

            AddAllCharacterToScrollView();

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

        private void AddAllCharacterToScrollView()
        {
            foreach (var c in _data.charactersList)
            {

                AddCharacterToScrollView(c);
            }
        }

        private void AddCharacterToScrollView(Character character)
        {
            var characterButton = Instantiate(characterButtonPrefab);
            characterButton.transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;
            characterButton.GetComponent<UnityEngine.UI.Button>().onClick
                .AddListener(delegate { PrintCharacterPanel(character); });

            // Image
            var sprite = Resources.Load<Sprite>(character.image);
            characterButton.transform.Find("Image").GetComponent<Image>().sprite = sprite;

            characterButton.transform.SetParent(scrollViewContentList);
            characterButton.transform.localScale = new Vector3(1, 1, 1);
        }

        public void PrintCharacterPanel(Character character)
        {
            // if scroll view not empty remove the panel 
            var panelPresence = scrollViewContentPlayer.childCount != 0;
            if (panelPresence)
            {
                Destroy(scrollViewContentPlayer.GetChild(0).gameObject);
                //idCharacterOnPanel = -1;
            }

            //idCharacterOnPanel = character.id;
            // Create the panel 
            var characterPanel = Instantiate(characterPanelPrefab);
            characterPanel.transform.position = gameObject.transform.position;
            characterPanel.transform.SetParent(scrollViewContentPlayer);
            characterPanel.transform.localScale = new Vector3(1, 1, 1);
            // set information
            characterPanel.GetComponent<CharacterPanelManager>().SetPanelInfo(character);
        }

    }

    [Serializable]
    public class UpdateLife
    {
        public string id;
        public int life;
    }
}