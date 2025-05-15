using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;

public class Com_VideoPlaylistManager : MonoBehaviour
{
    //Player UI 관련
    public VideoPlayer videoPlayer;
    private AudioSource audioSource;
    public RawImage rawImage;
    public RawImage FadeUI;
    public GameObject InfoUI;

    //곡 정보 관련
    public Com_SongInfoUIManager songInfoUIManager; // SongInfoUIManager 참조
    public string videoFolder = "StreamingAssets"; // 영상 폴더
    private bool isSwitchingSong = false;  // 비디오 전환 중인지 확인하는 변수
    private bool isPlayingRoutine = false; //루틴 중인지 확인하는 변수

    private List<Com_VideoPlaylistLoader.SongData> songList; //곡 리스트
    private int currentSongIndex = 0; //재생중인 곡 변수


    //페이드 인/아웃 관련
    [Header("페이드 인/아웃")]
    [SerializeField] private float fadeDuration = 1.0f; // 페이드인/아웃 시간

    [Header("소개 UI 표시 시간")]
    public float UIDisplayTime = 10f;


    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        StartCoroutine(WaitForPlaylistLoad()); // 로딩

        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0); // 원하는 해상도
        rawImage.texture = videoPlayer.targetTexture;
    }

    void Update()
    {
        // 키 입력이 있을 때, 키보드로 트랙 이동 가능
        if (Input.GetKeyDown(KeyCode.LeftArrow)) JumpToSong(currentSongIndex - 1);
        if (Input.GetKeyDown(KeyCode.RightArrow)) JumpToSong(currentSongIndex + 1);
        if (Input.GetKeyDown(KeyCode.Q)) JumpToSong(currentSongIndex - 10);
        if (Input.GetKeyDown(KeyCode.W)) JumpToSong(currentSongIndex + 10);
        if (Input.GetKeyDown(KeyCode.Z)) JumpToSong(0);
    }

    IEnumerator WaitForPlaylistLoad() //실행 시 CSV를 파싱해오는 메소드
    {
        Com_VideoPlaylistLoader loader = FindObjectOfType<Com_VideoPlaylistLoader>();

        if (loader == null)
        {
            Debug.LogError("VideoPlaylistLoader를 씬에서 찾을 수 없음!");
            yield break;
        }

        // 로딩하기
        float waitTime = 0f;
        while (loader.songList.Count == 0 && waitTime < 2f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        // songList 가져오기
        songList = loader.songList;

        // 최종 확인
        if (songList.Count > 0)
        {
            Debug.Log($"VideoPlaylistManager: {songList.Count}개의 곡을 로드 완료!");
            PlaySong(0);
        }
        else
        {
            Debug.LogError("songList가 비어 있음! CSV 로드 확인 필요");
        }
    }

    void PlaySong(int index) // 영상의 경로를 찾고 불러오는 메소드
    {
        StopAllCoroutines(); // 기존 코루틴을 정리하여 StartCoroutine(WaitForNextSong(playTime))의 다중 호출 방지

        if (index < 0 || index >= songList.Count)
        {
            Debug.LogError("잘못된 인덱스입니다.");
            return;
        }

        isSwitchingSong = true;
        currentSongIndex = index; // 목록에 맞는 인덱스 불러오기

        videoPlayer.Stop();

        // RenderTexture 초기화 코드 시작
        RenderTexture rt = videoPlayer.targetTexture;
        if (rt != null)
        {
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;
        }

        // videoPlayer 재설정
        videoPlayer.targetTexture = rt;
        videoPlayer.enabled = true;

        string videoFileName = songList[currentSongIndex].videoFileName;
        string imageFileName = songList[currentSongIndex].ImageFileName;

        // 영상 이름만 적어도, 확장자인 mp4, png를 붙여줌
        if (!videoFileName.EndsWith(".mp4")) videoFileName += ".mp4";
        if (!imageFileName.EndsWith(".png")) imageFileName += ".png";
        songList[currentSongIndex].ImageFileName = imageFileName;

        // "Videos"폴더로 경로를 설정함
        string filePath = Path.Combine(Application.streamingAssetsPath, "CompilationTracks", "Com_Videos", videoFileName);
        string imagePath = Path.Combine(Application.streamingAssetsPath, "CompilationTracks", "Com_Images", imageFileName);

        if (!File.Exists(filePath)) // 경로에 그 이름의 비디오가 없을 때 발생하는 디버그 로그
        {
            Debug.LogError($"파일을 찾을 수 없음: {filePath}");
            return;
        }
        
        videoPlayer.url = filePath; // Video Player에 파일 경로 설정
        videoPlayer.isLooping = false; //루프 방지

        // VideoPlayer 준비 완료 이벤트를 처리
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.loopPointReached -= OnVideoEnd;
        videoPlayer.loopPointReached += OnVideoEnd;

        StartCoroutine(PrepareVideoAfterEnable()); // 준비 후 비디오 준비 시작
    }

    IEnumerator PrepareVideoAfterEnable()
    {
        yield return null; // 1프레임 대기하여 videoPlayer.enabled 반영 시간 확보
        videoPlayer.Prepare();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        JumpToSong(currentSongIndex + 1, true);
    }

    private Texture2D LoadTextureFromFile(string path) //썸네일을 표시하는 메소드
    {
        Debug.Log($"[LoadTextureFromFile] 시도 중: {path}");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"이미지 파일을 찾을 수 없음: {path}");
            return null;
        }

        byte[] imageBytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageBytes))
        {
            return texture;
        }

        return null;
    }

    void OnVideoPrepared(VideoPlayer vp) // 준비가 완료될 경우 호출되는 메소드
    {
        isSwitchingSong = false; // 실행 완료 후 플래그 해제
        Debug.Log("비디오 로드 완료. 재생 시작");

        // CSV에서 읽어온 정보를 UI에 표시
        var songData = songList[currentSongIndex];

        string imageFileName = songData.ImageFileName;
        if (!imageFileName.EndsWith(".png")) imageFileName += ".png";
        string imagePath = Path.Combine(Application.streamingAssetsPath, "CompilationTracks", "Com_Images", songData.ImageFileName);
        Texture2D thumbnailTexture = LoadTextureFromFile(imagePath);

        songInfoUIManager.UpdateSongInfo(
            songData.songNumber,
            songData.ComposerName,
            songData.SongTitle,
            songData.Genre,
            songData.Missions,
            songData.Comments,
            thumbnailTexture
        );

        // 비디오가 준비되면, 페이드 인, UI 업데이트 및 영상 재생 순서로 진행
        StartCoroutine(PlayRoutine());
        videoPlayer.prepareCompleted -= OnVideoPrepared; // 1번만 실행
    }

    IEnumerator PlayRoutine() //코드의 작동 루틴
    {
        Debug.Log($"[영상 길이 - PlayRoutine 진입] {videoPlayer.length}초");
        isPlayingRoutine = true;

        //영상을 끄고 정보 UI 페이드 인
        rawImage.gameObject.SetActive(false);
        InfoUI.SetActive(true);
        FadeUI.color = new Color(0, 0, 0, 1f);
        yield return StartCoroutine(FadeIn());

        //UI 표시 시간동안 기다린 후 페이드 아웃
        float waitTimeBeforeFade = Mathf.Max(UIDisplayTime - fadeDuration, 0f);
        yield return new WaitForSeconds(waitTimeBeforeFade);
        yield return StartCoroutine(FadeOut(0f));

        //UI 끄고 영상 활성화
        InfoUI.SetActive(false);
        rawImage.gameObject.SetActive(true);
        videoPlayer.time = 0f;
        audioSource.volume = 0f;

        //페이드 인 하면서 영상 재생
        yield return StartCoroutine(FadeIn());
        videoPlayer.Play();

        //영상이 끝나고 페이드 아웃
        yield return null;

        //영상이 끝나고 페이드 아웃
        StartCoroutine(DelayedFadeOut((float)videoPlayer.length - fadeDuration));

        isPlayingRoutine = false;
    }

    void JumpToSong(int index, bool force = false)
    {
        if (!force && (isPlayingRoutine || isSwitchingSong))
        {
            Debug.Log("Jump 중단: UI 또는 영상 전환 중");
            return;
        }

        currentSongIndex = Mathf.Clamp(index, 0, songList.Count - 1);
        PlaySong(currentSongIndex);
    }

    IEnumerator FadeIn() //페이드인
    {
        float timer = 0f;
        Color startColor = FadeUI.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (timer < fadeDuration)
        {
            audioSource.volume = Mathf.Lerp(0f, 1f, timer / fadeDuration); // 볼륨
            FadeUI.color = Color.Lerp(startColor, endColor, timer / fadeDuration); //화면

            timer += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 1f;
        FadeUI.color = endColor;
    }

    IEnumerator FadeOut(float delay) //페이드아웃
    {
        yield return new WaitForSeconds(delay);

        float timer = 0f;
        Color startColor = FadeUI.color;
        Color endColor = new Color(0, 0, 0, 1f);
        float startVolume = audioSource.volume;

        while (timer < fadeDuration)
        {
            FadeUI.color = Color.Lerp(startColor, endColor, timer / fadeDuration);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            timer += Time.deltaTime;
            yield return null;
        }

        FadeUI.color = endColor;
        audioSource.volume = 0f;
    }

    IEnumerator DelayedFadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(FadeOut(0f));
    }
}