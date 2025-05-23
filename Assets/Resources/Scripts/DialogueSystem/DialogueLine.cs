using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public Sprite speakerImage;
    [TextArea(2, 5)]
    public string line;
}
