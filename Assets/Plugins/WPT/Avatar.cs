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
        private Bone[] Bones;


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

            #region GetBone

            // Body

            Bones[(int)BoneIndex.Hips].Transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            Bones[(int)BoneIndex.Spine].Transform = _animator.GetBoneTransform(HumanBodyBones.Spine);


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

            #endregion

            #region SetBone

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

            #endregion

            var forward = MatrixUtils.TriangleNormal(
                Bones[(int)BoneIndex.Hips].Transform.position,
                Bones[(int)BoneIndex.LeftHip].Transform.position,
                Bones[(int)BoneIndex.RightHip].Transform.position);

            foreach (var bone in Bones)
            {
                if (bone.Transform == null) continue;

                bone.InitRotation = bone.Transform.rotation;

                if (bone.Child != null && bone.Child.Transform != null)
                {
                    var rotation = MatrixUtils.LookRotation(
                        bone.Transform.position -
                        bone.Child.Transform.position, forward);

                    bone.Inverse = Quaternion.Inverse(rotation);
                    bone.InverseRotation = bone.Inverse * bone.InitRotation;
                }
            }

            var hips = Bones[(int)BoneIndex.Hips];
            hips.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
            hips.InverseRotation = hips.Inverse * hips.InitRotation;

            var spine = Bones[(int)BoneIndex.Spine];

            _initPosition = hips.Transform.position;

            _isInitialized = true;
        }

        private void SetPosition()
        {
            for (int i = 0; i < 32; i++)
            {
                Bones[i].Position = _runner.BonePositions[i];
            }

            Bones[(int)BoneIndex.Hips].Position = (
                Bones[(int)BoneIndex.LeftHip].Position +
                Bones[(int)BoneIndex.RightHip].Position) / 2f;

            Bones[(int)BoneIndex.Hips].Position += _hipOffset;

            Bones[(int)BoneIndex.Spine].Position = (((
                Bones[(int)BoneIndex.LeftShoulder].Position +
                Bones[(int)BoneIndex.RightShoulder].Position) / 2f) +
                Bones[(int)BoneIndex.Hips].Position) / 2f;
        }

        private void UpdateModel()
        {
            var forward = MatrixUtils.TriangleNormal(
                Bones[(int)BoneIndex.Hips].Position,
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

            var hips = Bones[(int)BoneIndex.Hips];

            hips.Transform.localPosition = hips.Position + _initPosition;
            hips.Transform.rotation = MatrixUtils.LookRotation(forward) * hips.InverseRotation;

            var spine = Bones[(int)BoneIndex.Spine];

            var upperChest = (Bones[(int)BoneIndex.LeftShoulder].Position + Bones[(int)BoneIndex.RightShoulder].Position) / 2f;

            spine.Transform.rotation = MatrixUtils.LookRotation(spine.Position - upperChest, forward) * spine.InverseRotation;
        }
    }
}