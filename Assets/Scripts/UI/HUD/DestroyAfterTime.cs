using UnityEngine;
namespace UI
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don't access instance data should be static", Justification = "Start can not be static")]
        private void Start()
        {
            Destroy(gameObject, 5f);
        }
    }
}
