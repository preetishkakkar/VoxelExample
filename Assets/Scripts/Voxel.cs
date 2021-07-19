using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A voxel is simply a cube of a certain size in world space
// Store position (which is center of cube) & extrapolate to create cube as required during rendering
public class Voxel 
{
  public Vector3 position;
}
