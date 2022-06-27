using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour
{
    bool isActive;
    [SerializeField] private Image pannel;
    [SerializeField] private RectTransform menuRect;
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            menuRect.DOLocalMoveY(isActive ? -1000 : 0, 0.5f);
            pannel.DOFade(isActive ? 0 : 0.5f, 0.5f);
            isActive = !isActive;
        }
    }
}
