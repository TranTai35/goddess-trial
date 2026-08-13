using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class VillageIntroCutscene : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField]
    private PlayableDirector director;

    [Header("Intro UI")]
    [SerializeField]
    private GameObject introUI;

    [SerializeField]
    private TMP_Text introText;

    [Header("Gameplay UI")]
    [SerializeField]
    private GameObject gameplayUI;

    [Header("Text Timing")]
    [SerializeField]
    private float secondTextTime = 4f;

    [SerializeField]
    private float thirdTextTime = 8f;

    [SerializeField]
    private float fourthTextTime = 12f;

    private Coroutine textCoroutine;

    private bool isPlaying;
    private bool hasFinished;

    private void Start()
    {
        if (!GameSession.IsNewGame)
        {
            if (introUI != null)
            {
                introUI.SetActive(false);
            }

            gameObject.SetActive(false);

            return;
        }

        GameSession.IsNewGame = false;

        StartIntro();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipIntro();
        }
    }

    private void StartIntro()
    {
        if (director == null)
        {
            Debug.LogError(
                "VillageIntroCutscene: PlayableDirector is missing."
            );

            return;
        }

        isPlaying = true;
        hasFinished = false;

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.StartCutscene();
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        if (introUI != null)
        {
            introUI.SetActive(true);
        }

        director.stopped += OnTimelineStopped;

        director.time = 0;

        director.Play();

        textCoroutine =
            StartCoroutine(
                IntroTextRoutine()
            );
    }

    private IEnumerator IntroTextRoutine()
    {
        if (introText == null)
            yield break;

        introText.text =
            "Beyond these peaceful lands lies the Goddess Trial.";

        yield return WaitUntilTimelineTime(
            secondTextTime
        );

        introText.text =
            "Warriors leave this village to face the monsters beyond its gates.";

        yield return WaitUntilTimelineTime(
            thirdTextTime
        );

        introText.text =
            "Those who overcome every trial may stand before the Goddess.";

        yield return WaitUntilTimelineTime(
            fourthTextTime
        );

        introText.text =
            "And the victorious warrior will be granted a single wish.";
    }

    private IEnumerator WaitUntilTimelineTime(
        double targetTime
    )
    {
        while (
            director != null &&
            director.state == PlayState.Playing &&
            director.time < targetTime
        )
        {
            yield return null;
        }
    }

    private void SkipIntro()
    {
        if (director == null)
            return;

        if (director.state != PlayState.Playing)
            return;

        director.time =
            director.duration;

        director.Evaluate();

        director.Stop();
    }

    private void OnTimelineStopped(
        PlayableDirector playableDirector
    )
    {
        FinishIntro();
    }

    private void FinishIntro()
    {
        if (hasFinished)
            return;

        hasFinished = true;
        isPlaying = false;

        if (director != null)
        {
            director.stopped -=
                OnTimelineStopped;
        }

        if (textCoroutine != null)
        {
            StopCoroutine(
                textCoroutine
            );

            textCoroutine = null;
        }

        if (introUI != null)
        {
            introUI.SetActive(false);
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.EndCutscene();
        }

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -=
                OnTimelineStopped;
        }
    }
}