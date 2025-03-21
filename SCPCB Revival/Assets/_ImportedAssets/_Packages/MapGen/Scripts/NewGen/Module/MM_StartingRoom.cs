using System.Collections;
using System.Collections.Generic;
using ALOB.Map;
using UnityEngine;

public class MM_StartingRoom : MapGenModule, IGenBeforeSpawn
{
    public MM_StartingRoom(System.Random randomGen, GeneratorMapPreset gMP) : base(randomGen, gMP)
    {
    }

    public void OnPrepareZones(Zone[,] zoneGrid)
    {

    }
}
