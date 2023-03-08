using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewBehaviourScript : MonoBehaviour
{
	[SerializeField] Slider RedSlider;

	[SerializeField] Slider GreenSlider;
    
	[SerializeField] Slider BlueSlider;

	Material colorMaterial;

    // Start is called before the first frame update
    void Start()
    {
        colorMaterial = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        colorMaterial.color = new Color(RedSlider.value, BlueSlider.value, GreenSlider.value);
    }
}

