using AnythingWorld.Animation;
using UnityEngine;

namespace AnythingWorld.Behaviour
{
    public enum MovementType
    {
        SpawnAroundPoint,
        ManualGoal,
        SpawnAroundModel
    }
    public class RandomMovement : MonoBehaviour
    {
        [Tooltip("Readonly, position of goal that object is moving towards. Can be regenerated.")]
        public Vector3 goalPosition = Vector3.zero;
        [Tooltip("When model enters this radius they have entered the goal.")]
        public float goalRadius = 1;
        public float speedScalar = 1f;

        public float ScaledMaxSpeed => maxSpeed * speedScalar;
        public float ScaledTurnSpeed => turnSpeed * speedScalar;
        //input
        [Header("Speed")]
        public bool scaleMovementToDatabaseSpeed = true;
        public float maxSpeed = 3;
        public float turnSpeed = 2;
        [Header("Thresholds")]
        [Tooltip("Speed above which walk animation is called.")]
        public float walkThreshold = 0.1f;
        [Tooltip("Speed % of max above which run animation is called.")]
        public float runThreshold = 0.7f;
        [Tooltip("Limit movement speed to the maximum threshold active.")]
        public bool clampToActivatedThresholds = false;
        [Header("Animation States Active")]
        public bool walk = true;
        public bool run = true;
        
        [Header("Braking")]
        public float brakeStrength = 1;

        public float brakeDist = 2;
        public bool brakeAtDestination = true;
        [Tooltip("Stop translational movement and rotation while continuing animation.")]
        public bool pauseMovement = false;
        public float stopThreshold = 0.1f;

        [Header("Goal Spawning")]
        public MovementType goalGenerationType = MovementType.SpawnAroundPoint;

        [Header("Set Manual Goal")]
        public Transform manualGoalTransform;


        [Header("Goal Randomization")]
        [Tooltip("If false, spawn radius will around the model position.")]
        public bool spawnAroundModel = false;




        public Vector3 spawnAnchor = Vector3.zero;
        public float positionSpawnRadius = 10;
        public bool generateNewPoints = true;

        [Header("Gizmo Settings")]
        public bool showGizmos = true;
        public Color handleColor = Color.white;
        public Color labelColor = Color.white;
        public float gizmoScale = 1f;

        [HideInInspector]
        public RunWalkIdleController animationController;

        //Debug
        private Vector3 directionToGoal;
        private float variableSpeed;
        private float distanceToGoal;


        public void Start()
        {

            if(TryGetComponent<Rigidbody>(out var rb))
            {
                rb.freezeRotation = true;              
            }
            if (scaleMovementToDatabaseSpeed && TryGetComponent<AnythingWorld.Utilities.ModelDataInspector>(out var inspector))
            {

                if (inspector.movement != null && inspector.movement.Count > 0)
                {
                    var averageScale = 0f;
                    foreach (var measurement in inspector.movement)
                    {
                        averageScale += measurement.value;
                    }
                    averageScale /= inspector.scales.Count;
                    speedScalar = averageScale / 50;
                }

                


            }
            if (GetComponentInChildren<LegacyAnimationController>())
            {
                animationController = GetComponentInChildren<RunWalkIdleController>();
                animationController.crossfadeTime = 0.5f;
            }

        }

        public void OnValidate()
        {
            UpdateSpawnAnchor();
        }

        
        public void Update()
        {
            variableSpeed = Mathf.Lerp(ScaledMaxSpeed * brakeStrength, variableSpeed, Time.deltaTime);
            if (variableSpeed < stopThreshold) variableSpeed = 0;
            distanceToGoal = Vector3.Distance(goalPosition, transform.position);

            UpdateSpawnAnchor();

            switch (goalGenerationType)
            {
                case MovementType.ManualGoal:
                    //If manual goal
                    if (manualGoalTransform == null)
                    {
                        Debug.LogWarning("Goal transform not set, default to spawning around point.", gameObject);
                        goalGenerationType = MovementType.SpawnAroundPoint;
                        return;
                    }
                    break;
                case MovementType.SpawnAroundPoint:
                    //Spawn around centroid
                    if (distanceToGoal <= goalRadius && generateNewPoints)
                    {
                        goalPosition = GetRandomPositionInsideSphere(spawnAnchor);
                    }
                    break;
                case MovementType.SpawnAroundModel:
                    //If not spawning around point, spawn around model.
                    if (distanceToGoal <= goalRadius && generateNewPoints)
                    {
                        goalPosition = GetRandomPositionInsideSphere(transform.position);
                    }
                    break;
            }

            //Brake when close to target
            if (brakeAtDestination) { brakeStrength = Mathf.Clamp(distanceToGoal - brakeStrength, 0, 1); } else { brakeStrength = 1; };
            //Calculate vector to goal
            directionToGoal = new Vector3(goalPosition.x, transform.position.y, goalPosition.z) - transform.position;

            if (walk && run)
            {
                //Blend animation
                animationController?.BlendAnimationOnSpeed(variableSpeed, walkThreshold * ScaledMaxSpeed, runThreshold*ScaledMaxSpeed);
            }
            else
            {
                variableSpeed = Mathf.Min(variableSpeed, runThreshold*ScaledMaxSpeed);
                animationController?.BlendAnimationOnSpeed(variableSpeed, walkThreshold * ScaledMaxSpeed);
            }
            if (pauseMovement) return;
            TurnTowardsTarget(directionToGoal);
            MoveTowardsTarget();

        }
        public void UpdateSpawnAnchor()
        {
            switch (goalGenerationType)
            {
                case MovementType.ManualGoal:
                    if (manualGoalTransform == null)
                    {
                        Debug.LogWarning("Goal transform not set, default to spawning around point.", gameObject);
                        goalGenerationType = MovementType.SpawnAroundPoint;
                        return;
                    }
                    spawnAnchor = manualGoalTransform.position;
                    break;
                case MovementType.SpawnAroundPoint:
                    break;
                case MovementType.SpawnAroundModel:
                    spawnAnchor = transform.position;
                    break;
            }
        }
        public void MoveTowardsTarget()
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + (transform.forward), variableSpeed * Time.deltaTime);

        }

        public void TurnTowardsTarget(Vector3 directionToTarget)
        {
            // Turn towards the target


            var normalizedLookDirection = directionToTarget.normalized;
            var m_LookRotation = Quaternion.LookRotation(normalizedLookDirection);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_LookRotation, Time.deltaTime * ScaledTurnSpeed);
        }
        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!showGizmos) return;
            try
            {
                GUI.color = Color.white;
                Gizmos.color = Color.white;
                var midpoint = (transform.position + goalPosition) * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, directionToGoal);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, transform.forward * distanceToGoal / 2);


                var angle = Vector3.SignedAngle(transform.forward, directionToGoal, Vector3.up);
                var re = Vector3.Cross(transform.forward, directionToGoal);
                UnityEditor.Handles.Label(Vector3.Lerp(transform.position, goalPosition, 0.5f), angle.ToString("F2") + "°");
                UnityEditor.Handles.DrawWireArc(transform.position, transform.up, transform.forward, angle, distanceToGoal * 0.5f);
                GUI.color = Color.white;
            }
            catch
            {

            }
#endif

        }

        private Vector3 GetRandomPositionInsideSphere(Vector3 spawnCentroid)
        {
            var randomPosition = Random.insideUnitSphere * positionSpawnRadius;
            randomPosition = new Vector3(randomPosition.x, 0, randomPosition.z);
            randomPosition = spawnCentroid + randomPosition;
            return randomPosition;

        }
    }
}
