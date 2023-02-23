using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ScratchCardAsset.Tools
{
    public class EventSystemHelper : MonoBehaviour
    {
        [SerializeField] private EventSystem eventSystem;
        
        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystem.TryGetComponent<StandaloneInputModule>(out var standaloneInputModule))
            {
                Destroy(standaloneInputModule);
            }
            if (!eventSystem.TryGetComponent<InputSystemUIInputModule>(out _))
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            var inputSystemUIModule = eventSystem.GetComponent("InputSystemUIInputModule");
            if (inputSystemUIModule != null)
            {
                Destroy(inputSystemUIModule);
            }
            if (!eventSystem.TryGetComponent<StandaloneInputModule>(out _))
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }

        private void OnValidate()
        {
            if (eventSystem == null)
            {
                TryGetComponent(out eventSystem);
            }
        }
    }
}