using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private List<Vector3> _spawnPos;

    [HideInInspector]
    public int TagId; // 0 to 3, set in GameplayManager

    [HideInInspector]
    public Score NextScore;

    private void Awake()
    {
        hasGameFinished = false;
        transform.position = _spawnPos[Random.Range(0, _spawnPos.Count)];
        // TagId is set in GameplayManager.SpawnScore
    }

    private void FixedUpdate()
    {
        if (hasGameFinished) return;
        transform.Translate(_moveSpeed * Time.fixedDeltaTime * Vector3.down);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Obstacle"))
        {
            GameplayManager.Instance.GameEnded();
        }
    }

    private void OnEnable()
    {
        GameplayManager.Instance.GameEnd += GameEnded;
    }

    private void OnDisable()
    {
        GameplayManager.Instance.GameEnd -= GameEnded;
    }

    private bool hasGameFinished;

    private void GameEnded()
    {
        hasGameFinished = true;
    }
}