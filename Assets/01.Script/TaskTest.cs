using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class SharedCounter
{
    private int count = 0;

    public async Task IncrementAsync()
    {
        for (int i = 0; i < 1000; i++)
        {
            count++; // 비원자적 연산
        }
    }

    public int GetCount() => count;
}

public class TaskTest : MonoBehaviour
{
    private async void Start()
    {
        var counter = new SharedCounter();
        var tasks = new Task[10];

        for (int i = 0; i < 10; i++)
        {
            tasks[i] = counter.IncrementAsync();
        }

        await Task.WhenAll(tasks);
        Debug.Log(counter.GetCount());
    }
}
