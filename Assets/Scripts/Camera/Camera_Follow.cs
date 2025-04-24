using SingletonManagers;
using System.Collections.Generic;
using UnityEngine;

namespace CameraManager
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);
        [SerializeField] private Vector3 _rotation = new Vector3(30f, 0f, 0f);
        [SerializeField] private float followSpeed = 5f;
        [SerializeField] private float movementOffsetMultiplier = 2f;

        [Header("Tree Transparency Settings")]
        [SerializeField] private float _fadeDistance = 3f;
        [SerializeField] private LayerMask _treeLayer;
        [SerializeField] private Material _transparentLeavesMaterial;

        private Transform _player;
        private Vector3 _targetOffset;

        private readonly Dictionary<Renderer, Material> _originalLeafMaterials = new();
        private readonly HashSet<Renderer> _currentlyFaded = new();

        private void Awake()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            _targetOffset = _offset;
        }

        private void LateUpdate()
        {
            if (_player == null) return;

            HandleCameraMovement();
            HandleTreeTransparency();
        }

        private void HandleCameraMovement()
        {
            var moveDirection =
                dialogue.InkDialogueManager.IsDialogueOpen || _player.gameObject.GetComponent<Player.PlayerAnimation>().IsDead
                ? Vector2.zero
                : InputHandler.Instance.MoveDirection;

            var movementOffset = new Vector3(moveDirection.x, 0, moveDirection.y) * movementOffsetMultiplier;
            _targetOffset = Vector3.Lerp(_targetOffset, _offset + movementOffset, Time.deltaTime * followSpeed);
            transform.position = _player.position + _targetOffset;
            transform.rotation = Quaternion.Euler(_rotation);
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S4158:Remove this call, the collection is known to be empty here.", Justification = "Field will be populated on second iteration")]
        private void HandleTreeTransparency()
        {
            var colliders = Physics.OverlapSphere(_player.position, _fadeDistance, _treeLayer);
            var newFaded = new HashSet<Renderer>();

            foreach (var col in colliders)
            {
                var rend = col.GetComponent<Renderer>();
                if (!rend || rend.sharedMaterials.Length < 2) continue;

                newFaded.Add(rend);

                if (!_originalLeafMaterials.ContainsKey(rend))
                {
                    _originalLeafMaterials[rend] = rend.sharedMaterials[1];
                }

                var materials = rend.materials;
                materials[1] = _transparentLeavesMaterial;
                rend.materials = materials;
            }

            foreach (var rend in _currentlyFaded)
            {
                if (newFaded.Contains(rend)) continue;
                if (!_originalLeafMaterials.ContainsKey(rend)) continue;

                var materials = rend.materials;
                materials[1] = _originalLeafMaterials[rend];
                rend.materials = materials;
            }

            _currentlyFaded.Clear();
            foreach (var r in newFaded) _currentlyFaded.Add(r);
           
        }
    }
}
