using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpriteLoaderService : MonoBehaviour
{
    private static SpriteLoaderService _instance;

    private bool _isLoading;
    private Dictionary<string, Action<Sprite>> _queue;
    private int _pixelsToUnit;
    private Vector2 _pivot;


    public static void Initialize(int pixelsPerUnit, Vector2 pivot)
    {
        _instance = new GameObject("spriteloader_service").AddComponent<SpriteLoaderService>();
        _instance._pixelsToUnit = pixelsPerUnit;
        _instance._pivot = pivot;
        _instance._queue = new Dictionary<string, Action<Sprite>>();
    }

    public static void LoadSpriteAsync(string url, Action<Sprite> callback = null)
    {
        if (_instance == null)
        {
            throw new Exception("SpriteLoaderService not initialized");
        }
        _instance.Enqueue(url, callback);
    }

    void Update()
    {
        if (_isLoading || !_queue.Any())  return;
        StartCoroutine(LoadingRoutine());

    }

    private void Enqueue(string url, Action<Sprite> callback)
    {
        _queue[url] = callback;
    }

    private void Dequeue(string url)
    {
        _queue.Remove(url);
    }

    private IEnumerator LoadingRoutine()
    {
        _isLoading = true;
        string url = _queue.Keys.First();
        //Debug.Log("Load " + url);

        WWW webRequest = new WWW(url);
        yield return webRequest;
        if (!string.IsNullOrEmpty(webRequest.error))
        {
            Debug.LogError(webRequest.error);
            _isLoading = false;
            yield break;
        }
        Texture2D tex = webRequest.texture;
        Sprite loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), _pivot, _pixelsToUnit);
        if (_queue[url] != null)
        {
            _queue[url](loadedSprite);
        }
        Dequeue(url);
        _isLoading = false;
    }
}
