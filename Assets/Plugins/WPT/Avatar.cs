using UnityEngine;

namespace WPT
{
    public sealed class Avatar : MonoBehaviour
    {
        // Fields

        [SerializeField] Animator _animator;
        [SerializeField] GameObject _nose;
        [SerializeField] bool enableDebug = false;

        Vector3 _initPosition;

        float _centerTall = 224 * 0.75f;
        float _tall = 224 * 0.75f;
        float _prevTall = 224 * 0.75f;
        float _zScale = 1;

        GameObject[] _points;


        // Properties

        public Bone[] Bones { get; private set; }

        public bool IsReady { get; private set; }


        // Methods

        public void SetBones(Vector3[] position)
        {
            if (IsReady == false || position == null) return;

            if (position.Length == Bones.Length)
            {
                for (int i = 0; i < position.Length; i++)
                {
                    Bones[i].Position3D = position[i];
                }
            }
        }


        void Awake()
        {
            if (_animator == null)
            {
                _animator = gameObject.GetComponent<Animator>();
            }

            Bones = new Bone[(int)BoneIndex.Count];

            for (int i = 0; i < (int)BoneIndex.Count; i++)
            {
                Bones[i] = new Bone();
            }

            IsReady = true;
        }

        void Start()
        {
            if (_animator == null || _nose == null) return;

            SetBone();
            SetChildBone();
            SetInverse();
        }

        void Update()
        {
            UpdateModel();

            if (enableDebug)
            {
                _points ??= new GameObject[(int)BoneIndex.Count];

                for (int i = 0; i < Bones.Length; i++)
                {
                    if (_points[i]) continue;

                    var bone = Bones[i];

                    if (bone == null) continue;

                    _points[i] = new GameObject(((BoneIndex)i).ToString());
                }

                for (int i = 0; i < Bones.Length; i++)
                {
                    if (_points[i] == null) continue;

                    var bone = Bones[i];

                    if (bone == null) continue;

                    _points[i].transform.position = bone.Position3D;
                }
            }
        }


        void SetBone()
        {
            // Right Arm
            Bones[(int)BoneIndex.RightShoulderBend].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            Bones[(int)BoneIndex.RightForearmBend].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Bones[(int)BoneIndex.RightHand].Transform = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            Bones[(int)BoneIndex.RightThumb2].Transform = _animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
            Bones[(int)BoneIndex.RightMid1].Transform = _animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

            // Left Arm
            Bones[(int)BoneIndex.LeftShoulderBend].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            Bones[(int)BoneIndex.LeftForearmBend].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Bones[(int)BoneIndex.LeftHand].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Bones[(int)BoneIndex.LeftThumb2].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
            Bones[(int)BoneIndex.LeftMid1].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);

            // Face
            Bones[(int)BoneIndex.LeftEar].Transform = _animator.GetBoneTransform(HumanBodyBones.Head);
            Bones[(int)BoneIndex.LeftEye].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Bones[(int)BoneIndex.RightEar].Transform = _animator.GetBoneTransform(HumanBodyBones.Head);
            Bones[(int)BoneIndex.RightEye].Transform = _animator.GetBoneTransform(HumanBodyBones.RightEye);
            Bones[(int)BoneIndex.Nose].Transform = _nose.transform;

            // Right Leg
            Bones[(int)BoneIndex.RightThighBend].Transform = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            Bones[(int)BoneIndex.RightShin].Transform = _animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Bones[(int)BoneIndex.RightFoot].Transform = _animator.GetBoneTransform(HumanBodyBones.RightFoot);
            Bones[(int)BoneIndex.RightToe].Transform = _animator.GetBoneTransform(HumanBodyBones.RightToes);

            // Left Leg
            Bones[(int)BoneIndex.LeftThighBend].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            Bones[(int)BoneIndex.LeftShin].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Bones[(int)BoneIndex.LeftFoot].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Bones[(int)BoneIndex.LeftToe].Transform = _animator.GetBoneTransform(HumanBodyBones.LeftToes);

            // Etc
            Bones[(int)BoneIndex.AbdomenUpper].Transform = _animator.GetBoneTransform(HumanBodyBones.Spine);
            Bones[(int)BoneIndex.Hip].Transform = _animator.GetBoneTransform(HumanBodyBones.Hips);
            Bones[(int)BoneIndex.Head].Transform = _animator.GetBoneTransform(HumanBodyBones.Head);
            Bones[(int)BoneIndex.Neck].Transform = _animator.GetBoneTransform(HumanBodyBones.Neck);
            Bones[(int)BoneIndex.Spine].Transform = _animator.GetBoneTransform(HumanBodyBones.Spine);
        }

