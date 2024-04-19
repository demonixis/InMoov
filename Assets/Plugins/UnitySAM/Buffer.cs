using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buffer
{
    private List<int> _data;

    public int Size => _data.Count;
    public int[] ToArray() => _data.ToArray();

    public Buffer()
    {
        _data = new List<int>();
    }

    public void Set(int position, int data)
    {
        while (position >= _data.Count)
        {
            _data.Add(0);
        }

        _data[position] = data;
    }

    public float[] GetFloats()
    {
        var floats = new float[Size];

        for (var i = 0; i < _data.Count; i++)
        {
            floats[i] = (_data[i] - 127) / 255.0f;
        }

        return floats;
    }
}