using UnityEngine;
using System.Collections;

public class LoadingCircle : MonoBehaviour
{
    private RectTransform rectComponent;
    private float rotateSpeed = 200f;

    private void Start()
    {
        rectComponent = GetComponent<RectTransform>();
        gameObject.SetActive(false); // Initially disable
    }

    private void Update()
    {
        rectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    public void StartLoading()
    {
        gameObject.SetActive(true); // Enable to make it visible and start rotating
        StartCoroutine(StopLoadingAfterDelay(2f));
    }

    IEnumerator StopLoadingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // Disable to make it invisible and stop rotating
    }
}
