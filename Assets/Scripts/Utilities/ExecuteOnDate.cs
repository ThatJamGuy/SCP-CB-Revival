using System;
using UnityEngine;

public class ExecuteOnDate : MonoBehaviour {
    private enum TaskToExecute {
        EnabledOnDate = 0,
        DisableOnDate = 1
    }

    [SerializeField] private GameObject affectedObject;

    [Header("Settings")]
    [SerializeField] private TaskToExecute taskToExecute;

    // Period of time to do this stuff on. Leave -1 to ignore this aspect
    [SerializeField] private int month = -1;
    [SerializeField] private int day = -1;
    [SerializeField] private int year = -1;

    private void Start() {
        DateTime rightNow = DateTime.Now;
        bool dateMatches = false;

        if (month != -1) dateMatches &= (rightNow.Month == month);
        if (day != -1) dateMatches &= (rightNow.Day == day);
        if (year != -1) dateMatches &= (rightNow.Year == year);

        if (!dateMatches) return;

        switch (taskToExecute) {
            case TaskToExecute.EnabledOnDate:
                affectedObject.SetActive(true);
                break;
            case TaskToExecute.DisableOnDate:
                affectedObject.SetActive(false);
                break;
        }
    }
}