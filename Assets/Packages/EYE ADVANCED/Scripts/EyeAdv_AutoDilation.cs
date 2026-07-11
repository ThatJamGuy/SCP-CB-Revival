using UnityEngine;
using System.Collections;

public class EyeAdv_AutoDilation : MonoBehaviour {

	public bool enableAutoDilation = true;
	public Transform sceneLightObject;
	public float lightSensitivity = 1.0f;
	public float dilationSpeed = 0.1f;
	public float maxDilation = 1.0f;

	private Light sceneLight;
	private float lightIntensity;
	private float lightAngle;
	private float dilateTime = 0.0f;
	private float pupilDilation = 0.5f;
	private float currTargetDilation = -1.0f;
	private float targetDilation = 0.0f;
	private float currLightSensitivity = -1f;
	private Renderer eyeRenderer;


	void Start () {
		eyeRenderer = gameObject.GetComponent<Renderer>();
		if (sceneLightObject != null){
			sceneLight = sceneLightObject.GetComponent<Light>();
		}
	}
	


	void LateUpdate () {

		if (sceneLight != null){

			//set scene lighting
			lightIntensity = sceneLight.intensity;

			//handle auto dilation
			if (enableAutoDilation){

				//handle timer
				if (currTargetDilation != targetDilation || currLightSensitivity != lightSensitivity){
					dilateTime = 0.0f;
					currTargetDilation = targetDilation;
					currLightSensitivity = lightSensitivity;
				}

				//calculate look angle
				lightAngle = Vector3.Angle(sceneLightObject.transform.forward,transform.forward) / 180.0f;
				targetDilation = Mathf.Lerp(1.0f, 0.0f,lightAngle * lightIntensity * lightSensitivity);

				//handle dilation
				dilateTime += Time.deltaTime*dilationSpeed;
				pupilDilation = Mathf.Clamp(pupilDilation, 0.0f, maxDilation);
				pupilDilation = Mathf.Lerp(pupilDilation, targetDilation, dilateTime);

				eyeRenderer.sharedMaterial.SetFloat("_pupilSize",pupilDilation);
				
			}
		}

	}
}
