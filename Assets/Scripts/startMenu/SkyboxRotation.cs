using UnityEngine;
public class SkyboxRotation : MonoBehaviour
{
    // private float _degree;
    //
    // void Start()
    // {
    //     _degree = 0;
    // }
    //
    // void Update()
    // {
    //     _degree += Time.deltaTime;
    //     if (_degree >= 360) 
    //         _degree = 0;
    //     
    //     RenderSettings.skybox.SetFloat("_Rotation", Mathf.Lerp(0, 1, _degree));
    //     
    // }
    public float rotationSpeed = 1.0f;
    
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
    
}
