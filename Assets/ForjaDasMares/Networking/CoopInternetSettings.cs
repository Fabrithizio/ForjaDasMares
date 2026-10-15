using System;
using UnityEngine;
namespace ForjaDasMares.Networking
{
    [CreateAssetMenu(menuName = "Forja das Mares/Internet settings")]
    public sealed class CoopInternetSettings : ScriptableObject
    {
        public const string ResourceName = "CoopInternet";
        [SerializeField] string environment = "production";
        [Tooltip("Vazio: seleção automática de região pelo SDK Relay.")]
        [SerializeField] string relayRegion = "";
        [SerializeField, Range(10, 120)] int connectionTimeoutSeconds = 45;
        public string Environment => environment?.Trim();
        public string RelayRegion => string.IsNullOrWhiteSpace(relayRegion) ? null : relayRegion.Trim();
        public int ConnectionTimeoutSeconds => connectionTimeoutSeconds;
        public bool IsValid => !string.IsNullOrWhiteSpace(Environment) && Environment.Length <= 64
            && connectionTimeoutSeconds >= 10 && connectionTimeoutSeconds <= 120;
        public static bool HasLinkedProject(string projectId) => Guid.TryParse(projectId, out var id) && id != Guid.Empty;
    }
}
