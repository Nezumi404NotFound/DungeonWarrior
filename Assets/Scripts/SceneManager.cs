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
    public PlayerController playerController;
    private Animator playerAnimator;
    public PlayerInput playerInput;
    public MouseLook mouseLook;
    public GameObject frontGate;
    public GameObject panel;
    public DialogueManager dialogueManager;
    public Camera mainCamara;
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
        mainCamara.enabled = true;
        bossRoomCamera.enabled = false;
        playerInput.enabled = false;
        playerController.enabled = false;
        mouseLook.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInstantiatingEnemy) 
        {
            StartCoroutine(InstantiateEnemy());
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
        float startTime = Time.time;
        while (Time.time < startTime + 2f)
        {
            player.transform.Translate(Vector3.forward * 2 * Time.deltaTime);
            playerAnimator.SetFloat("Speed",5);
            yield return null;
        }
        playerAnimator.SetFloat("Speed", 0);
        StartCoroutine(MovingCamara(mainCamara, "dialogue"));
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
            yield return new WaitForSeconds(0.5f);
        }
    }
}
