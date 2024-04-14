using DG.Tweening;
using UnityEngine;

public class BackgroundHandler : MonoBehaviour
{
    private int maxScore;
    private int currentScore;
    private int levelId = 0;

    [SerializeField] private SpriteRenderer tiktaalik;
    private void OnEnable()
    {
        RhythmManager.OnGetScore += HandleScore;
        RhythmManager.OnGetMaxScore += SetMaxScore;
        RhythmManager.OnChartLoaded += SetLevelId;
    }

    private void OnDisable()
    {
        RhythmManager.OnGetMaxScore -= SetMaxScore;
        RhythmManager.OnGetScore -= HandleScore;
        RhythmManager.OnChartLoaded -= SetLevelId;
    }
    private void HandleScore(int score)
    {
        if (maxScore == 0) return; // shouldn't be possible, but better safe than sorry
        currentScore = score;
        // TODO - add all the score related behaviour here. should divide score by maxscore and then add elements based on that
        // or divide it before and have subscores to check for idk, handle it somehow eventually to adds backgrounds and frogs
        if (levelId == 1) // level 2
        {
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
