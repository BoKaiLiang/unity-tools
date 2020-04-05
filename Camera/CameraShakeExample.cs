using UnityEngine;
using PKTools.VFX;

public class CameraShakeExample : MonoBehaviour
{
    [SerializeField]
    private Camera Camera;

    [SerializeField]
    private CameraShake cameraShake;

    private void Start() {
        cameraShake.Setup(Camera);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.A)) {
            cameraShake.Shake(0.6f, 30.0f, 10.0f);
        }

        if (Input.GetKeyDown(KeyCode.S)) {
            cameraShake.Shake(2.0f);
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            cameraShake.Shake();
        }
    }
}
