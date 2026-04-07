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
    private bool gameStart;
    public GameObject player;
    public Camera playerCamera;
    public Camera mainCamara;
    private PlayerController playerController;
    private Animator playerAnimator;
    private PlayerInput playerInput;
    private MouseLook mouseLook;
    public GameObject frontGate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameStart = false;
        playerInput = player.GetComponent<PlayerInput>();
        playerController = player.GetComponent<PlayerController>();
        mouseLook = playerCamera.GetComponent<MouseLook>();
        playerAnimator = player.GetComponent<Animator>();
        frontGate.GetComponent<Collider>().enabled = false;
        StartCoroutine(PlayerDefault());
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStart) 
        {
            canvas.enabled = false;
            playerCamera.enabled = false;
            mainCamara.enabled = true;
            playerInput.enabled = false;
            playerController.enabled = false;
            mouseLook.enabled = false;
        }
    }
    private void FixedUpdate()
    {
        if (gameStart) 
        {
            canvas.enabled = true;
            playerCamera.enabled = true;
            mainCamara.enabled = false;
            playerInput.enabled = true;
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
    }
}
