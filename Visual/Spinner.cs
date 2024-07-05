using UnityEngine;

public class Spinner : MonoBehaviour
{
    public bool IsBeingUsed = false;

    private float SpinVelocity = 0f;
    private float SpinAcceleration = 100f;
    private float NormalSpinVelocity = 90f;
    private float CurrentTimer = 0f;
    private float SpinningTime = 3f;
    
    private Quaternion InitialRotation;
    private Quaternion FromRotation;
    void Start()
    {
        InitialRotation = transform.rotation;
    }

    public void StartHyperSpinning(float spinTime)
    {
        IsBeingUsed = true;
        SpinVelocity = NormalSpinVelocity;
        SpinAcceleration = 1000f;
        SpinningTime = spinTime;
    }
    public void StopHyperSpinning()
    {
        IsBeingUsed = false;
        SpinVelocity = 0f;
        SpinAcceleration = 0f;
        FromRotation = transform.rotation;
    }

    void Update()
    {
        if (IsBeingUsed)
        {
            CurrentTimer += Time.deltaTime;
            if (CurrentTimer >= SpinningTime)
                StopHyperSpinning();
        }

        if (!IsBeingUsed)
            transform.Rotate(Vector3.up, NormalSpinVelocity * Time.deltaTime);
        else
        {
            SpinVelocity += SpinAcceleration * Time.deltaTime;
            transform.Rotate(Vector3.up, SpinVelocity * Time.deltaTime);
            transform.Rotate(Vector3.forward, SpinVelocity * Time.deltaTime);
            transform.Rotate(Vector3.right, SpinVelocity * Time.deltaTime);
        }
        

    }
}
