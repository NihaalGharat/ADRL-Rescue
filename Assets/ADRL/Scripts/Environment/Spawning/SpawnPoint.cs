namespace ADRL.Environment.Spawning
{
    using UnityEngine;

    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private string _category;

        public string Category => _category;

        public Vector3 Position => transform.position;

        public Quaternion Rotation => transform.rotation;
    }
}
