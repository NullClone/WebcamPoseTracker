using UnityEngine;

namespace WPT
{
    [SerializeField]
    public class Bone
    {
        public bool Enabled = true;
        public Transform Transform;
        public Bone Parent;
        public Bone Child;
        public Vector3 Position;
        public Quaternion InitRotation;
        public Quaternion Inverse;
        public Quaternion InverseRotation;
    }
}