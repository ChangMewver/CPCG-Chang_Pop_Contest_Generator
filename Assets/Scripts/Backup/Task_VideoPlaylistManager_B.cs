//using UnityEngine;
//using UnityEngine.Video;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine.UI;

//public class Task_VideoPlaylistManager : MonoBehaviour
//{
//    //Player UI 관련
//    public VideoPlayer videoPlayer;
//    private AudioSource audioSource;
//    public RawImage rawImage;
//    public RawImage FadeUI;

//    //곡 정보 관련
//    public Task_SongInfoUIManager songInfoUIManager; // SongInfoUIManager 참조
//    public string videoFolder = "StreamingAssets"; // 영상 폴더

//    private List<Task_VideoPlaylistLoader.SongData> songList; //곡 리스트
//    private int currentSongIndex = 0; //재생중인 곡 변수
//    private float playTime;

//    [Header("곡당 재생할 마디 수")] // 몇 마디 동안 재생할지 (예제: 16마디)
//    public int playMeasures = 16;
//    private bool isSwitchingSong = false;  // 비디오 전환 중인지 확인하는 변수

//    [Header("페이드 인/아웃 시간")] // 페이드인/아웃에 걸리는 시간
//    [SerializeField] private float fadeDuration = 1.0f;


//    void Start()
//    {
//        audioSource = gameObject.AddComponent<AudioSource>();
//        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
//        videoPlayer.SetTargetAudioSource(0, audioSource);

//        StartCoroutine(WaitForPlaylistLoad()); // 로딩
//    }

//    void Update()
//    {
//        if (videoPlayer.isPrepared)
//        {
//            // 비디오가 준비되었을 때만 다음 곡으로 넘어가도록
//            if (videoPlayer.isPlaying && videoPlayer.time >= videoPlayer.length - 0.1f)  // 종료 0.1초 전에 체크
//            {
//                // 비디오 전환 중이지 않으면 다음 곡으로 넘어가도록
//                if (!isSwitchingSong)
//                {
//                    NextSong();
//                }
//            }
//        }

//        // 좌우 방향키로 키보드 조작 가능.
//        if (Input.GetKeyDown(KeyCode.LeftArrow)) JumpToSong(currentSongIndex - 1);
//        if (Input.GetKeyDown(KeyCode.RightArrow)) JumpToSong(currentSongIndex + 1);
//        if (Input.GetKeyDown(KeyCode.Q)) JumpToSong(currentSongIndex - 10);
//        if (Input.GetKeyDown(KeyCode.W)) JumpToSong(currentSongIndex + 10);
//        if (Input.GetKeyDown(KeyCode.Z)) JumpToSong(0);
//    }

//    IEnumerator WaitForPlaylistLoad() //실행 시 CSV를 파싱해오는 메소드
//    {
//        Task_VideoPlaylistLoader loader = FindObjectOfType<Task_VideoPlaylistLoader>();

//        if (loader == null)
//        {
//            Debug.LogError("VideoPlaylistLoader가 씬에서 찾을 수 없음!");
//            yield break;
//        }

//        // 로딩하기
//        float waitTime = 0f;
//        while (loader.songList.Count == 0 && waitTime < 2f)
//        {
//            yield return new WaitForSeconds(0.1f);
//            waitTime += 0.1f;
//        }

//        // songList 가져오기
//        songList = loader.songList;

//        // 최종 확인
//        if (songList.Count > 0)
//        {
//            Debug.Log($"VideoPlaylistManager: {songList.Count}개의 곡을 로드 완료!");
//            PlaySong(0);
//        }
//        else
//        {
//            Debug.LogError("songList가 비어 있음! CSV 로드 확인 필요");
//        }
//    }

//    void PlaySong(int index) //영상의 경로를 찾고 불러오는 메소드
//    {
//        StopAllCoroutines(); //기존 코루틴을 정리하여 StartCoroutine(WaitForNextSong(playTime))의 다중 호출 방지

//        if (index < 0 || index >= songList.Count)
//        {
//            Debug.LogError("잘못된 인덱스입니다.");
//            return;
//        }

//        currentSongIndex = index; //목록에 맞는 인덱스 불러오기

//        string videoFileName = songList[currentSongIndex].videoFileName;

//        //영상 이름만 적어도, 확장자인 mp4를 붙여줌
//        if (!videoFileName.EndsWith(".mp4"))
//        {
//            videoFileName += ".mp4";
//        }

//        // "Videos"폴더로 경로를 설정함
//        string filePath = Path.Combine(Application.streamingAssetsPath, "TaskTracks", "Task_Videos", videoFileName);

//        if (!File.Exists(filePath)) //경로에 그 이름의 비디오가 없을 때 발생하는 디버그 로그
//        {
//            Debug.LogError($"파일을 찾을 수 없음: {filePath}");
//            return;
//        }

//        // Video Player에 파일 경로 설정
//        videoPlayer.Stop();
//        videoPlayer.url = filePath;

