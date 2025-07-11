using UnityEngine;
using UnityEngine.AI;
using System.Collections;

namespace scpcbr {
    public class NPC_106 : MonoBehaviour {
        [System.Serializable]
        public struct AnimationConfig {
            public string emergeTrigger, walkTrigger, wallTraverseTrigger, catchUpEmergeTrigger, chaseEndTrigger;
            public float emergeLength, wallTraverseLength, catchUpEmergeLength;
        }

        [System.Serializable]
        public struct WallTraverseConfig {
            public LayerMask layerMask;
            public float maxThickness, checkDistance, cooldownDuration;
            public AudioClip[] sounds;
        }

        [System.Serializable]
        public struct CatchUpConfig {
            public float distance, navSampleRadius, cooldownDuration;
            public AudioClip[] sounds;
        }

        [Header("Chase Settings")]
        [SerializeField] private int chaseDuration = 120;
        [SerializeField] private string chaseMusicTrackName;
        [SerializeField] private AudioClip chaseStartSound, chaseEndSound;
        [SerializeField] private AnimationConfig animationConfig;

        [Header("Systems")]
        [SerializeField] private WallTraverseConfig wallConfig;
        [SerializeField] private CatchUpConfig catchUpConfig;

        [Header("Audio")]
        [SerializeField] private AudioClip[] laughClips;
        [SerializeField] private int laughMinTime = 5, laughMaxTime = 10;
        [SerializeField] private AudioSource sfxSource, additionalSFX, laughSource;

        private NavMeshAgent agent;
        private Animator animator;
        private Transform player;
        private bool isChasing, isActive, isTraversing;
        private float wallCooldown, catchUpCooldown;

        private void Start() {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            // Enable animator and agent immediately, as NPC_106 should be active on spawn
            animator.enabled = true;
            agent.enabled = false; // Will be enabled after emerge
            isActive = true;
            player = GameObject.FindWithTag("Player")?.transform;
            animator.SetTrigger(animationConfig.emergeTrigger); // Emerge animation is default
            StartCoroutine(EmergeSequence());
            StartCoroutine(ChaseEndSequence());
        }

        private void Update() {
            UpdateCooldowns();
            if (ShouldSkipChaseLogic()) return;
            HandleChaseLogic();
        }

        private void UpdateCooldowns() {
            wallCooldown = Mathf.Max(0f, wallCooldown - Time.deltaTime);
            catchUpCooldown = Mathf.Max(0f, catchUpCooldown - Time.deltaTime);
        }

        private bool ShouldSkipChaseLogic() => !isChasing || !player || isTraversing;

        private void HandleChaseLogic() {
            if (ShouldCatchUp()) {
                StartCoroutine(CatchUp());
                return;
            }
            if (wallCooldown <= 0f && TryWallTraverse()) return;
            agent.SetDestination(player.position);
        }

        private bool ShouldCatchUp() =>
            catchUpCooldown <= 0f && Vector3.Distance(transform.position, player.position) > catchUpConfig.distance;

        [ContextMenu("Start Chase")]
        public void StartChase() {
            if (isActive) return;
            isActive = true;
            animator.enabled = true;
            animator.SetTrigger(animationConfig.emergeTrigger);
            StartCoroutine(EmergeSequence());
            StartCoroutine(ChaseEndSequence());
        }

        private void EndChase() {
            if (animator && !string.IsNullOrEmpty(animationConfig.chaseEndTrigger))
                animator.SetTrigger(animationConfig.chaseEndTrigger);

            isChasing = isActive = false;
            agent.isStopped = true;
            agent.ResetPath();
            MusicPlayer.Instance.ChangeMusic("LCZ");
        }

        private IEnumerator EmergeSequence() {
            yield return new WaitForSeconds(animationConfig.emergeLength);
            animator.SetTrigger(animationConfig.walkTrigger);
            agent.enabled = true;
            player = GameObject.FindWithTag("Player")?.transform;
            isChasing = true;

            StartCoroutine(LaughLoop());
            PlayAudio(additionalSFX, chaseStartSound);

            if (!string.IsNullOrEmpty(chaseMusicTrackName))
                MusicPlayer.Instance.ChangeMusic(chaseMusicTrackName);
        }

