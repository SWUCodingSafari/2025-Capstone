using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{

    public GameObject LoginView; // 로그인 화면
    public GameObject SignUpPanel; // 회원 가입 화면
    public GameObject MainView; // 메인 화면

    public InputField inputField_ID;
    public InputField inputFiled_PW;
    public Button play_button; // 로그인 버튼
    public Button sign_up_button; // 회원 가입 버튼
    public Text login_failed_text; // 로그인 실패 시 나타낼 메시지

    private string user = "User";
    private string password = "1234";

    public void PlayButtonClick()
    {
        if (inputField_ID.text == user && inputFiled_PW.text == password)
        {
            Debug.Log("로그인 성공");
            LoginView.SetActive(false); // 로그인 성공 시 로그인 화면 닫기
            MainView.SetActive(true); // 메인 화면 열기
        }
        else
        {
            Debug.Log("로그인 실패");
            login_failed_text.text = "아이디 또는 비밀번호가 잘못되었습니다";
            inputField_ID.text = ""; // ID 필드 초기화
            inputFiled_PW.text = ""; // PW 필드 초기화
        }
    }

    public void SignUpButtonClick()
    {
        SignUpPanel.SetActive(true); // 회원 가입 화면 띄우기
        login_failed_text.text = "";
    }
}
