using System;
using UnityEngine;

/// <summary>
/// Local and public modular script to do various things when the game is running at a certain date. (Mostly holidays)
/// </summary>
public class ExecuteOnDate : MonoBehaviour {
    // Enum that holds every task that the object can follow. Some if not all will conflict with each other
    private enum WhatToDo {
        DisableOnDate = 0,
        EnableOnDate = 1,
        DisableUnlessDate = 2
    }
    
    // An array containing the multiple tasks the script needs to execute and a toggle for if it should do these
    // things immediately. In most cases that will be true.
    [Header("Task Settings")]
    [SerializeField] private WhatToDo[] whatToDo;
    [SerializeField] private bool executeOnStart = true;
    
    // The three spots where the specific date or date range will be defined. (IE. December would be -1, 12, -1)
    [Header("Date Settings")]
    [Tooltip("Set to -1 to ignore. Use all for specific date, or just year/month for broad date.")]
    [SerializeField] private int year = -1;
    [SerializeField] private int month = -1;
    [SerializeField] private int day = -1;

    #region Unity Callbacks
    private void Start() {
        // If we're not supposed to do anything at the start, then don't do anything
        if (!executeOnStart) return;
        
        var now = DateTime.Now; // Set variable "now" to the current date and time
        var match = true; // Default the "match" variable to true
        
        // If the year should not be ignored, set match to true if the year matches. Then do the same for the rest
        if (year != -1)
            match &= (now.Year == year);
        if (month != -1)
            match &= (now.Month == month);
        if (day != -1)
            match &= (now.Day == day);

        // For every task in the defined array, check what the task is and execute as necessary
        foreach (var what in whatToDo) {
            switch (what) {
                case WhatToDo.DisableOnDate:
                    // If the current date matches the defined date disable the object
                    if (match)
                        gameObject.SetActive(false);
                    break;
                case WhatToDo.EnableOnDate:
                    // If the current date matches the defined date enable the object
                    if (match)
                        gameObject.SetActive(true);
                    break;
                case WhatToDo.DisableUnlessDate:
                    // If the current date does not match the defined date disable the object
                    if (!match)
                        gameObject.SetActive(false);
                    break;
                default:
                    // If none of these then don't do anything and throw this guy out there
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Publicly accessible method to execute the various defined tasks in this object's WhatToDo array
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void ExecuteGivenTasks() {
        // Everything here works exactly the same as how it does in the start method, so use that for reference
        var now = DateTime.Now;
        var match = true;
        
        if (year != -1)
            match &= (now.Year == year);
        if (month != -1)
            match &= (now.Month == month);
        if (day != -1)
            match &= (now.Day == day);

        foreach (var what in whatToDo) {
            switch (what) {
                case WhatToDo.DisableOnDate:
                    if (match)
                        gameObject.SetActive(false);
                    break;
                case WhatToDo.EnableOnDate:
                    if (match)
                        gameObject.SetActive(true);
                    break;
                case WhatToDo.DisableUnlessDate:
                    if (!match)
                        gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    #endregion
}