//        // VideoPlayer 준비 완료 이벤트를 처리합니다.
//        videoPlayer.prepareCompleted -= OnVideoPrepared;
//        videoPlayer.prepareCompleted += OnVideoPrepared;
//        videoPlayer.Prepare(); // 비디오 준비 시작

//        isSwitchingSong = true; // 곡 전환 시작 플래그 설정
//    }

//    void OnVideoPrepared(VideoPlayer vp) //준비가 완료될 경우 호출되는 메소드
//    {
//        isSwitchingSong = false; //실행 완료 후 플래그 해제
//        Debug.Log("비디오 로드 완료. 재생 시작");

//        //CSV에서 읽어온 정보를 UI에 표시
//        var songData = songList[currentSongIndex];
//        songInfoUIManager.UpdateSongInfo(
//            songData.songNumber,
//            songData.gameTitle,
//            songData.songTitle,
//            songData.artist,
//            songData.inGameLevel,
//            songData.displayBPM // 표기 BPM만 전달
//        );

//        //시작 시간 설정
//        vp.time = songList[currentSongIndex].startTime; //CSV에서 startTime을 받아옴
//        vp.Play(); // 비디오 재생
//        Debug.Log($"재생 시작: {songList[currentSongIndex].songTitle}");

//        //BPM 정보를 기반으로 곡을 얼마나 재생할지 설정함. 얼마나 재생되는지 로그에 출력됨
//        float beatDuration = 60f / songList[currentSongIndex].realBPM; //1박
//        float measureDuration = beatDuration * 4; // 1마디
//        playTime = measureDuration * playMeasures; // 클래스 멤버에 저장
//        Debug.Log($"설정된 재생 시간: {playTime}초");

//        audioSource.volume = 0f; //시작 시 볼륨 0으로 설정
//        FadeUI.color = new Color(FadeUI.color.r, FadeUI.color.g, FadeUI.color.b, 1f); //시작시 페이드인 준비

//        StartCoroutine(DelayedStartAfterPrepare());
//    }

//    IEnumerator DelayedStartAfterPrepare()
//    {
//        yield return new WaitForSeconds(0.5f); // 씬 전체 준비 기다리는 시간
//        StartCoroutine(FadeIn());
//        StartCoroutine(FadeOut(playTime - fadeDuration));
//        StartCoroutine(WaitForNextSong(playTime));
//    }

//    IEnumerator WaitForNextSong(float duration) //정해진 시간간 재생하고 NextSong을 호출하는 메소드
//    {
//        // duration 시간이 지난 후에 곡을 전환하도록 대기
//        yield return new WaitForSeconds(duration);

//        // 곡을 변경할 때 현재 인덱스를 업데이트
//        NextSong();
//    }

//    void NextSong() //다음곡으로 넘기는 메소드
//    {
//        if (isSwitchingSong)  // 전환 중이면 중복 실행 방지
//        {
//            Debug.Log("곡 전환이 이미 진행 중입니다.");
//            return;
//        }

//        isSwitchingSong = true;  // 실행 중 플래그 설정
//        Debug.Log($"NextSong() 호출 전, currentSongIndex: {currentSongIndex}");

//        currentSongIndex++; //다음곡

//        if (currentSongIndex >= songList.Count) //전부 재생시 처음으로
//        {
//            currentSongIndex = 0;
//            Debug.Log("곡이 끝나서 첫 번째 곡으로 돌아갑니다.");
//        }

//        PlaySong(currentSongIndex);

//        Debug.Log($"PlaySong() 호출됨, 현재 곡 인덱스: {currentSongIndex}");
//    }

//    void JumpToSong(int index) //키보드 조작을 담당하는 메소드
//    {
//        // 10개 이전/이후 혹은 처음으로 이동
//        currentSongIndex = Mathf.Clamp(index, 0, songList.Count - 1);  // 범위 내에서만 이동하도록 제한
//        PlaySong(currentSongIndex);
//    }

//    IEnumerator FadeIn() //페이드인
//    {
//        float timer = 0f;
//        Color startColor = FadeUI.color;
//        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

//        while (timer < fadeDuration)
//        {
//            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration); // 볼륨
//            FadeUI.color = Color.Lerp(startColor, endColor, timer / fadeDuration); //화면

//            timer += Time.deltaTime;
//            yield return null;
//        }
//        audioSource.volume = 1f;
//        FadeUI.color = endColor;
//    }

//    IEnumerator FadeOut(float duration) //페이드아웃
//    {

//        yield return new WaitForSeconds(duration); //종료 전까지 기다리기

//        //페이드 아웃
//        float startVolume = audioSource.volume;
//        Color startColor = FadeUI.color;
//        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

//        float timer = 0f;

//        while (timer < fadeDuration) // duration으로 페이드 아웃 시간 변경
//        {
//            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration); //볼륨
//            FadeUI.color = Color.Lerp(startColor, endColor, timer / fadeDuration); // 화면

//            timer += Time.deltaTime;
//            yield return null;
//        }
//        audioSource.volume = 0f;
//        FadeUI.color = endColor;
//    }
//}