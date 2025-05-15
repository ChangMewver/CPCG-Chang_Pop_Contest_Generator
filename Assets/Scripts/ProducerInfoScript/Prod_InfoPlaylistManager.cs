using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Video;

public class Prod_InfoPlaylistManager : MonoBehaviour
{
    public Prod_InfoUIManager infoUIManager;
    public RawImage FadeUI;

    [Header("배경 영상 음향 옵션")]
    [SerializeField] private bool muteAudio = false;
    [Range(0f, 1f)]
    [SerializeField] private float backgroundVolume = 1.0f;

    [Header("페이드 인/아웃")] // 페이드 인/아웃에 걸리는 시간
    [SerializeField] private float fadeDuration = 0.3f; 

    [Header("한 명당 정보 표시 시간")] // 정보를 이 시간만큼 보여주고, 다음 사람으로 넘어감
    [SerializeField] private float displayDuration = 7.0f; 

    private List<Prod_VideoPlaylistLoader.SongData> ProdList;
    private int currentIndex = 0;

    private bool isTransitioning = false; //키 입력 방지용
    Coroutine playLoopCoroutine = null; //자동 재생 코루틴 저장용

    void Start()
    {
        StartCoroutine(InitializeAfterVideoReady());
    }

    IEnumerator InitializeAfterVideoReady()
    {
        yield return StartCoroutine(SetupBackgroundVideo()); //배경 영상 준비
        yield return StartCoroutine(WaitForPlaylistLoad()); //CSV 로딩 및 플레이리스트 대기
    }

    void Update()
    {
        if (Input.anyKeyDown && !isTransitioning)
        {
            // 키 입력이 있을 때, 키보드로 이동 가능
            if (Input.GetKeyDown(KeyCode.LeftArrow)) Jump(currentIndex - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow)) Jump(currentIndex + 1);
            if (Input.GetKeyDown(KeyCode.Q)) Jump(currentIndex - 10);
            if (Input.GetKeyDown(KeyCode.W)) Jump(currentIndex + 10);
            if (Input.GetKeyDown(KeyCode.Z)) Jump(0);
        }
    }
    IEnumerator SetupBackgroundVideo() //배경 영상을 출력하는 메소드
    {
        //영상 준비
        GameObject videoPlayerObj = new GameObject("BackgroundVideoPlayer");
        VideoPlayer vp = videoPlayerObj.AddComponent<VideoPlayer>();
        AudioSource audioSource = videoPlayerObj.AddComponent<AudioSource>();

        //파일 위치 찾기
        string videoPath = Path.Combine(Application.streamingAssetsPath, "ProducerInfo", "video.mp4");
        vp.url = videoPath;


        //카메라에 직접 출력
        vp.renderMode = VideoRenderMode.CameraFarPlane;
        vp.targetCamera = Camera.main;

        //영상 설정
        vp.isLooping = true;
        vp.playOnAwake = false;

        //오디오 세팅
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audioSource);
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.mute = muteAudio;
        audioSource.volume = backgroundVolume;

        bool isPrepared = false;

        vp.prepareCompleted += (source) =>
        {
            Debug.Log("카메라에 영상 출력 준비 완료");
            vp.Play();
            audioSource.Play();
            isPrepared = true;
        };

        vp.Prepare();

        float timeout = 3f;
        while (!isPrepared && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator WaitForPlaylistLoad() //실행 시 CSV를 파싱해오는 메소드
    {
        Prod_VideoPlaylistLoader loader = FindObjectOfType<Prod_VideoPlaylistLoader>();

        if (loader == null)
        {
            Debug.LogError("Prod_listLoader를 씬에서 찾을 수 없음!");
            yield break;
        }

        // CSV 로딩 대기
        float waitTime = 0f;
        while (loader.ProdList.Count == 0 && waitTime < 2f)
        {
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
        }

        ProdList = loader.ProdList;

        if (ProdList.Count > 0)
        {
            currentIndex = 0;
            UpdateCurrentInfo();

            yield return StartCoroutine(FadeIn());

            playLoopCoroutine = StartCoroutine(PlayLoop()); // 코루틴 저장
        }
        else
        {
            Debug.LogError("로딩된 참가자 목록이 없음! CSV 로드 확인 필요");
        }
    }

    IEnumerator PlayLoop() //참가자 정보를 순차적으로 자동 표시하는 루프 메소드
    {
        while (true)
        {
            yield return new WaitForSeconds(displayDuration); // 이때 키 입력 허용

            isTransitioning = true;

            yield return StartCoroutine(FadeOut());

            currentIndex = (currentIndex + 1) % ProdList.Count;
            UpdateCurrentInfo();

            yield return StartCoroutine(FadeIn());

            isTransitioning = false;
        }
    }

    void UpdateCurrentInfo() //현재 참가자의 정보를 UI에 업데이트하는 메소드
    {
        var song = ProdList[currentIndex];
        Texture2D tex = LoadImage(song.ImageFileName);
        infoUIManager.UpdateSongInfo(song.songNumber, song.ComposerName, song.Genre, song.Missions, song.Comments, tex);
    }

    void Jump(int index) // 특정 인덱스로 점프+페이드 아웃/인
    {
        if (isTransitioning || ProdList == null || ProdList.Count == 0) return;
        StartCoroutine(JumpToIndex(index));
    }

    IEnumerator JumpToIndex(int newIndex)
    {
        isTransitioning = true;

        if (playLoopCoroutine != null)
        {
            StopCoroutine(playLoopCoroutine); // 기존 루프 종료
        }

        yield return StartCoroutine(FadeOut());

        currentIndex = Mathf.Clamp(newIndex, 0, ProdList.Count - 1);
        UpdateCurrentInfo();

        yield return StartCoroutine(FadeIn());

        isTransitioning = false;

        playLoopCoroutine = StartCoroutine(PlayLoop()); // 새로운 루프 시작
    }

    Texture2D LoadImage(string imageName) // 이미지 파일을 로드하여 Texture2D로 반환
    {
        if (!imageName.ToLower().EndsWith(".png")) //파일 형식은 png
        {
            imageName += ".png";
        }

        string path = Path.Combine(Application.streamingAssetsPath, "ProducerInfo", "Prod_Images", imageName);

        if (File.Exists(path))
        {
            byte[] data = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2);
            bool loaded = tex.LoadImage(data);
            if (loaded)
            {
                Debug.Log($"이미지 로딩 성공: {path} ({tex.width}x{tex.height})");
                return tex;
            }
            else //없으면 표시하지 않음
            {
                Debug.LogError($"이미지 로딩 실패 (LoadImage 실패): {path}");
            }
        }
        else
        {
            Debug.LogWarning($"이미지 파일이 존재하지 않음: {path}");
        }
        return null;
    }

    IEnumerator FadeIn() //페이드 인
    {
        float t = 0f;
        Color start = new Color(0, 0, 0, 1f);
        Color end = new Color(0, 0, 0, 0f);

        while (t < fadeDuration)
        {
            FadeUI.color = Color.Lerp(start, end, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        FadeUI.color = end;
    }

    IEnumerator FadeOut() //페이드 아웃
    {
        float t = 0f;
        Color start = new Color(0, 0, 0, 0f);
        Color end = new Color(0, 0, 0, 1f);

        while (t < fadeDuration)
        {
            FadeUI.color = Color.Lerp(start, end, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        FadeUI.color = end;
    }
}