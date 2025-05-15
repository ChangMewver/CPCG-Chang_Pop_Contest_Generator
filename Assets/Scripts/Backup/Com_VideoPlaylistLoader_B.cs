//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;

//public class Com_VideoPlaylistLoader : MonoBehaviour
//{
//    [System.Serializable]
//    public class SongData //곡 정보
//    {
//        public int songNumber;
//        public string ComposerName;
//        public string SongTitle;
//        public string Genre;
//        public string Missions;
//        public string Comments;
//        public string videoFileName;
//        public string ImageFileName;
//    }


//    public List<SongData> songList = new List<SongData>();

//    void Start()
//    {
//        LoadCSV();
//    }

//    void LoadCSV()
//    {
//        string filePath = Path.Combine(Application.streamingAssetsPath, "CompilationTracks", "Com_playlist.csv");

//        if (File.Exists(filePath))
//        {
//            using (StreamReader reader = new StreamReader(filePath))
//            {
//                bool isFirstLine = true;
//                while (!reader.EndOfStream)
//                {
//                    string line = ReadCsvLine(reader); // 줄바꿈 포함 라인 읽기

//                    if (isFirstLine)
//                    {
//                        isFirstLine = false;
//                        continue; // 첫 줄은 헤더
//                    }

//                    string[] row = ParseCSVLine(line);
//                    if (row.Length < 8)
//                    {
//                        Debug.LogWarning($"행이 유효하지 않음: {line}");
//                        continue;
//                    }

//                    SongData songData = new SongData
//                    {
//                        songNumber = int.Parse(row[0].Trim()),
//                        ComposerName = row[1].Trim(),
//                        SongTitle = row[2].Trim(),
//                        Genre = row[3].Trim(),
//                        Missions = row[4].Trim(),
//                        Comments = row[5].Trim(),
//                        videoFileName = row[6].Trim(),
//                        ImageFileName = row[7].Trim()
//                    };

//                    songList.Add(songData);
//                }
//            }

//            Debug.Log($"CSV 로드 완료! 총 {songList.Count}개의 곡 로드됨.");
//        }
//        else
//        {
//            Debug.LogError($"CSV 파일이 존재하지 않습니다! {filePath}");
//        }
//    }


//    // 줄바꿈 포함된 셀도 하나의 라인으로 처리
//    string ReadCsvLine(StreamReader reader)
//    {
//        string line = "";
//        bool inQuotes = false;

//        while (!reader.EndOfStream)
//        {
//            if (line != "") line += "\n";
//            line += reader.ReadLine();

//            int quoteCount = 0;
//            foreach (char c in line)
//                if (c == '"') quoteCount++;

//            inQuotes = (quoteCount % 2 != 0);
//            if (!inQuotes) break;
//        }

//        return line;
//    }

//    // 셀 파싱
//    string[] ParseCSVLine(string line)
//    {
//        List<string> result = new List<string>();
//        bool inQuotes = false;
//        string value = "";

//        for (int i = 0; i < line.Length; i++)
//        {
//            char c = line[i];

//            if (c == '"')
//            {
//                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
//                {
//                    value += '"';
//                    i++;
//                }
//                else
//                {
//                    inQuotes = !inQuotes;
//                }
//            }
//            else if (c == ',' && !inQuotes)
//            {
//                result.Add(value);
//                value = "";
//            }
//            else
//            {
//                value += c;
//            }
//        }

//        result.Add(value);
//        return result.ToArray();
//    }
//}