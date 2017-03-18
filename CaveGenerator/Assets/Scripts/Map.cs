using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Map : MonoBehaviour
{
    public List<Vector2> RoomCenters { get; set; }
    public List<Vector2> RoomBorders { get; set; }

    public int SizeY { get; set; }
    public int SizeX { get; set; }
    
}
