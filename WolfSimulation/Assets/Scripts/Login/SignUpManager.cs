using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignUpManager : MonoBehaviour
{
    public GameObject SignUpPanel; // 회원 가입 화면

    public InputField inputField_ID;
    public InputField inputFiled_PW;
    public Button create_button; // 계정 생성 버튼
    public Button x_button; // 창 닫기 버튼
    public Text create_failed_text; // 계정 생성 실패 시 나타낼 메시지

    private string user = "User";

    public void CreateButtonClick()
    {
        if (inputField_ID.text != user)
        {
            Debug.Log("회원 가입 성공");
            SignUpPanel.SetActive(false); // 회원 가입 성공 시 회원 가입 창 닫기
        }
        else
        {
            Debug.Log("회원 가입 실패");
            create_failed_text.text = "다른 아이디를 입력해 주세요";
            inputField_ID.text = ""; // ID 필드 초기화
            inputFiled_PW.text = ""; // PW 필드 초기화
        }
    }

    public void XButtonClick()
    {
        SignUpPanel.SetActive(false); // 창 닫기
        create_failed_text.text = "";
    }
}
