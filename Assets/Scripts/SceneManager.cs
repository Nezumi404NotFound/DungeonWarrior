using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class SceneManager : MonoBehaviour
{
    public GameObject[] enemys;
    public GameObject enemySpawnPosition;
    private int enemyIndex = 0;
    public GameObject rearGate;
    public Canvas canvas;
    public bool gameStart;
    public GameObject player;
    public Camera playerCamera;
    private PlayerController playerController;
    private Animator playerAnimator;
    private PlayerInput playerInput;
    private MouseLook mouseLook;
    public GameObject frontGate;
    public GameObject panel;
    public DialogueManager dialogueManager;
    public Camera mainCamara;
    public Camera bossRoomCamera;
    public GameObject bossAnimtionTrigger;
    private bool isMovingBossCamera = false;
    public GameObject dragon;
    private Animator dragonAnimator;
    public GameObject musicManager;
    private AudioSource audioSource;
    public AudioClip bossRoomBGM;
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
        StartCoroutine(PlayerDefault());
        canvas.enabled = false;
        dialogueManager = panel.GetComponent<DialogueManager>();
        dialogueManager.enabled = false;
        audioSource = musicManager.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStart) 
        {
            playerCamera.enabled = false;
            mainCamara.enabled = true;
            bossRoomCamera.enabled = false;
            playerInput.enabled = false;
            playerController.enabled = false;
            mouseLook.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        if (gameStart) 
        {
            playerCamera.enabled = true;
            mainCamara.enabled = false;
            playerInput.enabled = true;
            playerController.enabled = true;
            mouseLook.enabled = true;
            frontGate.GetComponent<Collider>().enabled = true;
            if (!GameObject.FindGameObjectWithTag("Enemy") && enemyIndex <= enemys.Length - 1)
            {
                Instantiate(enemys[enemyIndex], enemySpawnPosition.transform.position, enemys[enemyIndex].transform.rotation);
                enemyIndex++;
            }
            else if (!GameObject.FindGameObjectWithTag("Enemy") && enemyIndex == enemys.Length)
            {
                rearGate.GetComponent<Collider>().enabled = false;
            }
        }
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
                mainCamara.enabled = false;
                bossRoomCamera.enabled = true;
                StartCoroutine(MovingCamara(bossRoomCamera,"bossRoomDialogue"));
                isMovingBossCamera = true;
                playerInput.enabled = false;
                playerController.enabled = false;
                mouseLook.enabled = false;
                audioSource.generator = bossRoomBGM;
                audioSource.Stop();
                audioSource.Play();
            }
        }
    }
    IEnumerator PlayerDefault() 
    {
        float startTime = Time.time;
        while (Time.time < startTime + 2f)
        {
            player.transform.Translate(Vector3.forward * 2 * Time.deltaTime);
            playerAnimator.SetFloat("Speed",5);
            yield return null;
        }
        playerAnimator.SetFloat("Speed", 0);
        StartCoroutine(MovingCamara(mainCamara, "dialogue"));
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
        dialogueManager.dialogueFileName = dialogueFileName;
        dialogueManager.enabled = true;
        if (isMovingBossCamera)
        {
            dragonAnimator.SetTrigger("scream_trigger");
        }
    }
}
