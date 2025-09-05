using UnityEngine;

namespace WPT
{
    public class Bone
    {
        public Transform Transform;
        public bool Active;
        public Bone Parent;
        public Bone Child;
        public Vector3 Position;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;
    }
}