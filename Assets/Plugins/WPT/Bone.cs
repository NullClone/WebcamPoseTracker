using UnityEngine;

namespace WPT
{
    public class Bone
    {
        public Transform Transform;
        public Bone Child;
        public Bone Parent;

        public float score;

        public Vector3 Forward;
        public Vector3 Position3D;
        public Vector3 CurrentPosition3D;

        public Vector3 KalmanK;
        public Vector3 KalmanX;
        public Vector3 KalmanP;

        public Quaternion InitRotation;
        public Quaternion InverseRotation;
        public Quaternion Inverse;
    }
}