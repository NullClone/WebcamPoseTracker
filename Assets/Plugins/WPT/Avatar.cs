using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class Avatar : MonoBehaviour
    {
        // Fields

        [SerializeField] private InferenceRunner _runner;
        [SerializeField] private Vector3 _hipOffset;
        [SerializeField] private Vector3 _neckOffset;

        private Animator _animator;
        private Vector3 _initPosition;
        private bool _isInitialized;


        // Properties

        public Bone[] Bones { get; set; }

        public Bone Hips { get; set; }

        public Bone Spine { get; set; }

        public Bone Neck { get; set; }


        // Methods

        private void Start()
        {
            _animator = gameObject.GetComponent<Animator>();

            if (_animator == null || _runner == null) return;

            Initialize();
        }

        private void Update()
        {
            if (!_isInitialized) return;

            SetPosition();

            UpdateModel();
        }

        private void Initialize()
        {
            Bones = new Bone[(int)BoneIndex.Count];

            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i] = new();
            }

            Hips = new();
            Spine = new();
            Neck = new();

            GetBone();
            SetBone();
            SetInverse();

            _isInitialized = true;
        }

        private void GetBone()
        {
            // Body

            Hips.Transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            Spine.Transform = _animator.GetBoneTransform(HumanBodyBones.Spine);
            Neck.Transform = _animator.GetBoneTransform(HumanBodyBones.Neck);


            // Left Arm

            Bones[(int)BoneIndex.LeftShoulder].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Bones[(int)BoneIndex.LeftElbow].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Bones[(int)BoneIndex.LeftWrist].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftHand);


            // Right Arm

            Bones[(int)BoneIndex.RightShoulder].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Bones[(int)BoneIndex.RightElbow].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Bones[(int)BoneIndex.RightWrist].Transform = _animator.GetBoneTransform(HumanBodyBones.RightHand);


            // Left Leg

            Bones[(int)BoneIndex.LeftHip].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Bones[(int)BoneIndex.LeftKnee].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Bones[(int)BoneIndex.LeftAnkle].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);


            // Right Leg

            Bones[(int)BoneIndex.RightHip].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Bones[(int)BoneIndex.RightKnee].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Bones[(int)BoneIndex.RightAnkle].Transform = _animator.GetBoneTransform(HumanBodyBones.RightFoot);


            // Head

            Bones[(int)BoneIndex.Nose].Transform = _animator.GetBoneTransform(HumanBodyBones.Head);
        }

        private void SetBone()
        {
            // Left Arm

            Bones[(int)BoneIndex.LeftShoulder].Child = Bones[(int)BoneIndex.LeftElbow];
            Bones[(int)BoneIndex.LeftElbow].Child = Bones[(int)BoneIndex.LeftWrist];
            Bones[(int)BoneIndex.LeftElbow].Parent = Bones[(int)BoneIndex.LeftElbow];


            // Right Arm

            Bones[(int)BoneIndex.RightShoulder].Child = Bones[(int)BoneIndex.RightElbow];
            Bones[(int)BoneIndex.RightElbow].Child = Bones[(int)BoneIndex.RightWrist];
            Bones[(int)BoneIndex.RightElbow].Parent = Bones[(int)BoneIndex.RightElbow];


            // Left Leg

            Bones[(int)BoneIndex.LeftHip].Child = Bones[(int)BoneIndex.LeftKnee];
            Bones[(int)BoneIndex.LeftKnee].Child = Bones[(int)BoneIndex.LeftAnkle];
            Bones[(int)BoneIndex.LeftAnkle].Child = Bones[(int)BoneIndex.LeftHeel];
            Bones[(int)BoneIndex.LeftAnkle].Parent = Bones[(int)BoneIndex.LeftKnee];


            // Right Leg

            Bones[(int)BoneIndex.RightHip].Child = Bones[(int)BoneIndex.RightKnee];
            Bones[(int)BoneIndex.RightKnee].Child = Bones[(int)BoneIndex.RightAnkle];
            Bones[(int)BoneIndex.RightAnkle].Child = Bones[(int)BoneIndex.RightHeel];
            Bones[(int)BoneIndex.RightAnkle].Parent = Bones[(int)BoneIndex.RightKnee];
        }

        private void SetInverse()
        {
            var forward = MatrixUtils.TriangleNormal(
                Hips.Transform.position,
                Bones[(int)BoneIndex.LeftHip].Transform.position,
                Bones[(int)BoneIndex.RightHip].Transform.position);

            foreach (var bone in Bones)
            {
                if (bone.Transform == null) continue;

                bone.Rotation = bone.Transform.rotation;

                if (bone.Child != null && bone.Child.Transform != null)
                {
                    bone.Inverse = Quaternion.Inverse(
                        MatrixUtils.LookRotation(
                            bone.Transform.position -
                            bone.Child.Transform.position, forward));
                    bone.InverseRotation = bone.Inverse * bone.Rotation;
                }
            }

            Hips.Rotation = Hips.Transform.rotation;
            Spine.Rotation = Hips.Transform.rotation;
            Neck.Rotation = Neck.Transform.rotation;

            _initPosition = Hips.Transform.position;

            Hips.Inverse = Quaternion.Inverse(MatrixUtils.LookRotation(forward));
            Hips.InverseRotation = Hips.Inverse * Hips.Rotation;
        }

        private void SetPosition()
        {
            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i].Position = _runner.Positions[i];
            }

            Hips.Position = (
                Bones[(int)BoneIndex.LeftHip].Position +
                Bones[(int)BoneIndex.RightHip].Position) / 2f;

            Hips.Position += _hipOffset;

            var chest = (
                Bones[(int)BoneIndex.LeftShoulder].Position +
                Bones[(int)BoneIndex.RightShoulder].Position) / 2f;

            Neck.Position = chest + _neckOffset;

            Spine.Position = (chest + Hips.Position) / 2f;
        }

        private void UpdateModel()
        {
            var forward = MatrixUtils.TriangleNormal(
                Hips.Position,
                Bones[(int)BoneIndex.LeftHip].Position,
                Bones[(int)BoneIndex.RightHip].Position);

            foreach (var bone in Bones)
            {
                if (bone.Parent != null)
                {
                    bone.Transform.rotation = MatrixUtils.LookRotation(
                        bone.Position - bone.Child.Position,
                        bone.Parent.Position - bone.Position) * bone.InverseRotation;
                }
                else if (bone.Child != null)
                {
                    bone.Transform.rotation = MatrixUtils.LookRotation(
                        bone.Position - bone.Child.Position, forward) * bone.InverseRotation;
                }
            }

            /*

            var t1 = Vector3.Distance(Bones[(int)BoneIndex.Nose].Position, Neck.Position);
            var t2 = Vector3.Distance(Neck.Position, Spine.Position);
            var pm = (Bones[(int)BoneIndex.RightShoulder].Position + Bones[(int)BoneIndex.LeftShoulder].Position) / 2f;
            var t3 = Vector3.Distance(Spine.Position, pm);
            var t4r = Vector3.Distance(Bones[(int)BoneIndex.RightHip].Position, Bones[(int)BoneIndex.RightKnee].Position);
            var t4l = Vector3.Distance(Bones[(int)BoneIndex.LeftHip].Position, Bones[(int)BoneIndex.LeftKnee].Position);
            var t4 = (t4r + t4l) / 2f;
            var t5r = Vector3.Distance(Bones[(int)BoneIndex.RightKnee].Position, Bones[(int)BoneIndex.RightAnkle].Position);
            var t5l = Vector3.Distance(Bones[(int)BoneIndex.LeftKnee].Position, Bones[(int)BoneIndex.LeftAnkle].Position);
            var t5 = (t5r + t5l) / 2f;
            var t = t1 + t2 + t3 + t4 + t5;

            tall = (t * 0.7f) + (prevTall * 0.3f);
            prevTall = tall;

            if (tall == 0)
            {
                tall = centerTall;
            }

            float dz = (centerTall - tall) / centerTall * zScale;
            */

            Hips.Transform.position = (Hips.Position * 0.05f) + new Vector3(_initPosition.x, _initPosition.y, _initPosition.z);
            Hips.Transform.rotation = MatrixUtils.LookRotation(forward) * Hips.InverseRotation;
        }
    }
}