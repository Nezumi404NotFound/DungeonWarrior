using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MainSceneManager : MonoBehaviour
{
    public GameObject[] enemys;
    public GameObject enemySpawnPosition;
    private int enemyIndex = 0;
    public GameObject rearGate;
    public Canvas canvas;
    public bool gameStart;
    public GameObject player;
    public Camera playerCamera;
    public PlayerController playerController;
    private Animator playerAnimator;
    public PlayerInput playerInput;
    public MouseLook mouseLook;
    public GameObject frontGate;
    public GameObject panel;
    public DialogueManager dialogueManager;
    public Camera mainCamera;
    public Camera bossRoomCamera;
    public GameObject bossAnimtionTrigger;
    private bool isMovingBossCamera = false;
    public GameObject dragon;
    private DragonController dragonController;
    private Animator dragonAnimator;
    public GameObject musicManager;
    private AudioSource audioSource;
    public AudioClip bossRoomBGM;
    public bool isInstantiatingEnemy = false;
    public bool isFinish = false;
    public Camera finishCamera;
    public AudioClip gameFinishClip;
    public AudioClip BGM;
    public GameObject winPanel;
    public GameObject losePanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStart = false;
        playerInput = player.GetComponent<PlayerInput>();
        playerController = player.GetComponent<PlayerController>();
        mouseLook = playerCamera.GetComponent<MouseLook>();
        playerAnimator = player.GetComponent<Animator>();
        frontGate.GetComponent<Collider>().enabled = false;
        dragonAnimator = dragon.GetComponent<Animator>();
        dragonController = dragon.GetComponent<DragonController>();
        StartCoroutine(PlayerDefault());
        canvas.enabled = false;
        dialogueManager = panel.GetComponent<DialogueManager>();
        dialogueManager.enabled = false;
        audioSource = musicManager.GetComponent<AudioSource>();
        playerCamera.enabled = false;
        finishCamera.enabled = false;
        mainCamera.enabled = true;
        bossRoomCamera.enabled = false;
        playerInput.enabled = false;
        playerController.enabled = false;
        mouseLook.enabled = false;
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isInstantiatingEnemy) 
        {
            StartCoroutine(InstantiateEnemy());
        }
        if (isFinish) 
        {
            StartCoroutine(gameFinish("win"));
        }
        PlayerController platerController = player.GetComponent<PlayerController>();
        if (playerController.isPlayerDead) 
        {
            StartCoroutine(gameFinish("lose"));
        }
    }
    private void FixedUpdate()
    {
        CheckCollision();
    }
    //检测两个物体是否发生碰撞
    private void CheckCollision() 
    {
        if (isMovingBossCamera) return;
        Collider[] hitColliders = Physics.OverlapSphere(bossAnimtionTrigger.transform.position, 1.0f);
        foreach (Collider collider in hitColliders) 
        {
            if (collider.CompareTag("Player")) 
            {
                playerCamera.enabled = false;
                bossRoomCamera.enabled = true;
                isMovingBossCamera = true;
                playerInput.enabled = false;
                playerController.enabled = false;
                mouseLook.enabled = false;
                playerAnimator.SetFloat("Speed", 0);
                StartCoroutine(MovingCamara(bossRoomCamera,"bossRoomDialogue"));
                audioSource.generator = bossRoomBGM;
                audioSource.Stop();
                audioSource.Play();
            }
        }
    }
    IEnumerator PlayerDefault() 
    {
        float startTime = 0f;
        float moveDuration = 2f;
        while (startTime < moveDuration)
        {
            startTime += Time.deltaTime;
            player.transform.Translate(Vector3.forward * 2 * Time.deltaTime);
            playerAnimator.SetFloat("Speed",5);
            yield return null;
        }
        playerAnimator.SetFloat("Speed", 0);
        StartCoroutine(MovingCamara(mainCamera, "dialogue"));
        yield return 0;
    }
    IEnumerator MovingCamara(Camera camara, string dialogueFileName) 
    {
        float startTime = Time.time;
        while (Time.time < startTime + 1f) 
        {
            camara.transform.Translate(Vector3.forward * 4 * Time.deltaTime);
            yield return null;
        }
        canvas.enabled = true;
        panel.SetActive(true);
        dialogueManager.enabled = true;
        dialogueManager.OpenDialogue(dialogueFileName);
        if (isMovingBossCamera)
        {
            dragonAnimator.SetTrigger("scream_trigger");
            StartCoroutine(dragonController.EnemyStateRountine());
        }
        yield return 0;
    }
    public IEnumerator InstantiateEnemy() 
    {
        isInstantiatingEnemy = false;
        while (true) 
        {
            if (!GameObject.FindGameObjectWithTag("Enemy") && enemyIndex <= enemys.Length - 1)
            {
                Instantiate(enemys[enemyIndex], enemySpawnPosition.transform.position, enemys[enemyIndex].transform.rotation);
                enemyIndex++;
            }
            else if (!GameObject.FindGameObjectWithTag("Enemy") && enemyIndex == enemys.Length)
            {
                rearGate.GetComponent<Collider>().enabled = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    public IEnumerator gameFinish(string s) 
    {
        if (s == "win") 
        {
            isFinish = false;
            finishCamera.enabled = true;
            playerCamera.enabled = false;
            playerInput.enabled = false;
            playerController.enabled = false;
            mouseLook.enabled = false;
            Animator playerAnimator = player.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.Play("GameClear", 0);
            }
            AudioSource playerAudioSource = player.GetComponent<AudioSource>();
            if (playerAudioSource != null)
            {
                playerAudioSource.PlayOneShot(gameFinishClip);
            }
            float startTime = Time.time;
            while (Time.time < startTime + 1.5f)
            {
                yield return null;
            }
            canvas.enabled = true;
            winPanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else if (s == "lose")
        {
            isFinish = false;
            finishCamera.enabled = true;
            playerCamera.enabled = false;
            playerInput.enabled = false;
            playerController.enabled = false;
            mouseLook.enabled = false;
            float startTime = Time.time;
            while (Time.time < startTime + 1.5f)
            {
                yield return null;
            }
            canvas.enabled = true;
            losePanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            AudioSource sceneAudioSource = musicManager.GetComponent<AudioSource>();
            sceneAudioSource.clip = BGM;
            sceneAudioSource.Stop();
            sceneAudioSource.Play();
        }
    }
}
