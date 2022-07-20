using UnityEngine;

public class TransformDifferenceCalculator : MonoBehaviour
{
    public GameObject Object1, Object2, Container1, Container2;

    private const float distMax = 0.03f;
    private const float angleMax = 30.0f;
    private const float errorThreshold = 0.35f;
    private float _dist, _angle, _error;

    //for gradienting the normalized color
    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;
    private Renderer rendererObject1, rendererObject2;

    private void Start()
    {
        SetGradiency();
        rendererObject1 = Object1.GetComponent<Renderer>();
        rendererObject2 = Object2.GetComponent<Renderer>();
    }

    private void Update()
    {
        _dist = NormalizeToMax(Vector3.Distance(Object1.transform.position, Object2.transform.position), true);
        _angle = NormalizeToMax(Vector3.Angle(Object2.transform.forward, Object1.transform.forward), false);
        _error = (_dist + _angle) / 2;

        rendererObject1.material.color = gradient.Evaluate(_error);
        rendererObject2.material.color = gradient.Evaluate(_error);

        if (_error < errorThreshold) FinishLegoing();
    }

    private void SetGradiency()
    {
        gradient = new Gradient();

        // set from what key it changes color
        colorKey = new GradientColorKey[3];
        colorKey[0].color = Color.green;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.yellow;
        colorKey[1].time = 0.5f;
        colorKey[2].color = Color.red;
        colorKey[2].time = 1.0f;

        // always opaque
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
    }

    private float NormalizeToMax(float val, bool isDist)
    {
        float norm = 0.0f;

        if (isDist) norm = val / distMax;
        else norm = val / angleMax;

        if (norm < 1.0f) return norm;
        else return 1.0f;
    }

    private void FinishLegoing()
    {
        gameObject.transform.position = Object1.transform.position;

        Object1.transform.localPosition = Vector3.zero;
        Object1.transform.localRotation = Quaternion.identity;
        Object1.transform.SetParent(gameObject.transform, false);

        Object2.transform.localPosition = Vector3.zero;
        Object2.transform.localRotation = Quaternion.identity;
        Object2.transform.SetParent(gameObject.transform, false);

        Container1.SetActive(false);
        Container2.SetActive(false);

        gameObject.GetComponent<DyiPinchGrab.DyiHandManipulation>().enabled = true;
        gameObject.GetComponent<TransformDifferenceCalculator>().enabled = false;
    }
}