        void SetChildBone()
        {
            // Right Arm
            Bones[(int)BoneIndex.RightShoulderBend].Child = Bones[(int)BoneIndex.RightForearmBend];
            Bones[(int)BoneIndex.RightForearmBend].Child = Bones[(int)BoneIndex.RightHand];
            Bones[(int)BoneIndex.RightForearmBend].Parent = Bones[(int)BoneIndex.RightShoulderBend];

            // Left Arm
            Bones[(int)BoneIndex.LeftShoulderBend].Child = Bones[(int)BoneIndex.LeftForearmBend];
            Bones[(int)BoneIndex.LeftForearmBend].Child = Bones[(int)BoneIndex.LeftHand];
            Bones[(int)BoneIndex.LeftForearmBend].Parent = Bones[(int)BoneIndex.LeftShoulderBend];

            // Right Leg
            Bones[(int)BoneIndex.RightThighBend].Child = Bones[(int)BoneIndex.RightShin];
            Bones[(int)BoneIndex.RightShin].Child = Bones[(int)BoneIndex.RightFoot];
            Bones[(int)BoneIndex.RightFoot].Child = Bones[(int)BoneIndex.RightToe];
            Bones[(int)BoneIndex.RightFoot].Parent = Bones[(int)BoneIndex.RightShin];

            // Left Leg
            Bones[(int)BoneIndex.LeftThighBend].Child = Bones[(int)BoneIndex.LeftShin];
            Bones[(int)BoneIndex.LeftShin].Child = Bones[(int)BoneIndex.LeftFoot];
            Bones[(int)BoneIndex.LeftFoot].Child = Bones[(int)BoneIndex.LeftToe];
            Bones[(int)BoneIndex.LeftFoot].Parent = Bones[(int)BoneIndex.LeftShin];

            // Etc
            Bones[(int)BoneIndex.Spine].Child = Bones[(int)BoneIndex.Neck];
            Bones[(int)BoneIndex.Neck].Child = Bones[(int)BoneIndex.Head];
            Bones[(int)BoneIndex.Head].Child = Bones[(int)BoneIndex.Nose];
        }

