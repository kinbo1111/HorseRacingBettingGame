using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetsLoader : MonoBehaviour
{

    public List<string> textureURLs = new List<string>();
    public List<string> videoURLs = new List<string>();
    [ReadOnly] public List<Texture2D> downloadedTextures = new List<Texture2D>();
    [ReadOnly] public List<string> downloadedVideoPaths = new List<string>();
    [ReadOnly] public bool adsDataDownloadDone, textureDownloadDone, videoDownloadDone, assetLoadDone;

    IEnumerator Start()
    {
        Test_Init();

        StartCoroutine(DownloadAdsData());
        yield return new WaitUntil(() => adsDataDownloadDone);

        StartCoroutine(DownloadTextures());
        yield return new WaitUntil(() => textureDownloadDone);

        // StartCoroutine(DownloadVideos());
        //yield return new WaitUntil(() => videoDownloadDone);

        assetLoadDone = true;
    }

    void Test_Init()
    {
        textureURLs.Add("https://i.ibb.co/3hbKt4n/imgpsh-fullsize-anim.jpg");
    }

    IEnumerator DownloadAdsData()
    {
        adsDataDownloadDone = true;
        yield return null;
    }

    IEnumerator DownloadTextures()
    {
        for (int i = 0; i < textureURLs.Count; i++)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(textureURLs[i]))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError
                    || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    downloadedTextures.Add(DownloadHandlerTexture.GetContent(uwr));
                }
            }
        }
        textureDownloadDone = true;
    }

    IEnumerator DownloadVideos()
    {
        for (int i = 0; i < videoURLs.Count; i++)
        {
            using (UnityWebRequest uwr = UnityWebRequest.Get(videoURLs[i]))
            {
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.ConnectionError
                    || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    string path = System.IO.Path.Combine(Application.persistentDataPath, "downloadedVideo.mp4");
                    System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
                    downloadedVideoPaths.Add(path);
                }
            }
        }
        videoDownloadDone = true;
    }

}
