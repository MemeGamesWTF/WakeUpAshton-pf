using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
public class GameplayManager : MonoBehaviour
{
    #region START

    private bool hasGameFinished;
    private bool isPaused;

    public GameObject GameOverPanel;
    public GameObject PausePanel;

    public static GameplayManager Instance;

    [DllImport("__Internal")]
  private static extern void SendScore(int score, int game);

    private void Awake()
    {
        Instance = this;

        hasGameFinished = false;
        isPaused = false;
        GameManager.Instance.IsInitialized = true;

        score = 0;
        _scoreText.text = ((int)score).ToString();
        StartCoroutine(SpawnScore());

        GameOverPanel.SetActive(false);
        PausePanel.SetActive(false);
    }

    #endregion

    #region GAME_LOGIC

    [SerializeField] private ScoreEffect _scoreEffect;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !hasGameFinished && !isPaused)
        {
            if (CurrentScore == null)
            {
                GameEnded();
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            // Only process the click if it hits an object with the "Block" tag
            if (!hit.collider || !hit.collider.gameObject.CompareTag("Block"))
            {
                return; // Ignore the click if it’s not on a "Block" object
            }

            int currentScoreTagId = CurrentScore.TagId;
            int clickedTagId = hit.collider.gameObject.GetComponent<Player>().TagId;

            if (currentScoreTagId != clickedTagId)
            {
                GameEnded();
                return;
            }

            var t = Instantiate(_scoreEffect, CurrentScore.gameObject.transform.position, Quaternion.identity);
            t.Init(Color.white);

            var tempScore = CurrentScore;
            if (CurrentScore.NextScore != null)
            {
                CurrentScore = CurrentScore.NextScore;
            }
            Destroy(tempScore.gameObject);

            UpdateScore();
        }

        if (Input.GetKeyDown(KeyCode.P) && !hasGameFinished)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    #endregion

    #region SCORE

    private float score;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;

    private void UpdateScore()
    {
        score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)score).ToString();
    }

    [SerializeField] private float _spawnTime;
    [SerializeField] private List<Score> _scorePrefabs = new List<Score>();
    private Score CurrentScore;

    private readonly string[] _tags = { "Tag1", "Tag2", "Tag3", "Tag4" };

    private IEnumerator SpawnScore()
    {
        Score prevScore = null;

        while (!hasGameFinished)
        {
            int prefabIndex = Random.Range(0, _scorePrefabs.Count);
            var tempScore = Instantiate(_scorePrefabs[prefabIndex]);

            tempScore.TagId = prefabIndex;
            tempScore.gameObject.tag = _tags[prefabIndex];

            if (prevScore == null)
            {
                prevScore = tempScore;
                CurrentScore = prevScore;
            }
            else
            {
                prevScore.NextScore = tempScore;
                prevScore = tempScore;
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }

    #endregion

    #region GAME_OVER

    [SerializeField] private AudioClip _loseClip;
    public UnityAction GameEnd;

    public void GameEnded()
    {
        hasGameFinished = true;
        GameEnd?.Invoke();
        SoundManager.Instance.PlaySound(_loseClip);
        GameManager.Instance.CurrentScore = (int)score;
        StartCoroutine(GameOver());
    }

    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.001f);
        GameOverPanel.SetActive(true);
        SendScore((int)score, 107);
    }

    public void RestartBTN()
    {
        GameManager.Instance.GoToGameplay();
    }

    #endregion

    #region PAUSE_FUNCTIONALITY

    public void PauseGame()
    {
        if (!hasGameFinished)
        {
            isPaused = true;
            Time.timeScale = 0f;
            PausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        PausePanel.SetActive(false);
    }

    #endregion
}