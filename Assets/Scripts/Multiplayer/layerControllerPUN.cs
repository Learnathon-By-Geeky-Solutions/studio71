using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun; // Add this line for Photon PUN 2
using SingletonManagers;
using Player;


namespace PUNPlayer
{
    public class PlayerControllerPUN : MonoBehaviourPunCallbacks
    {
        // Movement related variables
        [Header("Variables for Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        private float _currentMoveSpeed = 1f;
        public Vector3 CurrentVelocity { get; private set; }
        private Vector3 _lastPosition;

        //Look around related variables
        [Header("Variables for Looking Around")]
        [SerializeField] private float _rotationSpeed = 5f;
        private Camera _mainCamera;
        private Plane _groundPlane;

        //Crouch related variables
        private CapsuleCollider _playerCollider;
        private bool _isCrouching;
        [Header("Variables for Crouch"), Range(0.1f,1f)]
        [SerializeField] private float _crouchModifier = 1f;

        //Sprint related variables
        public bool _isSprinting { get; private set; }
        [Header("Variables for Sprint"), Min(1f)]
        [SerializeField] private float _sprintModifier = 1f;

        //Gun variables
        [Header("Gun Variables")]
        private Weapon.Gun _equippedGun;
        private GameObject _equippedGunMagazine;
        private GameObject _playerGun;

        //Grenade variables
        [Header("Grenade Variables")]
        [SerializeField] private GameObject _grenade;
        [SerializeField] private Transform _throwPoint;
        [SerializeField] private int _grenadeCount = 0;
        public int GrenadeCount
        {
            get => _grenadeCount;
            private set => _grenadeCount = value;
        }
        private LineRenderer _lineRenderer;
        [SerializeField] LineRenderer _radiusRenderer;
        [SerializeField] private int _lineResolution = 30;
        [SerializeField] private int _radiusResolution = 30;
        private readonly float _timeStep = 0.05f;
        [SerializeField] private LayerMask _collisionMask;

        //PlayerAnimation Variable
        private PlayerAnimation _playerAnimation;

        // Movement Sounds
        [Header("Movement Sounds")]
        [SerializeField] private string walkSound = "walking";
        [SerializeField] private string crouchSound = "crouching";
        [SerializeField] private string sprintSound = "sprinting";

        private bool isPlayingMovementSound = false;
        private string currentMovementSound = "";

        private void OnEnable()
        {
            InputHandler.Instance.OnCrouch += Crouch;
            InputHandler.Instance.OnSprint += Sprint;
            InputHandler.Instance.OnPrimaryWeapon += PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon += SecondaryWeapon;
            InputHandler.Instance.OnAttack += Attack;
            InputHandler.Instance.OnReload += Reload;
            InputHandler.Instance.OnGrenade += Grenade;
        }

        private void OnDisable()
        {
            AudioManager.Instance.StopSound(currentMovementSound);
            InputHandler.Instance.OnCrouch -= Crouch;
            InputHandler.Instance.OnSprint -= Sprint;
            InputHandler.Instance.OnPrimaryWeapon -= PrimaryWeapon;
            InputHandler.Instance.OnSecondaryWeapon -= SecondaryWeapon;
            InputHandler.Instance.OnAttack -= Attack;
            InputHandler.Instance.OnReload -= Reload;
            InputHandler.Instance.OnGrenade -= Grenade;
        }

        private void Awake()
        {
            // Movement initialization
            _currentMoveSpeed = _moveSpeed;
            _lastPosition = transform.position;

            // Camera initialization
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                print("Main Camera is Missing");
                enabled = false;
                return;
            }

            _groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

            // Crouch initialization
            _isCrouching = false;
            _playerCollider = GetComponent<CapsuleCollider>();

            // Sprint initialization
            _isSprinting = false;

            // Gun initialization
            _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            _equippedGunMagazine = _equippedGun.transform.Find("Mag").gameObject;
            _playerGun = _equippedGun.gameObject;

            // Grenade initialization
            _lineRenderer = GetComponentInChildren<LineRenderer>();
            _lineRenderer.enabled = false;

            // Animation initialization
            _playerAnimation = GetComponent<PlayerAnimation>();
        }

        private void Start()
        {
            if (_rotationSpeed <= 0f)
            {
                print("Invalid Rotation Speed. Defaulting to 5");
                _rotationSpeed = 5f;
            }

            if (_playerCollider == null)
            {
                print($"Player named {gameObject.name} is Missing Collider");
            }

            if (_crouchModifier > 1)
            {
                print("Crouch Speed is set higher than standing speed. Setting Crouch Speed as standing speed");
                _crouchModifier = 1;
            }
        }

        private void Update()
        {
            if (!photonView.IsMine) return; // Ensure only the local player can control their actions

            MovePlayer();
            LookAround();
            DrawTrajectory();
            UpdateMovementSounds();
            CurrentVelocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;
        }

        private void MovePlayer()
        {
            transform.position += _currentMoveSpeed * Time.deltaTime * new Vector3(InputHandler.Instance.MoveDirection.x, 0, InputHandler.Instance.MoveDirection.y);
        }

        private void LookAround()
        {
            if (_playerAnimation.IsThrowingGrenade) return;

            Ray ray = _mainCamera.ScreenPointToRay(InputHandler.Instance.MousePosition);

            if (_groundPlane.Raycast(ray, out float Distance))
            {
                Vector3 mouseWorldPosition = ray.GetPoint(Distance);
                Vector3 direction = mouseWorldPosition - transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), _rotationSpeed * Time.deltaTime);
                }
            }
        }

        private void Crouch()
        {
            if (!_isCrouching)
            {
                _playerCollider.height /= 2;
                _playerCollider.center = new Vector3(_playerCollider.center.x, .5f, _playerCollider.center.z);
                _isCrouching = true;
                _currentMoveSpeed = _moveSpeed * _crouchModifier;
                _isSprinting = false;
            }
            else
            {
                _playerCollider.height *= 2;
                _playerCollider.center = new Vector3(_playerCollider.center.x, 0.93f, _playerCollider.center.z);
                _isCrouching = false;

                if (Keyboard.current.shiftKey.isPressed)
                {
                    _currentMoveSpeed = _moveSpeed * _sprintModifier;
                    _isSprinting = true;
                }
                else
                {
                    _currentMoveSpeed /= _crouchModifier;
                }
            }
        }

        private void Sprint(bool isPressed)
        {
            if (isPressed && !_isCrouching)
            {
                _currentMoveSpeed = _moveSpeed * _sprintModifier;
                _isSprinting = true;
                _equippedGun.StopShooting();
            }
            else if (!_isCrouching)
            {
                _currentMoveSpeed /= _sprintModifier;
                _isSprinting = false;
            }
        }

        private void Attack(bool isPressed)
        {
            if (_isSprinting || _playerAnimation.IsBusy)
            {
                return;
            }

            if (isPressed)
            {
                _equippedGun.StartShooting();
            }
            else
            {
                _equippedGun.StopShooting();
            }
        }

        private void PrimaryWeapon()
        {
            if (!_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.AutomaticGun>();
            }
        }

        private void SecondaryWeapon()
        {
            if (!_equippedGun.IsShooting)
            {
                _equippedGun = gameObject.GetComponentInChildren<Weapon.SemiAutomaticGun>();
            }
        }

        private void Reload()
        {
            AudioManager.PlaySound(SoundKeys.ReloadStart, transform.position);
            _equippedGun.StopShooting();
            StartCoroutine(DelayedAction(1f, () => { _equippedGunMagazine.SetActive(false); }));

            StartCoroutine(DelayedAction(2f, () =>
            { 
                _equippedGun.CurrentMagazineSize = _equippedGun.Magazine_Size; 
                _equippedGunMagazine.SetActive(true); 
            }));
        }

        private void Grenade()
        {
            if (_playerAnimation.IsThrowingGrenade || GrenadeCount <= 0) return;
            _playerAnimation.IsThrowingGrenade = true;
            AudioManager.PlaySound(SoundKeys.GrenadeThrow, transform.position);
            _equippedGun.StopShooting();
            _playerGun.SetActive(false);

            // Use PhotonRPC to spawn the grenade on all clients
            photonView.RPC("ThrowGrenade", RpcTarget.AllBuffered, _throwPoint.position);
        }

        [PunRPC]
        private void ThrowGrenade(Vector3 position)
        {
            Instantiate(_grenade, position, _grenade.transform.rotation);
            GrenadeCount -= 1;
            _playerGun.SetActive(true);
        }

        private void DrawTrajectory()
        {
            if (!InputHandler.Instance.GrenadeThrowStart)
            {
                _lineRenderer.enabled = false;
                _radiusRenderer.enabled = false;
                return;
            }

            if (GrenadeCount <= 0) return;

            _lineRenderer.enabled = true;
            List<Vector3> points = new List<Vector3>();
            Vector3 startPosition = _throwPoint.position;
            Vector3 startVelocity = (transform.forward * 5) + (Vector3.up * 6);

            points.Add(startPosition);

            for (float t = 0; t < _lineResolution * _timeStep; t += _timeStep)
            {
                Vector3 point = startPosition + startVelocity * t + 0.5f * Physics.gravity * t * t;
                points.Add(point);

                if (points.Count > 1)
                {
                    Vector3 lastPoint = points[^2];
                    Vector3 direction = (point - lastPoint).normalized;
                    float distance = (point - lastPoint).magnitude;

                    if (Physics.Raycast(lastPoint, direction, out RaycastHit hit, distance, _collisionMask))
                    {
                        points.Add(hit.point);
                        break;
                    }
                }
            }

            _lineRenderer.positionCount = points.Count;
            _lineRenderer.SetPositions(points.ToArray());

            // Draw explosion radius at the last point
            DrawExplosionRadius(points[^1]); // Last point
        }

        private void DrawExplosionRadius(Vector3 center)
        {
            _radiusRenderer.enabled = true;
            _radiusRenderer.positionCount = _radiusResolution + 1;

            float angleStep = 360f / _radiusResolution;
            for (int i = 0; i <= _radiusResolution; i++)
            {
                float angle = Mathf.Deg2Rad * angleStep * i;
                float x = Mathf.Cos(angle) * 4f;
                float z = Mathf.Sin(angle) * 4f;
                Vector3 point = center + new Vector3(x, 0.05f, z);
                _radiusRenderer.SetPosition(i, point);
            }
        }

        private void UpdateMovementSounds()
        {
            if (InputHandler.Instance.MoveDirection.magnitude > 0.1f)
            {
                string soundToPlay;

                if (_isCrouching)
                    soundToPlay = crouchSound;
                else if (_isSprinting)
                    soundToPlay = sprintSound;
                else
                    soundToPlay = walkSound;

                if (!isPlayingMovementSound || currentMovementSound != soundToPlay)
                {
                    if (isPlayingMovementSound)
                        AudioManager.Instance.StopSound(currentMovementSound);

                    AudioManager.PlaySound(soundToPlay, transform.position);
                    isPlayingMovementSound = true;
                    currentMovementSound = soundToPlay;
                }
            }
            else if (isPlayingMovementSound)
            {
                AudioManager.Instance.StopSound(currentMovementSound);
                isPlayingMovementSound = false;
                currentMovementSound = "";
            }
        }

        private static IEnumerator DelayedAction(float delay, System.Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }
    }
}
