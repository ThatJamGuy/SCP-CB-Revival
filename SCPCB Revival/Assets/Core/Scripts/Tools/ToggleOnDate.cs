using UnityEngine;
using System;

public class ToggleOnDate : MonoBehaviour {
    // Set these in the Inspector. Leave as -1 to ignore.
    [Tooltip("Set to -1 to ignore. Use all for specific date, or just year/month for broad date.")]
    public int year = -1;
    public int month = -1;
    public int day = -1;

    private void Start() {
        var now = DateTime.Now;
        bool match = true;

        if (year != -1)
            match &= (now.Year == year);
        if (month != -1)
            match &= (now.Month == month);
        if (day != -1)
            match &= (now.Day == day);

        gameObject.SetActive(match);
    }
}