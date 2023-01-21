using SocketIOClient;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ModeListener : MonoBehaviour
{
    InitiatlisationClient client;
    GameObject clientObject;
    public readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();
    // Datas
    Datas datas;
    [SerializeField] GameObject dataObject;


    // Start is called before the first frame update
    void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        client = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        datas = dataObject.GetComponent<Datas>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        DontDestroyOnLoad(gameObject);
    }

    void SocketThread()
    {
        while (client == null)
        {
            client = client = clientObject.GetComponent<InitiatlisationClient>(); ;
            Thread.Sleep(500);
        }

        // client.on
        client.client.On("switchState", (data) =>
        {
            _mainThreadhActions.Enqueue(() =>
            {
                switch (data.GetValue<string>(0))
                {
                    case "FREE":
                        datas.gameState = GameState.FREE;
                        break;
                    case "RESTRICTED":
                        datas.gameState = GameState.RESTRICTED;
                        break;
                    case "INIT_TURN_ORDER":
                        datas.gameState = GameState.INIT_TURN_ORDER;
                        break;
                    case "TURN_ORDER": // what is send when all the dice where launch 
                        datas.gameState = GameState.TURN_ORDER;
                        break;
                    default:
                        break;
                }
            });
        });


    }

}
