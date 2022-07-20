using UnityEngine;

public class LegoManager : MonoBehaviour
{
    public GameObject Object1, Object2, Container1, Container2;
    private Pose init_Parent, init_Obj1, init_Obj2, init_Cont1, init_Cont2;

    private Rigidbody rb1, rb2;

    private const float distMax = 0.03f;
    private const float angleMax = 30.0f;
    private const float errorThreshold = 0.35f;
    private float _dist, _angle, _error;

    //for gradienting the normalized color
    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;
    private Renderer rendererObject1, rendererObject2;

    public enum State
    {
        Start, Stop
    }
    public State LegoState
    {
        get; set;
    }
    private void ChangeState(State newState)
    {
        if (LegoState != newState) LegoState = newState;
    }

    private void Start()
    {
        ChangeState(State.Start);

        SetGradiency();
        rendererObject1 = Object1.GetComponent<Renderer>();
        rendererObject2 = Object2.GetComponent<Renderer>();

        SaveInitTransforms();

        rb1 = Container1.GetComponent<Rigidbody>();
        rb2 = Container2.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (LegoState == State.Stop) return;

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

        Object1.transform.SetParent(gameObject.transform, false);
        Object2.transform.SetParent(gameObject.transform, false);
        Container1.SetActive(false);
        Container2.SetActive(false);

        Object1.transform.localPosition = Vector3.zero;
        Object1.transform.localRotation = Quaternion.identity;
        Object2.transform.localPosition = Vector3.zero;
        Object2.transform.localRotation = Quaternion.identity;
        
        gameObject.GetComponent<DyiPinchGrab.DyiHandManipulation>().enabled = true;
        ChangeState(State.Stop);
    }

    public void ResetLegoing()
    {
        gameObject.GetComponent<DyiPinchGrab.DyiHandManipulation>().enabled = false;

        Container1.SetActive(true);
        Container2.SetActive(true);
        Object1.transform.SetParent(Container1.transform, false);
        Object2.transform.SetParent(Container2.transform, false);

        LoadInitTransforms();

        ChangeState(State.Start);
    }

    private void SaveInitTransforms()
    {
        init_Parent.position = gameObject.transform.localPosition;
        init_Parent.rotation = gameObject.transform.localRotation;

        init_Obj1.position = Object1.transform.localPosition;
        init_Obj1.rotation = Object1.transform.localRotation;

        init_Obj2.position = Object2.transform.localPosition;
        init_Obj2.rotation = Object2.transform.localRotation;

        init_Cont1.position = Container1.transform.localPosition;
        init_Cont1.rotation = Container1.transform.localRotation;

        init_Cont2.position = Container2.transform.localPosition;
        init_Cont2.rotation = Container2.transform.localRotation;
    }

    private void LoadInitTransforms()
    {
        gameObject.transform.localPosition = init_Parent.position;
        gameObject.transform.localRotation = init_Parent.rotation;

        Object1.transform.localPosition = init_Obj1.position;
        Object1.transform.localRotation = init_Obj1.rotation;

        Object2.transform.localPosition = init_Obj2.position;
        Object2.transform.localRotation = init_Obj2.rotation;

        Container1.transform.localPosition = init_Cont1.position;
        Container1.transform.localRotation = init_Cont1.rotation;

        Container2.transform.localPosition = init_Cont2.position;
        Container2.transform.localRotation = init_Cont2.rotation;
    }

    public void SetToPassthru(bool passthru)
    {
        if (rb1 == null || rb2 == null) return;
        rb1.isKinematic = passthru;
        rb2.isKinematic = passthru;
    }
}
