using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[System.Serializable]
public class NoteData
{
    public NoteType noteType;
    public float targetTime;
    public float duration;
}
public enum NoteType
{
    Tap,
    Hold,
}
[System.Serializable]
public class NoteDataList
{
    public List<NoteData> notes;
}