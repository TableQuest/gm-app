using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SocketIOClient;
using System.Threading;
using System.Collections.Concurrent;
using System;

public class ButtonScript : MonoBehaviour
{
    Text textField;
    TextMeshProUGUI textMesh;
    SocketIO client;

    // A thread-safe queue for passing a callback into the next Update call
    private readonly ConcurrentQueue<Action> _mainThreadhActions = new ConcurrentQueue<Action>();

    /*
    async void Start()
    {
        
        Debug.Log(textMesh);
        client = new SocketIO("http://localhost:3000/");

        client.On("world", response =>
        {
            Debug.Log(response);

            string textRecieve = response.GetValue<string>();
            ChangeText(textRecieve);

        });

        client.On("mjConnection", response =>
        {
            Debug.Log(response);

            string textRecieve = response.GetValue<string>();
            ChangeText(textRecieve);
            Debug.Log(textField.text);
            Debug.Log(textMesh.text);
        });


        await client.ConnectAsync();
    }

    */
    // Yes! You can make Start return IEnumerator
    // In this case Unity automatically starts it as a Coroutine instead
    private IEnumerator Start()
    {
        textField = GameObject.Find("TextToChange").GetComponent<Text>();
        textMesh = GameObject.Find("TmpToChange").GetComponent<TextMeshProUGUI>();

        // Create a new thread in order to run the InitSocketThread method
        var thread = new Thread(InitSocketThread);
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

    void InitSocketThread()
    {
        if (client == null)
        {
            client = new SocketIO("http://localhost:3000/");
            client.On("mjConnection", (data) =>
            {
                string str = data.GetValue(0).ToString();
                // Simply wrap your main thread code by wrapping it in a lambda expression
                // which is enqueued to the thread-safe queue
                _mainThreadhActions.Enqueue(() =>
                {
                    // This will be executed after the next Update call
                    textField.text = str;
                    textMesh.text = str;
                    //ChangeText(str);
                });
            });
            client.ConnectAsync();

        }
    }


    public async void onButtonClick(){
        //textField.text = "Nofeiofsdfhli"; 
        await client.EmitAsync("mjConnection", "world");
    }

    
// j'attend un message 
// puis je change le texte


    void ChangeText(string txt){
        textField.text = txt;
        textMesh.SetText(txt);
        textMesh.text= txt;
    }

    void Update(){

    }

}
