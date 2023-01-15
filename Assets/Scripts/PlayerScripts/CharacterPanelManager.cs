using System;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanelManager : MonoBehaviour
{
    private Character characterOfPanel;

    // Socket
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

        StartCoroutine(myUpdate());
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

    }

    public void SetPanelInfo(Character character)
    {
        characterOfPanel = character;

        // Character Name
        gameObject.transform.Find("NamePanel").transform.Find("Name").GetComponent<TextMeshProUGUI>().text = character.name;

        Transform basicInfoPanel = gameObject.transform.Find("BasicInfoPanel");

        // Life
        TMP_InputField inputFieldLife = basicInfoPanel.Find("Life").GetComponent<TMP_InputField>();

        inputFieldLife.text = character.life.ToString();
        inputFieldLife.onEndEdit.AddListener(delegate
        {
            // TODO verify if correct value
            // change data in object character
            character.life = Int32.Parse(inputFieldLife.text);
            // send to the server the changement
            SendModificationToServer("life", character.life.ToString());
        });

        // LifeMax
        TMP_InputField inputFieldLifeMax = basicInfoPanel.Find("LifeMax").GetComponent<TMP_InputField>();
        inputFieldLifeMax.text = character.lifeMax.ToString();
        inputFieldLifeMax.onEndEdit.AddListener(delegate
        {
            // TODO verify if correct value
            // change data in object character
            character.lifeMax = Int32.Parse(inputFieldLifeMax.text);
            // send to the server the changement
            SendModificationToServer("lifeMax", character.lifeMax.ToString());
        });

        // mana
        TMP_InputField inputFieldMana = basicInfoPanel.Find("Mana").GetComponent<TMP_InputField>();
        inputFieldMana.text = character.mana.ToString();
        inputFieldMana.onEndEdit.AddListener(delegate
        {
            // TODO verify if correct value
            // change data in object character
            character.mana = Int32.Parse(inputFieldMana.text);
            // send to the server the changement
            SendModificationToServer("mana", character.mana.ToString());


        });

        // ManaMax
        TMP_InputField inputFieldManaMax = basicInfoPanel.Find("ManaMax").GetComponent<TMP_InputField>();
        inputFieldManaMax.text = character.manaMax.ToString();
        inputFieldManaMax.onEndEdit.AddListener(delegate
        {
            // TODO verify if correct value
            // change data in object character
            character.manaMax = Int32.Parse(inputFieldManaMax.text);
            // send to the server the changement
            SendModificationToServer("manaMax", character.manaMax.ToString());

        });

        // Steps
        TMP_InputField inputFieldSteps = gameObject.transform.Find("StepPanel").Find("NumberSteps").GetComponent<TMP_InputField>();
        inputFieldSteps.text = character.speed.ToString();
        inputFieldSteps.onEndEdit.AddListener(delegate
        {
            // TODO verify if correct value
            // change data in object character
            character.speed = Int32.Parse(inputFieldSteps.text);
            // send to the server the changement
            SendModificationToServer("speed", character.speed.ToString());

        });

        // Image 
        Sprite sprite = Resources.Load<Sprite>("Pictures/dwarf");
        switch (character.name)
        {
            case "Dwarf":
                sprite = Resources.Load<Sprite>("Pictures/dwarf");
                break;
            case "Elf":
                sprite = Resources.Load<Sprite>("Pictures/elf");
                break;
        }
        basicInfoPanel.Find("Image").GetComponent<Image>().sprite = sprite;

    }

    public void SendModificationToServer(string variable, string value)
    {
        CharacterUpdateInfo cui = new(characterOfPanel.playerId, variable, value);
        string json = JsonUtility.ToJson(cui);
        client.client.EmitAsync("updateInfoCharacter", json);
    }
}