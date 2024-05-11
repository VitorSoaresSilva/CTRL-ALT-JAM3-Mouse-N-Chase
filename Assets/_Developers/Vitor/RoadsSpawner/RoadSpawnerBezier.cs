using System;
using PathCreation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Developers.Vitor.RoadsSpawner
{
    public class RoadSpawnerBezier : MonoBehaviour
    {
        [SerializeField] private PathCreator[] _pathCreator;
        public Transform[] waypoints;

        private void Start()
        {
            var pathPrefab = _pathCreator[0];
            var path = Instantiate (pathPrefab, Vector3.zero, Quaternion.identity);
            BezierPath bezierPath = new BezierPath (waypoints, false, PathSpace.xyz);
            path.bezierPath = bezierPath;
            
            
            // // var follower = Instantiate (followerPrefab);
            // // follower.pathCreator = path;
            // // path.transform.parent = t;
            // var point = path.path.GetPoint(path.path.NumPoints - 1);
            // var pointRotation = path.path.GetRotation(path.path.NumPoints - 1);
            // var path2 = Instantiate (pathPrefab, point, pointRotation);
            // Transform newTransform = path.transform;
            //
            // newTransform.position = path2.path.GetPoint(0) - point;
            // newTransform.rotation = pointRotation;
            // path2.path.UpdateTransform(newTransform);
        }

        public void SetPath()
        {
            
        }
    }
}