using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unmask_InfoUIManager : MonoBehaviour
{
    public TMP_Text NumberText;
    public TMP_Text ComposerNameText;
    public TMP_Text GenreText;
    public TMP_Text SongTitleText;
    public TMP_Text MissionsText;
    public RawImage Thumbnail;
    public RawImage IdentityImage;

    public void UpdateSongInfo
        (int songNumber, string ComposerName, string Genre, string SongTitle, string Missions,
        Texture2D ThumbnailTexture, Texture2D identityTexture)
    {
        // UI 텍스트에 값 설정, 양식을 수정할 땐 여기서!
        // ----------------------------------------------------------------
        NumberText.text = $"참가번호 {songNumber}번";
        ComposerNameText.text = $"{ComposerName}";
        GenreText.text = $"[{Genre}]";
        SongTitleText.text = $"{SongTitle}";
        MissionsText.text = $"미션 : {Missions}";
        // ----------------------------------------------------------------

        // 썸네일 이미지 설정 (없을 경우 비활성화)
        if (ThumbnailTexture != null)
        {
            Thumbnail.texture = ThumbnailTexture;
            Thumbnail.color = Color.white;

            // 크기를 원본 텍스처에 맞게 설정
            Thumbnail.rectTransform.sizeDelta = new Vector2(ThumbnailTexture.width, ThumbnailTexture.height);

            // 비율 맞춤용 AspectRatioFitter 설정
            AspectRatioFitter Thumbnailfitter = Thumbnail.GetComponent<AspectRatioFitter>();
            if (Thumbnailfitter != null)
            {
                Thumbnailfitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                Thumbnailfitter.aspectRatio = (float)ThumbnailTexture.width / ThumbnailTexture.height;
            }
            else
            {
                Debug.LogWarning("AspectRatioFitter가 RawImage에 붙어있지 않음!");
            }
        }
        else
        {
            Thumbnail.texture = null;
            Thumbnail.color = new Color(1, 1, 1, 0); // 투명 처리
        }

        //썸네일 이미지 설정 (없을 경우 비활성화)
        if (identityTexture != null)
        {
            IdentityImage.texture = identityTexture;
            IdentityImage.color = Color.white;

            // 비율 맞춤용 AspectRatioFitter 설정
            AspectRatioFitter IdentityFitter = IdentityImage.GetComponent<AspectRatioFitter>();
            if (IdentityFitter != null)
            {
                IdentityFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                IdentityFitter.aspectRatio = (float)identityTexture.width / identityTexture.height;
            }
            else
            {
                Debug.LogWarning("AspectRatioFitter가 IdentityImage에 붙어있지 않음!");
            }
        }
        else
        {
            IdentityImage.texture = null;
            IdentityImage.color = new Color(1, 1, 1, 0); // 투명처리
        }
    }
}
