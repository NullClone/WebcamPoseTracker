using UnityEngine;

namespace WPT
{
    public class Bone
    {
        public Transform Transform;

        public Bone Parent;
        public Bone Child;

        public Vector3 Position;
        public Quaternion Rotation;

        public Quaternion Inverse;
        public Quaternion InverseRotation;
    }
}