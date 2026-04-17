using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NameInputUI : MonoBehaviour
{
    public TMP_InputField inputField;

    public void OnClickPlay()
    {
        PlayerData.PlayerName = inputField != null && inputField.text != null ? inputField.text.Trim() : string.Empty;
        SceneManager.LoadScene("SampleScene");
    }
}