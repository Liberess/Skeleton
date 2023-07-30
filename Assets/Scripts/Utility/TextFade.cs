using System.Collections;
using UnityEngine;
using TMPro;

public class TxetFade : MonoBehaviour
{
    private TextMeshProUGUI txt;

    [SerializeField] private float fadeSpeed = 1.0f;

    private void Start()
    {
        txt = GetComponent<TextMeshProUGUI>();
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 0);
        while(txt.color.a < 1.0f)
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, txt.color.a + (Time.deltaTime / fadeSpeed));
            yield return null;
        }
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1);
        while (txt.color.a > 0.0f)
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, txt.color.a - (Time.deltaTime / fadeSpeed));
            yield return null;
        }
        StartCoroutine(FadeIn());
    }
}