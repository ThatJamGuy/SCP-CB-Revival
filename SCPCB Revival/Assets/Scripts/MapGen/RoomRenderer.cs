using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRenderer : MonoBehaviour
{
    public Transform player;
    public float disableRange = 10f;
    private List<GameObject> rooms = new List<GameObject>();
    private List<GameObject> lights = new List<GameObject>();

    void Start()
    {
        StartCoroutine(FindRooms());
        StartCoroutine(FindLights());
    }

    IEnumerator FindRooms()
    {
        yield return new WaitForSeconds(1f);
        GameObject[] roomObjs = GameObject.FindGameObjectsWithTag("Room");
        foreach (var obj in roomObjs)
        {
            if (!rooms.Contains(obj))
            {
                rooms.Add(obj);
            }
        }
    }

    IEnumerator FindLights()
    {
        yield return new WaitForSeconds(1f);
        Light[] lightObjs = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var lightObj in lightObjs)
        {
            if (!lights.Contains(lightObj.gameObject))
            {
                lights.Add(lightObj.gameObject);
            }
        }
    }

    void Update()
    {
        foreach (var room in rooms)
        {
            float distance = Vector3.Distance(player.position, room.transform.position);
            if (distance > disableRange)
            {
                room.SetActive(false);
            }
            else
            {
                room.SetActive(true);
            }
        }

        foreach (var light in lights)
        {
            float distance = Vector3.Distance(player.position, light.transform.position);
            if (distance > disableRange)
            {
                light.SetActive(false);
            }
            else
            {
                light.SetActive(true);
            }
        }
    }
}