using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class AssetsLoader : MonoBehaviour
{

    [Header("Ads CMS")]
    [SerializeField] string adsBaseUrl = "https://keibadeikkaku.top/ads";
    [SerializeField] string adsLoginUsername = "admin";
    [SerializeField] string adsLoginPassword = "admin123";
    [SerializeField] bool useFallbackAssetWhenLoadFailed = true;

    public List<string> textureURLs = new List<string>();
    public List<string> videoURLs = new List<string>();
    [ReadOnly] public List<string> videoClickUrls = new List<string>();
    [ReadOnly] public List<Texture2D> downloadedTextures = new List<Texture2D>();
    [ReadOnly] public List<string> downloadedVideoPaths = new List<string>();
    [ReadOnly] public List<string> downloadedVideoClickUrls = new List<string>();
    [ReadOnly] public bool adsDataDownloadDone, textureDownloadDone, videoDownloadDone, assetLoadDone;

    IEnumerator Start()
    {
        adsDataDownloadDone = false;
        textureDownloadDone = false;
        videoDownloadDone = false;
        assetLoadDone = false;

        StartCoroutine(DownloadAdsData());
        yield return new WaitUntil(() => adsDataDownloadDone);

        if (textureURLs.Count > 0)
        {
            StartCoroutine(DownloadTextures());
            yield return new WaitUntil(() => textureDownloadDone);
        }
        else
        {
            textureDownloadDone = true;
        }

        if (videoURLs.Count > 0)
        {
            StartCoroutine(DownloadVideos());
            yield return new WaitUntil(() => videoDownloadDone);
        }
        else
        {
            videoDownloadDone = true;
        }

        assetLoadDone = true;
    }

    void AddFallbackTestAsset()
    {
        if (!textureURLs.Contains("https://i.ibb.co/3hbKt4n/imgpsh-fullsize-anim.jpg"))
        {
            textureURLs.Add("https://i.ibb.co/3hbKt4n/imgpsh-fullsize-anim.jpg");
        }
    }

    IEnumerator DownloadAdsData()
    {
        textureURLs.Clear();
        videoURLs.Clear();
        videoClickUrls.Clear();
        downloadedTextures.Clear();
        downloadedVideoPaths.Clear();
        downloadedVideoClickUrls.Clear();

        string normalizedBase = NormalizeAdsBaseUrl(adsBaseUrl);
        string loginUrl = normalizedBase + "/login.php";
        string manageUrl = normalizedBase + "/manage_items.php";

        WWWForm form = new WWWForm();
        form.AddField("username", adsLoginUsername);
        form.AddField("password", adsLoginPassword);

        string phpSessionCookie = string.Empty;
        using (UnityWebRequest loginReq = UnityWebRequest.Post(loginUrl, form))
        {
            UnityWebRequestAsyncOperation loginOp = TryStartWebRequest(loginReq, "Ads login");
            if (loginOp == null)
            {
                if (useFallbackAssetWhenLoadFailed)
                {
                    AddFallbackTestAsset();
                }
                adsDataDownloadDone = true;
                yield break;
            }

            yield return loginOp;

            if (loginReq.result == UnityWebRequest.Result.ConnectionError
                || loginReq.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Ads login failed: " + loginReq.error);
            }
            else
            {
                if (loginReq.GetResponseHeaders() != null
                    && loginReq.GetResponseHeaders().TryGetValue("SET-COOKIE", out string setCookieHeader))
                {
                    phpSessionCookie = ExtractPhpSessionCookie(setCookieHeader);
                }
            }
        }

        using (UnityWebRequest pageReq = UnityWebRequest.Get(manageUrl))
        {
            if (!string.IsNullOrEmpty(phpSessionCookie))
            {
                pageReq.SetRequestHeader("Cookie", phpSessionCookie);
            }

            UnityWebRequestAsyncOperation pageOp = TryStartWebRequest(pageReq, "Ads list fetch");
            if (pageOp == null)
            {
                if (useFallbackAssetWhenLoadFailed && textureURLs.Count == 0 && videoURLs.Count == 0)
                {
                    AddFallbackTestAsset();
                }
                adsDataDownloadDone = true;
                yield break;
            }

            yield return pageOp;

            if (pageReq.result == UnityWebRequest.Result.ConnectionError
                || pageReq.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Ads list fetch failed: " + pageReq.error);
            }
            else
            {
                ParseAdsMediaLinks(pageReq.downloadHandler.text, normalizedBase);
            }
        }

        if (useFallbackAssetWhenLoadFailed && textureURLs.Count == 0 && videoURLs.Count == 0)
        {
            AddFallbackTestAsset();
        }

        adsDataDownloadDone = true;
    }

    UnityWebRequestAsyncOperation TryStartWebRequest(UnityWebRequest request, string requestName)
    {
        try
        {
            return request.SendWebRequest();
        }
        catch (System.InvalidOperationException ex)
        {
            Debug.LogError(requestName + " skipped. " + ex.Message + " URL=" + request.url);
            return null;
        }
    }

    string NormalizeAdsBaseUrl(string baseUrl)
    {
        if (string.IsNullOrEmpty(baseUrl))
        {
            return "https://keibadeikkaku.top/ads";
        }

        return baseUrl.TrimEnd('/');
    }

    string ExtractPhpSessionCookie(string setCookieHeader)
    {
        if (string.IsNullOrEmpty(setCookieHeader))
        {
            return string.Empty;
        }

        Match match = Regex.Match(setCookieHeader, @"PHPSESSID=[^;]+");
        if (match.Success)
        {
            return match.Value;
        }

        return string.Empty;
    }

    void ParseAdsMediaLinks(string html, string normalizedBaseUrl)
    {
        if (string.IsNullOrEmpty(html))
        {
            return;
        }

        // New dashboard format: parse table rows and only include entries marked as active ("表示中").
        bool hasActiveVideo = ParseActiveVideosFromManageTable(html, normalizedBaseUrl);

        // Fallback for old CMS format (collect all links if active marker is missing).
        if (hasActiveVideo)
        {
            return;
        }

        MatchCollection matches = Regex.Matches(html, "(?:href|src)=['\\\"]([^'\\\"]+)['\\\"]", RegexOptions.IgnoreCase);
        for (int i = 0; i < matches.Count; i++)
        {
            string relativePath = matches[i].Groups[1].Value;
            if (string.IsNullOrEmpty(relativePath))
            {
                continue;
            }

            if (relativePath.IndexOf("upload/videos/", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string absVideoUrl = BuildAbsoluteUrl(relativePath, normalizedBaseUrl);
                if (!string.IsNullOrEmpty(absVideoUrl) && !videoURLs.Contains(absVideoUrl))
                {
                    videoURLs.Add(absVideoUrl);
                    videoClickUrls.Add(string.Empty);
                }
            }
            else if (relativePath.IndexOf("upload/images/", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                string absTextureUrl = BuildAbsoluteUrl(relativePath, normalizedBaseUrl);
                if (!string.IsNullOrEmpty(absTextureUrl) && !textureURLs.Contains(absTextureUrl))
                {
                    textureURLs.Add(absTextureUrl);
                }
            }
        }
    }

    bool ParseActiveVideosFromManageTable(string html, string normalizedBaseUrl)
    {
        bool foundActive = false;

        MatchCollection rowMatches = Regex.Matches(html, "<tr>(.*?)</tr>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        for (int i = 0; i < rowMatches.Count; i++)
        {
            string rowHtml = rowMatches[i].Groups[1].Value;
            if (string.IsNullOrEmpty(rowHtml))
            {
                continue;
            }

            bool isActive = rowHtml.IndexOf("表示中", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isActive)
            {
                continue;
            }

            Match previewMatch = Regex.Match(rowHtml, "href=['\\\"]([^'\\\"]*upload/videos/[^'\\\"]+)['\\\"]", RegexOptions.IgnoreCase);
            if (!previewMatch.Success)
            {
                continue;
            }

            string absVideoUrl = BuildAbsoluteUrl(previewMatch.Groups[1].Value, normalizedBaseUrl);
            if (string.IsNullOrEmpty(absVideoUrl) || videoURLs.Contains(absVideoUrl))
            {
                continue;
            }

            string clickUrl = ExtractVideoClickUrlFromRow(rowHtml);
            videoURLs.Add(absVideoUrl);
            videoClickUrls.Add(clickUrl);
            foundActive = true;
        }

        return foundActive;
    }

    string ExtractVideoClickUrlFromRow(string rowHtml)
    {
        // Preferred source: title attribute from the URL column's span.
        Match titleMatch = Regex.Match(rowHtml, "<span[^>]*class=['\\\"]url-text['\\\"][^>]*title=['\\\"]([^'\\\"]+)['\\\"]", RegexOptions.IgnoreCase);
        if (titleMatch.Success)
        {
            string normalized = NormalizeClickUrl(titleMatch.Groups[1].Value);
            if (!string.IsNullOrEmpty(normalized))
            {
                return normalized;
            }
        }

        // Fallback: any direct URL text inside the row.
        Match textUrlMatch = Regex.Match(rowHtml, @"https?://[^\s<>'""]+", RegexOptions.IgnoreCase);
        if (textUrlMatch.Success)
        {
            return NormalizeClickUrl(textUrlMatch.Value);
        }

        return string.Empty;
    }

    string NormalizeClickUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return string.Empty;
        }

        string trimmed = url.Trim();
        if (trimmed == "-" || trimmed == "－")
        {
            return string.Empty;
        }

        if (trimmed.StartsWith("http://") || trimmed.StartsWith("https://"))
        {
            return trimmed;
        }

        return string.Empty;
    }

    string BuildAbsoluteUrl(string maybeRelativePath, string normalizedBaseUrl)
    {
        if (maybeRelativePath.StartsWith("http://") || maybeRelativePath.StartsWith("https://"))
        {
            return maybeRelativePath;
        }

        if (System.Uri.TryCreate(normalizedBaseUrl + "/", System.UriKind.Absolute, out System.Uri baseUri))
        {
            if (System.Uri.TryCreate(baseUri, maybeRelativePath, out System.Uri absUri))
            {
                return absUri.ToString();
            }
        }

        return normalizedBaseUrl + "/" + maybeRelativePath.TrimStart('/');
    }

    IEnumerator DownloadTextures()
    {
        for (int i = 0; i < textureURLs.Count; i++)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(textureURLs[i]))
            {
                UnityWebRequestAsyncOperation textureOp = TryStartWebRequest(uwr, "Ads texture download");
                if (textureOp == null)
                {
                    continue;
                }

                yield return textureOp;

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
                UnityWebRequestAsyncOperation videoOp = TryStartWebRequest(uwr, "Ads video download");
                if (videoOp == null)
                {
                    continue;
                }

                yield return videoOp;

                if (uwr.result == UnityWebRequest.Result.ConnectionError
                    || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError(uwr.error);
                }
                else
                {
                    string path = System.IO.Path.Combine(Application.persistentDataPath, "downloadedVideo_" + i + ".mp4");
                    System.IO.File.WriteAllBytes(path, uwr.downloadHandler.data);
                    downloadedVideoPaths.Add(path);
                    downloadedVideoClickUrls.Add(i < videoClickUrls.Count ? videoClickUrls[i] : string.Empty);
                }
            }
        }
        videoDownloadDone = true;
    }

}
