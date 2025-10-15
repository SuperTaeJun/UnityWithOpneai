using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

[System.Serializable]
public class NoteDataListWithBpm
{
    public float bpm;
    public List<NoteData> notes;
}

public class MidiToJsonWindow : EditorWindow
{
    private Object midiFile;
    private string outputFileName = "song_notes.json";

    [MenuItem("Tools/MIDI → JSON 변환기")]
    public static void ShowWindow()
    {
        GetWindow<MidiToJsonWindow>("MIDI to JSON");
    }

    void OnGUI()
    {
        GUILayout.Label("🎵 MIDI → JSON 변환기 (BPM 포함)", EditorStyles.boldLabel);
        GUILayout.Space(10);

        midiFile = EditorGUILayout.ObjectField("MIDI 파일", midiFile, typeof(Object), false);
        outputFileName = EditorGUILayout.TextField("출력 파일명", outputFileName);

        GUILayout.Space(10);

        if (GUILayout.Button("✅ 변환 실행", GUILayout.Height(30)))
        {
            ConvertMidi();
        }
    }

    void ConvertMidi()
    {
        if (midiFile == null)
        {
            EditorUtility.DisplayDialog("오류", "MIDI 파일을 선택해주세요!", "확인");
            return;
        }

        string midiPath = AssetDatabase.GetAssetPath(midiFile);
        string outputDir = Path.GetDirectoryName(midiPath);
        string jsonPath = Path.Combine(outputDir, outputFileName);

        try
        {
            var midi = MidiFile.Read(midiPath);
            var tempoMap = midi.GetTempoMap();
            var notes = midi.GetNotes();

            // 단일 BPM 추출 (첫 번째 템포 이벤트 기준)
            var tempoChanges = tempoMap.GetTempoChanges();
            float bpm = 120f; // 기본값
            foreach (var tempoEvent in tempoChanges)
            {
                bpm = (float)tempoEvent.Value.BeatsPerMinute;
                break;
            }

            List<NoteData> noteList = new List<NoteData>();

            foreach (var note in notes)
            {
                double startSec = note.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;
                double endSec = note.EndTimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;
                float duration = (float)(endSec - startSec);

                NoteData data = new NoteData
                {
                    noteType = duration > 0.3f ? NoteType.Hold : NoteType.Tap,
                    targetTime = (float)startSec,
                    duration = duration
                };

                noteList.Add(data);
            }

            // JSON 변환
            NoteDataListWithBpm jsonData = new NoteDataListWithBpm
            {
                bpm = bpm,
                notes = noteList
            };

            string json = JsonUtility.ToJson(jsonData, true);
            File.WriteAllText(jsonPath, json);

            EditorUtility.DisplayDialog("완료", $"✅ JSON 파일 생성 완료!\n\n경로:\n{jsonPath}", "확인");
            AssetDatabase.Refresh();
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("에러", $"변환 중 오류 발생:\n{ex.Message}", "닫기");
        }
    }
}