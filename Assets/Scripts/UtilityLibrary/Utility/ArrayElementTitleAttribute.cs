using UnityEngine;

public class ArrayElementTitleAttribute : PropertyAttribute
{
    public string Varname;
    public ArrayElementTitleAttribute()
    {

    }

    public ArrayElementTitleAttribute(string ElementTitleVar)
    {
        Varname = ElementTitleVar;
    }
}
