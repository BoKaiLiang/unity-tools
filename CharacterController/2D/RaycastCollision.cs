/*
 * -- Reference
 *
 * [SebLague/2DPlatformer-Tutorial] - (https://github.com/SebLague/2DPlatformer-Tutorial)
 *
 */

using UnityEngine;

namespace PKTools.Gameplay
{
    public class RaycastCollision
    {
        private struct RaycastOrigins
        {
            public Vector2 TopLeft, TopRight;
            public Vector2 BottomLeft, BottomRight;
        }

        public struct CollisionInfo
        {
            public bool above, below;
            public bool left, right;

            public int faceDir;

            public void Reset()
            {
                above = below = false;
                left = right = false;
            }
        }

        public int HorizontalRayCount { get; private set; }

        public int VerticalRayCount { get; private set; }

        private Collider2D collider2D;

        private LayerMask collisionMask;

        private RaycastOrigins raycastOrigins;

        public CollisionInfo collisionInfo;

        private float horizontalRaySpacing;

        private float verticalRaySpacing;

        private const float SKIN_WIDTH = 0.015f;

        public RaycastCollision(Collider2D collider2D, LayerMask collisionMask, int horizontalRayCount = 6, int verticalRayCount = 4)
        {
            this.collider2D = collider2D;
            this.collisionMask = collisionMask;
            
            Bounds bounds = collider2D.bounds;
            bounds.Expand(SKIN_WIDTH * -2);

            HorizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            VerticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

            Debug.Log("Vertical Ray Count: " + this.VerticalRayCount + ", Horizontal Ray Count: " + this.HorizontalRayCount);
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = collider2D.bounds;
            bounds.Expand(SKIN_WIDTH * -2);

            raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
        }

        public void HorizontalCollisions(ref Vector3 velocity)
        {
            float directionX = collisionInfo.faceDir;
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

            if (Mathf.Abs(velocity.x) < SKIN_WIDTH)
            {
                rayLength = 2 * SKIN_WIDTH;
            }

            for (int i = 0; i < HorizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.BottomLeft : raycastOrigins.BottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {

                    if (hit.distance == 0)
                    {
                        continue;
                    }

                    velocity.x = (hit.distance - SKIN_WIDTH) * directionX;
                    rayLength = hit.distance;

                    collisionInfo.left = directionX == -1;
                    collisionInfo.right = directionX == 1;
                }
            }
        }

        public void VerticalCollisions(ref Vector3 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

            for (int i = 0; i < VerticalRayCount; i++)
            {

                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.BottomLeft : raycastOrigins.TopLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (hit)
                {
                    velocity.y = (hit.distance - SKIN_WIDTH) * directionY;
                    rayLength = hit.distance;

                    collisionInfo.below = directionY == -1;
                    collisionInfo.above = directionY == 1;
                }
            }
        }
    }
}