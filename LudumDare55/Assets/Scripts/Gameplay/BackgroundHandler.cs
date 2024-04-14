using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundHandler : MonoBehaviour
{
    private int maxScore;
    private int currentScore;
    public GameObject[] frogObjects;
    public GameObject[] bgObjects;
    private int levelId = 0;
    private List<GameObject> disabledFrogObjects = new List<GameObject>();
    private float currentFade;
    private float fadeTarget = 0.0f;

    [SerializeField] private SpriteRenderer tiktaalik;
    private void Start()
    {
        frogHandler("despawn");
        bgHandler("despawn");
    }
    private void eventHandler()
    {
        switch (fadeTarget)
        {
            case 0.1f:
                frogHandler("spawn");
                break;
            case 0.2f:
                frogHandler("spawn");
                break;
            case 0.3f:
                frogHandler("spawn");
                var bg = bgObjects[0].GetComponent<SpriteRenderer>();
                bg.color = new Color(bg.color.r, bg.color.g, bg.color.b, 0f);
                bgObjects[0].SetActive(true);
                bg.DOFade(1f, 1f);
                break;
            case 0.4f:
                frogHandler("spawn");
                break;
            case 0.5f:
                frogHandler("spawn");
                break;
            case 0.6f:
                frogHandler("spawn");
                var bg2 = bgObjects[1].GetComponent<SpriteRenderer>();
                bg2.color = new Color(bg2.color.r, bg2.color.g, bg2.color.b, 0f);
                bgObjects[1].SetActive(true);
                bg2.DOFade(1f, 1f);
                break;
            case 0.7f:
                frogHandler("spawn");
                break;
            case 0.8f:
                frogHandler("spawn");
                break;
            case 0.9f:
                frogHandler("spawn");
                var bg3 = bgObjects[2].GetComponent<SpriteRenderer>();
                bg3.color = new Color(bg3.color.r, bg3.color.g, bg3.color.b, 0f);
                bgObjects[2].SetActive(true);
                bg3.DOFade(1f, 1f);
                break;
            case 1.0f:
                break;

        }

    }
    private void bgHandler(string cmd)
    {
        if (cmd == "despawn")
        {
            Debug.Log("All bg despawned");
            foreach (GameObject bg in bgObjects)
            {
                bg.SetActive(false);
            }
        }

    }
    private void frogHandler(string cmd)
    {
        if (cmd == "despawn")
        {
            Debug.Log("All frogs despawned");
            foreach (GameObject frog in frogObjects)
            {
                frog.SetActive(false);
                disabledFrogObjects.Add(frog);
            }
        }
        else if (cmd == "spawn")
        {
            System.Random random = new System.Random();
            int i = random.Next(0, disabledFrogObjects.Count);
            var froggy = disabledFrogObjects[i].GetComponent<SpriteRenderer>();
            froggy.color = new Color(froggy.color.r, froggy.color.g, froggy.color.b, 0f);
            disabledFrogObjects[i].SetActive(true);
            froggy.DOFade(1f, 1f);
            disabledFrogObjects.RemoveAt(i);
            Debug.Log("Frog spawned");
        }

    }
    private void OnEnable()
    {
        RhythmManager.OnGetScore += HandleScore;
        RhythmManager.OnGetMaxScore += SetMaxScore;
        RhythmManager.OnSongLoad += SetLevelId;
    }


    private void OnDisable()
    {
        RhythmManager.OnGetMaxScore -= SetMaxScore;
        RhythmManager.OnGetScore -= HandleScore;
        RhythmManager.OnSongLoad -= SetLevelId;
    }
    private void HandleScore(int score)
    {
        currentFade = currentScore / (maxScore * 0.6f);
        //Score for triggerable events 
        float roundedFade = (float)Math.Floor(currentFade * 10) / 10;
        if (roundedFade > fadeTarget)
        {
            fadeTarget = roundedFade;
            eventHandler();
        }
        if (maxScore == 0) return; // shouldn't be possible, but better safe than sorry
        currentScore = score;
        // TODO - add all the score related behaviour here. should divide score by maxscore and then add elements based on that
        // or divide it before and have subscores to check for idk, handle it somehow eventually to adds backgrounds and frogs
        if (levelId == 1) // level 2
        {
            tiktaalik.DOKill(); // l�petame hetkel k�ivad tween animatsioonid
            var currentFade = currentScore / (maxScore * 0.6f);
            tiktaalik.DOFade(currentFade, 0.5f);
        }
    }

    private void SetMaxScore(int score)
    {
        maxScore = score;
    }

    private void SetLevelId(int id)
    {
        levelId = id;
    }
}
