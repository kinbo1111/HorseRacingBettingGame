using UnityEngine;

namespace GlobalSnowEffect {
    public class DemoWalk : MonoBehaviour {
        GlobalSnow snow;

        void Start() {
            snow = GlobalSnow.instance;
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.T)) {
                snow.enabled = !snow.enabled;
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                Camera cam = Camera.main;
                //Debug.Log(snow.GetSnowAmountAt(cam.transform.position + Vector3.up));
                Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit)) {
                    //GlobalSnow.instance.MarkSnowAt(hit.point, 3f);
                    GlobalSnow.instance.FootprintAt(hit.point, cam.transform.forward);
                }
            }


        }
    }
}