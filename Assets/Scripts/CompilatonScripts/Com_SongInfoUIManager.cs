using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Com_SongInfoUIManager : MonoBehaviour
{
    public TMP_Text NumberText;
    public TMP_Text ComposerNameText;
    public TMP_Text SongTitleText;
    public TMP_Text GenreText;
    public TMP_Text MissionsText;
    public TMP_Text CommentsText;
    public RawImage Thumbnail;

    public void UpdateSongInfo
        (int songNumber, string ComposerName, string songTitle, string Genre, string Missions, string Comment,
        Texture2D ThumbnailTexture)
    {
        // UI 텍스트에 값 설정, 양식을 수정할 땐 여기서!
        // ----------------------------------------------------------------
        NumberText.text = $"DJ NO.{songNumber}";
        ComposerNameText.text = $": {ComposerName}";
        SongTitleText.text = $"{songTitle}";
        GenreText.text = $"[{Genre}]";
        MissionsText.text = $"사용곡 : {Missions}";
        CommentsText.text = $"아티스트 코멘트\n\n{Comment}";
        // ----------------------------------------------------------------

        // 이미지 설정 (없을 경우 비활성화)
        if (ThumbnailTexture != null)
        {
            Thumbnail.texture = ThumbnailTexture;
            Thumbnail.gameObject.SetActive(true);

            // 크기를 원본 텍스처에 맞게 설정
            Thumbnail.rectTransform.sizeDelta = new Vector2(ThumbnailTexture.width, ThumbnailTexture.height);

            // 비율 유지 설정
            AspectRatioFitter fitter = Thumbnail.GetComponent<AspectRatioFitter>();
            if (fitter != null)
            {
                fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                fitter.aspectRatio = (float)ThumbnailTexture.width / ThumbnailTexture.height;
            }
            else
            {
                Debug.LogWarning("AspectRatioFitter가 RawImage에 붙어있지 않음!");
            }
        }
        else
        {
            Thumbnail.gameObject.SetActive(false); // 이미지가 없으면 숨김
        }
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }
}
