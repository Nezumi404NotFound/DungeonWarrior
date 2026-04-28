using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;
    public SceneManager sceneManagerCTL;
    private List<DialogueLine> dialogueList;
    private int currentIndex;
    private float typingSpeed = 0.05f;
    private Coroutine typingCoroutine;
    public string dialogueFileName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() 
    {
    }
    
    private void Update() 
    {
        if (Input.GetButtonDown("Fire1")) 
        {
            NextLine();
        }
    }
    public void OpenDialogue(string fileName)
    {
        this.dialogueFileName = fileName;
        LoadDialogue(fileName);
        currentIndex = 0;

        if (dialogueList != null && dialogueList.Count > 0)
        {
            ShowCurrentLine();
        }
    }
    public void LoadDialogue(string dialogueFileName) 
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
            SceneManager sceneManager = Object.FindFirstObjectByType<SceneManager>();
            sceneManager.mainCamara.enabled = false;
            sceneManager.bossRoomCamera.enabled = false;
            sceneManager.playerCamera.enabled = true;
            sceneManager.playerInput.enabled = true;
            sceneManager.playerController.enabled = true;
            sceneManager.mouseLook.enabled = true;
            sceneManager.frontGate.GetComponent<Collider>().enabled = true;
            sceneManager.isInstantiatingEnemy = true;
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