        private bool TryWallTraverse() {
            Vector3 direction = (player.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance < 2f) return false;

            var hits = Physics.RaycastAll(transform.position, direction,
                Mathf.Min(distance, wallConfig.checkDistance), wallConfig.layerMask);

            if (hits.Length < 2) return false;

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            return ProcessWallHits(hits[0], hits[1]);
        }

        private bool ProcessWallHits(RaycastHit entry, RaycastHit exit) {
            float thickness = Vector3.Distance(entry.point, exit.point);
            bool differentColliders = entry.collider != exit.collider;
            float normalDot = Vector3.Dot(entry.normal, exit.normal);

            if (thickness <= wallConfig.maxThickness && (normalDot < -0.7f || differentColliders)) {
                StartCoroutine(TraverseWall(entry, exit));
                return true;
            }
            return false;
        }

        private IEnumerator TraverseWall(RaycastHit entry, RaycastHit exit) {
            SetTraversing(true);
            animator.SetTrigger(animationConfig.wallTraverseTrigger);
            PlayRandomSound(wallConfig.sounds);

            Vector3 exitPosition = exit.point + (exit.point - entry.point).normalized * 0.15f;
            TeleportAgent(exitPosition, -exit.normal);

            animator.SetTrigger(animationConfig.emergeTrigger);
            yield return new WaitForSeconds(1f);

            SetTraversing(false);
            wallCooldown = wallConfig.cooldownDuration;
        }

        private IEnumerator CatchUp() {
            catchUpCooldown = catchUpConfig.cooldownDuration;
            SetTraversing(true);

            Vector3 targetPosition = player.position - player.forward * 2f;
            if (FindValidNavMeshPosition(targetPosition, out Vector3 validPosition)) {
                TeleportAgent(validPosition, (player.position - validPosition).normalized);
                PlayRandomSound(catchUpConfig.sounds);
                animator.SetTrigger(animationConfig.catchUpEmergeTrigger);

                yield return new WaitForSeconds(animationConfig.catchUpEmergeLength);
            }

            SetTraversing(false);
        }

        private bool FindValidNavMeshPosition(Vector3 target, out Vector3 validPosition) {
            return NavMesh.SamplePosition(target, out NavMeshHit hit, catchUpConfig.navSampleRadius, NavMesh.AllAreas) ||
                   NavMesh.SamplePosition(player.position, out hit, catchUpConfig.navSampleRadius, NavMesh.AllAreas)
                   ? (validPosition = hit.position) != Vector3.zero : (validPosition = Vector3.zero) == Vector3.zero;
        }

        private void SetTraversing(bool traversing) {
            isTraversing = traversing;
            agent.isStopped = traversing;
            if (traversing) agent.ResetPath();
            else if (player) agent.SetDestination(player.position);
        }

        private void TeleportAgent(Vector3 position, Vector3 forward) {
            agent.enabled = false;
            transform.position = position;
            transform.forward = forward;
            agent.enabled = true;
            agent.Warp(position);
        }

        private IEnumerator LaughLoop() {
            while (isChasing && laughSource) {
                yield return new WaitForSeconds(Random.Range(laughMinTime, laughMaxTime));
                PlayRandomSound(laughClips, laughSource);
            }
        }

        private IEnumerator ChaseEndSequence() {
            yield return new WaitForSeconds(chaseDuration);
            PlayAudio(additionalSFX, chaseEndSound);
            yield return new WaitForSeconds(1f);
            EndChase();
            yield return new WaitForSeconds(10f);
            Destroy(gameObject);
        }

        private void PlayAudio(AudioSource source, AudioClip clip) {
            if (source && clip) {
                source.clip = clip;
                source.Play();
            }
        }

        private void PlayRandomSound(AudioClip[] clips, AudioSource source = null) {
            if (clips.Length == 0) return;
            var audioSource = source ?? sfxSource;
            if (!audioSource) return;

            audioSource.transform.position = transform.position;
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}