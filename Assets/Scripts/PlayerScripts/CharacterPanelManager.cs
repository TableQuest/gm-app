using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CharacterPanelManager : MonoBehaviour
{
    private Character character;

    // Socket and http
    InitiatlisationClient client;
    GameObject clientObject;

    // Prefab
    GameObject characterPanelPrefab;

    // Datas
    Datas data;
    GameObject dataObject;

    private void Start()
    {
        clientObject = GameObject.Find("SocketIOClient");
        client = clientObject.GetComponent<InitiatlisationClient>();

        dataObject = GameObject.Find("DataContainer");
        data = dataObject.GetComponent<Datas>();


        var thread = new Thread(SocketThread);
        thread.Start();
    }

    private IEnumerator myUpdate()
    {
        while (true)
        {
            yield return new WaitUntil(() => client._mainThreadhActions.Count > 0);

            if (!client._mainThreadhActions.TryDequeue(out var action))
            {
                Debug.LogError("Something Went Wrong ! ", this);
                yield break;
            }

            action?.Invoke();
        }
    }


    void SocketThread()
    {
        while (client == null)
        {

            clientObject = GameObject.Find("SocketIOClient");
            client = clientObject.GetComponent<InitiatlisationClient>();
            Thread.Sleep(500);
        }
        while (data == null)
        {
            dataObject = GameObject.Find("DataContainer");
            data = dataObject.GetComponent<Datas>();
            Thread.Sleep(300);
        }

        // client.On

        // client.on mana / manaMax change
        // client.on life / lifeMax change
    }

    public void SetPanelInfo(Character character)
    {
        // Character Name
        gameObject.transform.Find("NamePanel").transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;

        Transform basicInfoPanel = gameObject.transform.Find("BasicInfoPanel");

        // Life
        Debug.Log("find: " + basicInfoPanel.Find("Life"));
        Debug.Log("inputfield: " + basicInfoPanel.Find("Life").GetComponent<TMP_InputField>());
        TMP_InputField inputFieldLife = basicInfoPanel.Find("Life").GetComponent<TMP_InputField>();

        inputFieldLife.text = character.life.ToString();
        inputFieldLife.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.life = Int32.Parse(inputFieldLife.text);
            // send to the server the changement
            UpdateLife up = new UpdateLife() { id = character.playerId, life = character.life };
            string json = JsonUtility.ToJson(up);
            client.client.EmitAsync("updateLifePlayer", json);
            });

        // LifeMax
        TMP_InputField inputFieldLifeMax = basicInfoPanel.Find("LifeMax").GetComponent<TMP_InputField>();
        inputFieldLifeMax.text = character.lifeMax.ToString();
        inputFieldLifeMax.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.lifeMax = Int32.Parse(inputFieldLifeMax.text);
            // TODO send to the server the changement

        });

        // mana
        TMP_InputField inputFieldMana = basicInfoPanel.Find("Mana").GetComponent<TMP_InputField>();
        inputFieldMana.text = character.mana.ToString();
        inputFieldMana.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.mana = Int32.Parse(inputFieldMana.text);
            // TODO send to the server the changement

        });

        // ManaMax
        TMP_InputField inputFieldManaMax = basicInfoPanel.Find("ManaMax").GetComponent<TMP_InputField>();
        inputFieldManaMax.text = character.manaMax.ToString();
        inputFieldManaMax.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.manaMax = Int32.Parse(inputFieldManaMax.text);
            // TODO send to the server the changement

        });
    }



}
