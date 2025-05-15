using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class Task_VideoPlaylistLoader : MonoBehaviour
{
    [System.Serializable]
    public class SongData //곡 정보
    {
        public int songNumber;
        public string videoFileName;
        public float startTime;

        public string gameTitle;
        public string songTitle;
        public string artist;
        public string inGameLevel;
        public string displayBPM;
        public float realBPM;
    }


    public List<SongData> songList = new List<SongData>();

    void Start()
    {
        LoadCSV();
    }

    void LoadCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "TaskTracks", "Task_playlist.csv");

        if (File.Exists(filePath))
        {
            string[] csvContent = File.ReadAllLines(filePath);
            Debug.Log($"CSV 파일 발견: {filePath}, 총 {csvContent.Length - 1}개 데이터");

            for (int i = 1; i < csvContent.Length; i++)
            {
                string[] row = ParseCSVLine(csvContent[i]);

                // CSV 각 항목을 SongData 객체에 맞게 할당
                SongData songData = new SongData();
                songData.songNumber = int.Parse(row[0].Trim());
                songData.videoFileName = row[1].Trim();
                songData.realBPM = float.Parse(row[2].Trim());
                songData.startTime = float.Parse(row[3].Trim());
                songData.gameTitle = row[4].Trim();
                songData.songTitle = row[5].Trim();
                songData.artist = row[6].Trim();
                songData.inGameLevel = row[7].Trim();
                songData.displayBPM = row[8].Trim();

                songList.Add(songData);

                //디버깅 로그
                Debug.Log($"[{i}] {songData.songTitle} - {songData.videoFileName}");
            }

            Debug.Log($"CSV 로드 완료! 총 {songList.Count}개의 곡 로드됨.");
        }
        else
        {
            Debug.LogError($"CSV 파일이 존재하지 않습니다! {filePath}");
        }
    }



    // 줄바꿈 포함된 셀도 하나의 라인으로 처리
    string ReadCsvLine(StreamReader reader)
    {
        string line = "";
        bool inQuotes = false;

        while (!reader.EndOfStream)
        {
            if (line != "") line += "\n";
            line += reader.ReadLine();

            int quoteCount = 0;
            foreach (char c in line)
                if (c == '"') quoteCount++;

            inQuotes = (quoteCount % 2 != 0);
            if (!inQuotes) break;
        }

        return line;
    }

    // 셀 파싱
    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string value = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    value += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(value);
                value = "";
            }
            else
            {
                value += c;
            }
        }

        result.Add(value);
        return result.ToArray();
    }
}