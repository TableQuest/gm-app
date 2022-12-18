using SocketIOClient;
using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ModeManager : MonoBehaviour
{
    // Socket and mainthread
    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;

    // Datas
    Datas datas;
    [SerializeField] GameObject dataObject;

    // Start is called before the first frame update
    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        datas = dataObject.GetComponent<Datas>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        setButtonsAction();

        StartCoroutine(myUpdate());
    }


    void SocketThread()
    {
        while (client == null)
        {
            client = initClient.client;
            Thread.Sleep(500);
        }

        // client.on

    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            // Wait until a callback action is added to the queue
            yield return new WaitUntil(() => initClient._mainThreadhActions.Count > 0);
            // If this fails something is wrong ^^
            // simply get the first added callback
            if (!initClient._mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            // Execute the code of the added callback
            action?.Invoke();
        }
    }

    private void setButtonsAction()
    {
        Button FreeButoon = gameObject.transform.Find("GlobalPanel").Find("ModePanel").Find("FreeButton").GetComponent<Button>();
        Button RestrictedButoon = gameObject.transform.Find("GlobalPanel").Find("ModePanel").Find("RestrictedButton").GetComponent<Button>();
        Button TurnButoon = gameObject.transform.Find("GlobalPanel").Find("ModePanel").Find("TurnButton").GetComponent<Button>();

        FreeButoon.onClick.AddListener(delegate { sendStateFree(); });
        RestrictedButoon.onClick.AddListener(delegate { sendStateRestricted(); });
        TurnButoon.onClick.AddListener(delegate { sendStateTurn(); });
    }

    private void sendStateFree()
    {
        client.EmitAsync("switchState", "FREE");
        datas.gameState = GameState.FREE;
        Debug.Log("switchState FREE");
    }

    private void sendStateRestricted()
    {
        client.EmitAsync("switchState", "RESTRICTED");
        datas.gameState = GameState.RESTRICTED;
        Debug.Log("switchState Resctricted");
    }

    private void sendStateTurn()
    {
        client.EmitAsync("switchState", "TURN");
        datas.gameState = GameState.TURN;
        Debug.Log("switchState TURN");
    }

}
