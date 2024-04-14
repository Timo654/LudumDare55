using UnityEngine;

public class BackgroundHandler : MonoBehaviour
{
    private int maxScore;
    private int currentScore;
    private void OnEnable()
    {
        RhythmManager.OnGetScore += HandleScore;
        RhythmManager.OnGetMaxScore += SetMaxScore;
    }

    private void OnDisable()
    {
        RhythmManager.OnGetMaxScore -= SetMaxScore;
        RhythmManager.OnGetScore -= HandleScore;
    }
    private void HandleScore(int score)
    {
        if (maxScore == 0) return; // shouldn't be possible, but better safe than sorry
        currentScore = score;
        // TODO - add all the score related behaviour here. should divide score by maxscore and then add elements based on that
        // or divide it before and have subscores to check for idk, handle it somehow eventually to adds backgrounds and frogs
    }

    private void SetMaxScore(int score)
    {
        maxScore = score;
    }
}
