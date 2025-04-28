using UnityEngine;
using TMPro;
using SingletonManagers;
using Unity.VisualScripting;

public class WIN_LOSE : MonoBehaviour
{
    [SerializeField] private GameObject _winText;
    [SerializeField] private GameObject _loseText;


    private void Start()
    {
        LevelConditionManager.Instance._currentConditions.OnWin += WIN;
        LevelConditionManager.Instance._currentConditions.OnLose += LOSE;

    }
    private void OnDisable()
    {
        LevelConditionManager.Instance._currentConditions.OnWin -= WIN;
        LevelConditionManager.Instance._currentConditions.OnLose -= LOSE;
    }
    private void LOSE()
    {
        _loseText.SetActive(true);
        Invoke("ReturnToLevelSelect", 2f);
    }
    private void WIN()
    {
        _winText.SetActive(true);
    }
    private void ReturnToLevelSelect()
    {
        SceneIndexes.LoadSceneByIndexAsync(SceneIndexes.SlidingMenuScene);
    }
}
