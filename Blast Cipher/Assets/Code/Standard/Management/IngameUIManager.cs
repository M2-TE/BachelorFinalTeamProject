using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameUIManager : MonoBehaviour
{
    private static IngameUIManager instance;
    public static IngameUIManager Instance { get => instance ?? (instance = new IngameUIManager()); }

    public GameObject[] Teams, SingleDigits1, DoubleDigits1, SingleDigits2, DoubleDigits2, SingleDigits3, DoubleDigits3, SingleDigits4, DoubleDigits4, SingleDigitsClock, DoubleDigitsClock;

    private bool[] activeTeams;

    private void Start()
    {
        Set(activeTeams = GameManager.Instance.GetActiveTeams(), new int[4] { 0, 0, 0, 0 }, 1);
    }

    public void UpdateUI(int[] scores, int round)
    {
        Set(activeTeams, scores, round);
    }

    private void Set(bool[] teamsActive, int[] scores, int round)
    {
        for (int i = 0; i < 4; i++)
        {
            Set(i, scores[i], teamsActive[i]);
        }

        Set(round);
    }

    private void Set(int team, int score, bool active)
    {
        Teams[team].SetActive(active);
        int singleDigits = score % 10;
        int doubleDigits = Mathf.FloorToInt(score / 10f);
        switch (team)
        {
            case 0:
                for (int i = 0; i < 10; i++)
                {
                    if (i == singleDigits)
                        SingleDigits1[i].SetActive(true);
                    else
                        SingleDigits1[i].SetActive(false);

                    if (i == doubleDigits)
                        DoubleDigits1[i].SetActive(true);
                    else
                        DoubleDigits1[i].SetActive(false);
                }
                break;
            case 1:
                for (int i = 0; i < 10; i++)
                {
                    if (i == singleDigits)
                        SingleDigits2[i].SetActive(true);
                    else
                        SingleDigits2[i].SetActive(false);

                    if (i == doubleDigits)
                        DoubleDigits2[i].SetActive(true);
                    else
                        DoubleDigits2[i].SetActive(false);
                }
                break;
            case 2:
                for (int i = 0; i < 10; i++)
                {
                    if (i == singleDigits)
                        SingleDigits3[i].SetActive(true);
                    else
                        SingleDigits3[i].SetActive(false);

                    if (i == doubleDigits)
                        DoubleDigits3[i].SetActive(true);
                    else
                        DoubleDigits3[i].SetActive(false);
                }
                break;
            case 3:
                for (int i = 0; i < 10; i++)
                {
                    if (i == singleDigits)
                        SingleDigits4[i].SetActive(true);
                    else
                        SingleDigits4[i].SetActive(false);

                    if (i == doubleDigits)
                        DoubleDigits4[i].SetActive(true);
                    else
                        DoubleDigits4[i].SetActive(false);
                }
                break;
            default:
                break;
        }
    }

    private void Set(int round)
    {
        int singleDigits = round % 10;
        int doubleDigits = Mathf.FloorToInt(round / 10f);
        for (int i = 0; i < 10; i++)
        {
            if (i == singleDigits)
                SingleDigitsClock[i].SetActive(true);
            else
                SingleDigitsClock[i].SetActive(false);

            if (i == doubleDigits)
                DoubleDigitsClock[i].SetActive(true);
            else
                DoubleDigitsClock[i].SetActive(false);
        }
    }
}
