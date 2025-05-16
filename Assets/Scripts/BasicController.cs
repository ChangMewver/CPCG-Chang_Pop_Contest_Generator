using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class BasicController : MonoBehaviour
{
    //곡 이동용
    private int currentSongIndex = 0;
    private List<string> songList = new List<string>();

    //씬을 넘어가기 위한 코드. 버튼에 넣어 사용
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    void Update()
    {
        HandleCursorVisibility(); // 커서 처리

        //    KeyInput();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "MainScene")
            {
                Application.Quit();
            }
            else
            {
                SceneManager.LoadScene("MainScene");
            }
        }
    }

    void HandleCursorVisibility()
    {
        bool isMainScene = SceneManager.GetActiveScene().name == "MainScene";

        if (isMainScene)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    //void KeyInput()
    //{
    //    // 키 입력이 있을 때, 키보드로 트랙 이동 가능
    //    if (Input.GetKeyDown(KeyCode.LeftArrow)) JumpToSong(currentSongIndex - 1);
    //    if (Input.GetKeyDown(KeyCode.RightArrow)) JumpToSong(currentSongIndex + 1);
    //    if (Input.GetKeyDown(KeyCode.Q)) JumpToSong(currentSongIndex - 10);
    //    if (Input.GetKeyDown(KeyCode.W)) JumpToSong(currentSongIndex + 10);
    //    if (Input.GetKeyDown(KeyCode.Z)) JumpToSong(0);
    //}


    //void JumpToSong(int index)
    //{
    //    // 유효한 인덱스인지 확인
    //    currentSongIndex = Mathf.Clamp(index, 0, songList.Count - 1);
    //    Debug.Log($"현재 곡: {songList[currentSongIndex]}"); // 곡 정보 출력 (가정)
    //}
}


