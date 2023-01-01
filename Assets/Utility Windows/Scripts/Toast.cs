using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PopUpWindows
{
    public class Toast : MonoBehaviour
    {
        [SerializeField] TMP_Text _txt = null;
        [SerializeField] Image _image = null;
        [SerializeField] LayoutGroup[] _layoutGroups = null;

        static float Lifespan => PopUpController.Instance.ToastLifespan;
        static AnimationCurve FadeCurve => PopUpController.Instance.ToastFadeCurve;

        public void Initialize(string text, LogType type, bool log)
        {
            name = $"Toast: {text}";
            Dictionary<LogType, Color> colors = PopUpController.Instance.ToastColors;
            _txt.text = text;
            transform.SetSiblingIndex(10000);
            StartCoroutine(Lifetime(Lifespan, colors[type]));

            if(log)
            {
                switch(type)
                {
                    case LogType.Log:
                        Debug.Log(text);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning(text);
                        break;
                    case LogType.Error:
                        Debug.LogError(text);
                        break;
                    default:
                        Debug.LogError($"Toast Type {nameof(type)} not handled");
                        break;
                }
            }
        }

        IEnumerator Lifetime(float duration, Color color)
        {
            yield return null;
            foreach(LayoutGroup g in _layoutGroups)
            {
                g.enabled = false;
                g.enabled = true;
                g.CalculateLayoutInputHorizontal();
                g.CalculateLayoutInputVertical();
            }

            Color minColor = new Color(color.r, color.g, color.b, 0);
            Color maxColor = color;

            Color txtMinColor = new Color(1f, 1f, 1f, 0f);
            Color txtMaxColor = Color.white;

            for(float t = 0f; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                _image.color = Color.Lerp(minColor, maxColor, FadeCurve.Evaluate(progress));
                _txt.color = Color.Lerp(txtMinColor, txtMaxColor, FadeCurve.Evaluate(progress));
                yield return null;
            }

            PopUpController.Instance.ToastPool.ReturnToPool(gameObject);
        }

        private void OnDestroy()
        {
            PopUpController.Instance.ToastPool.NukeFromPool(gameObject);
        }
    }
}