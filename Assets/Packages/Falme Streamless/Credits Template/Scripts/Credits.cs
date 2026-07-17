using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace FalmeStreamless.Credits {
    [RequireComponent(typeof(CanvasScaler))]
    public class Credits : MonoBehaviour {
        public static event Action OnCreditsFinished;

        [Header("All Credits Data")]
        [SerializeField] private TextAsset creditsJSON;

        [Header("References")]
        [SerializeReference] private Scroll scroll;

        private void OnEnable() {
            End.onCreditEndReached += CreditEndReached;

            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Credits);
        }

        private void OnDisable() {
            End.onCreditEndReached -= CreditEndReached;

            MusicManager.Instance.SetTrack(MusicManager.MusicTrack.Menu);
        }

        private void OnDestroy() {
            End.onCreditEndReached -= CreditEndReached;
        }

        private void Start() {
            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();

            scroll.Initialize(
                canvasScaler.referenceResolution,
                GetJsonData()
                );
        }

        private CreditsData GetJsonData() {
            return JsonConvert.DeserializeObject<CreditsData>(creditsJSON.text);
        }

        private void CreditEndReached(float difference) {
            scroll.StopScrolling();
            scroll.ScrollAdd(-difference); // Fix Overshot position
            OnCreditsFinished?.Invoke();
        }
    }
}
