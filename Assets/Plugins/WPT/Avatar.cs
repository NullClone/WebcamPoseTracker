using UnityEngine;
using WPT.Utilities;

namespace WPT
{
    [RequireComponent(typeof(Animator))]
    public sealed class Avatar : MonoBehaviour
    {
        // Fields

        [SerializeField] private PoseDetection _poseDetection;

        private Animator _animator;
        private GameObject _baseObject;
        private Vector3 _basePosition;
        private bool _isInitialized;

        private readonly Bone[] Bones = new Bone[(int)BoneIndex.Count];


        // Methods

        private void Start()
        {
            if (_poseDetection == null) return;

            _animator = gameObject.GetComponent<Animator>();

            if (_animator == null) return;

            _baseObject = _animator.gameObject;

            if (_baseObject == null) return;

            _basePosition = _baseObject.transform.position;

            for (int i = 0; i < Bones.Length; i++)
            {
                Bones[i] = new();
            }

            GetBones();
            SetBones();
            SetInverse();

            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized) return;

            SetPosition();

            UpdateModel();
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

            Bones[(int)BoneIndex.Nose].Transform = _animator.GetBoneTransform(HumanBodyBones.Neck);
        }

        private void SetBones()
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
            hips.Inverse = Quaternion.Inverse(MatrixUtils.LookRotation(forward, Vector3.up));
            hips.InverseRotation = hips.Inverse * hips.InitRotation;

            var spine = Bones[(int)BoneIndex.Spine];
            spine.Inverse = Quaternion.Inverse(MatrixUtils.LookRotation(forward, Vector3.up));
            spine.InverseRotation = spine.Inverse * spine.InitRotation;

            var head = Bones[(int)BoneIndex.Nose];
            head.Inverse = Quaternion.Inverse(MatrixUtils.LookRotation(forward, Vector3.up));
            head.InverseRotation = head.Inverse * head.InitRotation;
        }

        private void SetPosition()
        {
            for (int i = 0; i < 32; i++)
            {
                Bones[i].Position = _poseDetection.BonePositions[i];
            }

            Bones[(int)BoneIndex.Hips].Position = (
                Bones[(int)BoneIndex.LeftHip].Position +
                Bones[(int)BoneIndex.RightHip].Position) / 2f;

            Bones[(int)BoneIndex.Hips].Position.y += 0.01f;

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
                if (bone.Transform == null || bone.Child == null) continue;

                if (bone.Position - bone.Child.Position == Vector3.zero) continue;

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

            hips.Transform.rotation = MatrixUtils.LookRotation(forward, Vector3.up) * hips.InverseRotation;

            var spine = Bones[(int)BoneIndex.Spine];

            var targetSpineUp = ((
                Bones[(int)BoneIndex.LeftShoulder].Position +
                Bones[(int)BoneIndex.RightShoulder].Position) / 2f) - hips.Position;

            var shoulderWidthVector = Bones[(int)BoneIndex.RightShoulder].Position - Bones[(int)BoneIndex.LeftShoulder].Position;

            var spineForwardDirection = Vector3.Cross(shoulderWidthVector, targetSpineUp);

            spine.Transform.rotation = MatrixUtils.LookRotation(spineForwardDirection, targetSpineUp) * spine.InverseRotation;

            var head = Bones[(int)BoneIndex.Nose];

            var eyeMidPoint = (Bones[(int)BoneIndex.LeftEye].Position + Bones[(int)BoneIndex.RightEye].Position) / 2f;
            var earMidPoint = (Bones[(int)BoneIndex.LeftEar].Position + Bones[(int)BoneIndex.RightEar].Position) / 2f;

            head.Transform.rotation = MatrixUtils.LookRotation(eyeMidPoint - earMidPoint, Vector3.up) * head.InverseRotation;

            gameObject.transform.position = hips.Position + _basePosition;
        }
    }
}