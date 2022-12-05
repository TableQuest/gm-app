using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Datas : MonoBehaviour
{
    public List<Character> characterslList;
    private const string URL = "localhost:3000";

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(GetAllPlayers(URL+"/characters"));
        // init characterList with rest request
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    IEnumerator GetAllPlayers(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            string data =www.downloadHandler.text;
            Debug.Log(data);
            characterslList = JsonUtility.FromJson<List<Character>>(data);
            Debug.Log(characterslList);

        }
    }
}
