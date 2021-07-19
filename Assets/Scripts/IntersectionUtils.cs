using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IntersectionUtils 
{
  
  // Steps 
  // 1.) Move triangle to center of AABB
  // 2.) calculate triangle's edges normals
  // 3.) take cross product of all edges with aabb's axes 
  // 4.) Check all calculated axes and see if anyone of those don't intersect, if all do then return true else false
  public static bool InsersectTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Bounds aabb)
  {
    // step1
    v1 -= aabb.center;
    v2 -= aabb.center;
    v3 -= aabb.center;

    // step2
    var v12 = (v2 - v1).normalized;
    var v23 = (v3 - v2).normalized;
    var v31 = (v1 - v3).normalized;


    //step3 
    var xAxes = new Vector3(1.0f, 0.0f, 0.0f);
    var yAxes = new Vector3(0.0f, 1.0f, 0.0f);
    var zAxes = new Vector3(0.0f, 0.0f, 1.0f);

    var v12XAxes = Vector3.Cross(v12, xAxes);
    var v23XAxes = Vector3.Cross(v23, xAxes);
    var v31XAxes = Vector3.Cross(v31, xAxes);

    var v12YAxes = Vector3.Cross(v12, yAxes);
    var v23YAxes = Vector3.Cross(v23, yAxes);
    var v31YAxes = Vector3.Cross(v31, yAxes);

    var v12ZAxes = Vector3.Cross(v12, zAxes);
    var v23ZAxes = Vector3.Cross(v23, zAxes);
    var v31ZAxes = Vector3.Cross(v31, zAxes);

    var triangleNormal = Vector3.Cross(v12, v23);

    // step 4
    List<Vector3> axesToCheckForIntsersection = new List<Vector3> {xAxes, yAxes, zAxes,
      v12XAxes, v23XAxes, v31XAxes,
      v12YAxes, v23YAxes, v31YAxes,
      v12ZAxes, v23ZAxes, v31ZAxes, triangleNormal};

    // https://gamedevelopment.tutsplus.com/tutorials/collision-detection-using-the-separating-axis-theorem--gamedev-169
    foreach (Vector3 axis in axesToCheckForIntsersection)
    {
      float dist1 = Vector3.Dot(v1, axis);
      float dist2 = Vector3.Dot(v2, axis);
      float dist3 = Vector3.Dot(v3, axis);

      float dist = aabb.extents.x * Mathf.Abs(Vector3.Dot(axis, xAxes));
      dist += aabb.extents.y * Mathf.Abs(Vector3.Dot(axis, yAxes));
      dist += aabb.extents.z * Mathf.Abs(Vector3.Dot(axis, zAxes));

      float maxD = Mathf.Max(dist1, Mathf.Max(dist2, dist3));
      float minD = Mathf.Min(dist1, Mathf.Min(dist2, dist3));

      if(Mathf.Max(-maxD, minD) > dist)
      {
        return false;
      }

    }

    return true;
  }
}
