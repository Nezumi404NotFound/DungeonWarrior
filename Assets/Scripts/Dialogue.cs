using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string name;
    public string content;
}

[System.Serializable]
public class DialogueData
{
    public List<DialogueLine> dialogues;
}