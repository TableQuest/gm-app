using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;

public class Logger : MonoBehaviour
{
    private InitiatlisationClient _initClient;

    private SocketIO _client;
    private GameObject _clientObject;

    [SerializeField]
    GameObject logPanelPrefab;
    // Start is called before the first frame update
    void Start()
    {
        _clientObject = GameObject.Find("SocketIOClient");
        _initClient = _clientObject.GetComponent<InitiatlisationClient>();

        // dataObject = GameObject.Find("DataContainer");
        // _data = dataObject.GetComponent<Datas>();
        // Debug.Log("start attack manager "+_data.placedNpcList);
        var thread = new Thread(SocketThread);
        thread.Start();

        StartCoroutine(myUpdate());
    }
        
    private IEnumerator myUpdate()
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

    void SocketThread()
    {
        while (_client == null)
        {

            _clientObject = GameObject.Find("SocketIOClient");
            _initClient = _clientObject.GetComponent<InitiatlisationClient>();
            _client = _initClient.client;
            Thread.Sleep(500);
        }
        // while (_data == null)
        // {
        //     Debug.Log("Data null attack manager");
        //     dataObject = GameObject.Find("DataContainer");
        //     _data = dataObject.GetComponent<Datas>();
        //     Thread.Sleep(300);
        // }

        _client.On("log", (data) =>
        {
            _initClient._mainThreadhActions.Enqueue(() =>
            {
                LogInfo logInfo = JsonUtility.FromJson<LogInfo>(data.GetValue(0).ToString());
                Debug.Log("Logging " + logInfo.logText);

                GameObject logPanel = Instantiate(logPanelPrefab);
                Transform logScrollView = gameObject.transform.Find("GlobalPanel").Find("LogsPanel").Find("ScrollView");
                logPanel.transform.SetParent(logScrollView.Find("Viewport").Find("Content"));

                fillLogPanel(logInfo, logPanel.transform);
                
                logScrollView.GetComponent<ScrollRect>().verticalNormalizedPosition = 0;
            });
        });

    }

    private void fillLogPanel(LogInfo log, Transform logPanelTransform)
    {
        // Image 
        Sprite sprite = Resources.Load<Sprite>(log.imagePath);
        logPanelTransform.Find("HeaderGroup").Find("Image").GetComponent<Image>().sprite = sprite;

        //Title
        logPanelTransform.Find("HeaderGroup").Find("Title").GetComponent<TextMeshProUGUI>().text = log.title;

        //Content
        logPanelTransform.Find("LogText").GetComponent<TextMeshProUGUI>().text = log.logText;

        logPanelTransform.transform.localScale = Vector3.one;
    }
}

public class LogInfo
{
    public string imagePath;
    public string title;
    public string logText;
}