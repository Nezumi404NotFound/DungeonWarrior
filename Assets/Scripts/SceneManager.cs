using UnityEngine;
using UnityEngine.AI;

public class SceneManager : MonoBehaviour
{
    public GameObject[] enemys;
    public GameObject enemySpawnPosition;
    private int enemyIndex = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (!GameObject.FindGameObjectWithTag("Enemy") && enemyIndex <= enemys.Length - 1)
        {
            Instantiate(enemys[enemyIndex], enemySpawnPosition.transform.position, enemys[enemyIndex].transform.rotation);
            enemyIndex++;
        }
    }
}
