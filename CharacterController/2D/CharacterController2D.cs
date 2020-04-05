/*
 * -- Reference
 *
 * [SebLague/2DPlatformer-Tutorial] - (https://github.com/SebLague/2DPlatformer-Tutorial)
 *
 */

using UnityEngine;

namespace PKTools.Gameplay
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class CharacterController2D : MonoBehaviour
    {   

        public LayerMask collisionMask;
        public float moveSpeed = 6;
        public float maxJumpHeight = 4;
        public float minJumpHeight = 1;
        public float timeToJumpApex = .4f;
        float accelerationTimeAirborne = .2f;
        float accelerationTimeGrounded = .1f;

        float gravity = -20;
        Vector3 velocity;
        float maxJumpVelocity;
        float minJumpVelocity;
        float velocityXSmoothing;

        private RaycastCollision raycastCollision;
        private Collider2D characterCollider;

        private void Start()
        {   
            characterCollider = GetComponent<Collider2D>();

            raycastCollision = new RaycastCollision(characterCollider, collisionMask);

            gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        }

        private void Update()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            float targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (raycastCollision.collisionInfo.below) ? accelerationTimeGrounded : accelerationTimeAirborne);


            if (raycastCollision.collisionInfo.above || raycastCollision.collisionInfo.below)
            {
                velocity.y = 0;
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {

                if (raycastCollision.collisionInfo.below)
                {
                    velocity.y = maxJumpVelocity;
                }
            }
    
            if (Input.GetKeyUp(KeyCode.Space))
            {
                if (velocity.y > minJumpVelocity)
                {
                    velocity.y = minJumpVelocity;
                }
            }

            velocity.y += gravity * Time.fixedDeltaTime;
            
            Move(velocity * Time.fixedDeltaTime);
        }

        void Move(Vector3 velocity)
        {
            raycastCollision.UpdateRaycastOrigins();
            raycastCollision.collisionInfo.Reset();

            if (velocity.x != 0)
            {
                raycastCollision.collisionInfo.faceDir = (int)Mathf.Sign(velocity.x);
            }

            raycastCollision.HorizontalCollisions(ref velocity);
            if (velocity.y != 0)
            {
                raycastCollision.VerticalCollisions(ref velocity);
            }

            transform.Translate(velocity);
        }
    }
}
