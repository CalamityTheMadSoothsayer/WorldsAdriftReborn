using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WorldsAdriftRebornGameServer.Game.Entities
{
    internal class PlayerEntities
    {
        public long EntityId { get; set; }
        public string? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public Vector2 Camera { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public string? TeleportCommand { get; set; }
        public Vector3 Direction { get; set; }
        public bool DoingJump { get; set; }
        public bool DoWalk { get; set; }
        public bool IsAlive { get; set; }
        public bool IsAttemptingCrouch { get; set; }
        public bool IsAttemptingGrab { get; set; }
        public bool IsAttemptingGrabNearWall { get; set; }
        public bool IsClimbing { get; set; }
        public bool IsClimbingAndMoving { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsEmoting { get; set; }
        public bool IsInRespawner { get; set; }
        public bool IsKnockedOut { get; set; }
        public bool IsNearWall { get; set; }
        public bool IsRapelling { get; set; }
        public bool IsSprinting { get; set; }
        public bool LastGrounded { get; set; }
        //public PlayerMultitool Multitool { get; set; }
        public Vector3 PreviousFrameVelocity { get; set; }
        //public PlayerScannerTool ScannerTool { get; set; }
        public float Speed { get; set; }
        public float SprintLevel { get; set; }
        public bool SlopeTooSteep { get; set; }
    }
}
