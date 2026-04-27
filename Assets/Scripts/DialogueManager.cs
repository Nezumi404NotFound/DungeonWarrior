using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public GameObject sceneManager;
    public SceneManager sceneManagerCTL;
    private List<DialogueLine> dialogueList;
    private int currentIndex = 0;
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    public string dialogueFileName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
        sceneManagerCTL = sceneManager.GetComponent<SceneManager>();
        LoadDialogue(dialogueFileName);
        Debug.Log(dialogueFileName);
        ShowCurrentLine();
    }
    private void Update() 
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            NextLine();
        }
    }
    void LoadDialogue(string dialogueFileName) 
    {
        string file = dialogueFileName.EndsWith(".json") ? dialogueFileName : dialogueFileName + ".json";
        string filePath = Path.Combine(Application.streamingAssetsPath, file);
        if (File.Exists(filePath)) 
        {
            string jsonContent = File.ReadAllText(filePath);
            DialogueData data = JsonUtility.FromJson<DialogueData>(jsonContent);
            if (data != null)
            {
                dialogueList = data.dialogues;
            }
        }
    }
    void ShowCurrentLine() 
    {
        if (currentIndex < dialogueList.Count) 
        {
            nameText.text = dialogueList[currentIndex].name;
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeSentence(dialogueList[currentIndex].content));
        }
    }
    void NextLine() 
    {
        if (currentIndex < dialogueList.Count - 1)
        {
            currentIndex++;
            ShowCurrentLine();
        }
        else 
        {
            sceneManagerCTL.gameStart = true;
            gameObject.SetActive(false);
        }
    }
    IEnumerator TypeSentence(string sentence)
    {
        contentText.text = "";
        foreach (char c in sentence.ToCharArray()) 
        {
            contentText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

}
