using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DiagnosticController : MonoBehaviour
{
    public TextMeshProUGUI OutputElement;

    public string CurrentContent;

    public Dictionary<string, string> ContentElements = new Dictionary<string, string>();

    public void Clear()
    {
        CurrentContent = string.Empty;
        ContentElements.Clear();
    }

    public void AddContent<T>(string label, T content)
    {
        if (OutputElement == null)
            return;

        if (label == null)
        {
            if (CurrentContent.Length > 0)
            {
                CurrentContent += System.Environment.NewLine;
            }

            CurrentContent += content.ToString();
        }
        else
        {
            ContentElements[label] = content.ToString();
        }
    }

    StringBuilder sb = new StringBuilder();

    void Update()
    {
        if (OutputElement != null)
        {
            sb.Length = 0;

            foreach (KeyValuePair<string, string> kvp in ContentElements)
            {
                sb.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            sb.AppendLine(CurrentContent);
            OutputElement.text = sb.ToString();
        }
    }

    static public DiagnosticController Current
    {
        get
        {
            if (GameController.TheGameController != null)
                return GameController.TheGameController.Diagnostics;

            return null;
        }
    }
}
