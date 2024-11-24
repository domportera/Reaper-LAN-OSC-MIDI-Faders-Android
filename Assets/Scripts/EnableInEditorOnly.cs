using UnityEngine;

public class EnableInEditorOnly : MonoBehaviour
{
    private void Awake()
    {
        #if UNITY_EDITOR
            gameObject.SetActive(true);
        #else
            gameObject.SetActive(false);
        #endif
    }
}
