using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class ListenTest : MonoBehaviour
{

    InitiatlisationClient initClient;
    [SerializeField] GameObject clientObject;
    SocketIO client;
    TextMeshProUGUI sendButton;

    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    private IEnumerator Start()
    {
        initClient = clientObject.GetComponent<InitiatlisationClient>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(SocketThread);
        // start the thread
        thread.Start();

        // Wait until a callback action is added to the queue
        yield return new WaitUntil(() => _mainThreadhActions.Count > 0);

        // If this fails something is wrong ^^
        // simply get the first added callback
        if (!_mainThreadhActions.TryDequeue(out var action))
        {
            Debug.LogError("Something Went Wrong ! ", this);
            yield break;
        }

        // Execute the code of the added callback
        action?.Invoke();
    }

    void SocketThread()
    {
        while (client == null)
        {
      
            Debug.Log("Client null");
            initialisationClient();
            Thread.Sleep(500);
        }

        client.On("test", (data) =>

        {
            string str = data.GetValue(0).ToString();
            // Simply wrap your main thread code by wrapping it in a lambda expression
            // which is enqueued to the thread-safe queue
            _mainThreadhActions.Enqueue(() =>
            {
                // This will be executed after the next Update call
                createButton();
            });
        });

        
    }

    public GameObject buttonPrefab;
    private void initialisationClient()
    {
        client = initClient.client;
    }

    public void testClient()
    {
        if (client == null)
        {
            initialisationClient();
        }
        //client.EmitAsync("hello", "coucou");
        createButton();
    }

    public void createButton()
    {
        GameObject go = Instantiate(buttonPrefab);
        go.transform.position = gameObject.transform.position;
        go.GetComponent<RectTransform>().SetParent(gameObject.transform);
        go.GetComponent<Button>().onClick.AddListener(FooOnClick);

    }

    void FooOnClick()
    {
        if (client == null)
        {
            initialisationClient();
        }
        client.EmitAsync("hello", "coucou");
    }
}
