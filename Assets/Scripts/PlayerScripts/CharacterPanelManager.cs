using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System;
public class CharacterPanelManager : MonoBehaviour
{

    public GameObject characterPanel;
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

        // client.on mana change
        // client.on life change
    }

    public void SetPanelInfo(Character character)
    {
        // Character Name
        characterPanel.transform.Find("NamePanel").GetComponent<GameObject>().transform.Find("Name").GetComponent<TextMeshProUGUI>().text = name;

        GameObject basicInfoPanel = characterPanel.transform.Find("basicInfoPanel").GetComponent<GameObject>();

        // Life
        InputField inputFieldLife = basicInfoPanel.transform.Find("Life").GetComponent<InputField>();
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
        InputField inputFieldLifeMax = basicInfoPanel.transform.Find("LifeMax").GetComponent<InputField>();
        inputFieldLifeMax.text = character.lifeMax.ToString();
        inputFieldLifeMax.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.lifeMax = Int32.Parse(inputFieldLifeMax.text);
            // TODO send to the server the changement

        });

        // mana
        InputField inputFieldMana = basicInfoPanel.transform.Find("Mana").GetComponent<InputField>();
        inputFieldMana.text = character.mana.ToString();
        inputFieldMana.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.mana = Int32.Parse(inputFieldMana.text);
            // TODO send to the server the changement

        });

        // ManaMaw
        InputField inputFieldManaMax = basicInfoPanel.transform.Find("ManaMax").GetComponent<InputField>();
        inputFieldManaMax.text = character.manaMax.ToString();
        inputFieldManaMax.onEndEdit.AddListener(delegate {
            // TODO verify if correct value
            // change data in object character
            character.manaMax = Int32.Parse(inputFieldManaMax.text);
            // TODO send to the server the changement

        });
    }
}
