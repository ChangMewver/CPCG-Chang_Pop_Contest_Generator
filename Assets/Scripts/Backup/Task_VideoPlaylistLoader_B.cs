//using UnityEngine;
//using System.Collections.Generic;
//using System.IO;
//using System.Text.RegularExpressions;

//public class Task_VideoPlaylistLoader : MonoBehaviour
//{
//    [System.Serializable]
//    public class SongData //곡 정보
//    {
//        public int songNumber;
//        public string gameTitle;
//        public string songTitle;
//        public string artist;
//        public string inGameLevel;
//        public string displayBPM;
//        public float realBPM;
//        public string videoFileName;
//        public float startTime;
//    }


//    public List<SongData> songList = new List<SongData>();

//    void Start()
//    {
//        LoadCSV();
//    }

//    void LoadCSV()
//    {
//        string filePath = Path.Combine(Application.streamingAssetsPath, "TaskTracks", "Task_playlist.csv");

//        if (File.Exists(filePath))
//        {
//            string[] csvContent = File.ReadAllLines(filePath);
//            Debug.Log($"CSV 파일 발견: {filePath}, 총 {csvContent.Length - 1}개 데이터");

//            for (int i = 1; i < csvContent.Length; i++)
//            {
//                string[] row = ParseCSVLine(csvContent[i]);

//                // CSV 각 항목을 SongData 객체에 맞게 할당
//                SongData songData = new SongData();
//                songData.songNumber = int.Parse(row[0].Trim());
//                songData.gameTitle = row[1].Trim();
//                songData.songTitle = row[2].Trim();
//                songData.artist = row[3].Trim();
//                songData.inGameLevel = row[4].Trim();
//                songData.displayBPM = row[5].Trim();
//                songData.realBPM = float.Parse(row[6].Trim());
//                songData.videoFileName = row[7].Trim();
//                songData.startTime = float.Parse(row[8].Trim());

//                songList.Add(songData);

//                //디버깅 로그
//                Debug.Log($"[{i}] {songData.songTitle} - {songData.videoFileName}");
//            }

//            Debug.Log($"CSV 로드 완료! 총 {songList.Count}개의 곡 로드됨.");
//        }
//        else
//        {
//            Debug.LogError($"CSV 파일이 존재하지 않습니다! {filePath}");
//        }
//    }


//    // CSV 한 줄을 파싱하는 함수
//    string[] ParseCSVLine(string line)
//    {
//        // 큰따옴표로 묶인 값은 하나의 항목으로 처리하도록 정규식 사용
//        var regex = new Regex("\"([^\"]*)\"|([^,]+)");
//        var matches = regex.Matches(line);

//        List<string> result = new List<string>();
//        foreach (Match match in matches)
//        {
//            result.Add(match.Value.Trim('"')); // 큰따옴표 제거
//        }

//        return result.ToArray();
//    }
//}