        void SetInverse()
        {
            var forward = TriangleNormal(
                Bones[(int)BoneIndex.Hip].Transform.position,
                Bones[(int)BoneIndex.LeftThighBend].Transform.position,
                Bones[(int)BoneIndex.RightThighBend].Transform.position);

            foreach (var bone in Bones)
            {
                if (bone.Transform != null)
                {
                    bone.InitRotation = bone.Transform.rotation;
                }

                if (bone.Child != null)
                {
                    var rotation = Quaternion.LookRotation(bone.Transform.position - bone.Child.Transform.position, forward);

                    bone.Inverse = Quaternion.Inverse(rotation);
                    bone.InverseRotation = bone.Inverse * bone.InitRotation;
                }
            }

            // Hip
            var hip = Bones[(int)BoneIndex.Hip];

            _initPosition = hip.Transform.position;

            hip.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward));
            hip.InverseRotation = hip.Inverse * hip.InitRotation;


            // Head
            var head = Bones[(int)BoneIndex.Head];

            head.InitRotation = head.Transform.rotation;
            head.Inverse = Quaternion.Inverse(Quaternion.LookRotation(Bones[(int)BoneIndex.Nose].Transform.position - head.Transform.position));
            head.InverseRotation = head.Inverse * head.InitRotation;


            // Left Hand
            var leftHand = Bones[(int)BoneIndex.LeftHand];
            var leftMid1 = Bones[(int)BoneIndex.LeftMid1];
            var leftThumb2 = Bones[(int)BoneIndex.LeftThumb2];

            var lf = TriangleNormal(leftHand.Position3D, leftMid1.Position3D, leftThumb2.Position3D);

            leftHand.InitRotation = leftHand.Transform.rotation;
            leftHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(leftThumb2.Transform.position - leftMid1.Transform.position, lf));
            leftHand.InverseRotation = leftHand.Inverse * leftHand.InitRotation;

            // Right Hand
            var rightHand = Bones[(int)BoneIndex.RightHand];
            var rightMid1 = Bones[(int)BoneIndex.RightMid1];
            var rightThumb2 = Bones[(int)BoneIndex.RightThumb2];

            var rf = TriangleNormal(rightHand.Position3D, rightMid1.Position3D, rightThumb2.Position3D);

            rightHand.InitRotation = rightHand.Transform.rotation;
            rightHand.Inverse = Quaternion.Inverse(Quaternion.LookRotation(rightThumb2.Transform.position - rightMid1.Transform.position, rf));
            rightHand.InverseRotation = rightHand.Inverse * rightHand.InitRotation;

            Bones[(int)BoneIndex.Hip].score = 1f;
            Bones[(int)BoneIndex.Neck].score = 1f;
            Bones[(int)BoneIndex.Nose].score = 1f;
            Bones[(int)BoneIndex.Head].score = 1f;
            Bones[(int)BoneIndex.Spine].score = 1f;
        }

        void UpdateModel()
        {
            var t1 = Vector3.Distance(
                Bones[(int)BoneIndex.Head].Position3D,
                Bones[(int)BoneIndex.Neck].Position3D);
            var t2 = Vector3.Distance(
                Bones[(int)BoneIndex.Neck].Position3D,
                Bones[(int)BoneIndex.Spine].Position3D);
            var pm = (
                Bones[(int)BoneIndex.RightThighBend].Position3D +
                Bones[(int)BoneIndex.LeftThighBend].Position3D) / 2f;
            var t3 = Vector3.Distance(Bones[(int)BoneIndex.Spine].Position3D, pm);
            var t4r = Vector3.Distance(
                Bones[(int)BoneIndex.RightThighBend].Position3D,
                Bones[(int)BoneIndex.RightShin].Position3D);
            var t4l = Vector3.Distance(
                Bones[(int)BoneIndex.LeftThighBend].Position3D,
                Bones[(int)BoneIndex.LeftShin].Position3D);
            var t4 = (t4r + t4l) / 2f;
            var t5r = Vector3.Distance(
                Bones[(int)BoneIndex.RightShin].Position3D,
                Bones[(int)BoneIndex.RightFoot].Position3D);
            var t5l = Vector3.Distance(
                Bones[(int)BoneIndex.LeftShin].Position3D,
                Bones[(int)BoneIndex.LeftFoot].Position3D);
            var t5 = (t5r + t5l) / 2f;
            var t = t1 + t2 + t3 + t4 + t5;

            _tall = (t * 0.7f) + (_prevTall * 0.3f);
            _prevTall = _tall;

            if (_tall == 0) _tall = _centerTall;

            var dz = (_centerTall - _tall) / _centerTall * _zScale;

            var forward = TriangleNormal(
                Bones[(int)BoneIndex.Hip].Position3D,
                Bones[(int)BoneIndex.LeftThighBend].Position3D,
                Bones[(int)BoneIndex.RightThighBend].Position3D);

            Bones[(int)BoneIndex.Hip].Transform.position = (Bones[(int)BoneIndex.Hip].Position3D * 0.005f) + new Vector3(_initPosition.x, _initPosition.y, _initPosition.z + dz);
            Bones[(int)BoneIndex.Hip].Transform.rotation = Quaternion.LookRotation(forward) * Bones[(int)BoneIndex.Hip].InverseRotation;

            foreach (var bone in Bones)
            {
                if (bone.Parent != null)
                {
                    bone.Transform.rotation = Quaternion.LookRotation(
                        bone.Position3D - bone.Child.Position3D,
                        bone.Parent.Position3D - bone.Position3D) * bone.InverseRotation;
                }
                else if (bone.Child != null)
                {
                    bone.Transform.rotation = Quaternion.LookRotation(
                        bone.Position3D - bone.Child.Position3D, forward) * bone.InverseRotation;
                }
            }

            // Head Rotation
            var gaze = Bones[(int)BoneIndex.Nose].Position3D - Bones[(int)BoneIndex.Head].Position3D;

            var f = TriangleNormal(
                Bones[(int)BoneIndex.Nose].Position3D,
                Bones[(int)BoneIndex.RightEar].Position3D,
                Bones[(int)BoneIndex.LeftEar].Position3D);

            var head = Bones[(int)BoneIndex.Head];

            head.Transform.rotation = Quaternion.LookRotation(gaze, f) * head.InverseRotation;

            // Wrist Rotation
            var lHand = Bones[(int)BoneIndex.LeftHand];
            var lf = TriangleNormal(lHand.Position3D,
                Bones[(int)BoneIndex.LeftMid1].Position3D,
                Bones[(int)BoneIndex.LeftThumb2].Position3D);
            lHand.Transform.rotation = Quaternion.LookRotation(Bones[(int)BoneIndex.LeftThumb2].Position3D - Bones[(int)BoneIndex.LeftMid1].Position3D, lf) * lHand.InverseRotation;

            var rHand = Bones[(int)BoneIndex.RightHand];
            var rf = TriangleNormal(
                rHand.Position3D,
                Bones[(int)BoneIndex.RightThumb2].Position3D,
                Bones[(int)BoneIndex.RightMid1].Position3D);
            rHand.Transform.rotation = Quaternion.LookRotation(Bones[(int)BoneIndex.RightThumb2].Position3D - Bones[(int)BoneIndex.RightMid1].Position3D, rf) * rHand.InverseRotation;
        }


        Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            var result = Vector3.Cross(a - b, a - c);
            result.Normalize();

            return result;
        }
    }
}