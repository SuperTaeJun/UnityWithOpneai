using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Net.Sockets;
using UnityEditor.PackageManager;
public class WebText : MonoBehaviour
{
    public Text MyTextUI;

    void Start()
    {
        StartCoroutine(GetText());
    }

    IEnumerator GetText()
    {
        string url = "https://openapi.naver.com/v1/search/news.json?query=ÀÌ½º¶ó¿¤&display=30";

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("X-Naver-Client-Id", "KCc6AY8Hc2Nf8IY4O3kz");
        www.SetRequestHeader("X-Naver-Client-Secret", "49AJ207GFg");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            // Show results as text
            MyTextUI.text = www.downloadHandler.text;
        }
    }
}
