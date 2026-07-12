using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static MainThreadDispatcher _instance;

    public static void Enqueue(Action action)
    {
        lock (_queue) { _queue.Enqueue(action); }
    }

    void Awake()
    {
        if (_instance != null) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        while (_queue.Count > 0)
        {
            Action action;
            lock (_queue) { action = _queue.Dequeue(); }
            action?.Invoke();
        }
    }
}