using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The state we're in when we're climbing a ladder or a rope.
/// </summary>
public class ClimbingState : State {

    public ContactFilter2D ladderFilter;
    public LayerMask ladderLayerMask;
    [HideInInspector] public List<Collider2D> ladderColliders;

    private void Start() {
        ladderColliders = new List<Collider2D>();
    }

    public override bool CanEnter() {
        if (player.directionalInput.y == 0) {
            return false;
        }
        // TODO: This maybe makes it too hard to grab a ladder playing on a keyboard?
        // if (Mathf.Abs(player.directionalInput.y) < Mathf.Abs(player.directionalInput.x)) {
        //     return false;
        // }
        if (player.recentlyJumped) {
            return false;
        }
        // Find any nearby ladder colliders.
        player.PhysicsObject.collider.OverlapCollider(ladderFilter, ladderColliders);
        if (ladderColliders.Count <= 0) {
            return false;
        }

        Vector2 direction = Vector2.up;
        Vector3 position = transform.position + Vector3.up * 16;
        RaycastHit2D hit = Physics2D.Raycast(position, direction, 9, ladderLayerMask);
        Debug.DrawRay(position, direction * 9, Color.magenta);
        if (hit.collider == null) {
            return false;
        }

        return true;
    }

    public override void Enter() {
        base.Enter();

        player.PhysicsObject.collisions.fallingThroughPlatform = true;
        float xPos = ladderColliders[0].transform.position.x;
        player.graphics.animator.Play("ClimbRope");
        if (ladderColliders[0].CompareTag("Ladder")) {
            xPos += LevelGenerator.instance.TileWidth / 2f;
            player.graphics.animator.Play("ClimbLadder");
        }

        transform.position = new Vector3(xPos, transform.position.y, 0);
        player.audio.Play(player.audio.grabClip);
    }

    private void Update() {
        if (player.directionalInput.y != 0) {
            // Set the framerate of the climbing animation dynamically based on our climbing speed.
            player.graphics.animator.fps = Mathf.RoundToInt(Mathf.Abs(player.directionalInput.y).Remap(0.1f, 1.0f, 4, 18));
        }
        else {
            player.graphics.animator.fps = 0;
        }

        if (player.directionalInput.y < 0 && player.PhysicsObject.collisions.below && !player.PhysicsObject.collisions.colliderBelow.CompareTag("OneWayPlatform")) {
            player.stateMachine.AttemptToChangeState(player.groundedState);
        }

        // Find any nearby ladder colliders.
        player.PhysicsObject.collider.OverlapCollider(ladderFilter, ladderColliders);
        if (ladderColliders.Count <= 0) {
            player.stateMachine.AttemptToChangeState(player.inAirState);
        }
    }

    public override void ChangePlayerVelocity(ref Vector2 velocity) {
        velocity.y = player.directionalInput.y * player.climbSpeed;
        velocity.x = 0;

        // Raycast ahead of us and set our velocity to 0 if we are no longer on
        // a ladder.
        Vector2 direction = Vector2.down;
        Vector3 position = transform.position + Vector3.up * 16;
        if (player.directionalInput.y > 0) {
            direction = Vector2.up;
        }
        RaycastHit2D hit = Physics2D.Raycast(position, direction, 9, ladderLayerMask);
        Debug.DrawRay(position, direction * 9, Color.magenta);
        if (hit.collider == null) {
            velocity.y = 0;
        }

    }

}
