using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public static class LocalizationSheetImporter
{
    private const string WEB_SERVICE_URL = "https://script.google.com/macros/s/AKfycbzaDgQp_xnLqyqs67hkvl6kKZBtCAFEf6tFI8qYBbl0x86qIlMA/exec";
    private const int TIME_OUT = 300000;
    private const string PARAM_ACTION = "action";
    private const string PARAM_DATA = "data";

    private enum QueryType
    {
        Get,
        Set
    }

    public static T DownloadContent<T>()
    {
        var form = new Dictionary<string, string>();
        form.Add(PARAM_ACTION, QueryType.Get.ToString().ToLower());
        var response = ExecuteRequest(WEB_SERVICE_URL, form);
        Debug.Log("Response: " + response);
        try
        {
            var content = JsonUtility.FromJson<T>(response);
            return content;
        }
        catch (Exception)
        {
            Debug.LogError(response);
        }
        return default(T);
    }

    public static bool UploadContent<T>(T content)
    {
        var form = new Dictionary<string, string>();
        form.Add(PARAM_ACTION, QueryType.Set.ToString().ToLower());
        var serializedData = JsonUtility.ToJson(content);
        Debug.Log("Serialized Object: " + serializedData);
        form.Add(PARAM_DATA, serializedData);
        var response = ExecuteRequest(WEB_SERVICE_URL, form);
        if (response == "success")
        {
            return true;
        }

        Debug.LogError(response);
        return false;
    }

    private static string ExecuteRequest(string requestUriString, Dictionary<string, string> postParameters)
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ServicePointManager.Expect100Continue = false;

        var cookieContainer = new CookieContainer();
        var webResponse = SendPostRequest(requestUriString, postParameters, cookieContainer);

        while (webResponse.StatusCode == HttpStatusCode.Found)
        {
            webResponse.Close();
            webResponse = SendGetRequest(webResponse.Headers["Location"], cookieContainer);
        }

        StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
        string fullResponse = responseReader.ReadToEnd();
        webResponse.Close();
        return fullResponse;
    }

    private static HttpWebResponse SendPostRequest(string requestUriString, Dictionary<string, string> postParameters, CookieContainer cookieContainer)
    {
        var request = WebRequest.Create(requestUriString) as HttpWebRequest;
        if (request == null)
        {
            throw new NullReferenceException("request is not a http request");
        }
        request.Method = "POST";
        request.Timeout = TIME_OUT;
        request.CookieContainer = cookieContainer;
        request.AllowAutoRedirect = false;

        byte[] formData = UnityWebRequest.SerializeSimpleForm(postParameters);
        request.ContentType = "application/x-www-form-urlencoded";
        request.ContentLength = formData.Length;
        request.Headers.Add("Accept-Encoding", "identity");
        request.Accept = "*/*";
        request.AllowAutoRedirect = false;

        using (Stream requestStream = request.GetRequestStream())
        {
            requestStream.Write(formData, 0, formData.Length);
            requestStream.Close();
        }

        return request.GetResponse() as HttpWebResponse;
    }

    private static HttpWebResponse SendGetRequest(string requestUriString, CookieContainer cookieContainer)
    {
        var request = (HttpWebRequest)WebRequest.Create(requestUriString);
        if (request == null)
        {
            throw new NullReferenceException("request is not a http request");
        }
        request.Method = "GET";
        request.Timeout = TIME_OUT;
        request.CookieContainer = cookieContainer;
        request.AllowAutoRedirect = false;
        return request.GetResponse() as HttpWebResponse;
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Google Sheet Service/Test Upload")]
    public static void TestUpload()
    {
        Debug.Log("Test Upload");
        var testObject = new TestDataObject
        {
            keys = new[] { "k1." + DateTime.UtcNow, "k2." + DateTime.UtcNow, "k3." + DateTime.UtcNow },
            values = new[] { UnityEngine.Random.Range(0, 9999).ToString(), UnityEngine.Random.Range(0, 9999).ToString(), UnityEngine.Random.Range(0, 9999).ToString() }
        };
        bool result = UploadContent(testObject);

        Debug.Log("Upload success: " + result);
    }

    [UnityEditor.MenuItem("Tools/Google Sheet Service/Test Download")]
    public static void TestDownload()
    {
        Debug.Log("Test Download");
        var testObject = DownloadContent<TestDataObject>();
        for (int i = 0; i < testObject.keys.Length; i++)
        {
            Debug.Log(testObject.keys[i] + " => " + testObject.values[i]);
        }

        Debug.Log("Download done.");
    }
#endif

    [Serializable]
    public class TestDataObject
    {
        public string[] keys;
        public string[] values;
    }
}