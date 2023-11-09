using UnityEngine;
using UnityEngine.UI;

public class NextInstruction: MonoBehaviour {

    Text Instruction;

    private void Start()
    {
        Instruction = GetComponent<Text>();
    }

    public void RenderInstruction(string inst)
    {      
        Instruction.text = inst;
    }
	
	
}
