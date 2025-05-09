using UnityEngine;

namespace FC_CutsceneSystem
{

    public class SimplePlayerController : MonoBehaviour
    {
        [SerializeField] float walkSpeed = 3;
        [SerializeField] float rotationSpeed = 500f;

        bool isGrounded;
        bool inCutscene;

        float ySpeed;
        Quaternion targetRotation;

        CameraController cameraController;
        Animator animator;
        CharacterController characterController;
        private void Awake()
        {
            cameraController = Camera.main.GetComponent<CameraController>();
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            CutsceneManager.instance.OnCutsceneStart += () =>
            {
                inCutscene = true;
                animator.SetBool("Walk", false);
            };
            CutsceneManager.instance.OnCutsceneEnd += () =>
            {
                targetRotation = transform.rotation; // Rest rotation
                inCutscene = false;
            };
        }

        private void Update()
        {
            if (inCutscene) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            float moveAmount = Mathf.Clamp01(Mathf.Abs(h) + Mathf.Abs(v));

            var moveInput = (new Vector3(h, 0, v)).normalized;

            var moveDir = cameraController.PlanarRotation * moveInput;

            var velocity = moveDir * walkSpeed;

            characterController.Move(velocity * Time.deltaTime);

            if (moveAmount > 0)
            {
                targetRotation = Quaternion.LookRotation(moveDir);
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);

            animator.SetBool("Walk", moveAmount > 0.2f);
        }
    }

}
