using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    public sealed class Avatar : MonoBehaviour
    {
        // Fields

        [SerializeField] private InferenceRunner _runner;
        [SerializeField] private Vector3 _movementSenstivity = new Vector3(0.01f, 0.01f, 0.01f);
        [SerializeField] private Vector3 _hipOffset;
        [SerializeField] private Vector3 _neckOffset;

        private Animator _animator;
        private GameObject _baseObject;
        private Vector3 _initPosition;
        private bool _isInitialized;
        private Bone[] Bones;


        // Methods

        private void Start()
        {
            if (_runner == null) return;

            _animator = gameObject.GetComponent<Animator>();

            if (_animator == null) return;

            _baseObject = _animator.gameObject;

            if (_baseObject == null) return;

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

            GetBones();

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

        private void GetBones()
        {
            // Body

            Bones[(int)BoneIndex.Hips].Transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            Bones[(int)BoneIndex.Spine].Transform = _animator.GetBoneTransform(HumanBodyBones.Spine);


            // Left Arm

            Bones[(int)BoneIndex.LeftShoulder].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Bones[(int)BoneIndex.LeftElbow].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Bones[(int)BoneIndex.LeftWrist].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Bones[(int)BoneIndex.LeftThumb].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
            Bones[(int)BoneIndex.LeftIndex].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
            Bones[(int)BoneIndex.LeftPinky].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal);


            // Right Arm

            Bones[(int)BoneIndex.RightShoulder].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Bones[(int)BoneIndex.RightElbow].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Bones[(int)BoneIndex.RightWrist].Transform = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            Bones[(int)BoneIndex.RightThumb].Transform = _animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
            Bones[(int)BoneIndex.RightIndex].Transform = _animator.GetBoneTransform(HumanBodyBones.RightIndexProximal);
            Bones[(int)BoneIndex.RightPinky].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLittleProximal);


            // Left Leg

            Bones[(int)BoneIndex.LeftHip].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Bones[(int)BoneIndex.LeftKnee].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Bones[(int)BoneIndex.LeftAnkle].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Bones[(int)BoneIndex.LeftFootIndex].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftToes);


            // Right Leg

            Bones[(int)BoneIndex.RightHip].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Bones[(int)BoneIndex.RightKnee].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Bones[(int)BoneIndex.RightAnkle].Transform = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Bones[(int)BoneIndex.RightFootIndex].Transform = _animator.GetBoneTransform(HumanBodyBones.RightToes);


            // Head

            Bones[(int)BoneIndex.Nose].Transform = _animator.GetBoneTransform(HumanBodyBones.Head);
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
            //hips.Transform.rotation = MatrixUtils.LookRotation(forward) * hips.InverseRotation;

            var spine = Bones[(int)BoneIndex.Spine];

            var upperChest = (Bones[(int)BoneIndex.LeftShoulder].Position + Bones[(int)BoneIndex.RightShoulder].Position) / 2f;

            spine.Transform.rotation = MatrixUtils.LookRotation(spine.Position - upperChest, forward) * spine.InverseRotation;

            hips.Transform.rotation = Quaternion.FromToRotation(Vector3.forward, forward) * hips.InverseRotation;

            var currentSpineDir = (upperChest - Bones[(int)BoneIndex.Spine].Position).normalized;

            //Quaternion spineWorldRot = Quaternion.FromToRotation(spine.Transform.up, currentSpineDir);

            //spine.Transform.localRotation = Quaternion.Inverse(spine.Transform.parent.rotation) * spineWorldRot * spine.InitRotation;
        }
    }
}