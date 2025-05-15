using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class Unmask_VideoPlaylistLoader : MonoBehaviour
{
    [System.Serializable]
    public class SongData //곡 정보
    {
        public int songNumber;
        public string ImageFileName;
        public string UnmaskImageFileName;
        public string ComposerName;
        public string Genre;
        public string SongTitle;
        public string Missions;
    }

    public List<SongData> UnmaskList = new List<SongData>();

    void Start()
    {
        LoadCSV();
    }

    void LoadCSV() //CSV를 파싱하는 메소드
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Unmasking", "Unmask_playlist.csv");

        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                bool isFirstLine = true;
                while (!reader.EndOfStream)
                {
                    string line = ReadCsvLine(reader); // 줄바꿈 고려한 한 줄 파싱

                    if (isFirstLine) // 첫 줄은 헤더
                    {
                        isFirstLine = false;
                        continue;
                    }

                    string[] row = ParseCSVLine(line);
                    if (row.Length < 7)
                    {
                        Debug.LogWarning($"행이 유효하지 않음: {line}");
                        continue;
                    }

                    SongData songData = new SongData
                    {
                        songNumber = int.Parse(row[0].Trim()),
                        ImageFileName = row[1].Trim(),
                        UnmaskImageFileName = row[2].Trim(),
                        ComposerName = row[3].Trim(),
                        Genre = row[4].Trim(),
                        SongTitle = row[5].Trim(),
                        Missions = row[6].Trim()
                    };

                    UnmaskList.Add(songData);
                }
            }

            Debug.Log($"CSV 로드 완료! 총 {UnmaskList.Count}개의 곡 로드됨.");
        }
        else
        {
            Debug.LogError($"CSV 파일이 존재하지 않습니다! {filePath}");
        }
    }

    // 줄바꿈 포함된 셀도 한 줄로 읽는 함수
    string ReadCsvLine(StreamReader reader)
    {
        string line = "";
        bool inQuotes = false;

        while (!reader.EndOfStream)
        {
            if (line != "") line += "\n"; // 줄바꿈 복원
            line += reader.ReadLine();

            // 큰따옴표가 닫히는지 확인
            int quoteCount = Regex.Matches(line, "\"").Count;
            inQuotes = (quoteCount % 2 != 0);

            if (!inQuotes)
                break;
        }

        return line;
    }

    // 셀 파싱 함수
    string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string value = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '\"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"')
                {
                    value += '\"';
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